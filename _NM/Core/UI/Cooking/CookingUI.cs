using System.Collections.Generic;
using _NM.Core.Cook;
using _NM.Core.Input;
using _NM.Core.Item;
using _NM.Core.Manager;
using _NM.Core.Manager.Scene;
using _NM.Core.UI.Common;
using _NM.Core.UI.Inventory;
using _NM.Core.UI.UICanvas;
using _NM.Core.Utils;
using _NM.Core.Utils.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SDUnityExtension.Scripts.Extension;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace _NM.Core.UI.Cooking
{
    public class CookingUI : CanvasPage
    {
        enum CookingState
        {
            Recipe,
            Cook,
            Result
        }

        [SerializeField] private CookingManager manager;
        
        [Title("Recipe Info")]
        [SerializeField] private TextMeshProUGUI recipeFoodText;
        [SerializeField] private TextMeshProUGUI recipeDescriptionText;
        [SerializeField] private Image recipeFoodImage;
        [SerializeField] private List<RequiredSlot> recipeRequiredList = new();
        [SerializeField] private TextMeshProUGUI recipeCurrentAmountText;
        [SerializeField] private List<Button> recipeButtons = new();
        [SerializeField] private List<int> recipeItemIDs = new();

        [Title("Cook Amount Slider")] 
        [SerializeField] private Image cookFoodImage;
        [SerializeField] private Slider cookAmountSlider;
        [SerializeField] private TextMeshProUGUI cookCurrentAmountText;
        [SerializeField] private TextMeshProUGUI cookAmountText;
        [SerializeField] private TextMeshProUGUI cookMinimumAmountText;
        [SerializeField] private TextMeshProUGUI cookMaximumAmountText;
        
        [Title("Cook Result")] 
        [SerializeField] private GameObject resultNormalFoodPlateObject;
        [SerializeField] private GameObject resultGreatFoodPlateObject;
        [SerializeField] private TextMeshProUGUI resultFoodNameText;
        [SerializeField] private TextMeshProUGUI resultFoodAmountText;
        [SerializeField] private Image resultFoodImage;
        [SerializeField] private Image resultFootNamePlateImage;
        [SerializeField] private Sprite normalNamePlateSprite;
        [SerializeField] private Sprite greatNamePlateSprite;
        [SerializeField] private ParticleSystem resultNormalParticle;
        [SerializeField] private ParticleSystem resultGreatParticle;
        [SerializeField] private ParticleSystem resultGreatNamePlateParticle;
        
        [Title("Animations")]
        [SerializeField] private DoController recipePageAnimation;
        [SerializeField] private DoController cookPageAnimation;
        [SerializeField] private DoController resultPageAnimation;

        [Title("Component")] 
        [SerializeField] private AudioSource shiningAudio;
        [SerializeField] private FadeUI fadeUI;
        [SerializeField] private Button backButton;
        
        private CookingState CurrentState { get; set; } = CookingState.Recipe;
        private int currentCookAmount = 1;

        private void Awake()
        {
            DisableRequireSlot();
            recipeFoodImage.enabled = false;
            
            cookAmountSlider.onValueChanged.AddListener(value =>
            {
                currentCookAmount = (int)value;
                cookAmountText.text = currentCookAmount.ToString();
            });
            cookAmountSlider.value = currentCookAmount;

            manager.onRecipeChanged += SetFoodInfo;
            manager.onCookSucceed += (cookedAmount, isGreatSucceeded) => OnCookSucceed(cookedAmount, isGreatSucceeded).Forget();
            
            recipeButtons.ForEach((button, index) => button.onClick?.AddListener(() => OnClick_SetSelectedItem(recipeItemIDs[index])));
            
            backButton.onClick.AddListener(OnClick_Back);
        }

        public void SetFoodInfo(RecipeData data)
        {
            recipeFoodImage.enabled = true;
            DisableRequireSlot();
        
            ItemInfoContainer? selectedFood = ItemTableManager.GetItemInfo(data.CookingItemID);

            if (!selectedFood.HasValue) return;
            var foodInfo = selectedFood.Value;
            recipeFoodText.text = foodInfo.ItemName;
            recipeDescriptionText.text = foodInfo.ItemDescription;
            resultFoodNameText.text = foodInfo.ItemName;

            recipeRequiredList.ForEach(e => e.gameObject.SetActive(false));
                
            if (data.IngredientItemID0 > 0)
            {
                SetRequireItemInfo(0, data.IngredientItemID0, data.IngredientItemCount0);
            }
            if (data.IngredientItemID1 > 0)
            {
                SetRequireItemInfo(1, data.IngredientItemID1, data.IngredientItemCount1);
            }
            if (data.IngredientItemID2 > 0)
            {
                SetRequireItemInfo(2, data.IngredientItemID2, data.IngredientItemCount2);
            }
            if (data.IngredientItemID3 > 0)
            {
                SetRequireItemInfo(3, data.IngredientItemID3, data.IngredientItemCount3);
            }

            var availableAmount = manager.GetAvailableAmount();
            SetSliderAmount(availableAmount);
            SetCurrentAmount(data.CookingItemID);
            resultFoodImage.sprite = cookFoodImage.sprite = recipeFoodImage.sprite =
                ItemIconTable.Local.GetItemSprite(foodInfo.ItemIconName, ItemIconTable.SpriteSize.Big);
        }
        
        private void SetRequireItemInfo(int index, int ingredientID, int requireCount)
        {
            recipeRequiredList[index].gameObject.SetActive(true);
            var requireItem = ItemTableManager.GetItemInfo(ingredientID);
            if (requireItem.HasValue)
            {
                var item = requireItem.Value;
                Item.Implementaion.Item currentItem = InventoryManager.I.GetItem(ingredientID);
                int materialAmount = InventoryManager.I.GetAllAmount(currentItem);
                var sprite = ItemIconTable.Local.GetItemSprite(item.ItemIconName, ItemIconTable.SpriteSize.Small);
                recipeRequiredList[index].SetInfo(sprite, materialAmount, requireCount);
            }
        }

        private void DisableRequireSlot()
        {
            foreach (var slot in recipeRequiredList)
            {
                slot.gameObject.SetActive(false);
            }
        }

        private void SetSliderAmount(int maxAmount)
        {
            if (maxAmount > 0)
            {
                cookAmountSlider.maxValue = Mathf.Clamp(maxAmount, 1, 99);
                cookAmountSlider.minValue = 1;
                cookAmountSlider.value = 1;
            }
            else
            {
                cookAmountSlider.maxValue = 0;
                cookAmountSlider.minValue = 0;
                cookAmountSlider.value = 0;
            }

            cookMinimumAmountText.text = cookAmountSlider.minValue.ToString("N0");
            cookMaximumAmountText.text = cookAmountSlider.maxValue.ToString("N0");
        }

        private void SetCurrentAmount(int itemID)
        {
            Item.Implementaion.Item currentItem = InventoryManager.I.GetItem(itemID);
            cookCurrentAmountText.text = recipeCurrentAmountText.text = "현재 보유량 " + InventoryManager.I.GetAllAmount(currentItem);
        }
        
        private async UniTaskVoid OnCookSucceed(int amount, bool greatSucceed)
        {
            backButton.gameObject.SetActive(false);
            cookPageAnimation.PlayReverse();
            
            await fadeUI.FadeOut();
            fadeUI.FadeIn().Forget();
            var daytime = SceneLoadingController.Instance.CurrentScene.name.Contains("Safe_Night") == false;
            if (daytime)
            {
                var daytimeCutScene = GameObject.Find("Daytime Cooking Cut Scene").GetComponent<PlayableDirector>();
                daytimeCutScene.Play();
                Core.Character.Character.Local.gameObject.SetActive(false);
                await UniTask.WaitUntil(() => daytimeCutScene.state != PlayState.Playing);
                
                SafeNightManager.ShouldShowPrologueCutScene.Save(false);
                SafeNightManager.ShouldSpawnAtCookLocation.Save(true);
                SafeNightManager.SpawnPosition = Core.Character.Character.Local.transform.position;
                SafeNightManager.SpawnRotation = Core.Character.Character.Local.transform.eulerAngles;
                SceneLoadingController.Instance.ChangeScene(SceneName.Shelter_Night, showLoading: false, showFade: true, fadeColor: Color.black);
                await UniTask.WaitUntil(() => SceneLoadingController.Instance.SceneChanging == false);
                CanvasUtils.UpdateCanvasCamera(canvas);
                CanvasController.I.ProgressUI(this, true);
                
                Core.Character.Character.Local.gameObject.SetActive(false);
                var nightCutScene = GameObject.Find("Night Cooking Cut Scene").GetComponent<PlayableDirector>();
                nightCutScene.Play();
                await UniTask.WaitUntil(() => nightCutScene.state != PlayState.Playing);
                nightCutScene.Stop();
            }
            else
            {
                CanvasController.I.ProgressUI(this, true);
                
                Core.Character.Character.Local.gameObject.SetActive(false);
                var nightCutScene = GameObject.Find("Night Cooking Short Cut Scene").GetComponent<PlayableDirector>();
                nightCutScene.Play();
                await UniTask.WaitUntil(() => nightCutScene.state != PlayState.Playing);
                nightCutScene.Stop();
            }
            
            CanvasController.I.ProgressUI(this, true);
            IsOpened = true;
            await OnOpen();
            
            Core.Character.Character.Local.gameObject.SetActive(true);
            Core.Character.Character.Local.Reset();
            
            CurrentState = CookingState.Result;
            resultNormalFoodPlateObject.SetActive(greatSucceed == false);
            resultGreatFoodPlateObject.SetActive(greatSucceed);
            resultFoodNameText.text = recipeFoodText.text;
            resultFoodAmountText.text = amount.ToString();
            resultFootNamePlateImage.sprite = greatSucceed ? greatNamePlateSprite : normalNamePlateSprite;
            
            cookPageAnimation.ResetToStart();
            await resultPageAnimation.Play();
            if (IsOpened)
            {
                shiningAudio.Play();
                
                if (greatSucceed)
                {
                    resultGreatParticle.Play();
                    resultGreatNamePlateParticle.Play();
                }
                else
                {
                    resultNormalParticle.Play();
                }
            }

            backButton.gameObject.SetActive(true);
        }

        public override async UniTask Open()
        {
            recipePageAnimation.ResetToEnd();
            cookPageAnimation.ResetToStart();
            resultPageAnimation.ResetToStart();
            OnClick_SetSelectedItem(recipeItemIDs[0]);
            
            await base.Open();
        }

        protected override UniTask OnClose()
        {
            resultGreatParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            resultGreatNamePlateParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            resultNormalParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            shiningAudio.Stop();
            return base.OnClose();
        }

        public override async UniTask Back()
        {
            OnClick_Back();
        }

        public void OnClick_SetSelectedItem(int id)
        {
            manager.SetSelectedItem(id);
        }
        
        public void OnClick_ConfirmRecipe()
        {
            CurrentState = CookingState.Cook;
            cookPageAnimation.Play();
            recipePageAnimation.PlayReverse();
        }
        
        public void OnClick_Cook()
        {
            if (manager.CanCook(currentCookAmount))
            {
                manager.Cook(currentCookAmount);
            }
        }

        public void OnClick_Back()
        {
            switch (CurrentState)
            {
                case CookingState.Recipe:
                    Close().Forget();
                    break;
                case CookingState.Cook:
                    cookPageAnimation.PlayReverse();
                    recipePageAnimation.Play();
                    CurrentState = CookingState.Recipe;
                    break;
                case CookingState.Result:
                    Close().Forget();
                    break;
            }
        }
    }
}
