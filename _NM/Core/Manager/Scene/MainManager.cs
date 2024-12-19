using _NM.Core.Quest;
using _NM.Core.UI.Inventory;
using _NM.Core.UI.UICanvas;
using _NM.Core.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _NM.Core.Manager.Scene
{
    public class MainManager : MonoBehaviour
    {
        [Title("메인 버튼")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button optionButton;
        [SerializeField] private Button crewButton;
        [SerializeField] private Button exitButton;

        [Title("설정 화면")] 
        [SerializeField] private CanvasPage optionPage;
        
        [Title("크레딧 화면")]
        [SerializeField] private DoController crewCanvasAnimation;
        [SerializeField] private Sprite[] crewSprites;
        [SerializeField] private Image crewImage;
        private int crewSpriteIndex;
        private bool crewCanvasOpened = false;

        private void Awake()
        {
            var lastSpawnedScene = SaveManager.LastSpawnedScene.Load();
            DestroySingleton();
            if (string.IsNullOrEmpty(lastSpawnedScene))
            {
                resumeButton.gameObject.SetActive(false);
            }
            else
            {
                resumeButton.onClick.AddListener(() => PressStartButton(lastSpawnedScene));
            }
            startButton.onClick.AddListener(PressRestartButton);
            optionButton.onClick.AddListener(PressOptionButton);
            crewButton.onClick.AddListener(PressCrewButton);
            exitButton.onClick.AddListener(PressQuitButton);
            
            crewCanvasAnimation.ResetToStart();
        }
        
        private void PressStartButton(string sceneName)
        {
            SceneLoadingController.Instance.ChangeScene(sceneName);
        }
        
        private void PressRestartButton()
        {
            GameManager.ResetProgress();
            SceneLoadingController.Instance.ChangeScene(SceneName.Shelter_Night);
        }
        
        private void PressOptionButton()
        {
            optionPage.Open().Forget();
        }

        private void PressCrewButton()
        {
            crewSpriteIndex = 0;
            crewCanvasOpened = true;
            crewImage.sprite = crewSprites[crewSpriteIndex];
            crewCanvasAnimation.Play();
        }

        public void PressCrewNext()
        {
            if (crewSpriteIndex == crewSprites.Length-1)
            {
                crewImage.sprite = crewSprites[^1];
                return;
            }

            crewImage.sprite = crewSprites[++crewSpriteIndex];
        }
    
        public void PressCrewPrev()
        {
            if (crewSpriteIndex == 0)
            {
                crewImage.sprite = crewSprites[0];
                return;
            }

            crewImage.sprite = crewSprites[--crewSpriteIndex];
        }
    
        public void PressCrewConfirmButton()
        {
            crewCanvasOpened = false;
            crewCanvasAnimation.PlayReverse();
        }

        private void Update()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                if (optionPage.IsOpened)
                {
                    optionPage.Close().Forget();
                }

                if (crewCanvasOpened)
                {
                    PressCrewConfirmButton();
                }
            }
        }

        private void DestroySingleton()
        {
            Destroy(CanvasController.I?.gameObject);
            Destroy(NpcManager.I?.gameObject);
            Destroy(QuestManager.I?.gameObject);
            Destroy(StageManager.I?.gameObject);
            Destroy(InventoryManager.I?.gameObject);
        }

        private void PressQuitButton()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
