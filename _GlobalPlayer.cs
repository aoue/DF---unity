using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _GlobalPlayer : MonoBehaviour
{
    public static _GlobalPlayer instance;
    private GameObject playerGO;

    void Awake()
    {
        if (instance == null)
        {
            playerGO = gameObject;
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else 
        {
            if (instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    
}
