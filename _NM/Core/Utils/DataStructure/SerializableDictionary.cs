using System;
using System.Collections.Generic;
using UnityEngine;
namespace _NM.Core.Utils
{
    [Serializable]
    public class SerializableDictionary<K, V> : SerializableDictionary<K, V, K, V>
    {
        public override K SerializeKey(K key) => key;

        public override V SerializeValue(V val) => val;

        public override K DeserializeKey(K key) => key;

        public override V DeserializeValue(V val) => val;
    }

    [Serializable]
    public abstract class SerializableDictionary<K, V, SK, SV> : Dictionary<K, V>, ISerializationCallbackReceiver
    {
        [SerializeField]
        List<SK> m_Keys = new List<SK>();

        [SerializeField]
        List<SV> m_Values = new List<SV>();

        public abstract SK SerializeKey(K key);

        public abstract SV SerializeValue(V value);

        public abstract K DeserializeKey(SK serializedKey);

        public abstract V DeserializeValue(SV serializedValue);

        public void OnBeforeSerialize()
        {
            m_Keys.Clear();
            m_Values.Clear();

            foreach (var kvp in this)
            {
                m_Keys.Add(SerializeKey(kvp.Key));
                m_Values.Add(SerializeValue(kvp.Value));
            }
        }

        public void OnAfterDeserialize()
        {
            for (int i = 0; i < m_Keys.Count; i++)
                if (!ContainsKey(DeserializeKey(m_Keys[i])))
                {
                    Add(DeserializeKey(m_Keys[i]), DeserializeValue(m_Values[i]));
                }
                

            m_Keys.Clear();
            m_Values.Clear();
        }
    }
}