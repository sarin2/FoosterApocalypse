using System;
using System.Collections;
using System.Collections.Generic;
using SDUnityExtension.Scripts.Pattern;
using UnityEngine;

public class EnemyBroadcastManager : SDSingleton<EnemyBroadcastManager>
{
    private readonly Dictionary<int,Dictionary<string, Action>> eventActionDictionary = new();
    public Action this[int instanceID,string eventName]
    {
        get
        {
            if (!eventActionDictionary.TryGetValue(instanceID, out _))
            {
                eventActionDictionary[instanceID] = new();
            }
            
            if(!eventActionDictionary[instanceID].TryGetValue(eventName, out _))
            {
                eventActionDictionary[instanceID][eventName] = () => { };
            }

            return eventActionDictionary[instanceID][eventName];
        }
        set
        {
            if (eventActionDictionary[instanceID] == null)
            {
                eventActionDictionary[instanceID] = new();
            }
            eventActionDictionary[instanceID][eventName] = value;
        }
    }

    public void SendEvent(string eventName, int ownerInstance, bool invokeOwnerMethod = true)
    {
        foreach (var instancePair in eventActionDictionary)
        {
            if (ownerInstance == instancePair.Key && !invokeOwnerMethod)
            {
                continue;
            }
            
            foreach (var action in instancePair.Value)
            {
                if (action.Key.Equals(eventName))
                {
                    action.Value.Invoke();
                }
            }
        }
    }

    public void UnregisterInstance(int instanceID)
    {
        eventActionDictionary[instanceID].Clear();
        eventActionDictionary.Remove(instanceID);
    }
}
