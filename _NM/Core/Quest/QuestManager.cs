using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _NM.Core.Character;
using _NM.Core.Data.Savable;
using _NM.Core.Manager;
using _NM.Core.UI.Inventory;
using _NM.Core.UI.Navigation;
using _NM.Core.UI.UICanvas;
using _NM.Core.Utils;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using SDUnityExtension.Scripts.Extension;
using SDUnityExtension.Scripts.Pattern;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _NM.Core.Quest
{
    public class QuestManager : SDSingleton<QuestManager>, ISavable
    {
        public Dictionary<int, Quest> CurrentQuests { get; private set; }
        public Dictionary<int, Quest> PossibleQuests { get; private set; }
        public Dictionary<int, Quest> CompletedQuests { get; private set; }
        
        public event Action<long,int> onItemAcquired;
        public event Action<long,int> onItemAmountChanged;
        public event Action<int,bool> onConditionChanged;
        public event Action<int> onFinishedQuest;
        public event Action onQuestRefreshed;
        public event Action<int> onQuestAdded;
        
        private Dictionary<int, Quest> questList;
        private bool Initialized { get; set; }
        
        [SerializeField] private CurrentQuestInfo[] questInfo;
        private QuestNotifyUI questNotifyUI = null;
        private Quest currentMainQuest = null;
        private readonly List<Quest> currentSubQuests = new();

        protected override void Awake()
        {
            questList = new();
            CurrentQuests = new();
            PossibleQuests = new();
            CompletedQuests = new();
            currentMainQuest = null;
            
            SetInstance(this);
            
            // Enter Play Mode로 진입 시 scene Loaded 이벤트가 호출되지 않으므로 수동으로 호출
            Initialize(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += Initialize;
            DataManager.onInitialized += InitializeQuestList;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= Initialize;
            DataManager.onInitialized -= InitializeQuestList;
        }

        private void Initialize(Scene scene, LoadSceneMode mode)
        {
            questNotifyUI = CanvasController.I.transform.Find("UICanvas").transform.Find("Quest Info Panel")
                .GetComponentInChildren<QuestNotifyUI>();
            questInfo = CanvasController.I.transform.Find("UICanvas").transform.Find("Quest Info Panel")
                .GetComponentsInChildren<CurrentQuestInfo>();
            UpdateQuestInfo();
        }

        private void InitializeQuestList()
        {
            if (Initialized) return;
            questList.Clear();
            CurrentQuests.Clear();
            PossibleQuests.Clear();
            CompletedQuests.Clear();
            currentMainQuest = null;
            currentSubQuests.Clear();
            
            foreach (var column in DataManager.GetSheetData(GoogleSheetsConstantData.ESheetPage.QuestSettings))
            {
                QuestData currentInfo = QuestData.Parse(column);
                if (currentInfo != null)
                {
                    Quest quest = new Quest(currentInfo);
                    questList.TryAdd(currentInfo.ID, quest);
                }
            }
            Load();
            UpdateQuestInfo();
            Initialized = true;
        }

        public Quest GetQuest(int questID)
        {
            return questList.GetValueOrDefault(questID);
        }

        public Quest GetCurrentQuest(int questID)
        {
            return CurrentQuests.GetValueOrDefault(questID);
        }

        public Quest GetCompletedQuest(int questID)
        {
            return CompletedQuests.GetValueOrDefault(questID);
        }

        public void UpdateQuestInfo()
        {
            if (CurrentQuests is { Count: > 0 })
            {
                foreach (var current in CurrentQuests.Select(pair => pair.Value))
                {
                    foreach (var require in current.RequiredItems)
                    {
                        var currentItem = InventoryManager.I.GetItem(require.Key);

                        if (currentItem != null)
                        {
                            current.SetCurrentItemCount(require.Key, currentItem.Amount);
                        }
                    }

                    if (!current.IsCompletable) continue;
                    if (current.QuestStarter == current.QuestEnder
                        || !PossibleQuests.TryGetValue(current.QuestStarter, out var nextQuest)
                        || current.QuestID != nextQuest.QuestID) continue;
                    PossibleQuests.Remove(current.QuestStarter);
                    PossibleQuests[current.QuestEnder] = current;
                }
            }

            if (questInfo != null)
            {
                questInfo.ForEach(e => e?.ClosePanel());
                questInfo[0]?.SetData(currentMainQuest);

                var questInfoIndex = currentMainQuest == null ? 0 : 1;
                var subQuestIndex = 0;
                for (; questInfoIndex < questInfo.Length && subQuestIndex < currentSubQuests.Count; questInfoIndex++)
                {
                    questInfo[questInfoIndex].SetData(currentSubQuests[subQuestIndex++]);
                }
            }
            
            onQuestRefreshed?.Invoke();
        }
        
        public void AddQuest(int questID)
        {
            if (CurrentQuests.ContainsKey(questID)) return;
            var quest = questList[questID];
                
            NavigationIndicator.I.RemoveMarker(quest.QuestStarter.ToString());

            if (quest.QuestType is QuestTypeInfo.MainQuest or QuestTypeInfo.DayEndQuest)
            {
                currentMainQuest = quest;
                if (quest.RequireStages.Count > 0)
                {
                    foreach (var stage in quest.RequireStages)
                    {
                        StageManager.I.UnlockedStages.Enqueue(stage);
                    }
                }
            }
            else
            {
                currentSubQuests.Add(quest);
            }
            PossibleQuests[quest.QuestStarter] = quest;
            CurrentQuests.Add(questID, quest);
            quest.InitializeCurrentCount();
            questNotifyUI?.PlayQuestNotify(quest.QuestName, false).Forget();
            onQuestAdded?.Invoke(questID);
            UpdateQuestInfo();
        }

        public void CompleteQuest(int questID)
        {
            var completedQuest = GetCurrentQuest(questID);
            
            if (completedQuest != null)
            {
                foreach (var requiredItem in completedQuest.RequiredItems)
                {
                    var item = InventoryManager.I.GetItem(requiredItem.Key);
                    if (item == null) continue;
                    
                    var itemAmount = completedQuest.QuestType == QuestTypeInfo.SubQuest ? item.Amount : requiredItem.Value.value;
                    InventoryManager.I.DisAmountItem(item, itemAmount);

                    if (item.ItemInfoData.RewardHealAmount > 0)
                    {
                        Character.Character.Local.Stat.AddStat(new StatData()
                        {
                            type = Stat.Health,
                            value = item.ItemInfoData.RewardHealAmount * itemAmount
                        });
                    }
                    
                    if (item.ItemInfoData.RewardDamageAmount > 0)
                    {
                        Character.Character.Local.Stat.AddStat(new StatData()
                        {
                            type = Stat.Damage,
                            value = item.ItemInfoData.RewardDamageAmount * itemAmount
                        });
                    }
                    
                    if (item.ItemInfoData.RewardStaminaAmount > 0)
                    {
                        Character.Character.Local.Stat.AddStat(new StatData()
                        {
                            type = Stat.Stamina,
                            value = item.ItemInfoData.RewardStaminaAmount * itemAmount
                        });
                    }
                }
                
                completedQuest.SetCompleted(true);
                PossibleQuests.Remove(completedQuest.QuestEnder);
                if (completedQuest.QuestType == QuestTypeInfo.SubQuest)
                {
                    currentSubQuests.Remove(completedQuest);
                }
                else
                {
                    currentMainQuest = null;
                }
                CompletedQuests.TryAdd(questID, completedQuest);
                CurrentQuests.Remove(questID);
                
                questNotifyUI.PlayQuestNotify(completedQuest.QuestName, true).Forget();
                
                // 다음 퀘스트 ID가 지정되어 있으면
                if (completedQuest is { HasNextQuest: true })
                {
                    foreach (var nextQuestID in completedQuest.NextQuestID)
                    {
                        var nextQuest = GetQuest(nextQuestID);

                        if (nextQuest != null)
                        {
                            PossibleQuests.TryAdd(nextQuest.QuestStarter, nextQuest);
                        }
                    }
                }
                onFinishedQuest?.Invoke(questID);
            } 
            SaveManager.Save();
            UpdateQuestInfo();
        }
        
        [Obsolete]
        public void OnEnemySlain(long enemyID, int enemyCount) { }

        public void OnItemAcquired(long itemID, int itemCount)
        {
            onItemAcquired?.Invoke(itemID, itemCount);
            UpdateQuestInfo();
        }

        public void OnConditionChanged(int conditionID, bool condition)
        {
            foreach (var questPair in CurrentQuests)
            {
                Quest currentQuest = questPair.Value;
            
                if (currentQuest.RequiredConditions.ContainsKey(conditionID))
                {
                    currentQuest.SetCondition(conditionID, condition);
                }
            }
            onConditionChanged?.Invoke(conditionID, condition);
            UpdateQuestInfo();
        }
        
        public void Load()
        {
            var currentQuestIdJson = PlayerPrefs.GetString(nameof(CurrentQuests));
            var possibleQuestIdJson = PlayerPrefs.GetString(nameof(PossibleQuests));
            var completedQuestIdJson = PlayerPrefs.GetString(nameof(CompletedQuests));
            var conditionList = PlayerPrefs.GetString("ConditionList");
            
            if (string.IsNullOrEmpty(currentQuestIdJson) || string.IsNullOrEmpty(possibleQuestIdJson) || string.IsNullOrEmpty(completedQuestIdJson) || string.IsNullOrEmpty(conditionList))
            {
                return;
            }
            
            var currentQuestIdList = JsonConvert.DeserializeObject<List<int>>(currentQuestIdJson);
            var possibleQuestIdList = JsonConvert.DeserializeObject<List<int>>(possibleQuestIdJson);
            var completedQuestIdList = JsonConvert.DeserializeObject<List<int>>(completedQuestIdJson);
            var currentQuestConditionList = JsonConvert.DeserializeObject<List<Dictionary<int, bool>>>(conditionList);
            
            foreach (var questID in currentQuestIdList)
            {
                //Debug.Log(questID);
                AddQuest(questID);
            }
            
            foreach (var questID in possibleQuestIdList)
            {
                Quest possibleQuest = GetQuest(questID);
                
                if(!PossibleQuests.TryAdd(possibleQuest.QuestStarter, possibleQuest))
                {
                    PossibleQuests[possibleQuest.QuestStarter] = possibleQuest;
                }
            }
            
            foreach (var questID in completedQuestIdList)
            {
                CompletedQuests.TryAdd(questID, GetQuest(questID));
            }

            foreach (var pair in currentQuestConditionList.SelectMany(condition => condition))
            {
                OnConditionChanged(pair.Key, pair.Value);
            }
            
            UpdateQuestInfo();
        }

        public void Save()
        {
            var currentQuestIdList = CurrentQuests.Select(e => e.Key).ToList();
            var possibleQuestIdList = PossibleQuests.Select(e => e.Value.QuestID).ToList();
            var completedQuestIdList = CompletedQuests.Select(e => e.Key).ToList();
            var currentQuestConditionList = CurrentQuests.Select(e => e.Value.CurrentConditions).ToList();
            
            PlayerPrefs.SetString(nameof(CurrentQuests), JsonConvert.SerializeObject(currentQuestIdList));
            PlayerPrefs.SetString(nameof(PossibleQuests), JsonConvert.SerializeObject(possibleQuestIdList));
            PlayerPrefs.SetString(nameof(CompletedQuests), JsonConvert.SerializeObject(completedQuestIdList));
            PlayerPrefs.SetString("ConditionList", JsonConvert.SerializeObject(currentQuestConditionList));
        }
        
        private static readonly string completeTag = "<color=#9b9da1>";
        public static string GetRequireItemDescription(string itemName, int current, int target)
        {
            return $"{(current >= target ? completeTag : string.Empty)}{itemName} {current}/{target}";
        }
        
        public static string GetRequireStageDescription(string questName, bool isCleared)
        {
            return $"{(isCleared ? completeTag : string.Empty)}{questName} 스테이지 클리어";
        }
        
        public static string GetRequireConditionDescription(string description, bool isCleared)
        {
            return isCleared ? $"{completeTag}{description}" : description;
        }
    }
}

