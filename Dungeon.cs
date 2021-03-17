using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dungeon : MonoBehaviour
{
    //to be implemented later, a dungeon also contains:
    // -non-combat music track
    // -combat music track
    //

    //contains information about a single dungeon.
    //information contained is:
    // -threat: increases after battle, after new floor. as it increases, more and stronger monsters start to appear and so does better loot.
    //  ^note: threat is represented somewhere on screen as a bar or something. a dungeon has a max threat too.
    // -chestlist: list of loot that can be found randomly in chests. necessary info is: Loot, min threat, % chance to find.
    //  ^note, % chance to find objects are rolled in decreasing threat order. once a roll passes, do no further rolls.


    //combat
    private static int threat;
    private int lastChosen;

    [SerializeField] private Formation[] formationList;

    [SerializeField] private GameObject[] mobs;
    
    //loot
    [SerializeField] private Loot[] chestItems;
    [SerializeField] private int[] chestChances;

    //reset on dungeon load
    
    private static bool[] deadList = new bool[2]; //parallel with mobs. helps control spawning. //set to true all the way through
    private bool firstTime = true; //when loading a dungeon through the world map, set firstTime to true again.

    void Awake()
    {
        //threat += 1; //temp
        //Debug.Log("dungeon 0 threat = " + threat);

        if ( firstTime)
        {
            firstTime = false;
            Debug.Log("dungeon first time");

            for (int i = 0; i < mobs.Length; i++)
            {
                if (deadList[i])
                {
                    Destroy(mobs[i]);
                }
            }
        }
    }

    public bool[] get_deadList() { return deadList; } 

    //retrieves a list of enemies from the various lists created by hand
    public Enemy[] generate_enemies()
    {
        //starting at end of mobList, descend and check until we reach threat > minThreat and rand < chance    
        int chosen = 0;
        for (int i = formationList.Length-1; i >= 0; i--)
        {
            if (i != lastChosen && threat > formationList[i].get_minThreat() && UnityEngine.Random.Range(0, 100) < formationList[i].get_chance())
            {
                chosen = i;
                break;
            }
        }

        //------------ override for testing purposes.
        chosen = 0;
        //------------

        //also, increases threat here
        threat += formationList[chosen].get_threatIncrease();

        
        //returns picked list
        return formationList[chosen].get_troops(); //place units according to a deployment pattern chosen by formation
    }
    
    public Loot generate_loot()
    {
        //same logic as generate_enemies
        int chosen = 0;
        for (int i = chestItems.Length; i > 0; i--)
        {
            if (UnityEngine.Random.Range(0, 100) < chestChances[i])
            {
                chosen = i;
                break;
            }
        }

        //returns picked loot
        return chestItems[chosen];
    }

    public int get_threat() { return threat; }

}
