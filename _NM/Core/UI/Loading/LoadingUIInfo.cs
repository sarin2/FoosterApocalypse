using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace _NM.Core.UI.Loading
{


    [CreateAssetMenu(fileName = "LoadingUIInfo", menuName = "Scriptable Object/LoadingUIInfo")]
    public class LoadingUI : ScriptableObject
    {
        [field: LabelText("로딩 백그라운드"), SerializeField] public Sprite LoadingBackground { get; private set; }
        [field: LabelText("로딩바 백그라운드"),SerializeField] public Sprite LoadingBarBG { get; private set; }
        [field: LabelText("로딩바"),SerializeField] public Sprite LoadingBar { get; private set; }
        [field: LabelText("폰트"), SerializeField] public TMP_FontAsset Font { get; private set; }
        [field: LabelText("글자 색"), SerializeField] public Color FontColor { get; private set; }
        [field: LabelText("폰트 머티리얼"), SerializeField] public Material FontMaterial { get; private set; }
        [field: LabelText("로딩 글자 이미지"),SerializeField] public Sprite LoadingTextImage { get; private set; }
    }
}