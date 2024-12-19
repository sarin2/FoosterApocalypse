using System;
using _NM.Core.Object;
using _NM.Core.UI.Inventory;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _NM.Core.Item
{
    enum ItemDurationType
    {
        Duration_Finite,
        Duration_Infinite
    }
    public class ItemBase : LootableObject
    {
        [SerializeField] private long itemId;
        [SerializeField] private string itemName;
        [SerializeField] private ItemDurationType ItemDurationType;
        [SerializeField] private Implementaion.Item itemData;
        [SerializeField] private bool moveToPlayer;
        [SerializeField] private float moveTime;
        [SerializeField] private float currTime;
        [SerializeField] private Vector3 startPos;
        [SerializeField] private AnimationCurve moveCurve;
        [SerializeField] private Transform playerTransform; 


        private void Awake()
        {
            ItemInfoContainer? itemInfo = ItemTableManager.GetItemInfo(itemId);
            if (itemInfo.HasValue)
            {
                itemData = new(itemInfo.Value);
                itemData.SetAmount(1);
                itemName = itemData.ItemInfoData.ItemName;
            }
            
            moveCurve = Resources.Load<ItemAcquireCurve>("Data/Item/ItemAcquireCurve").AcquireCurve;

            onLoot?.AddListener(OnLoot);
            onDrop?.AddListener(OnDrop);
            onDisappear?.AddListener(OnDisappear);
            currTime = 0f;
        }

        private void Start()
        {
            playerTransform = Character.Character.Local.transform;
            moveToPlayer = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                onLoot?.Invoke(itemData);
            }
        }

        private void Update()
        {
            if (moveToPlayer)
            {
                currTime += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos,playerTransform.position,moveCurve.Evaluate(currTime));
            }
          
        }

        private void OnEnable()
        {
            disappearCts = new();
            dropCts = new();
        }

        private void OnCollisionEnter(Collision other)
        {
            rigid.useGravity = false;
            rigid.velocity = Vector3.zero;
            rigid.constraints = RigidbodyConstraints.FreezePosition;
            GetComponent<BoxCollider>().enabled = false;
            dropCts.Cancel();
            StartObjectFloating().Forget();
            startPos = transform.position;
            moveToPlayer = true;
        }

        protected override void OnDrop()
        {
            rigid.AddForce(0,10.0f,0,ForceMode.Impulse);
            DropFromEnemy(dropCts.Token).Forget();
        }

        protected override void OnLoot(Item.Implementaion.Item item)
        {
            disappearCts.Cancel();
        }

        protected override void OnDisappear()
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private async UniTaskVoid StartObjectFloating()
        {
            bool result = await SetFloating(disappearCts.Token);

            if (result)
            {
                onDisappear?.Invoke();
            }
        }

        private void OnDestroy()
        {
            if (!disappearCts.IsCancellationRequested)
            { 
                disappearCts.Cancel();
            }
        }
    }
}

