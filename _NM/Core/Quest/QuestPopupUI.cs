using System;
using System.Collections.Generic;
using System.Linq;
using _NM.Core.Manager;
using _NM.Core.Object;
using _NM.Core.UI.UICanvas;
using Cysharp.Threading.Tasks;
using SDUnityExtension.Scripts.Extension;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _NM.Core.Quest
{
    public class QuestPopupUI : CanvasPage
    {
        [Title("Quest Button List")]
        [SerializeField] private ScrollRect currentQuestButtonList;
        [SerializeField] private ScrollRect completeQuestButtonList;
        [SerializeField] private GameObject questButtonPrefab;
        
        [Title("Quest Category Button")]
        [SerializeField] private Button selectedCategoryButton;
        [SerializeField] private Button inprogressCategoryButton;
        [SerializeField] private Button completeCategoryButton;
        
        [Title("Quest Info")]
        [SerializeField] private TextMeshProUGUI[] completeConditions;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI completeTargetText;
        
        [Title("Components")]
        [SerializeField] private GameObject[] randomDoodles;
        [SerializeField] private Button confirmButton;
        
        private readonly List<GameObject> inprogressQuestButtons = new();
        private readonly List<GameObject> completeQuestButtons = new();
        
        private void Awake()
        {
            if (confirmButton)
            {
                confirmButton.onClick?.AddListener(() => Close().Forget());
            }
        }

        protected override UniTask OnPreOpen()
        {
            GameManager.TimeScale = 0f;
            selectedCategoryButton.Select();
            selectedCategoryButton.onClick?.Invoke();
            randomDoodles.ForEach(e => e.SetActive(false));
            randomDoodles[Random.Range(0, randomDoodles.Length)].SetActive(true);
            return base.OnPreOpen();
        }

        protected override UniTask OnPostClose()
        {
            GameManager.TimeScale = 1f;
            return base.OnPostClose();
        }

        private void OnRefreshed()
        {
            foreach (var button in inprogressQuestButtons)
            {
                ObjectPool.Despawn(button);
            }
            foreach (var button in completeQuestButtons)
            {
                ObjectPool.Despawn(button);
            }
            
            inprogressQuestButtons.Clear();
            completeQuestButtons.Clear();
            
            foreach (var questPair in QuestManager.I.CurrentQuests)
            {
                Quest quest = questPair.Value;
                var instance = SpawnQuestButton(quest.QuestName, quest.QuestID, false, currentQuestButtonList.content);
                inprogressQuestButtons.Add(instance);
            }

            foreach (var questPair in QuestManager.I.CompletedQuests.Reverse())
            {
                Quest quest = questPair.Value;
                var instance = SpawnQuestButton(quest.QuestName, quest.QuestID, true, completeQuestButtonList.content);
                completeQuestButtons.Add(instance);
            }
        }
        
        private GameObject SpawnQuestButton(string questName, int questID, bool isCompletedQuest, Transform parent)
        {
            var instance = ObjectPool.Spawn(questButtonPrefab, parent);
            var text = instance.GetComponentInChildren<TextMeshProUGUI>();
            if (text)
            {
                text.text = questName;
            }

            var button = instance.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SetQuestDescription(questID, isCompletedQuest));

            return instance;
        }

        public void OnClickInProgressButtonList()
        {
            selectedCategoryButton.GetComponent<Image>().sprite = selectedCategoryButton.spriteState.disabledSprite;
            selectedCategoryButton = inprogressCategoryButton;
            selectedCategoryButton.GetComponent<Image>().sprite = selectedCategoryButton.spriteState.pressedSprite;
            completeQuestButtonList.gameObject.SetActive(false);
            currentQuestButtonList.gameObject.SetActive(true);
            if (inprogressQuestButtons.Count > 0)
            {
                inprogressQuestButtons[0].TryGetComponent(out Button button); 
                button.Select();
                button.onClick.Invoke();
            }
            else
            {
                SetQuestDescription(0,false);
            }
        
        }

        public void OnClickCompleteButtonList()
        {
            selectedCategoryButton.GetComponent<Image>().sprite = selectedCategoryButton.spriteState.disabledSprite;
            selectedCategoryButton = completeCategoryButton;
            selectedCategoryButton.GetComponent<Image>().sprite = selectedCategoryButton.spriteState.pressedSprite;
            completeQuestButtonList.gameObject.SetActive(true);
            currentQuestButtonList.gameObject.SetActive(false);
            if (completeQuestButtons.Count > 0)
            {
                completeQuestButtons[0].TryGetComponent(out Button button); 
                button.Select();
                button.onClick.Invoke();
            }
            else
            {
                SetQuestDescription(0,true);
            }
        }

        private void OnEnable()
        {
            QuestManager.I.onQuestRefreshed += OnRefreshed;
        }

        private void OnDisable()
        {
            QuestManager.I.onQuestRefreshed -= OnRefreshed;
        }

        private void SetQuestDescription(int questID, bool isCompletedQuest)
        {
            Quest quest = null;
            quest = isCompletedQuest ? QuestManager.I.GetCompletedQuest(questID) : QuestManager.I.GetCurrentQuest(questID);
            
            if (quest != null)
            {
                Debug.Log(quest.QuestName);
                titleText.text = quest.QuestName;
                descriptionText.text = quest.QuestDescription;

                var ender = quest.QuestEnder;
                if (ender > 0 && NpcManager.I.NpcDataDictionary.TryGetValue(ender, out var npc))
                {
                    completeTargetText.text = $"완료 대상 : {npc.Name}";
                }
            
                SetQuestCompleteInfo(quest);
            }
            else
            {
                titleText.text = String.Empty;
                descriptionText.text = String.Empty;
                completeTargetText.text = String.Empty;
            }
        }

        private void SetQuestCompleteInfo(Quest quest)
        {
            completeConditions.ForEach(text => text.gameObject.SetActive(false));
            
            var descriptionCount = 0;
            foreach (var pair in quest.RequiredItems)
            {
                var currentAmount = quest.IsCompleted ? pair.Value.value : quest.CurrentItems[pair.Key];
                var text = QuestManager.GetRequireItemDescription(pair.Value.description, currentAmount, pair.Value.value);
                completeConditions[descriptionCount].gameObject.SetActive(true);
                completeConditions[descriptionCount].text = text;
                descriptionCount++;
            }

            foreach (var stage in quest.RequireStages)
            {
                StageManager.I.GetStage(stage, out var data);
                var text = QuestManager.GetRequireStageDescription(data.StageName,
                    StageManager.I.IsStageCleared(stage));
                completeConditions[descriptionCount].gameObject.SetActive(true);
                completeConditions[descriptionCount].text = text;
                descriptionCount++;
            }
        
            foreach (var pair in quest.RequiredConditions)
            {
                var text = QuestManager.GetRequireConditionDescription(pair.Value, quest.IsFinishedCondition(pair.Key));
                completeConditions[descriptionCount].gameObject.SetActive(true);
                completeConditions  [descriptionCount].text = text;
                descriptionCount++;
            }
        }
    }
}
