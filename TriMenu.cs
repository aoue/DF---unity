using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//todo:
//bottom of party portraits: show hp bar and mp bar

public class TriMenu : MonoBehaviour
{
    //just has a bunch of ui stuff

    [SerializeField] private Image[] partyPortraits;
    [SerializeField] private Text buttonDescripton;
    [SerializeField] private Text dateAndTime;
    [SerializeField] private string[] partyUnitNames;

    public void open(PlayerUnit[] party, string DAT)
    {
        string[] names = new string[party.Length];
        for (int i = 0; i < party.Length; i++)
        {
            names[i] = party[i].get_nom();
        }
        partyUnitNames = names;

        gameObject.SetActive(true);

        //set party portraits

        dateAndTime.text = DAT;

        //correctly display the party unit's portraits. hide portraits if they would be unused.
        int count = 0;
        while( count < party.Length)
        {
            //partyPortraits[count] = party[count].get_portrait();
            partyPortraits[count].gameObject.SetActive(true);
            partyPortraits[count].sprite = party[count].get_moveSelectPortrait();
            count++;
        }
        while(count < partyPortraits.Length)
        {
            partyPortraits[count].gameObject.SetActive(false);
            count++;
        }

    }
    public void close()
    {
        gameObject.SetActive(false);
        WorldManager.isPaused = false;
    }

    public void hover_over_button(int index)
    {
        //when a button is being hovered over, set description text to its description.
        //when a button is unhovered, hide descriptionos
        buttonDescripton.text = "";

        switch (index)
        {
            case 0: //journal
                buttonDescripton.text = "View Journal";
                break;
            case 1: //pattern
                buttonDescripton.text = "Change Combat Pattern";
                break;
            case 2: //encyclopedia
                buttonDescripton.text = "View Encyclopedia";
                break;
            case 3: //inventory
                buttonDescripton.text = "View Inventory";
                break;
            case 4: //letters
                buttonDescripton.text = "View Letters";
                break;
            case 5: //game options
                buttonDescripton.text = "Adjust Game Options";
                break;
            case 6: //game settings
                buttonDescripton.text = "Adjust Settings";
                break;
            case 7: //party unit 0
                buttonDescripton.text = "Inspect - " + partyUnitNames[0];
                break;
            case 8: //party unit 1
                buttonDescripton.text = "Inspect - " + partyUnitNames[1];
                break;
            case 9: //party unit 2
                buttonDescripton.text = "Inspect - " + partyUnitNames[2];
                break;
            case 10: //party unit 3
                buttonDescripton.text = "Inspect - " + partyUnitNames[3];
                break;
        }
    }


}
