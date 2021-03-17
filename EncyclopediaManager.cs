using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//todo:
// -save last entry

public class EncyclopediaManager : MonoBehaviour
{
    [SerializeField] private TriMenu triMenu;

    private bool firstTime = true; //not game first, just local first.

    //from left to right
    [SerializeField] private GameObject topicView; //all inactive by default
    [SerializeField] private GameObject entryView;   //also all inactive by default
    [SerializeField] private Text entryDescription;
    [SerializeField] private Text entryTitleDescription;

    //quest array for each chapter
    private int lastViewedEntryIndex = 0; //index of last viewed quest among its own chapter. 
    private int lastViewedTopicIndex = 0; //index of chapter of last viewed quest. 
    [SerializeField] private Entry[] historyEntries;
    [SerializeField] private Entry[] organizationEntries;
    [SerializeField] private Entry[] technologyEntries;
    [SerializeField] private Entry[] placesEntries;
    [SerializeField] private Entry[] peopleEntries;
    //etc, then:
    private Entry[][] topicList = new Entry[5][]; //replace 3 with actual number of chapters

    void Awake()
    {
        topicList[0] = historyEntries;
        topicList[1] = organizationEntries;
        topicList[2] = technologyEntries;
        topicList[3] = placesEntries;
        topicList[4] = peopleEntries;
        //etc
    }

    public void add_entry(Entry newEntry)
    {
        topicList[newEntry.get_topicFlag()][newEntry.get_flag()] = newEntry;
    }
    public void close()
    {
        gameObject.SetActive(false);
        triMenu.gameObject.SetActive(true);
    }

    public void load_mainView()
    {
        show_lastViewed(); //last quest

    }

    public void show_lastViewed()
    {
        show_topic(lastViewedTopicIndex);
        show_entry(lastViewedEntryIndex);
    }

    public void show_topic(int chp) //called by chapter buttons, displays quests
    {
        //chp = chapter we're showing. fill the scroller with the quest buttons for the corresponding list.

        if (lastViewedTopicIndex == chp && !firstTime)
        {
            return;
        }
        firstTime = false;
        lastViewedTopicIndex = chp;
        entryView.GetComponent<PopulateScroller>().populate_entries(topicList[chp]);
    }

    public void show_entry(int id) //called by quest buttons, displays quest description
    {
        //id = index of the quest we're showing.
        lastViewedEntryIndex = id;
        //manage highlighting. selected quest in white, all others in blue
        entryView.GetComponent<PopulateScroller>().quests_highlight(id);

        //fills quest title
        //title + expiry
        entryTitleDescription.text = topicList[lastViewedTopicIndex][id].get_title();

        //fills questDescription
        entryDescription.text = "";
        for (int i = 0; i < topicList[lastViewedTopicIndex][id].get_progress(); i++)
        {
            entryDescription.text += topicList[lastViewedTopicIndex][id].get_stepFlavour()[i] + "\n";
        }

        //if progress < steps: difference info to go!
        if(topicList[lastViewedTopicIndex][id].get_progress() < topicList[lastViewedTopicIndex][id].get_steps())
        {
            entryDescription.text += "<color=blue>" + (topicList[lastViewedTopicIndex][id].get_steps() - topicList[lastViewedTopicIndex][id].get_progress()) + " infobits left. </color>";
        }
        else
        {
            entryDescription.text += "<color=blue>Entry Completed</color>";
        }

    }
}
