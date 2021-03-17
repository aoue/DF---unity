using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//remember, there is now FreeDialogue obj. it's a Dialogue obj with many fields left blank.

[System.Serializable]
public class Dialogue : MonoBehaviour
{
    //public string name;
    [SerializeField] private PortraitManager[] portraitManagers;
    [SerializeField] private int[] startingPortrait; //set to 0 to disable portrait

    [TextArea(3, 10)]
    [SerializeField] private string[] sentences;
    [SerializeField] private string[] names;
    [SerializeField] private int[] portraitIndexes; //only the speaker's portrait changes each new sentence.
    [SerializeField] private int[] speakers; //index that portraitIndexes applies to for a sentence.

    //branching
    [SerializeField] bool hasBranches; //if true, then dia options will show up with branches after.
    [SerializeField] string[] branchOptions; //strings of text for each branch option. max 4.
    [SerializeField] int[] jumpIds; //ints that match up to above branches in dia library.

    public bool get_hasBranches() { return hasBranches; }
    public string[] get_branchOptions() { return branchOptions; }
    public int get_jumpId(int i) { return jumpIds[i]; }


    public PortraitManager[] get_portraitManagers() { return portraitManagers; }
    public int[] get_startingPortrait() { return startingPortrait; }
    public string[] get_names() { return names; }
    public string[] get_sentences() { return sentences; }
    public int[] get_portraitIndexes() { return portraitIndexes; }
    public int[] get_speakers() { return speakers; }

    //whole bunch of things that can be given to player through post_dia()
    //[SerializeField] private Quest addQuest;
    //[SerializeField] private Gear addGear;
    //etc


    public virtual void post_dia()
    {
        //hello. by default does nothing. 
        //can be overidden to do whatever a quest wants.
        //some examples: 
        // -could give an item to inventory
        // -add a quest to journal
        // -progress a quest
        // -etc

        //coded example: (adds the test quest to journal)
        //if (addQuest == null) return;
        //WorldManager.add_quest_to_chapter(addQuest);
    }

}