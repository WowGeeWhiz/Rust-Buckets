using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fusion_Camera_Follow : MonoBehaviour
{
    public static Fusion_Camera_Follow Singleton 
    {
        get => _singleton;
        set 
        {
            if (value == null)
            {
                _singleton = null;
            }
            else if (_singleton == null)
            {
                _singleton = value;
            }
            else if (_singleton != value)
            {
                Destroy(value);
                Debug.LogError($"There should only ever be one instance of {nameof(Fusion_Camera_Follow)}!");
            }
        }
    }

    private static Fusion_Camera_Follow _singleton;

    private Transform target;

    private void Awake()
    {
        Singleton = this;

    }

    private void OnDestroy()
    {
        if (Singleton == this) 
        {
            Singleton = null;
        }
    }

    private void LateUpdate()
    {
        if (target != null) 
        {
            this.transform.SetPositionAndRotation(target.position, target.rotation);
        }
    }

    public void SetTarget(Transform newTarget) 
    {
        target = newTarget;
    }

}
