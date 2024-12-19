using System.Collections.Generic;
using _NM.Core.Input;
using _NM.Core.Manager;
using _NM.Core.NPC;
using _NM.Core.NPC.Dialogue;
using _NM.Core.UI.Interaction;
using _NM.Core.UI.UICanvas;
using _NM.Core.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SDUnityExtension.Scripts.Pattern;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace _NM.Core.UI.Dialogue
{
    public class DialogueController : SDSingleton<DialogueController>
    {
        private bool Initialized { get; set; }
        public Dictionary<long, DialogueData> Dialogues { get; } = new();
        public bool CanInteract { get; set; } = true;
        
        [Header("Dialogue Settings")]
        [LabelText("대화 출력 속도 (초당 글자 수)"), SerializeField] private float dialogueSpeed;
        private bool shouldSkip;
        private bool isMonologue;
        
        [Header("Components")]
        [SerializeField] private CanvasController canvasController;
        private InteractableItemController interactableItemController;
        private Object.Interactable.Interaction interaction;

        [Header("UI Elements")]
        [FormerlySerializedAs("textObject")]
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private TextMeshProUGUI talkerName;
        [SerializeField] private TextMeshProUGUI talkerSubName;
        [SerializeField] private GameObject nextButtonObject;
        
        public Npc ContactingNpc { get; private set; }
        public long CurrentDialogueID { get; private set; }
        public DialogueData CurrentDialogue { get; private set; }
        
        private ResponseButtonController responseButtonController;

        private void Start()
        {
            Initialize(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private void OnEnable()
        {
            DataManager.onInitialized += Initialize;
            SceneManager.sceneLoaded += Initialize;
        }
        
        private void OnDisable()
        {
            DataManager.onInitialized -= Initialize;
            SceneManager.sceneLoaded -= Initialize;
        }

        private void Update()
        {
            if (SceneLoadingController.Instance.SceneChanging) return;
            
            var inputData = InputProvider.I.LatestInput;
            if (inputData.Interact && interaction is { IsReadyToInteract: true })
            {
                InteractDialogue();
            }

            if (inputData.Next)
            {
                shouldSkip = true;
            }
        }

        private void Initialize()
        {
            if (Initialized) return;
            foreach (var column in DataManager.GetSheetData(GoogleSheetsConstantData.ESheetPage.DialogueSettings))
            {
                var dialogue = DialogueData.Parse(column);
                if (dialogue != null)
                {
                    Dialogues.TryAdd(dialogue.ID, dialogue);
                }
            }
            Initialized = true;
        }
        
        private void Initialize(Scene scene, LoadSceneMode mode)
        {
            if (interaction == null)
            {
                interaction = FindAnyObjectByType<Object.Interactable.Interaction>(FindObjectsInactive.Include);
            }
            
            if (!canvasController)
            {
                canvasController = FindAnyObjectByType<CanvasController>(FindObjectsInactive.Include);
            }

            if (!interactableItemController)
            {
                interactableItemController = FindAnyObjectByType<InteractableItemController>(FindObjectsInactive.Include);
            }

            if (!responseButtonController)
            {
                responseButtonController = GetComponentInChildren<ResponseButtonController>();
            }
        }

        private void InteractDialogue()
        {
            if (CanInteract == false) return;
            
            var interactables = interaction.Interactables;
            foreach (var interactable in interactables)
            {
                if (interactable is not Npc npcBase) continue;
                ContactingNpc = npcBase;
                break;
            }

            if (ContactingNpc == null) return;
            
            CurrentDialogueID = ContactingNpc.CurrentDialogueID;

            if (CurrentDialogueID <= 0) return;
            CurrentDialogue = Dialogues[CurrentDialogueID];
            
            talkerName.text = CurrentDialogue.MainName;
            talkerSubName.text = CurrentDialogue.SubName;
            dialogueText.text = string.Empty;
            
            var canStart = ContactingNpc.OnDialogueStarted();
            if (canStart == false)
                return;
            
            interactableItemController.DisableInteractables();
            ProgressDialogue().Forget();
        }

        private async UniTaskVoid ProgressDialogue()
        {
            canvasController.SetDialogueUIActive(true);
            
            foreach (var emote in CurrentDialogue.NpcEmotes) 
                ContactingNpc.SetEmote(emote).Forget();
            
            foreach (var emote in CurrentDialogue.DanviEmotes) 
                Core.Character.Character.Local.SetEmote(emote).Forget();

            foreach (var perform in CurrentDialogue.CameraPerforms)
                ContactingNpc.SetCameraPerform(perform).Forget();
            
            await ProgressTyping(CurrentDialogue);

            if (isMonologue)
            {
                shouldSkip = false;
                await UniTask.WaitUntil(() => shouldSkip);
                
                if (CurrentDialogue.NextDialogueID > 0)
                {
                    ChangeNpcDialogueID(CurrentDialogue.NextDialogueID);
                }
                else
                {
                    EndDialogueUI();
                }
            }
        }
    
        private async UniTask ProgressTyping(DialogueData dialogueInfo)
        {
            shouldSkip = isMonologue = false;
            nextButtonObject.SetActive(false);
            
            var text = dialogueInfo.Text;
            var tween = dialogueText.DOText(text, dialogueSpeed).SetSpeedBased().SetEase(Ease.Linear).SetAutoKill(false);
            
            await UniTask.WaitUntil(() => tween.IsComplete() || shouldSkip);

            if (shouldSkip)
            {
                tween.Complete();
            }
            nextButtonObject.SetActive(true);

            isMonologue = dialogueInfo.Responses.Count == 0;
            if (isMonologue == false)
            {
                responseButtonController.SetResponseButton(dialogueInfo.Responses);
            }
        }

        public void EndDialogueUI()
        {
            interactableItemController.EnableInteratables();
            canvasController.SetDialogueUIActive(false);
            responseButtonController.ResetResponseButton();
            ContactingNpc.OnDialogueEnded();
        }

        public void ChangeNpcDialogueID(long dialogueDataID)
        {
            ContactingNpc.SetCurrentDialogueData(dialogueDataID);
            InteractDialogue();
        }
    }
}
