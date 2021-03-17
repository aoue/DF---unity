using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    //runs cutscenes that have been routed throgh worldmanager from cutscene holder.
    //runs cutscene using cutscene obj that is passed in, and any dialogue objs that are attached to that cutscene obj.
    
    //when dialogue runs:
    // - show background image (if applicable)
    //then, function like a regular dialogue.

    [SerializeField] private CutsceneLibrary cLi;
    [SerializeField] private GameObject cg;   
    
    public Cutscene get_cutscene(int csType, int cutsceneId)
    {
        return cLi.retrieve_cutscene(csType, cutsceneId);
    }

    public void startCutscene(Cutscene cs)
    {
        //called from worldmanager when the cutscene is first triggered.
        //all cutscene manager does is show the background.
        //then, passes back to worldmanager.

        if (cs.get_cg() == null)
            return;

        cg.gameObject.GetComponent<Image>().sprite = cs.get_cg();
        cg.gameObject.SetActive(true);        
    }
    public void exit_cutscene()
    {
        //called from worldmanager once the cutscene is over.
        //hides the background, then passes back to worldmanager.
        cg.gameObject.SetActive(false);
    }

}
