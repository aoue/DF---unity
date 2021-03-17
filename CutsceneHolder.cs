using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneHolder : MonoBehaviour
{
    //attached to a gameobject. used to trigger the Cutscene object(s) attached.
    //triggers them through WorldManager -> CutsceneManager

    private bool triggered;
    [SerializeField] private int csType; //unique id for the cutscene in cutscene library
    [SerializeField] private int cutsceneId; //index for cutscene in cutscene library


    //cutsceneholder is used to trigger using zones on the ground.
    //other cutscene triggers, like on a scene transition or an npc talk, will be handled on their respective ends.

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            //if player, then start cutscene.
            triggered = true;
            WorldManager.begin_cutscene(this);
        }
    }

    public int get_csType() { return csType; }
    public int get_cutsceneId() { return cutsceneId; }


}
