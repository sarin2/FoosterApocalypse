using System;
using _NM.Core.Manager;
using _NM.Core.UI.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _NM.Core.Utils
{
    public enum SceneName
    {
        Main,
        Shelter_Day,
        Shelter_Night,
        Stage1,
        Stage2,
        Stage3,
        BossCutScene,
        BossRoom,
        BossDeadCutScene,
        EndingCredit
    }
    
    public struct SceneLoadData
    {
        public string SceneName;
        public bool ShowLoading;
        public bool ShowFade;
        public Color FadeColor;
    }

    public class SceneLoadingController : MonoBehaviour
    {
        private static SceneLoadingController instance = null;
        public static SceneLoadingController Instance
        {
            get
            {
                if (instance != null) return instance;
                
                var prefab = Addressables.LoadAssetAsync<GameObject>("SceneLoadingController")
                    .WaitForCompletion();
                instance = Instantiate(prefab).GetComponent<SceneLoadingController>();
                instance.Initialize();
                DontDestroyOnLoad(instance.gameObject);
                return instance;
            }
        }
        
        public bool SceneChanging { get; private set; }
        public Scene CurrentScene => SceneManager.GetActiveScene();

        public static event Action<string> onLoadStarted;
        public static event Action<string> onLoadCompleted;
        
        [SerializeField] private Graphic backgroundGraphic;
        [SerializeField] private CanvasGroup backgroundCanvasGroup;
        [SerializeField] private CanvasGroup loadingCanvasGroup;
        [SerializeField] private FadeUI fadeUI;
        [SerializeField] private SerializedDictionary<SceneName, string> SceneNames = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeStaticEvents()
        {
            instance = null;
            onLoadStarted = null;
            onLoadCompleted = null;
        }
        
        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        { 
            if (fadeUI == null) 
                fadeUI = GetComponentInChildren<FadeUI>();

            SceneNames.TryAdd(SceneName.Main, "Main");
            SceneNames.TryAdd(SceneName.Shelter_Day, "Safe_Daytime");
            SceneNames.TryAdd(SceneName.Shelter_Night, "Safe_Night_Real");
            SceneNames.TryAdd(SceneName.Stage1, "Stage1");
            SceneNames.TryAdd(SceneName.Stage2, "Stage2");
            SceneNames.TryAdd(SceneName.Stage3, "Stage3");
            SceneNames.TryAdd(SceneName.BossCutScene, "BossRoom_CS_Real");
            SceneNames.TryAdd(SceneName.BossRoom, "BossRoom");
            SceneNames.TryAdd(SceneName.BossDeadCutScene, "Boss_die_cut");
            SceneNames.TryAdd(SceneName.EndingCredit, "EndingCredit");
            
            HideLoadingCanvas();
            HideBackgroundCanvas();
        }
        
        private void ShowBackgroundCanvas()
        {
            backgroundCanvasGroup.alpha = 1f;
            backgroundCanvasGroup.blocksRaycasts = true;
        }
        
        private void HideBackgroundCanvas()
        {
            backgroundCanvasGroup.alpha = 0f;
            backgroundCanvasGroup.blocksRaycasts = false;
        }

        private void ShowLoadingCanvas()
        {
            loadingCanvasGroup.alpha = 1f;
            loadingCanvasGroup.blocksRaycasts = true;
        }
        
        private void HideLoadingCanvas()
        {
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.blocksRaycasts = false;
        }
        
        public void ChangeScene(SceneLoadData data)
        {
            ChangeScene_Internal(data).Forget();
        }
        
        public void ChangeScene(string sceneName, bool showLoading = true, bool showFade = true, Color? fadeColor = null)
        {
            ChangeScene(new SceneLoadData
            {
                SceneName = sceneName,
                ShowLoading = showLoading,
                ShowFade = showFade,
                FadeColor = fadeColor ?? Color.black
            });
        }
        
        public void ChangeScene(SceneName sceneName, bool showLoading = true, bool showFade = true, Color? fadeColor = null)
        {
            ChangeScene(SceneNames[sceneName], showLoading, showFade, fadeColor);
        }
        
        private async UniTaskVoid ChangeScene_Internal(SceneLoadData data)
        {
            if (SceneChanging)
                return;
            
            onLoadStarted?.Invoke(data.SceneName);
            SceneChanging = true;

            GameManager.SceneLoading = true;
            if (SoundManager.I != null)
                SoundManager.I.PlayBgm(null, 0.5f, 0f).Forget();
            if (data.ShowFade)
                await fadeUI.FadeOut(color: data.FadeColor);
            backgroundGraphic.color = data.FadeColor;
            ShowBackgroundCanvas();
            if (data.ShowLoading)
                ShowLoadingCanvas();
            if (data.ShowFade)
                await fadeUI.FadeIn(color: data.FadeColor);

            AsyncOperation result = SceneManager.LoadSceneAsync(data.SceneName);
            result.allowSceneActivation = false;
            
            await UniTask.WaitUntil(() => result.progress >= 0.9f);
            result.allowSceneActivation = true;
            
            await UniTask.WaitUntil(() => result.isDone);
            if (data.ShowFade)
                await fadeUI.FadeOut(color: data.FadeColor);
            HideBackgroundCanvas();
            HideLoadingCanvas();
            
            SceneChanging = false;
            
            await fadeUI.FadeIn(color: data.FadeColor);
            GameManager.SceneLoading = false;
            onLoadCompleted?.Invoke(data.SceneName);
        }
    }
}

