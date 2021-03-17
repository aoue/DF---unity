using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    //used to change scene.
    //attached to a trigger that, when triggered, loads the attached scene.

    //when player runs into portal, show thing that says 'press space to change scenes'
    //

    [SerializeField] private int portalKey;
    [SerializeField] private string portalMsgKey;

    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player"))
        {
            //if player, prime to go through portal
            WorldManager.open_portal(portalKey);
        }


    }

    void OnTriggerExit2D(Collider2D other)
    {

        if (other.CompareTag("Player"))
        {
            //if player, hide portal stuff
            WorldManager.close_portal();
        }


    }


}

