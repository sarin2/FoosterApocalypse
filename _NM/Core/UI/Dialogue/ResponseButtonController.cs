using System.Collections.Generic;
using _NM.Core.Object;
using _NM.Core.Utils;
using UnityEngine;

namespace _NM.Core.UI.Dialogue
{
    public class ResponseButtonController : MonoBehaviour
    {
        [SerializeField] private Transform buttonRoot;
        [SerializeField] private List<ResponseButton> currentButtons = new();
        [SerializeField] private SerializableDictionary<ResponseType, GameObject> registeredButtons = new();

        public void SetResponseButton(List<ResponseData> responseData)
        {
            ResetResponseButton();
            
            for (int i = 0; i < responseData.Count; i++)
            {
                var data = responseData[i];
                GameObject button = ObjectPool.Spawn(registeredButtons[data.Type], buttonRoot);
                button.TryGetComponent(out ResponseButton buttonComponent);
                buttonComponent.SetButtonData(this, data);
                buttonComponent.SetAnimation(i);
                currentButtons.Add(buttonComponent);
            }
        }

        public void ResetResponseButton()
        {
            foreach (var button in currentButtons)
            {
                ObjectPool.Despawn(button.gameObject);
            }

            currentButtons.Clear();
        }
    }
}
