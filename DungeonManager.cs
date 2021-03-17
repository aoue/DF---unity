using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    //contains a reference to all dungeons.
    //is a member of WorldManager.

    //when entering a dungeon, worldmanager sets dungeonID to a unique id for each dungeon so dungeonManager
    //knows what dungeon to find information from.

    private int id; //holds index of what dungeon we're currently in
    [SerializeField] private Dungeon[] dungeonChildren;

    private static DungeonManager _instance;

    void Awake()
    {        
        if ( _instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }


        id = 0; //temp
        
    }
       
    public Enemy[] generate_encounter()
    {
        return dungeonChildren[id].generate_enemies();
    }

    public void adjust_aliveList(EnemyMob mob)
    {
        dungeonChildren[id].get_deadList()[mob.get_identifier()] = true;


    }

    public int get_threatValue()
    {
        return dungeonChildren[id].get_threat();
    }

}
