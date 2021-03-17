using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueLibrary : MonoBehaviour
{
    //holds references to all dialogue in game. dialogue is retrieved from here
    //by a dialogue/freedialogue manager from a dialogue id taken from the npc
    //that is being spoken to.

    //talking npcs have 2 states:
    // -new, at which point the npc has a unique dialogue to talk to the player about.
    // -loop, the npc plays a dialogue every time they're spoken to.

    //when loading scenes
    //-control what npcs should have spokenTo = true and set it for them.
    //-adjust npc's diaIds depending on chapter/game state.

    //dialogue is divided firstly by character.
    //then by chapter.
    //then by state/time.

    //[SerializeField] private Dialogue[]

    [SerializeField] private Dialogue[] testQuestDialogues;
    [SerializeField] private Dialogue[] prologueFreeDialogues;
    [SerializeField] private Dialogue[] testBranchDialogues;

    private int heldNpcId = -3; //held. -3 is invalid index.
    //access functions
    public Dialogue retrieve_dialogue(int npcId, int diaId)
    {
        //called from worldmanager.
        //retrieves a dialogue with the npcid and the dia id.
        //return null;
        heldNpcId = npcId;
        switch (npcId)
        {            
            case -2: //test branching
                return testBranchDialogues[diaId];
            case -1: //test for first quest
                return testQuestDialogues[diaId];
            case 0: //freedialogues
                return prologueFreeDialogues[diaId];
            case 1: //yve, for example
                //return yveDialogueArray[diaId]
                return null;
            default:
                Debug.Log("retrieve_dialogue() called with invalid npcId");
                return null; //lol
        }   
        
    }

    public int get_heldNpcId() { return heldNpcId; }


}
