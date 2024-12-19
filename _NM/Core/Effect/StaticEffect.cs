using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StaticEffect : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Transform Root;
    [SerializeField] private GameObject staticEffectContainer;
    [SerializeField] private string staticEffectContainerName = "============ StaticEffects";
    [SerializeField] private ParticleSystem particleSystem;
    [SerializeField] private bool enableOnStart;
    [SerializeField] private Renderer effectRenderer;
    [SerializeField] private bool rotateX;
    

    private void OnValidate()
    {
        if (!particleSystem)
        {
            particleSystem = GetComponent<ParticleSystem>();
            
        }

        if (!effectRenderer)
        {
            
        }
    }

    private void Awake()
    {
        staticEffectContainer = GameObject.Find(staticEffectContainerName);
        if (!staticEffectContainer)
        {
            staticEffectContainer = new GameObject(staticEffectContainerName);
            transform.parent = staticEffectContainer.transform;
        }
        staticEffectContainer.transform.position = Vector3.zero;
        transform.parent = staticEffectContainer.transform;

        if (!enableOnStart)
        {
            gameObject.SetActive(false);
        }
        
    }

    private void OnEnable()
    {
        if (Root)
        {
            transform.SetPositionAndRotation(Root.position,Root.rotation);
            if (rotateX)
            {
                transform.Rotate(new Vector3(-90,0,0));   
            }
            
        }
        

        if (particleSystem)
        {
            particleSystem.Clear();
            particleSystem.Play();
        }

    }

    private void OnDisable()
    {
        if (particleSystem)
        {
            particleSystem.Stop();
        }
    }
}
