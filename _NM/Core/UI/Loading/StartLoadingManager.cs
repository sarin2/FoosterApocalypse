using System;
using System.Threading;
using _NM.Core.Manager;
using _NM.Core.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _NM.Core.UI.Loading
{
    public class StartLoadingManager : MonoBehaviour
    { 
        [SerializeField] private CanvasGroup mainCanvasGroup;
        [SerializeField] private CanvasGroup[] canvasGroups;
        [SerializeField] private int canvasGroupIndex;
        
        private Sequence creditSequence;
        private UniTask sequenceTask;
        private CancellationTokenSource sequenceCts;

        private void Awake()
        {
            canvasGroupIndex = 0;
        }

        private void Start()
        {
            LoadAsync().Forget();
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                sequenceCts?.Cancel();
            }
        }

        private async UniTask LoadAsync()
        {
            while (canvasGroupIndex < canvasGroups.Length)
            {
                sequenceCts = new();
                var token = sequenceCts.Token;
            
                bool result = await DOTween.Sequence().OnStart(() =>
                    {
                        canvasGroups[canvasGroupIndex].alpha = 0f;
                        canvasGroups[canvasGroupIndex].DOFade(1f, 1.0f).ToUniTask(cancellationToken: token);
                    }).AppendInterval(2.0f)
                    .Append(t:canvasGroups[canvasGroupIndex].DOFade(0f, 1.0f)).ToUniTask(cancellationToken: token).SuppressCancellationThrow();
                await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: token).SuppressCancellationThrow();
                if (!result)
                {
                    canvasGroups[canvasGroupIndex].alpha = 0f;
                }
                canvasGroupIndex++;
            }
            sequenceCts?.Cancel();
            sequenceCts?.Dispose();
            sequenceCts = null;
            await DataManager.WaitUntilInitialize;
            SceneLoadingController.Instance.ChangeScene(SceneName.Main);
        }
    }
}
