using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExamineManager : MonoBehaviour
{
    //what it does:
    //is called up by worldmanager to show things in a box in the center of the screen while the game is paused.
    //occasions when it is used:
    // -when examining something, e.g. a sign.
    // -chest opens
    // -quest pickup, advancement, finish

    //click checkmark to close and return to the game. (unpauses)

    //[SerializeField] private GameObject examineBox;
    [SerializeField] private Text topText;
    [SerializeField] private Text bottomText;
    [SerializeField] private Button dismissButton;
    [SerializeField] private GameObject holder; //used to hide/show everything together

    //type: corresponds to a place in the below case select.
    //type legend:
    //0: point of interest examined.
    //1: chest opened, gear.
    //2: chest opened, loot.
    //2: quest pickup.
    //3: quest finished.
    //4: quest advancement.

    //formatting functions. each assembles the messages string[] and passes in to show_box()
    public void format_poi(string[] twoStrings)
    {
        //point of interest.
    }
    public void format_chest_gear(Gear g)
    {
        //on chest open and gear found.
        string[] msg = new string[2];
        msg[0] = g.get_name();
        //msg[1] = g.get_flavour();
        msg[1] = "<b>" + g.get_name() + "</b>\n" + g.get_flavour();
        if (g.get_hp() != 0) msg[1] += "\nHP: " + g.get_hp();
        if (g.get_mp() != 0) msg[1] += "\nMP: " + g.get_mp();
        if (g.get_physa() != 0) msg[1] += "\nP-ATK: " + g.get_physa();
        if (g.get_physd() != 0) msg[1] += "\nP-DEF: " + g.get_physd();
        if (g.get_maga() != 0) msg[1] += "\nM-ATK: " + g.get_maga();
        if (g.get_magd() != 0) msg[1] += "\nM-DEF: " + g.get_magd();
        if (g.get_hit() != 0) msg[1] += "\nHIT: " + g.get_hit();
        if (g.get_dodge() != 0) msg[1] += "\nDODGE: " + g.get_dodge();
        if (g.get_speed() != 0) msg[1] += "\nSPD: " + g.get_speed();
        show_box(msg);
    }
    public void format_chest_loot(Loot l)
    {
        //on chest open and loot found.
        string[] msg = new string[2];
        msg[0] = l.get_title();
        msg[1] = l.get_flavour();
        show_box(msg);
    }
    public void format_quest_pickup(Quest q)
    {
        //quest picked up.
        //take pickup string
        string[] msg = new string[2];
        msg[0] = q.get_title() + " acquired!";
        msg[1] = q.get_stepFlavour()[q.get_progress()];
        show_box(msg);
    }
    public void format_quest_progress(Quest q)
    {
        //quest progressed.
        //take most recent step
        string[] msg = new string[2];
        msg[0] = q.get_title() + " progressed!";
        msg[1] = q.get_stepFlavour()[q.get_progress()];
        show_box(msg);
    }

    void show_box(string[] msg)
    {
        topText.text = msg[0];
        bottomText.text = msg[1];
        //show box
        WorldManager.isPaused = true;
        holder.gameObject.SetActive(true);
    }

    public void dismiss()
    {
        //called when the checkmark button is pressed.
        //hides the examinemanager and unpauses the game through worldmanager.
        holder.gameObject.SetActive(false);
        WorldManager.isPaused = false;
    }

}
