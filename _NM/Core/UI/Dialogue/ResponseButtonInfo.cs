using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _NM.Core.UI.Dialogue
{
    [Serializable]
    public struct ResponseData
    {
        // 선택지 관련
        [field: LabelText("응답 타입"), SerializeField] public ResponseType Type { get; private set; }
        [field: LabelText("선택지 출력 내용"), SerializeField] public string Text { get; private set; }
        
        // 대화 관련
        [field: LabelText("다음 대화 ID"), SerializeField] public long NextDialogueID { get; private set; }
        
        // 퀘스트 관련
        [field: LabelText("퀘스트 수락 ID"), SerializeField] public int AcceptQuestID { get; private set; }
        [field: LabelText("퀘스트 완료 ID"), SerializeField] public int CompleteQuestID { get; private set; }

        public ResponseData(ResponseType type, string text, long nextDialogueID = 0, int acceptQuestID = 0, int completeQuestID = 0)
        {
            Type = type;
            Text = text;
            NextDialogueID = nextDialogueID;
            AcceptQuestID = acceptQuestID;
            CompleteQuestID = completeQuestID;
        }
    }
}