using System;
using System.Collections;
using System.Collections.Generic;
using _NM.Core.Quest;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    private VerticalLayoutGroup verticalGroup;

    private void Update()
    {
        if (verticalGroup)
        {
            verticalGroup.CalculateLayoutInputHorizontal();
            verticalGroup.SetLayoutHorizontal();
            verticalGroup.CalculateLayoutInputVertical();
            verticalGroup.SetLayoutVertical();
        }
    }
}
