using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest : MonoBehaviour
{
    [SerializeField] private int chapterExpiry; //what chapter the quest expires at the end of
    [SerializeField] private int flag; //unique identifier 
    [SerializeField] private int chapterFlag; //what chapter it appears in under the journal
    [SerializeField] private string teaser; //brief description of quest
    [SerializeField] private string title; //title of quest

    [SerializeField] private int progress; //marks at what point the player is in the quest
    [SerializeField] private int steps; //max value of progress. when progress == steps, quest is over. 

    [SerializeField] private string[] stepFlavour; //array of strings with size steps. describes new info colourfully.
    [SerializeField] private string[] stepRequirement; //array of strings with size steps. describes next action to be taken

    [SerializeField] private string turnIn;
    
    void Awake()
    {
        progress = 0;
    }

    public string get_title() { return title; }
    public int get_flag() { return flag; }
    public int get_chapterFlag() { return chapterFlag; }
    public int get_chapterExpiry() { return chapterExpiry; }
    public int get_progress() { return progress; }
    public int get_steps() { return steps; }
    public string[] get_stepFlavour() { return stepFlavour; }
    public string[] get_stepRequirement() { return stepRequirement; }
    public string get_turnIn() { return turnIn; }

    public void inc_progress() { progress += 1; }
}
