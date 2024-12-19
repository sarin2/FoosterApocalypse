using _NM.Core.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _NM.Core.Object.Interactable.Implementation
{
    public class MapTeleport : MonoBehaviour
    {
        [SerializeField] private bool useSceneName;
        [SerializeField] private bool showLoading = false;
        [SerializeField] private bool showFade = true;
        [SerializeField] private Color fadeColor = Color.black;
        [ShowIf(nameof(useSceneName)), SerializeField] private string sceneName;
        [HideIf(nameof(useSceneName)), SerializeField] private SceneName targetScene;

        public void Teleport()
        {
            if (useSceneName)
            {
                SceneLoadingController.Instance.ChangeScene(sceneName, showLoading, showFade, fadeColor);
            }
            else
            {
                SceneLoadingController.Instance.ChangeScene(targetScene, showLoading, showFade, fadeColor);
            }
        }
    }
}
