using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passive : MonoBehaviour
{
    public string title { get; set; }
    public int type { get; set; }

    //passives should just influence something that's already happened, like damage numbers, or returning from ooa,
    //and not so much do their own thing.

    //a unit can only have one passive equipped at a time? at least for now.
    //also, add gear passives.

    //Passive types:
    //these determine when the passive is called, as well as what args it has to work with.
    // -0: round end
    // -1: unit is attacked
    // -2: unit is attacking
    // -3: unit is healed
    // -4: unit is healing
    // -5: unit is put ooa
    // -6: unit puts an enemy ooa

    //different types of gear are limited to when their passives can be:
    // -unit's natural passive: any 
    // -weapon passive: 2, 4
    // -armour passive: 1, 3
    // -acc passive:    0, 5, 6



    //each passive will have its own exert function with its own arguments, depending on the time its called. (not implemented)
    public void exert(PlayerUnit unit)
    {

    }
    public void e_exert(Enemy unit)
    {

    }

    //example passives:
    // -take less damage from a certain element,
    // -deal more damage to a certain element
    // -survive one fatal hit per battle
    // -mana consumption is lower and mana moves deal lower damage
    // -mana consumption is higher and mana moves deal higher damage
    // -heal some hp/mp on round end
    // -if your move killed an enemy, the recovery time is halved
    // -when put ooa, x% chance to return at half health
    // restore hp/mp on kill

}
