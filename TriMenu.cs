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

    public void open(PlayerUnit[] party, string DAT)
    {
        gameObject.SetActive(true);

        dateAndTime.text = DAT;

        //correctly display the party unit's portraits. hide portraits if they would be unused.
        int count = 0;
        while( count < party.Length)
        {
            //partyPortraits[count] = party[count].get_portrait();
            partyPortraits[count].gameObject.SetActive(true);
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
        }
    

    }

    public void hover_over_partyMember(int index)
    {
        //changes party unit's image when being hovered over

        //also adds some char info too? idk
    }

}
