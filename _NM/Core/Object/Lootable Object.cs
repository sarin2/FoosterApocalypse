using System;
using System.Threading;
using _NM.Core.Item;
using _NM.Core.Item.Implementaion;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;


namespace _NM.Core.Object
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    public abstract class LootableObject : MonoBehaviour
    {
        [SerializeField] protected Rigidbody rigid;
        
        [SerializeField] protected CancellationTokenSource disappearCts = new();
        [SerializeField] protected CancellationTokenSource dropCts = new();
        [HideInInspector] public UnityEvent onDrop;
        [HideInInspector] public UnityEvent<Item.Implementaion.Item> onLoot;
        [HideInInspector] public UnityEvent onDisappear;
        
        [SerializeField] protected bool isFloatingObject;
        [ShowIf(nameof(isFloatingObject)),SerializeField] private float degreesPerSecond = 180.0f;
        [SerializeField] private float rotateX = 0f;
        [SerializeField] private float rotateZ = 0f;

        /// <summary>
        /// 회전의 기준을 정합니다.
        /// Space.Slef는 현 객체의 회전 값을 기준으로 XYZ를 회전 시키며
        /// Space.World는 0,0,0 을 기준으로 XYZ를 회전 시킵니다.
        /// </summary>
        [field:LabelText("회전 기준"),SerializeField, Tooltip("회전의 기준을 정합니다.\n" +
                                                          "Space.Slef는 현 객체의 회전 값을 기준으로\n " + "XYZ를 회전 시키며\n" +
                                                          "Space.World는 0,0,0 을 기준으로\n" + "XYZ를 회전 시킵니다.")] 
        private Space rotateSpace;

        [ShowIf(nameof(isFloatingObject)),SerializeField] private Vector3 posOffset = new();
        [ShowIf(nameof(isFloatingObject)),SerializeField] private Vector3 tempPos = new();

        private void OnValidate()
        {
            if (!rigid)
                rigid = GetComponent<Rigidbody>();
        }

        protected virtual void OnLoot(Item.Implementaion.Item item)
        {
            
        }

        protected virtual void OnDrop()
        {
            
        }

        protected virtual void OnDisappear()
        {
            
        }

        protected async UniTask<bool> SetFloating(CancellationToken cts)
        {
            if (isFloatingObject)
            {
                rigid.constraints = RigidbodyConstraints.FreezeAll;
                rigid.velocity = Vector3.zero;
                transform.rotation = quaternion.identity;
                transform.Rotate(rotateX,0,rotateZ);
                while (!cts.IsCancellationRequested)
                {
                    transform.Rotate(new Vector3(0f, Time.deltaTime * degreesPerSecond, 0f),rotateSpace);

                    //tempPos = transform.position;
                    //tempPos.y += Mathf.Sin (Time.fixedTime * Mathf.PI * frequency) * amplitude;
                    //transform.position = tempPos;
                    await UniTask.WaitForFixedUpdate();
                }

                return true;
            }

            return false;
        }
        
        protected async UniTaskVoid DropFromEnemy(CancellationToken cts)
        {
            float angle = 0f;
            transform.rotation = Quaternion.identity;
            while (!cts.IsCancellationRequested)
            {
                angle += 720.0f * Time.fixedDeltaTime;
                if (angle >= 360)
                {
                    angle = 0.0f;
                }
                transform.rotation = Quaternion.Euler(angle,0, 0);

                await UniTask.WaitForFixedUpdate();
            }

            if (!cts.IsCancellationRequested)
            {
                transform.rotation = Quaternion.identity;
            }
            
        }
        
    }
    
}
