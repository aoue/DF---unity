using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeDialogueHolder : MonoBehaviour
{
    private bool triggered;
    [SerializeField] private int npcId;
    [SerializeField] private int diaId;

    void OnTriggerEnter2D(Collider2D other)
    {       
        if (other.gameObject.name == "Player")
        {
            //pass in dialog array to free manager
            if (!triggered)
            {
                triggered = true;
                WorldManager.begin_freeDialogue(npcId, diaId);
            }
        }
        
    }
    
}
