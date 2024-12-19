using System;
using System.Collections;
using System.Collections.Generic;
using _NM.Core.Character;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace _NM.Core.UI.Minimap
{
    public class Minimap : MonoBehaviour
    {
        [SerializeField] private Transform cameraTrans;
        [SerializeField] private Transform playerTrans;
        [SerializeField] private Vector3 offset;

        private Transform PlayerTrans
        {
            get
            {
                if (Core.Character.Character.Local)
                {
                    playerTrans = Core.Character.Character.Local.transform;
                }
                    
                return playerTrans;
            }
        }
        
        // Start is called before the first frame update
        void Start()
        {
            cameraTrans = transform.GetChild(0);
        }


        private void LateUpdate()
        {
            if (cameraTrans && PlayerTrans)
            {
                cameraTrans.position = PlayerTrans.position + offset;
            }
            
        }
        
    }
}