using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class AngleConverter : MonoBehaviour
{
    [field:LabelText("UV Center 오프셋 사용 여부"),SerializeField] private bool usingUVCenter;
    [SerializeField,ShowIf(nameof(usingUVCenter))] private float uvCenterXPos;
    [SerializeField,ShowIf(nameof(usingUVCenter))] private float uvCenterYPos;
    [field:LabelText("현재 슬라이더 값(변경 불가능)"),SerializeField,Range(0,-1f)] private float sliderValue;
    [field:LabelText("현재 각도"),SerializeField] private float matAngle;

    [field:LabelText("게임 종료 시 초기값으로 초기화"),SerializeField] private bool resetValue;
    
    private float prevUVCenterXPos;
    private float prevUVCenterYPos;
    private float prevSliderValue;

    private Material mat;
    
    [Header("프로퍼티")]
    [field:LabelText("슬라이더 프로퍼티"),SerializeField] private string sliderPropertyName = "_SliderValue";
    [field:ShowIf(nameof(usingUVCenter)),LabelText("UV Center X 오프셋 프로퍼티"),SerializeField] private string UVCenterXPropertyName = "_UVCenterXPos";
    [field:ShowIf(nameof(usingUVCenter)),LabelText("UV Center Y 오프셋 프로퍼티"),SerializeField] private string UVCenterYPropertyName = "_UVCenterYPos";

    private void OnValidate()
    {
        sliderValue = ConvertToUV(matAngle);
    }

    private void Awake()
    {
        mat = GetComponent<Renderer>().material;
    }

    public static float ConvertToUV(float angle)
    { 
        float oldRange = 360.0f;
        float newRange = -1.0f;
        return angle / oldRange + (-1.0f);
    }
    public static float ConvertToAngle(float UV)
    {
        return 360 - (-UV  * 360f);
    }

    private void Start()
    {
        if (usingUVCenter)
        {
            prevUVCenterXPos = uvCenterXPos;
            prevUVCenterYPos = uvCenterYPos;
        }

        prevSliderValue = sliderValue;
        matAngle = ConvertToAngle(sliderValue);
    }

    private void Update()
    {

        if (usingUVCenter)
        {
            mat.SetFloat(UVCenterXPropertyName, uvCenterXPos);
            mat.SetFloat(UVCenterYPropertyName, uvCenterYPos);
        }

        sliderValue = ConvertToUV(matAngle);
        mat.SetFloat(sliderPropertyName,sliderValue);
    }

    private void OnDestroy()
    {
        if (resetValue)
        {
            if (usingUVCenter)
            {
                uvCenterXPos = prevUVCenterXPos;
                uvCenterYPos = prevUVCenterYPos;
            }

            sliderValue = prevSliderValue;
        }
    }
}
