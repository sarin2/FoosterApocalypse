using System.Collections;
using System.Collections.Generic;
using _NM.Core.Utils;
using UnityEngine;

public enum MinimapMode
{
    Mini, Fullscreen
}
public class MinimapController : MonoBehaviour
{
    public static MinimapController Instance;

    [SerializeField]
    Vector2 worldSize;

    [SerializeField]
    Vector2 fullScreenDimensions = new Vector2(1000, 1000);

    [SerializeField] private Transform offsetTransform;
    [SerializeField] private Vector2 offset;

    [SerializeField]
    float zoomSpeed = 0.1f;

    [SerializeField]
    float maxZoom = 10f;

    [SerializeField]
    float minZoom = 1f;

    [SerializeField]
    RectTransform scrollViewRectTransform;

    [SerializeField]
    RectTransform contentRectTransform;

    [SerializeField]
    MinimapIcon minimapIconPrefab;

    Matrix4x4 transformationMatrix;

    [SerializeField]private MinimapMode currentMiniMapMode = MinimapMode.Mini;
    [SerializeField]private MinimapIcon followIcon;
    [SerializeField]private Vector2 scrollViewDefaultSize;
    [SerializeField]private Vector2 scrollViewDefaultPosition;
    [SerializeField] SerializableDictionary<MinimapWorldObject, MinimapIcon> miniMapWorldObjectsLookup = new ();
    private void Awake()
    {
        Instance = this;
        scrollViewDefaultSize = scrollViewRectTransform.sizeDelta;
        scrollViewDefaultPosition = scrollViewRectTransform.anchoredPosition;
    }

    private void Start()
    {
        CalculateTransformationMatrix();
    }

    private void Update()
    {
        CalculateTransformationMatrix();
        if (Input.GetKeyDown(KeyCode.M))
        {
            SetMinimapMode(currentMiniMapMode == MinimapMode.Mini ? MinimapMode.Fullscreen : MinimapMode.Mini);
        }

        float zoom = Input.GetAxis("Mouse ScrollWheel");
        ZoomMap(zoom);
        UpdateMiniMapIcons();
        CenterMapOnIcon();
    }

    public void RegisterMinimapWorldObject(MinimapWorldObject miniMapWorldObject, bool followObject = false)
    {
        var minimapIcon = Instantiate(minimapIconPrefab);
        minimapIcon.transform.SetParent(contentRectTransform);
        minimapIcon.transform.SetParent(contentRectTransform);
        minimapIcon.Image.sprite = miniMapWorldObject.MinimapIcon;
        miniMapWorldObjectsLookup[miniMapWorldObject] = minimapIcon;

        if (followObject)
            followIcon = minimapIcon;
    }

    public void RemoveMinimapWorldObject(MinimapWorldObject minimapWorldObject)
    {
        if (miniMapWorldObjectsLookup.TryGetValue(minimapWorldObject, out MinimapIcon icon))
        {
            miniMapWorldObjectsLookup.Remove(minimapWorldObject);
            if (icon)
            {
                Destroy(icon.gameObject);
            }
            
        }
    }


    private Vector2 halfVector2 = new Vector2(0.5f, 0.5f);
    public void SetMinimapMode(MinimapMode mode)
    {
        const float defaultScaleWhenFullScreen = 1.3f;
        
        if (mode == currentMiniMapMode)
            return;

        switch (mode)
        {
            case MinimapMode.Mini:
                scrollViewRectTransform.sizeDelta = scrollViewDefaultSize;
                scrollViewRectTransform.anchorMin = Vector2.one;
                scrollViewRectTransform.anchorMax = Vector2.one;
                scrollViewRectTransform.pivot = Vector2.one;
                scrollViewRectTransform.anchoredPosition = scrollViewDefaultPosition;
                currentMiniMapMode = MinimapMode.Mini;
                break;
            case MinimapMode.Fullscreen:
                scrollViewRectTransform.sizeDelta = fullScreenDimensions;
                scrollViewRectTransform.anchorMin = halfVector2;
                scrollViewRectTransform.anchorMax = halfVector2;
                scrollViewRectTransform.pivot = halfVector2;
                scrollViewRectTransform.anchoredPosition = Vector2.zero;
                currentMiniMapMode = MinimapMode.Fullscreen;
                contentRectTransform.transform.localScale = Vector3.one * defaultScaleWhenFullScreen;
                break;
        }
    }

    private void ZoomMap(float zoom)
    {
        if (zoom == 0)
            return;

        float currentMapScale = contentRectTransform.localScale.x;
        float zoomAmount = (zoom > 0 ? zoomSpeed : -zoomSpeed) * currentMapScale;
        float newScale = currentMapScale + zoomAmount;
        float clampedScale = Mathf.Clamp(newScale, minZoom, maxZoom);
        contentRectTransform.localScale = Vector3.one * clampedScale;
    }

    private void CenterMapOnIcon()
    {
        if (followIcon != null)
        {
            float mapScale = contentRectTransform.transform.localScale.x;
            contentRectTransform.anchoredPosition = (-followIcon.RectTransform.anchoredPosition * mapScale);
        }
    }

    private void UpdateMiniMapIcons()
    {
        float iconScale = 1 / contentRectTransform.transform.localScale.x;
        foreach (var kvp in miniMapWorldObjectsLookup)
        {
            var miniMapWorldObject = kvp.Key;
            var miniMapIcon = kvp.Value;
            var mapPosition = WorldPositionToMapPosition((miniMapWorldObject.transform.position - offsetTransform.position));

            miniMapIcon.RectTransform.anchoredPosition = mapPosition;
            var rotation = miniMapWorldObject.transform.rotation.eulerAngles;
            miniMapIcon.IconRectTransform.localRotation = Quaternion.AngleAxis(-rotation.y, Vector3.forward);
            miniMapIcon.IconRectTransform.localScale = Vector3.one * iconScale;
        }
    }

    private Vector2 WorldPositionToMapPosition(Vector3 worldPos)
    {
        var pos = new Vector2(worldPos.x, worldPos.z);
        return transformationMatrix.MultiplyPoint3x4(pos);
    }


    private void CalculateTransformationMatrix()
    {
        var minimapSize = contentRectTransform.rect.size;
        var worldSize = new Vector2(this.worldSize.x, this.worldSize.y);

        var translation = -minimapSize / 2;
        var scaleRatio = minimapSize / (worldSize-new Vector2(offsetTransform.position.x,offsetTransform.position.z));

        transformationMatrix = Matrix4x4.TRS(translation, Quaternion.identity, scaleRatio);
    }
}