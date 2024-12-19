using System;
using System.Collections.Generic;
using _NM.Core.Character;
using _NM.Core.Utils;
using Cysharp.Threading.Tasks;
using SDUnityExtension.Scripts.Extension;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _NM.Core.Quest
{
    [Serializable]
    public enum QuestTypeInfo
    {
        MainQuest,
        SubQuest,
        DayEndQuest,
        None
    }

    [Serializable]
    public struct RequireInfo
    {
        [SerializeField] public int value;
        [SerializeField] public string description;
    }

    [CreateAssetMenu(fileName = "Quest Data", menuName = "Scriptable Object/Quest Data")]
    public class QuestData : ScriptableObject
    {
        [field: LabelText("퀘스트 ID"), SerializeField]
        public int ID { get; private set; }

        [field: LabelText("퀘스트 타입"), SerializeField]
        public QuestTypeInfo Type { get; private set; }

        [field: LabelText("퀘스트 이름"), SerializeField]
        public string Name { get; private set; }
        
        [field: LabelText("퀘스트 수락 혹은 완료시 따로 출력할 요약"), SerializeField]
        public string Summary { get; private set; }

        [field: LabelText("퀘스트 설명"), SerializeField,TextArea]
        public string Description { get; private set; }

        [field: LabelText("퀘스트 시작 NPC ID"), SerializeField]
        public int StartNpcID { get; private set; }

        [field: LabelText("퀘스트 종료 NPC ID"), SerializeField]
        public int EndNpcID { get; private set; }

        [field: LabelText("다음 퀘스트 ID"), SerializeField]
        public List<int> NextQuestID { get; private set; }
        
        [field: LabelText("퀘 수주 가능시 출력할 다이얼로그 ID"), SerializeField]
        public long PossibleDialogueID { get; private set; }

        [field: LabelText("퀘 미완료 시 출력할 다이얼로그 ID"), SerializeField]
        public long InProgressDialogueID { get; private set; }

        [field: LabelText("퀘 완료 시 출력할 다이얼로그 ID"), SerializeField]
        public long CompleteDialogueID { get; private set; }

        [field: LabelText("필요한 아이템 목록"), SerializeField]
        public SerializableDictionary<long, RequireInfo> RequiredItems { get; private set; }

        [field: LabelText("필요한 완료 스테이지 목록"), SerializeField]
        public HashSet<int> RequiredStages { get; private set; }

        [field: LabelText("특수 조건"), SerializeField]
        public SerializableDictionary<int,string> RequiredConditions { get; private set; }

        public static QuestData Parse(Dictionary<string, string> column)
        {
            try
            {
                if (string.IsNullOrEmpty(column["Quest_ID"]))
                    return null;
                QuestData info = CreateInstance<QuestData>();
                info.ID = int.Parse(column["Quest_ID"]);
                info.Type = Enum.TryParse(column["Quest_Type"], out QuestTypeInfo typeInfo) ? typeInfo : QuestTypeInfo.None;
                info.Name = column["Quest_Name"];
                info.Summary = column["Quest_Summary"];
                info.Description = column["Quest_Description"];
                info.StartNpcID = int.TryParse(column["Start_Npc_ID"], out var startNpcID) ? startNpcID : 0;
                info.EndNpcID = int.TryParse(column["End_Npc_ID"], out var endNpcID) ? endNpcID : 0;
                info.NextQuestID = new();
                info.PossibleDialogueID = int.TryParse(column["Possible_Dialogue_ID"], out var possibleDialogueID) ? possibleDialogueID : 0;
                info.InProgressDialogueID = int.TryParse(column["InProgress_Dialogue_ID"], out var inProgressDialogueID) ? inProgressDialogueID : 0;
                info.CompleteDialogueID = int.TryParse(column["Complete_Dialogue_ID"], out var completeDialogueID) ? completeDialogueID : 0;
                info.RequiredItems = new();
                info.RequiredStages = new();
                info.RequiredConditions = new();
                    
                if (column["Next_Quest_ID"].IsNotEmpty())
                {
                    column["Next_Quest_ID"].Split(',').ForEach(x =>
                    {
                        info.NextQuestID.Add(int.Parse(x));
                    });
                }

                if (column["Require_Items"].IsNotEmpty())
                {
                    column["Require_Items"].Split(',').ForEach(x =>
                    {
                        string[] requireItem = x.Split(':');
                        info.RequiredItems.TryAdd(int.Parse(requireItem[0]), new RequireInfo()
                        {
                            value = int.Parse(requireItem[1]),
                            description = requireItem[2]
                        });
                    });
                }

                if (column["Require_Stages"].IsNotEmpty())
                {
                    column["Require_Stages"].Split(',').ForEach(x =>
                    {
                        info.RequiredStages.Add(int.Parse(x));
                    });
                }

                if (column["Require_Conditions"].IsNotEmpty())
                {
                    column["Require_Conditions"].Split(',').ForEach(x =>
                    {
                        string[] requireCondition = x.Split(':');
                        info.RequiredConditions.TryAdd(int.Parse(requireCondition[0]), requireCondition[1]);
                    });
                }

                return info;
            }
            catch (Exception e)
            {
                Debug.LogError($"퀘스트 데이터 파싱 중 에러 발생: {e.Message} | {e.StackTrace}");
                return null;
            }
        }
    }
}