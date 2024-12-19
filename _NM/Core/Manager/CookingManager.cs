using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _NM.Core.Cook;
using _NM.Core.Item;
using _NM.Core.UI.Inventory;
using _NM.Core.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _NM.Core.Manager
{
    public class CookingManager : MonoBehaviour
    {
        private readonly Dictionary<int, RecipeData> recipeData = new();
        
        public RecipeData CurrentRecipeData => recipeData[SelectedCookingItemID];
        public int SelectedCookingItemID => selectedCookingItemID;
        private int selectedCookingItemID;
        
        public event Action<RecipeData> onRecipeChanged;
        public event Action<int, bool> onCookSucceed; 
        
        private async UniTaskVoid Start()
        {
            await DataManager.WaitUntilInitialize;
            foreach (var recipeSetting in DataManager.GetSheetData(GoogleSheetsConstantData.ESheetPage.RecipeSettings))
            {
                var parsedData = RecipeData.Parse(recipeSetting);
                if (parsedData != null)
                {
                    recipeData.Add(parsedData.CookingItemID, parsedData);
                }
            }
        }

        public void SetSelectedItem(int id)
        {
            selectedCookingItemID = id;
            SetRequireItemInfo();
        }

        private void SetRequireItemInfo()
        {
            ItemInfoContainer? item = ItemTableManager.GetItemInfo(CurrentRecipeData.CookingItemID);

            if (item.HasValue)
            {
                onRecipeChanged?.Invoke(CurrentRecipeData);
            }
        }
        
        public int GetAvailableAmount()
        {
            if (SelectedCookingItemID == 0)
                return 0;
            
            List<int> possibleAmounts = new();
            
            if (CurrentRecipeData.IngredientItemID0 > 0)
            {
                var ingredientItem0 = InventoryManager.I.GetItem(CurrentRecipeData.IngredientItemID0);
                if (ingredientItem0 == null)
                    return 0;
                possibleAmounts.Add(ingredientItem0.Amount / CurrentRecipeData.IngredientItemCount0);
            }
            
            if (CurrentRecipeData.IngredientItemID1 > 0)
            {
                var ingredientItem1 = InventoryManager.I.GetItem(CurrentRecipeData.IngredientItemID1);
                if (ingredientItem1 == null)
                    return 0;
                possibleAmounts.Add(ingredientItem1.Amount / CurrentRecipeData.IngredientItemCount1);
            }
            
            if (CurrentRecipeData.IngredientItemID2 > 0)
            {
                var ingredientItem2 = InventoryManager.I.GetItem(CurrentRecipeData.IngredientItemID2);
                if (ingredientItem2 == null)
                    return 0;
                possibleAmounts.Add(ingredientItem2.Amount / CurrentRecipeData.IngredientItemCount2);
            }
            
            if (CurrentRecipeData.IngredientItemID3 > 0)
            {
                var ingredientItem3 = InventoryManager.I.GetItem(CurrentRecipeData.IngredientItemID3);
                if (ingredientItem3 == null)
                    return 0;
                possibleAmounts.Add(ingredientItem3.Amount / CurrentRecipeData.IngredientItemCount3);
            }
            
            return possibleAmounts.Min();
        }

        public bool CanCook(int amount)
        {
            if (SelectedCookingItemID == 0 || amount <= 0)
                return false;

            if (CurrentRecipeData.IngredientItemID0 > 0)
            {
                var ingredientItem0 = InventoryManager.I.GetItem(CurrentRecipeData.IngredientItemID0);
                if (ingredientItem0 == null || ingredientItem0.Amount < CurrentRecipeData.IngredientItemCount0 * amount)
                    return false;
            }
            
            if (CurrentRecipeData.IngredientItemID1 > 0)
            {
                var ingredientItem1 = InventoryManager.I.GetItem(CurrentRecipeData.IngredientItemID1);
                if (ingredientItem1 == null || ingredientItem1.Amount < CurrentRecipeData.IngredientItemCount1 * amount)
                    return false;
            }
            
            if (CurrentRecipeData.IngredientItemID2 > 0)
            {
                var ingredientItem2 = InventoryManager.I.GetItem(CurrentRecipeData.IngredientItemID2);
                if (ingredientItem2 == null || ingredientItem2.Amount < CurrentRecipeData.IngredientItemCount2 * amount)
                    return false;
            }
            
            if (CurrentRecipeData.IngredientItemID3 > 0)
            {
                var ingredientItem3 = InventoryManager.I.GetItem(CurrentRecipeData.IngredientItemID3);
                if (ingredientItem3 == null || ingredientItem3.Amount < CurrentRecipeData.IngredientItemCount3 * amount)
                    return false;
            }
            
            return true;
        }

        public void Cook(int amount)
        {
            if (CurrentRecipeData.IngredientItemID0 > 0)
            {
                var ingredientItem = InventoryManager.I.GetItem(CurrentRecipeData.IngredientItemID0);
                if (ingredientItem != null)
                {
                    ingredientItem.Use(CurrentRecipeData.IngredientItemCount0 * amount);
                    InventoryManager.I.SetSlotInfo(EItemType.Goods, ingredientItem.Slot);
                }
            }
            if (CurrentRecipeData.IngredientItemID1 > 0)
            {
                var ingredientItem = InventoryManager.I.GetItem(CurrentRecipeData.IngredientItemID1);
                if (ingredientItem != null)
                {
                    ingredientItem.Use(CurrentRecipeData.IngredientItemCount1 * amount);
                    InventoryManager.I.SetSlotInfo(EItemType.Goods, ingredientItem.Slot);
                }
            }
            if (CurrentRecipeData.IngredientItemID2 > 0)
            {
                var ingredientItem = InventoryManager.I.GetItem(CurrentRecipeData.IngredientItemID2);
                if (ingredientItem != null)
                {
                    ingredientItem.Use(CurrentRecipeData.IngredientItemCount2 * amount);
                    InventoryManager.I.SetSlotInfo(EItemType.Goods, ingredientItem.Slot);
                }
            }
            if (CurrentRecipeData.IngredientItemID3 > 0)
            {
                var ingredientItem = InventoryManager.I.GetItem(CurrentRecipeData.IngredientItemID3);
                if (ingredientItem != null)
                {
                    ingredientItem.Use(CurrentRecipeData.IngredientItemCount3 * amount);
                    InventoryManager.I.SetSlotInfo(EItemType.Goods, ingredientItem.Slot);
                }
            }

            var greatSuccess = UnityEngine.Random.Range(0f, 100f) <= CurrentRecipeData.GreatSuccessPercentage;
            if (greatSuccess)
                amount *= 2;
            InventoryManager.I.AddItem(CurrentRecipeData.CookingItemID, amount);
            InventoryManager.I.SortInventory(EItemType.Goods);
            InventoryManager.I.SortInventory(EItemType.Food);
            onCookSucceed?.Invoke(amount, greatSuccess);
        }
    }
}
