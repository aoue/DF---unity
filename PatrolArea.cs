using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolArea : MonoBehaviour
{
    [SerializeField] private Collider2D walkZone;
    [SerializeField] private EnemyMob mobFriend;

    void Awake()
    {

    }

    //on enemy trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("chase begins!");
            mobFriend.begin_pursuit(other);
        }        
    }
}
