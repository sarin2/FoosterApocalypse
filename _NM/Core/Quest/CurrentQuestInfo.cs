using System;
using System.Collections.Generic;
using System.Linq;
using _NM.Core.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _NM.Core.Quest
{
    public class CurrentQuestInfo : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private List<TextMeshProUGUI> descriptionText;

        [SerializeField] private Image questTitleImage;
        [SerializeField] private Sprite mainQuestTitleSprite;
        [SerializeField] private Sprite subQuestTitleSprite;
    
        [SerializeField] private GameObject titleObject;
        [SerializeField] private VerticalLayoutGroup vertical;
        [SerializeField] private CanvasGroup canvasGroup;

        private void OnValidate()
        {
            if (titleObject == null) titleObject = transform.Find("Quest Title Main").gameObject;
            if (vertical == null) vertical = transform.Find("Quest Info").GetComponentInChildren<VerticalLayoutGroup>();
            if (titleText == null) titleText = titleObject.transform.Find("Quest Title").GetComponent<TextMeshProUGUI>();
            if (descriptionText == null || descriptionText.Count == 0) descriptionText = vertical.GetComponentsInChildren<TextMeshProUGUI>().ToList();
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        }

        public void SetData(Quest quest)
        {
            canvasGroup.alpha = 1;
            
            foreach (var description in descriptionText)
            {
                description.text = string.Empty;
                description.gameObject.SetActive(false);
            }
            
            if (quest == null)
            {
                questTitleImage.sprite = mainQuestTitleSprite;
                titleText.text = "진행중인 부탁이 없습니다";
                return;
            }
            
            questTitleImage.sprite = quest.QuestType == QuestTypeInfo.MainQuest ? mainQuestTitleSprite : subQuestTitleSprite;
            
            titleText.text = quest.QuestName;
            var descriptionCount = 0;
            foreach (var pair in quest.RequiredItems)
            {
                var text = QuestManager.GetRequireItemDescription(pair.Value.description, quest.CurrentItems[pair.Key], pair.Value.value);
                descriptionText[descriptionCount].gameObject.SetActive(true);
                descriptionText[descriptionCount].text = text;
                descriptionCount++;
            }

            foreach (var stage in quest.RequireStages)
            {
                StageManager.I.GetStage(stage, out var data);
                var text = QuestManager.GetRequireStageDescription(data.StageName,
                    StageManager.I.IsStageCleared(stage));
                descriptionText[descriptionCount].gameObject.SetActive(true);
                descriptionText[descriptionCount].text = text;
                descriptionCount++;
            }
        
            foreach (var pair in quest.RequiredConditions)
            {
                var text = QuestManager.GetRequireConditionDescription(pair.Value, quest.IsFinishedCondition(pair.Key));
                descriptionText[descriptionCount].gameObject.SetActive(true);
                descriptionText[descriptionCount].text = text;
                descriptionCount++;
            }
        }

        public void ClosePanel()
        {
            titleText.text = String.Empty;
            foreach (var description in descriptionText)
            {
                description.text = string.Empty;
                description.gameObject.SetActive(false);
            }
            canvasGroup.alpha = 0;
        }

        private void Update()
        {
            if (vertical)
            {
                vertical.CalculateLayoutInputHorizontal();
                vertical.SetLayoutHorizontal();
                vertical.CalculateLayoutInputVertical();
                vertical.SetLayoutVertical();
            }
        }
    }
}
