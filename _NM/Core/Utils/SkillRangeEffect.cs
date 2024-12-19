using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _NM.Core.Utils
{
    public class SkillRangeEffect : MonoBehaviour
    {
        [SerializeField] private bool isAalphaChange;
        [SerializeField] private Material skillRangeMaterial;
        [SerializeField] private MeshRenderer meshRenderer;
        // Start is called before the first frame update
    
        private void OnValidate()
        {
            if (!meshRenderer)
                meshRenderer = GetComponent<MeshRenderer>();
        }
    
        private void Init()
        {
            skillRangeMaterial = meshRenderer.material;
        }
    
        private void Awake()
        {
            Init();
        }
    
        private void OnEnable()
        {
            isAalphaChange = true;
            skillRangeMaterial.color = Color.red;
            ChangeAlpha().Forget();
        }
    
        private void OnDisable()
        {
            isAalphaChange = false;
        }
    
        public async UniTaskVoid ChangeAlpha()
        {
            float r, g, b;
            bool change = false;
    
            r = skillRangeMaterial.color.r;
            g = skillRangeMaterial.color.g;
            b = skillRangeMaterial.color.b;
            while (isAalphaChange)
            {
                await UniTask.Delay(TimeSpan.FromMilliseconds(10));
                
                if (!change)
                {
                    skillRangeMaterial.color = new Color(r, g, b, skillRangeMaterial.color.a + 0.02f);
                    if (skillRangeMaterial.color.a >= 1f)
                        change = true;
                }
                
                else
                {
                    skillRangeMaterial.color = new Color(r, g, b, skillRangeMaterial.color.a - 0.02f);
                    if (skillRangeMaterial.color.a <= 0)
                        change = false;
                }
            }
        }
    }
}

