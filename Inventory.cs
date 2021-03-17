using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory
{
    //inventory works like this:
    // -there's no gear quantity, once you have, you have as many copies as you can use.
    // ^^so when we remove gear, don't need to do anything with it.

    //Tuple<Loot, int>[] lootList { get; set; } //Type of loot, quantity of loot
    public (Loot item, int quantity)[] lootList { get; set; } //meaning there is 100 types of loot

    //a bunch of smaller lists in here
    //each list can only have 100 gear items in it at a time.
    public Gear[] reserveArmour { get; set; } 
    public Gear[] reserveWeapon { get; set; }
    public Gear[] reserveAccessory { get; set; }
    public KeyItem[] keyItems { get; set; }
    public int money { get; set; }
    //  

    public void build_starting_inventory(Gear example)
    {
        //setting 100 as the size of all arrays. their size will be the exact number of all different item types.
        reserveArmour = new Gear[100];
        reserveWeapon = new Gear[100];
        reserveAccessory = new Gear[100];
        lootList = new (Loot, int)[100];

        money = 0;

        //give each gear list the empty gear object
        Debug.Log("building inventory");

        reserveArmour[0] = example;
        reserveArmour[1] = example;

        reserveWeapon[0] = example;
        reserveWeapon[1] = example;

        reserveAccessory[0] = example;
        reserveAccessory[1] = example;


        //reserveArmour[1] = GameObject.Find("Empty Gear").GetComponent<Gear>();
        //reserveArmour[2] = GameObject.Find("Empty Gear").GetComponent<Gear>();
        //reserveWeapon[0] = GameObject.Find("Empty Gear").GetComponent<Gear>();
        //reserveAccessory[0] = GameObject.Find("Empty Gear").GetComponent<Gear>();
    }

    public void add_gear(Gear newGear)
    {
        switch (newGear.get_flag())
        {
            default: //armour
                reserveArmour[newGear.get_id()] = newGear;
                break;
            case 2: //weapon
                reserveWeapon[newGear.get_id()] = newGear;
                break;
            case 3: //accessory
                reserveAccessory[newGear.get_id()] = newGear;
                break;
        }
    }

    public void add_loot(Loot newLoot)
    {
        //id-based system. max 99 of any one item being held in inventory at once.
        //check newLoot's id, and inc lootList's corresponding element.
        if ( lootList[newLoot.get_id()].item == null)
        {
            lootList[newLoot.get_id()].item = newLoot;
        }

        lootList[newLoot.get_id()].quantity = Math.Min(99, lootList[newLoot.get_id()].quantity + 1);

    }
    public void remove_loot(Loot newLoot)
    {
        lootList[newLoot.get_id()].quantity = Math.Max(0, lootList[newLoot.get_id()].quantity - 1);
    }

}
