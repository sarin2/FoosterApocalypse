using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
namespace _NM.Core.UI.Loading
{
    public class LoadingUIGenerator : MonoBehaviour
    {
        [SerializeField] private GameObject loadingObject;
        [SerializeField] private GameObject loadingObjectWithoutLoadingbar;


        [SerializeField] private List<LoadingUI> loadingUI;
        [SerializeField] private Image backgroundImg;
        [SerializeField] private Image loadingBarBGImg;
        [SerializeField] private Image loadingBarImg;
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private Image loadingTextImage;

        [SerializeField] private bool hasLoadingbar;

        private void Awake()
        {
            int rand = Random.Range(0, loadingUI.Count);
            
            backgroundImg.sprite = loadingUI[rand].LoadingBackground;
            loadingText.color = loadingUI[rand].FontColor;
            loadingTextImage.sprite = loadingUI[rand].LoadingTextImage;
            if (hasLoadingbar)
            {
                loadingObjectWithoutLoadingbar.SetActive(false);
                loadingObject.SetActive(true);
                loadingBarBGImg.sprite = loadingUI[rand].LoadingBarBG;
                loadingBarImg.sprite = loadingUI[rand].LoadingBar;
            }
            else
            {
                loadingObjectWithoutLoadingbar.SetActive(true);
                loadingObject.SetActive(false);
                loadingBarBGImg.enabled = false;
                loadingBarImg.enabled = false;
            }


        }
    }
}
