using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollerObject : MonoBehaviour
{
    //this is the script on the entry in the scroller

    public int id { get; set; }
    [SerializeField] private PartyManager pManager; //party manager. only ever needs one of these three at a time.
    [SerializeField] private QuestManager qManager; //quest manager
    [SerializeField] private EncyclopediaManager eManager; //encyclopedia manager
    [SerializeField] private InventoryManager iManager; //inventory manager

    public void set_text(string newText)
    {
        gameObject.transform.GetChild(0).GetComponent<Text>().text = newText;
    }

    //on hover (also, use id and lastUnit to make sure you show the right thing): change colour

    public void call_manage_equipment()
    {
        //set so we know it's been selected.
        gameObject.GetComponent<Image>().color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
        pManager.manage_equipment(id);
    }

    public void call_object_preview()
    {
        if (pManager.get_focus() == 5 || pManager.get_focus() == 6)
        {
            //move preview
            pManager.load_preview("2 " + id);
        }
        else
        {
            //gear preview
            pManager.load_preview("3 " + id);
        }
        
    }

    public void call_manage_quest()
    {
        qManager.show_quest(id);
    }

    public void call_manage_entry()
    {
        eManager.show_entry(id);
    }

    public void call_manage_item()
    {
        iManager.show_item(id);
    }


}
