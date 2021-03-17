using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//todo:
// in unity: create all the chapter buttons and link them to 'show_chapter(int)'
// -save last entry

public class QuestManager : MonoBehaviour
{
    [SerializeField] private TriMenu triMenu;

    private bool firstTime = true; //not game first, just local first.

    //from left to right
    [SerializeField] private GameObject chapterView; //all inactive by default
    [SerializeField] private GameObject questView;   //also all inactive by default
    [SerializeField] private Text questDescription;
    [SerializeField] private Text questTitleDescription;

    //quest array for each chapter
    private int lastViewedQuestIndex = 0; //index of last viewed quest among its own chapter. 
    private int lastViewedChapterIndex = 0; //index of chapter of last viewed quest. 

    //the length of each of these Quest[] should be the actual number of quests in their respective chapter.
    [SerializeField] private Quest[] prologueQuests;
    [SerializeField] private Quest[] chapterOneQuests;
    [SerializeField] private Quest[] chapterTwoQuests;
    //etc, then:
    private static Quest[][] chapterList = new Quest[3][]; //replace 3 with actual number of chapters

    public void setup()
    {
        chapterList[0] = prologueQuests;
        chapterList[1] = chapterOneQuests;
        chapterList[2] = chapterTwoQuests;
    }


    public void add_quest(Quest newQuest)
    {
        //inserts quest into waiting spot in chapter list.
        chapterList[newQuest.get_chapterFlag()][newQuest.get_flag()] = newQuest;

        //add notification: : "quest_title added"
        WorldManager.add_notification(newQuest.get_title() + "added");

    }
    public void progress_quest(Quest progQuest)
    {
        //progresses quest.
        if (chapterList[progQuest.get_chapterFlag()][progQuest.get_flag()] == null) return;
        chapterList[progQuest.get_chapterFlag()][progQuest.get_flag()].inc_progress();

        //add notification: "quest_title has progressed"
        WorldManager.add_notification(progQuest.get_title() + "has progressed");
    }
    public void end_quest()
    {
        //calls quest.finish()? on whatever the suspect quest is. then the quest does whatever is next.
        //individual method for each quest or idk what
    }

    public void close()
    {
        gameObject.SetActive(false);
        triMenu.gameObject.SetActive(true);
    }

    public void load_mainView(int chp)
    {

        for(int i = 0; i < chp; i++) //enable chapter buttons
        {
            chapterView.transform.GetChild(i).gameObject.SetActive(true);
        }

        show_lastViewed(); //last quest

    }

    public void show_lastViewed()
    {
        show_chapter(lastViewedChapterIndex);
        show_quest(lastViewedQuestIndex);
    }

    public void show_chapter(int chp) //called by chapter buttons, displays quests
    {
        //chp = chapter we're showing. fill the scroller with the quest buttons for the corresponding list.

        if (lastViewedChapterIndex == chp && !firstTime)
        {
            return;
        }
        firstTime = false;
        lastViewedChapterIndex = chp;
        questView.GetComponent<PopulateScroller>().populate_quests(chapterList[chp]);
    }

    public void show_quest(int id) //called by quest buttons, displays quest description
    {
        //id = index of the quest we're showing.
        lastViewedQuestIndex = id;

        //manage highlighting. selected quest in white, all others in blue
        questView.GetComponent<PopulateScroller>().quests_highlight(id);       

        //fills quest title
        //title + expiry
        questTitleDescription.text = chapterList[lastViewedChapterIndex][id].get_title() + "   | expires by chapter " + chapterList[lastViewedChapterIndex][id].get_chapterExpiry();

        //fills questDescription
        questDescription.text = "";
        for (int i = 0; i <= chapterList[lastViewedChapterIndex][id].get_progress(); i++)
        {
            if (i == chapterList[lastViewedChapterIndex][id].get_progress()) //last descr pair is blue, so it jumps out
            {
                questDescription.text += "<color=blue>" + chapterList[lastViewedChapterIndex][id].get_stepFlavour()[i] + "\n" + chapterList[lastViewedChapterIndex][id].get_stepRequirement()[i] + "</color>";
            }
            else //normal
            {
                questDescription.text += chapterList[lastViewedChapterIndex][id].get_stepFlavour()[i] + "\n" + chapterList[lastViewedChapterIndex][id].get_stepRequirement()[i] + "\n";
            }
            
        }
    }



}
