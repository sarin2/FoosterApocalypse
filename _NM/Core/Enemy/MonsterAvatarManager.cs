using System;
using System.Collections;
using System.Collections.Generic;
using _NM.Core.Utils;
using UnityEngine;

public class MonsterAvatarManager : MonoBehaviour
{
    [SerializeField] private SerializableDictionary<string, Avatar> monsterAvatar = new();
    [SerializeField] private Animator enemyAnimator;

    private void OnValidate()
    {
        if (!enemyAnimator)
        {
            GetComponent<Animator>();
        }
    }

    public void SetAvatar(string avatarName)
    {
        enemyAnimator.avatar = monsterAvatar[avatarName];
    }
}
