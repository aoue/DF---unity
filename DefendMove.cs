using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefendMove : MonoBehaviour
{
    [SerializeField] private string title;
    [SerializeField] private string descr0; 
    [SerializeField] private string descr1; 
    [SerializeField] private string descr2; 
    [SerializeField] private string descr3;

    [SerializeField] private int rec_delay;


    public string get_title() { return title; }
    public string get_descr0() { return descr0; }
    public string get_descr1() { return descr1; }
    public string get_descr2() { return descr2; }
    public string get_descr3() { return descr3; }

    public int get_recDelay() { return rec_delay; }



    //exactly what 'exert' does will either have to be:
    // -defined in its itself in a unique function for each defend move. there won't be too many, so not a terrible idea.
    // -given by more variables given in here by this defendMove... messy + inflexible. more organised once working though.

    //is called when the player hits the defend button
    public virtual void exert(PlayerUnit unit)
    {
        //does a variety of fun stuff, who knows what yet.

        //probably modifies stats in some interesting way, DEFup, ATKup, guard, berserk... anything 
        //each unit has access to different defend moves, further, they can equip only one at a time.
        
        //even though it's called a defense move, you can make it do anything. could be a berserk move, or a 
        //channeling move or whatever.

        unit.state = unitState.DEFENDING;

        //also, enter recovery type thing.
        Color color = new Color(211f / 255f, 211f / 255f, 211f / 255f); 
        unit.stats.update_color(color); //make it gray

        unit.stats.update_moveNamePreview("Defending");
        //unit.stats.update_delayText("DEF");
        unit.stats.update_delayText("-/-");
        unit.stats.update_slider(100, true); //fills the delay bar. it won't move again until the unit exits defend.

        unit.status.enter_defend();
    }


}
