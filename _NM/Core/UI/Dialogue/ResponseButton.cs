using System.Linq;
using _NM.Core.Quest;
using _NM.Core.UI.Cooking;
using _NM.Core.UI.Interaction;
using _NM.Core.UI.UICanvas;
using _NM.Core.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _NM.Core.UI.Dialogue
{
    public enum ResponseType
    {
        Exit,
        Talk,
        Cooking,
        NextDay
    }
    public class ResponseButton : MonoBehaviour
    {
        [Title("Animation")]
        [SerializeField] private RectTransform buttonAnimationTransform;
        [SerializeField] private Vector2 buttonAnimationStartPosition = new (-30, 0);
        [SerializeField] private float buttonAnimationDuration = 0.2f;
        [SerializeField] private Ease buttonAnimationEase = Ease.OutQuad;
        [SerializeField] private float buttonAnimationDelay = 0.1f;
        [SerializeField] private float buttonAnimationDelayPerIndex = 0.15f;
        
        [Title("Button")]
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private Button currentButton;
        
        private ResponseType responseType;
        private long nextDialogueId;
        private int acceptQuestID;
        private int completeQuestID;
        
        private DialogueController dialogueController;
        private ResponseButtonController dialogueButtonController;
        private CanvasPage phochaUI;
        private InteractableItemController interactableItemController;

        public void SetButtonData(ResponseButtonController controller, ResponseData data)
        {
            dialogueButtonController = controller;
            switch (data.Type)
            {
                case ResponseType.NextDay:
                case ResponseType.Cooking:
                case ResponseType.Talk:
                    buttonText.text = data.Text;
                    responseType = data.Type;
                    nextDialogueId = data.NextDialogueID;
                    acceptQuestID = data.AcceptQuestID;
                    completeQuestID = data.CompleteQuestID;
                    break;
                case ResponseType.Exit:
                    buttonText.text = data.Text;
                    responseType = data.Type;
                    break;
            }
        }

        public void SetAnimation(int index)
        {
            buttonAnimationTransform.anchoredPosition = buttonAnimationStartPosition;
            buttonAnimationTransform.DOAnchorPos(Vector2.zero, buttonAnimationDelay + index * buttonAnimationDuration)
                .SetEase(buttonAnimationEase);
        }

        private void OnValidate()
        {
            if (!currentButton)
            {
                currentButton = GetComponent<Button>();
            }

            if (!buttonText)
            {
                buttonText = GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        private void Awake()
        {
            currentButton.onClick.AddListener(OnClick);
            phochaUI = FindAnyObjectByType<CookingUI>(FindObjectsInactive.Include);
            dialogueController = FindAnyObjectByType<DialogueController>(FindObjectsInactive.Include);
            interactableItemController = FindAnyObjectByType<InteractableItemController>(FindObjectsInactive.Include);
        }

        private void OnClick()
        {
            switch (responseType)
            {
                case ResponseType.Exit:
                    OnExit();
                    break;
                case ResponseType.Talk:
                    OnTalk();
                    break;
                case ResponseType.Cooking:
                    OnCooking();
                    break;
                case ResponseType.NextDay:
                    OnNextDay();
                    break;
            }
        }

        private void OnExit()
        {
            dialogueController.EndDialogueUI();
        }

        private void OnTalk()
        {
            dialogueButtonController.ResetResponseButton();
            
            if (completeQuestID != 0)
            {
                QuestManager.I.CompleteQuest(completeQuestID);
            }
            
            if (acceptQuestID != 0)
            {
                QuestManager.I.AddQuest(acceptQuestID);
            }
            
            if (nextDialogueId > 0)
            {
                dialogueController.ChangeNpcDialogueID(nextDialogueId);
            }
            else
            {
                dialogueController.EndDialogueUI();
            }
        }

        private void OnCooking()
        {
            dialogueButtonController.ResetResponseButton();
            
            if (SceneLoadingController.Instance.CurrentScene.name.Contains("Night") || QuestManager.I.CurrentQuests.Any(quest =>
                    quest.Value.RequiredConditions.Any(condition => condition.Key == 13)))
            {
                dialogueController.EndDialogueUI();
                interactableItemController.DisableInteractables();
                phochaUI.Open().Forget();
            }
            else if (nextDialogueId > 0)
            {
                dialogueController.ChangeNpcDialogueID(nextDialogueId);
            }
        }

        private void OnNextDay()
        {
            dialogueController.EndDialogueUI();
            if (completeQuestID != 0)
            {
                QuestManager.I.CompleteQuest(completeQuestID);
            }
        }
    }
}
