using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DialogueHolder : MonoBehaviour
{
    //so, a dialogueHolder doubles as an npc behaviour thing.
    private bool spokenTo;
    [SerializeField] private int npcId; //unique id for the npc in dialogue library
    [SerializeField] private int loopDiaId; //index for loop dialogue in dialogueLibrary.
    [SerializeField] private int uniDiaId; //index for one time dialogue in dialogueLibrary.

    //trigger
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.name == "Player")
        {
            if (!WorldManager.inDialogue && Input.GetKey(KeyCode.Space))
            {
                //if the player is talking to something that has the CatMovement script
                transform.parent.GetComponent<CatMovement>().canMove = false; //then stop the something from moving

                //pass in dialogue array to manager
                if (!spokenTo)
                {
                    spokenTo = true;
                    WorldManager.begin_dialogue(npcId, uniDiaId);
                }
                else
                {
                    WorldManager.begin_dialogue(npcId, loopDiaId);
                }
                
                
            }
        }
    }

}
