using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using _NM.Core.Utils;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _NM.Core.Enemy.AI.EnemyAI
{
    public abstract class AIBase : MonoBehaviour
    {
        private List<CancellationTokenSource> tokens = new();
        [SerializeField] protected CancellationTokenSource rootCts;
        
        private EnemyBase ownerBase;
        protected Character.Character player;
        protected int prevHP;

        [Header("Component")] [SerializeField] protected NavMeshAgent navMesh;
        [SerializeField] protected Animator animator;
        [SerializeField] protected Rigidbody rigidbody;
        [SerializeField] protected AreaController area;

        [Header("Timeline")] [SerializeField] protected PlayableDirector playableDirector;

        [field: LabelText("타임라인 에셋"), SerializeField]
        protected SerializableDictionary<string, TimelineAsset> timelineAssets = new();

        [Header("Animation Keys")] [SerializeField]
        protected AnimationKey damagedKey = new AnimationKey("Damaged");

        [SerializeField] protected AnimationKey bigDamagedKey = new AnimationKey("BigDamaged");
        [SerializeField] protected AnimationKey damageTypeKey = new AnimationKey("DamageType");
        [SerializeField] protected AnimationKey attackKey = new AnimationKey("Attack");
        [SerializeField] protected AnimationKey guardKey = new AnimationKey("Guard");
        [SerializeField] protected AnimationKey guardAttackKey = new("GuardAttack");
        [SerializeField] protected AnimationKey moveKey = new("Speed");

        [Header("BT용 변수")] [SerializeField] protected BehaviorTree enemyBT;
        [SerializeField] protected SerializableDictionary<string, SharedVariable> btVariables;
        [SerializeField] public bool IsDead => !ownerBase.Health.IsAlive;

        protected virtual void InitializeBT()
        {
            ownerBase = GetComponent<EnemyBase>();
            ownerBase.ChangeFacial("Idle");
            area = GetComponentInParent<AreaController>();
            player = Character.Character.Local;
            prevHP = ownerBase.Health.CurrentHp;
            enemyBT = GetComponent<BehaviorTree>();

            btVariables = new();
            foreach (var item in enemyBT.GetAllVariables())
            {
                btVariables.TryAdd(item.Name,item);
            }
        }

        
        public object GetBtVariableValue(string variable)
        {
            if (enemyBT)
            {
                SharedVariable sharedVariable = enemyBT.GetVariable(variable);
                if (sharedVariable != null)
                {
                    return sharedVariable.GetValue();
                }
            }

            return null;
        }

        public void SetBtVariableValue(string variable, object value)
        {
            if (enemyBT)
            {
                SharedVariable sharedVariable = enemyBT.GetVariable(variable);
                if (sharedVariable != null)
                {
                    if (sharedVariable.GetValue().GetType() == value.GetType())
                    {
                        sharedVariable.SetValue(value);
                        return;
                    }
                    
                    Debug.LogError(variable+"의 타입과 Value 값의 타입이 일치하지 않습니다.");
                    return;
                }
                
                Debug.LogError("해당 SharedVariable이 존재하지 않습니다.");
            }
        }
        /// <summary>
        /// 몬스터 BT 내부에 전역 변수를 추가합니다.
        /// </summary>
        public void SetBtVariable(string variableName, SharedVariable variable, bool changeIfExist = false)
        {
            if (enemyBT)
            {
                SharedVariable sharedVariable = enemyBT.GetVariable(variableName);

                if (sharedVariable != null)
                {
                    if (changeIfExist)
                    {
                        Debug.LogWarning(variableName+"라는 이름을 가진 변수가 이미 존재합니다. 해당 변수의 값을 변경합니다.");
                        SetBtVariableValue(variableName,variable.GetValue());
                    }
                    else
                    {
                        Debug.LogWarning(variableName+"라는 이름을 가진 변수가 이미 존재합니다. 해당 변수의 값을 변경하지 않습니다.");
                    }
                }
                else
                {
                    if (variable != null)
                    {
                        enemyBT.SetVariable(variableName,variable);
                    }
                }
            }
        }
    }
}