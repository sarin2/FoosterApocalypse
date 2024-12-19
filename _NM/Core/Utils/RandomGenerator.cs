using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace _NM.Core.Manager
{
    public static class RandomGenerator
    {
        private static bool Initalized = false;
        /// <summary>
        /// 유니티 랜덤의 시드값을 Random.org를 기준으로 설정합니다.
        /// </summary>
        public static void InitSeed()
        {
            System.Random random = new();
            Random.InitState(random.Next(2,200000000));

            Initalized = true;

        }
        
        
        /// <summary>
        /// 여러 확률 중 하나를 선택하는 함수입니다.
        /// 합계가 100%가 아니어도 선택됩니다.
        /// </summary>
        public static int Choose (float[] probs) {

            if (!Initalized)
            {
                InitSeed();
            }
            float total = 0;

            foreach (float elem in probs) {
                //Debug.Log(elem);
                total += elem;
            }

            float randomPoint = Random.value * total;

            for (int i= 0; i < probs.Length; i++) {
                if (randomPoint < probs[i]) {
                    return i;
                }
                else {
                    randomPoint -= probs[i];
                }
            }
            return probs.Length - 1;
        }
        
        /// <summary>
        /// 애니메이션 커브를 이용하여 가중치를 지정 할 수 있는 함수입니다.
        /// </summary>
        public static float CurveWeightedRandom(AnimationCurve curve) {
            return curve.Evaluate(Random.value);
        }
        
        /// <summary>
        /// 주사위를 굴리듯이 랜덤값을 가져오는 함수입니다.
        /// 최소 숫자는 minValue, 최대 숫자는 maxValue 입니다.
        /// 음수는 나오지 않습니다.
        /// </summary>
        public static float RollDice(float minValue = 1f, float maxValue = 100f)
        {
            float result = Random.Range(minValue, maxValue + 1);
            return Mathf.Clamp(result, 1, maxValue);
        }
    }
}