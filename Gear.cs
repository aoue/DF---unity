using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gear : MonoBehaviour
{
    [SerializeField] private string name; //weapon's name
    [SerializeField] private string flavour; //flavour text
    [SerializeField] private int flag; //0: unequippable, 1: armour, 2: weapon, 3: accessory
    [SerializeField] private int type; //who can equip the gear within its flag.
    [SerializeField] private int id; //unique id for just that one type of gear. every unique gear gets its own id.

    [SerializeField] private int element; //gear's affinity

    //is added to unit's stats. each piece of gear can affect all of these
    [SerializeField] private int hp;
    [SerializeField] private int mp;
    [SerializeField] private int physa;
    [SerializeField] private int physd;
    [SerializeField] private int maga;
    [SerializeField] private int magd;
    [SerializeField] private int dodge;
    [SerializeField] private int hit;
    [SerializeField] private int speed; //affects unit's delay.

    //passive
    [SerializeField] private bool hasPassive; //true: has passive, false: has no passive
    [SerializeField] private Passive passive;

    public string get_name() { return name; }
    public string get_flavour() { return flavour; }
    public int get_flag() { return flag; }
    public int get_type() { return type; } 
    public int get_id() { return id; }

    public int get_element() { return element; }

    public int get_hp() { return hp; }
    public int get_mp() { return mp; }
    public int get_physa() { return physa; }
    public int get_physd() { return physd; }
    public int get_maga() { return maga; }
    public int get_magd() { return magd; }
    public int get_dodge() { return dodge; }
    public int get_hit() { return hit; }

    public int get_speed() { return speed; }

    public bool get_hasPassive() { return hasPassive; }
    public Passive get_passive() { return passive; }
}
