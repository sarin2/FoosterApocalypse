using System.Collections.Generic;
using System.Linq;
using _NM.Core.Input;
using _NM.Core.Manager;
using _NM.Core.Quest;
using _NM.Core.UI.Dialogue;
using _NM.Core.UI.Interaction;
using _NM.Core.UI.Inventory;
using _NM.Core.UI.Navigation;
using _NM.Core.UI.Stage;
using Cysharp.Threading.Tasks;
using SDUnityExtension.Scripts.Pattern;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _NM.Core.UI.UICanvas
{
    public class CanvasController : SDSingleton<CanvasController>
    {
        [SerializeField] private GameObject quickSlotUI;
        [SerializeField] private GameObject mainUI;
        [SerializeField] private GameObject playerUI;
        [SerializeField] private List<string> hideCanvasSceneList = new ()
        {
            "Main", "Boss_die_cut", "EndingCredit", "BossRoom"
        };
        
        private CanvasGroup rootCanvasGroup;
        private InteractableItemController interactableItemController;
        
        private List<CanvasPage> pageList;
        private InventoryUI inventoryUI;
        private QuestPopupUI questPopupUI;
        private DialogueUI dialogueUI;
        private StageSelectUI stageSelectUI;
        
        private CanvasPage currentPage;
        
        private bool IsHidden => rootCanvasGroup.alpha <= 0;
        private bool IsMainUIHidden { get; set; }
        
        protected override void Awake()
        {
            SetInstance(this);
            RandomGenerator.InitSeed();
            rootCanvasGroup = GetComponent<CanvasGroup>();
            
            pageList = GetComponentsInChildren<CanvasPage>(true).ToList();
            inventoryUI = pageList.First(e => e is InventoryUI) as InventoryUI;
            questPopupUI = pageList.First(e => e is QuestPopupUI) as QuestPopupUI;
            dialogueUI = pageList.First(e => e is DialogueUI) as DialogueUI;
            stageSelectUI = pageList.First(e => e is StageSelectUI) as StageSelectUI;
            pageList.ForEach(e => e.gameObject.SetActive(true));
        }

        private void Start()
        {
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
        
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode _)
        {
            playerUI = GameObject.Find("Player Canvas");
            interactableItemController = FindAnyObjectByType<InteractableItemController>(FindObjectsInactive.Include);
            IsMainUIHidden = scene.name.Equals("BossRoom");
            
            if (hideCanvasSceneList.Any(scene.name.Equals))
            {
                Hide();
            }
            else
            {
                Show();
                ProgressUI(null, false);
            }
        }

        private void Update()
        {
            if (GameManager.SceneLoading || GameManager.TimelinePlaying || IsHidden) return;
            if (dialogueUI.IsOpened) return;
            
            var inputData = InputProvider.I.LatestInput;
            
            var anyPageOpened = currentPage?.IsOpened ?? false;
            if (inputData.Menu)
            {
                if (anyPageOpened)
                {
                    currentPage.Back().Forget();
                }
                else
                {
                    GameManager.Paused = !GameManager.Paused;
                }
            }
            
            if (inputData.Inventory)
            {
                if (currentPage == null || currentPage.Equals(inventoryUI))
                    inventoryUI.Toggle();
            }
            
            if (inputData.QuestList)
            {
                if (currentPage == null || currentPage.Equals(questPopupUI))
                    questPopupUI.Toggle();
            }
        }
        
        public void Show()
        {
            rootCanvasGroup.alpha = 1;
            rootCanvasGroup.interactable = rootCanvasGroup.blocksRaycasts = true;
            if (DialogueController.I) DialogueController.I.CanInteract = true;
            if (interactableItemController) interactableItemController.EnableInteratables();
            if (playerUI) playerUI.SetActive(true);
            if (mainUI) mainUI.SetActive(IsMainUIHidden == false);
            if (quickSlotUI) quickSlotUI.SetActive(true);
            if (NavigationIndicator.I != null) NavigationIndicator.I.Show();
            if (DebugManager.I != null) DebugManager.I.Show();
        }

        public void Hide()
        {
            rootCanvasGroup.alpha = 0;
            rootCanvasGroup.interactable = rootCanvasGroup.blocksRaycasts = false;
            if (DialogueController.I) DialogueController.I.CanInteract = false;
            if (interactableItemController) interactableItemController.DisableInteractables();
            if (playerUI) playerUI.SetActive(false);
            if (mainUI) mainUI.SetActive(false);
            if (quickSlotUI) quickSlotUI.SetActive(false);
            if (NavigationIndicator.I != null) NavigationIndicator.I.Hide();
            if (DebugManager.I != null) DebugManager.I.Hide();
        }
        
        public void SetDialogueUIActive(bool active)
        {
            if (active)
            {
                dialogueUI.Open();
            }
            else
            {
                dialogueUI.Close();
            }
        }

        public void SetStageSelectUIActive(bool active)
        {
            if (active)
            {
                stageSelectUI.Open();
            }
            else
            {
                stageSelectUI.Close();
            }
        }

        public void ProgressUI(CanvasPage page, bool open)
        {
            if (open)
            {
                currentPage?.ForceClose();
                pageList.ForEach(e =>
                {
                    if (e != page && e.IsOpened)
                    {
                        e.ForceClose();
                    }
                });
                if (interactableItemController) interactableItemController.DisableInteractables();
                if (InputProvider.I) InputProvider.I.ShowCursor(CursorLockMode.None);
                if (playerUI) playerUI.SetActive(false);
                if (mainUI) mainUI.SetActive(false);
                if (quickSlotUI) quickSlotUI.SetActive(false);
                if (NavigationIndicator.I) NavigationIndicator.I.Hide();
                if (DebugManager.I) DebugManager.I.Hide();

                currentPage = page;
            }
            else
            {
                pageList.ForEach(e =>
                {
                    if (e != page && e.IsOpened)
                    {
                        e.ForceClose();
                    }
                });
                if (interactableItemController) interactableItemController.EnableInteratables();
                if (playerUI) playerUI.SetActive(true);
                if (mainUI) mainUI.SetActive(IsMainUIHidden == false);
                if (quickSlotUI) quickSlotUI.SetActive(true);
                if (NavigationIndicator.I) NavigationIndicator.I.Show();
                if (DebugManager.I) DebugManager.I.Show();
                if (InputProvider.I) InputProvider.I.ShowCursor(CursorLockMode.Locked, true);

                currentPage = null;
            }
        }
    }

}

