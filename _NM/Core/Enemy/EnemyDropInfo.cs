using System.Collections.Generic;
using UnityEngine;

namespace _NM.Core.Enemy
{
    public class EnemyDropInfo : ScriptableObject
    {
        [SerializeField] public List<GameObject> Items { get; private set; }
    }
}
