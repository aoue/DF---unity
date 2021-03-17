using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entry : MonoBehaviour
{
    [SerializeField] private int flag; //unique identifier 
    [SerializeField] private int topicFlag; //what topic it appears in under the journal
    [SerializeField] private string teaser; //brief description of entry
    [SerializeField] private string title; //title of entry

    [SerializeField] private int progress; //how much info the player has in an entry. an entry continues to develop as more info is gathered.
    [SerializeField] private int steps; //max value of progress. when maxed, all info about this entry has been found.

    [SerializeField] private string[] stepFlavour; //array of strings with size steps. describes new info colourfully.


    public string get_title() { return title; }
    public int get_flag() { return flag; }
    public int get_topicFlag() { return topicFlag; }
    public int get_progress() { return progress; }
    public int get_steps() { return steps; }
    public string[] get_stepFlavour() { return stepFlavour; }
}
