using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : MonoBehaviour
{
    //items dropped from monsters or found by seaching. either way, attached to another obj. like a Chest or an EnemyMob
    //that cannot be equipped. measured in quantities.
    //can be sold and used in crafting and used for quests too probably

    //what a loot object needs:
    // - string title   (name of loot.)
    // - string flavour (short descr of loot)
    // - int value      (manipulated to become sell price)
    // - int id         (serves as index in inventory lists)
    //eventually:
    // -crafting id. like what fields the loot can fill in crafting.

    [SerializeField] private string title;
    [SerializeField] private string flavour;
    [SerializeField] private int value;
    [SerializeField] private int id;
    

    public string get_title() { return title; }
    public string get_flavour() { return flavour; }
    public int get_value() { return value; }
    public int get_id() { return id; }
}