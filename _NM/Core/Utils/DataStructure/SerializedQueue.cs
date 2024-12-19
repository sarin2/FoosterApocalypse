using System;
using System.Collections.Generic;
using UnityEngine;

namespace _NM.Core.Utils
{
    [Serializable]
    public class SerializedQueue<E> : SerializedQueue<E, E>
    {
        public override E SerializeElement(E element) => element;
        public override E DeserializeElement(E element) => element;

        public SerializedQueue(int capacity)
        {
            SetCapacity(capacity);
        }

        public SerializedQueue()
        {
            
        }
        
    }
    
    [Serializable]
    public abstract class SerializedQueue<E,SE> : Queue<E>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<SE> elements = new();

        protected void SetCapacity(int capacity)
        {
            elements.Capacity = capacity;
        }

        ~SerializedQueue()
        {
            elements.Clear();
        }
        
        public abstract SE SerializeElement(E key);
        public abstract E DeserializeElement(SE serializedKey);
        
        public void OnBeforeSerialize()
        {
            elements.Clear();

            foreach (var element in this)
            {
                elements.Add(SerializeElement(element));
            }
        }

        public void OnAfterDeserialize()
        {
            for (int i = 0; i < elements.Count; i++)
            {
                E deserializedElement = DeserializeElement(elements[i]);

                if (!Contains(deserializedElement))
                {
                    Enqueue(deserializedElement);
                }
            }
            
            elements.Clear();
        }
    }
}