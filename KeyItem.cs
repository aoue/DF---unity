using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyItem : MonoBehaviour
{

    //key items are used for opening doors, completing quests, etc
    //used to keep track of player's activities, really. have they done this yet/ever?

    //each key item has a unique ID that corresponds to a unique event.

    [SerializeField] private string title;
    [SerializeField] private string flavour;
    [SerializeField] private int id;


    public string get_title() { return title; }
    public string get_flavour() { return flavour; }
    public int get_id() { return id; }



}
