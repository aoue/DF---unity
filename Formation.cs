using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Formation : MonoBehaviour
{
    //a formation is a squad of enemies that is encountered in a dungeon.
    //it holds info that is converted into a sensible Enemy[]
    //a dungeon has an array of Formations

    //a formation holds:
    // -Enemy[] of the base units
    // -several possible deployments for the units. (script)
    // -troopName

    [SerializeField] Enemy[] troops; //list of Enemy objects that are in the formation
    [SerializeField] int minThreat; //minimum threat the formation can appear at
    [SerializeField] int threatIncrease; //how much the formation increases/decreases the dungeon's threat
    [SerializeField] int chance; //% chance of the party appearing if its in the threat limit

    public Enemy get_troop(int i) { return troops[i]; }
    public int get_minThreat() { return minThreat; }
    public int get_threatIncrease() { return threatIncrease; }
    public int get_chance() { return chance; }

    public virtual void place_troops()
    {
        //void function that places enemies
        //overridden by specific formation function.
        //it chooses one setup, of which there are several, just for that one group of monsters.

        return;
    }
    public Enemy[] get_troops()
    {
        place_troops();
        return troops;
    }
}
