using System;
using System.Collections.Generic;
using System.Linq;
using _NM.Core.Manager;
using UnityEngine;

namespace _NM.Core.Quest
{
    [Serializable]
    public class Quest
    {
        public QuestTypeInfo QuestType { get; private set; }
        
        //<summary>이 퀘스트의 ID</summary>
        public int QuestID { get; private set; }
        
        //<summary>퀘스트 이름</summary>
        public string QuestName { get; private set; }
        
        //<summary>퀘스트 설명</summary>
        public string QuestDescription { get; private set; } 
        
        //<summary>퀘스트 시작 NPC</summary>
        public int QuestStarter { get; private set; }
        
        //<summary>퀘스트 완료 NPC</summary>
        public int QuestEnder { get; private set; }

        public bool HasNextQuest => NextQuestID.Count > 0;
        //<summary>다음 퀘스트 ID</summary>
        public List<int> NextQuestID { get; private set; } 

        //<summary>필요한 아이템 목록</summary>
        public Dictionary<long, RequireInfo> RequiredItems { get; private set; }
        public bool HasRequiredItems => RequiredItems.Count > 0;
        
        //<summary>현재 아이템 목록</summary>
        private Dictionary<long, int> currentItems = new();
        public Dictionary<long, int> CurrentItems => currentItems;

        //<summary>클리어 스테이지 목록</summary>
        public HashSet<int> RequireStages { get; private set; }
        public bool HasRequiredStages => RequireStages.Count > 0;
        
        //<summary>필요한 완료 조건</summary>
        public Dictionary<int, string> RequiredConditions { get; private set; }
        public bool HasRequiredConditions => RequiredConditions.Count != 0;
        
        //<summary>현재 조건</summary>
        private Dictionary<int, bool> currentConditions = new();
        public Dictionary<int, bool> CurrentConditions => currentConditions;

        private string questSummary;
        public string QuestSummary => questSummary;
    
        public bool IsCompleted { get; private set; } //이 퀘스트가 완료 되었는가
        public bool IsForceCompletable { get; set; } //이 퀘스트를 강제로 완료할 수 있는가
    
        //이 퀘스트를 현재 완료 할 수 있는가 (필요 조건 모두 만족 했을 때)
        public bool IsCompletable
        {
            get
            {
                if (IsForceCompletable)
                {
                    return true;
                }
                
                if (HasRequiredItems)
                {
                    if (RequiredItems.Any(required => currentItems.ContainsKey(required.Key) == false ||
                                                      currentItems[required.Key] < required.Value.value))
                    {
                        return false;
                    }
                }

                if (HasRequiredStages)
                {
                    if (RequireStages.Any(required => StageManager.I.IsStageCleared(required) == false))
                    {
                        return false;
                    }
                }
                
                if (HasRequiredConditions)
                {
                    if (currentConditions.Any(condition => condition is { Key: > 0, Value: false }))
                    {
                        return false;
                    }
                }
                
                return true;
            }
        }
        
        //퀘 수주시 출력 대사
        public long PossibleDialogueID { get; private set; }
        //퀘 미완료시 출력 대사
        public long WaitDialogueID { get; private set; }
        //퀘 완료시 출력 대사
        public long CompleteDialogueID { get; private set; }

        public Quest(QuestData info)
        {
            QuestType = info.Type;
            QuestID = info.ID;
            QuestName = info.Name;
            QuestDescription = info.Description;
            NextQuestID = info.NextQuestID;
            RequiredItems = info.RequiredItems;
            RequireStages = info.RequiredStages;
            RequiredConditions = info.RequiredConditions;
            QuestStarter = info.StartNpcID;
            QuestEnder = info.EndNpcID == 0 ? QuestStarter : info.EndNpcID;

            PossibleDialogueID = info.PossibleDialogueID;
            WaitDialogueID = info.InProgressDialogueID;
            CompleteDialogueID = info.CompleteDialogueID;

            questSummary = info.Summary;
            
            currentConditions.Clear();
            currentItems.Clear();
            IsCompleted = false;
        }

        public void SetCompleted(bool state)
        {
            IsCompleted = state;
        }

        public void InitializeCurrentCount()
        {
            currentItems.Clear();
            currentConditions.Clear();
            
            foreach (var required in RequiredItems)
            {
                currentItems.TryAdd(required.Key, 0);
            }

            foreach (var condition in RequiredConditions)
            {
                currentConditions.TryAdd(condition.Key, false);
            }
        }
        
        public bool IsFinishedRequireItem(long itemID)
        {
            return RequiredItems.ContainsKey(itemID) && RequiredItems[itemID].value <= currentItems[itemID];
        }

        public bool IsFinishedCondition(int condition)
        {
            return currentConditions.ContainsKey(condition) && currentConditions[condition];
        }
        
        public void AddCurrentItemCount(long itemID, int itemCount)
        {
            if (currentItems.ContainsKey(itemID) == false) return;
            currentItems[itemID] += + itemCount;
        }
        
        public void SetCurrentItemCount(long itemID, int itemCount)
        {
            if (currentItems.ContainsKey(itemID))
            {
                currentItems[itemID] = Mathf.Clamp(itemCount,0,RequiredItems[itemID].value);
            }
        }

        public void SetCondition(int conditionID, bool condition)
        {
            currentConditions[conditionID] = condition;
        }

    }
}