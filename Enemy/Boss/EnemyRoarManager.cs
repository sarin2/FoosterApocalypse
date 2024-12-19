using System;
using _NM.Core.Camera;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _NM.Core.Enemy
{
    public class EnemyRoarManager : MonoBehaviour
    {
        [SerializeField] private Material radialBlur;
        [SerializeField] private CameraController cameraController;
        [field:SerializeField] private readonly string matValueKey = "_BlurAmount";

        private void Start()
        {
            cameraController = Character.Character.Local.Camera.Controller;
            if (radialBlur)
            {
                radialBlur.SetFloat(matValueKey,0f);
            }
        }

        public async UniTask Roar(float duration,float intensity,float endDuration)
        {
            float curTime = 0f;

            SetBlurIntensity(intensity,0.3f).Forget();
            while (curTime < duration)
            {
                curTime += Time.deltaTime;
                await UniTask.Yield();
            }
            
            SetBlurIntensity(0f,endDuration).Forget();
        }

        public async UniTask SetBlurIntensity(float value,float time)
        {
            float prevValue = radialBlur.GetFloat(matValueKey);
            float currentTime = 0f;
            while (time > currentTime)
            {
                currentTime += Time.deltaTime;
                var curValue = Mathf.Lerp(prevValue, value, currentTime/time);
                radialBlur.SetFloat(matValueKey,curValue);
                await UniTask.Yield();
            }
        }

        private void OnDestroy()
        {
            radialBlur.SetFloat(matValueKey,0f);
        }
    }
}
