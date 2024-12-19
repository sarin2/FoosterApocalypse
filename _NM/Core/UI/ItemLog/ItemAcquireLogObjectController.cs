using System;
using _NM.Core.Item.Implementaion;
using _NM.Core.Sound;
using _NM.Core.UI.Inventory;
using _NM.Core.Utils;
using DG.Tweening;
using UnityEngine;

public class ItemAcquireLogObjectController : MonoBehaviour
{
    [SerializeField] private SerializedQueue<ItemAcquireLogObject> logQueue = new();
    [SerializeField] private SerializableDictionary<ItemAcquireLogObject,RectTransform> activatedObject;
    [SerializeField] private Vector2 startRectPos;
    [SerializeField] private Vector2[] objectPos;
    [SerializeField] private ItemAcquireLogObject currentObject;
    [SerializeField] private ItemAcquireLogObject lastObject;
    [SerializeField] private SerializedQueue<Action> actionQueue = new();
    [SerializeField] private float actionInterval;
    [SerializeField] private PlaySound acquireSound;
    private float currentTime = 0f;
    
    private void OnAcquireItem(Item item)
    {
        actionQueue.Enqueue(() =>
        {
            currentObject = logQueue.Dequeue();
            if (activatedObject.TryGetValue(currentObject, out RectTransform rectTransform))
            {
                rectTransform.DOPause();
                rectTransform.DORewind();
                rectTransform.anchoredPosition = startRectPos;
                currentObject.YPosDest = startRectPos.y;
                currentObject.ShowItemAcquireLog(item,item.Amount);
                PushElementsToTop();
                logQueue.Enqueue(currentObject);
                if (acquireSound)
                    acquireSound.Play();
            }
        });
    }
    

    private void PushElementsToTop()
    {
        foreach (var pair in activatedObject)
        {
            if (!pair.Key.Equals(currentObject))
            {
                pair.Value.DORewind();
                pair.Value.DOAnchorPosY(pair.Key.YPosDest, 0f);
            }
            pair.Key.YPosDest += 50f;
            pair.Value.DOAnchorPosY(pair.Key.YPosDest,0.4f);
        }
    }

    private void Update()
    {
        if (currentTime >= actionInterval && actionQueue.TryDequeue(out Action action))
        {
            action.Invoke();
            currentTime = 0f;
        }
        else if (currentTime < actionInterval)
        {
            currentTime += Time.deltaTime;
        }
    }

    private void Awake()
    {
        currentTime = 0f;
        activatedObject = new();
        actionQueue = new();
        foreach (var item in logQueue)
        {
            activatedObject.Add(item,item.gameObject.GetComponent<RectTransform>());
        }
        
    }

    private void Start()
    {
        SceneLoadingController.onLoadStarted += _ =>
        {
            InventoryManager.I?.onAcquireItem.RemoveListener(OnAcquireItem);
        };
        
        SceneLoadingController.onLoadCompleted += _ =>
        {
            InventoryManager.I?.onAcquireItem.AddListener(OnAcquireItem);
        };
    }
}
