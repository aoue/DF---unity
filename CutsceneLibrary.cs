using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneLibrary : MonoBehaviour
{
    //like dialogue library, but for cutscenes.

    [SerializeField] private Cutscene[] testCutscenes; //for testing purposes

    public Cutscene retrieve_cutscene(int csType, int cutsceneId)
    {

        switch (csType)
        {
            
            case -2: //test branching
                return testCutscenes[cutsceneId];
            default:
                Debug.Log("retrieve_dialogue() called with invalid npcId");
                return null; //lol
        }

    }

}
