using System.Collections.Generic;
using _NM.Core.Object;
using _NM.Core.Utils.Camera;
using SDUnityExtension.Scripts.Pattern;
using UnityEngine;

namespace _NM.Core.UI.Navigation
{
    public enum ENavigationType
    {
        Normal,
        Quest,
        SubQuest,
        QuestComplete,
    }
    
    public class NavigationIndicator : SDSingleton<NavigationIndicator>
    {
        private readonly Dictionary<string, NavigationMarker> Markers = new();
        
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private NavigationMarker markerPrefab;
        [SerializeField] private RectTransform markerRoot;
        [SerializeField] private float screenBoundOffset = 0.45f;

        public void Show()
        {
            canvasGroup.alpha = 1;
        }

        public void Hide()
        {
            canvasGroup.alpha = 0;
        }
        
        public void AddMarker(NavigationMarkerData data)
        {
            AddMarker(data.Key, data.Position, data.Type);
        }
        
        public void AddMarker(string key, Vector3 position, ENavigationType type)
        {
            if (Markers.TryGetValue(key, out var marker))
            {
                marker.SetType(type);
                return;
            }
            var instance = ObjectPool.Spawn(markerPrefab, markerRoot);
            instance.Initialize(position, type);
            Markers.Add(key, instance);
        }
        
        public bool TryGetMarker(string key, out NavigationMarker marker)
        {
            return Markers.TryGetValue(key, out marker);
        }
        
        public void RemoveMarker(NavigationMarkerData marker)
        {
            RemoveMarker(marker.Key);
        }

        public void RemoveMarker(string key)
        {
            if (Markers.TryGetValue(key, out var marker))
            {
                ObjectPool.Despawn(marker);
                Markers.Remove(key);
            }
        }

        private void LateUpdate()
        {
            foreach (var markerPair in Markers)
            {
                var marker = markerPair.Value;
                UpdateMarker(marker);
            }
        }

        private void UpdateMarker(NavigationMarker marker)
        {
            var result = CameraUtils.WorldToScreenPoint(marker.TargetPosition, true, screenBoundOffset);
            var rectTransform =  marker.transform as RectTransform;
            rectTransform.anchoredPosition = result.ScreenPosition;
            marker.ShowArrow(result.IsOutOfScreen);
            marker.SetArrowAngle(result.Rotation);
        }
    }
}
