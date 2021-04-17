using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulateScroller : MonoBehaviour
{
    //this is the script on the scroller itself

    [SerializeField] private GameObject entryTemplate;// { get; set; }

    void Awake()
    {
        entryTemplate = gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
        entryTemplate.SetActive(false);
    }


    //clears the scroller
    public void clear()
    {
        GameObject tester = gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject; //<-- this is Content

        for (var i = tester.transform.childCount - 1; i >= 1; i--)
        {
            Destroy(tester.transform.GetChild(i).gameObject);
        }
    }

    //removes the highlight on all entries
    public void quests_highlight(int id)
    {
        GameObject tester = gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject; //<-- this is Content

        foreach (Transform child in tester.transform)
        {
            child.gameObject.GetComponent<Image>().color = new Color(110f / 255f, 180f / 255f, 255f / 255f); //unselected ones
        }
        tester.transform.GetChild(id+1).gameObject.GetComponent<Image>().color = new Color(255f / 255f, 255f / 255f, 255f / 255f); //the selected one
    }

    //populates the scroller with encyclopedia entries
    public void populate_entries(Entry[] contents)
    {
        GameObject tester = gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject; //<-- this is Content

        for (var i = tester.transform.childCount - 1; i >= 1; i--)
        {
            Destroy(tester.transform.GetChild(i).gameObject);
        }

        //populate the list
        for (int i = 0; i < contents.Length; i++)
        {
            //generates a new button
            GameObject entry = Instantiate(entryTemplate) as GameObject;
            //also, set an identifier to 'i'
            entry.SetActive(true);

            entry.GetComponent<ScrollerObject>().set_text(contents[i].get_title());
            entry.GetComponent<ScrollerObject>().id = i;
            entry.transform.SetParent(entryTemplate.transform.parent, false); //the false is to control its position.
        }
    }

    //populates the scroller with quests
    public void populate_quests(Quest[] contents)
    {
        GameObject tester = gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject; //<-- this is Content

        for (var i = tester.transform.childCount - 1; i >= 1; i--)
        {
            Destroy(tester.transform.GetChild(i).gameObject);
        }

        //populate the list
        for (int i = 0; i < contents.Length; i++)
        {
            if (contents[i] != null)
            {
                //generates a new button
                GameObject entry = Instantiate(entryTemplate) as GameObject;
                //also, set an identifier to 'i'
                entry.SetActive(true);

                entry.GetComponent<ScrollerObject>().set_text(contents[i].get_title());
                entry.GetComponent<ScrollerObject>().id = i;
                entry.transform.SetParent(entryTemplate.transform.parent, false); //the false is to control its position.
            }
        }
    }

    public void populate_outH_moves(PlayerMove[] contents, int unitMp)
    {
        //clear the list (we keep the first child, because that's the template and we need that)
        GameObject tester = gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject; //<-- this is Content

        for (var i = tester.transform.childCount - 1; i >= 1; i--)
        {
            Destroy(tester.transform.GetChild(i).gameObject);
        }

        //populate the list
        for (int i = 0; i < contents.Length; i++)
        {
            //generates a new button
            GameObject entry = Instantiate(entryTemplate) as GameObject;
            //also, set an identifier to 'i'
            entry.SetActive(true);

            entry.GetComponent<ScrollerObject>().set_text(contents[i].get_title());
            entry.GetComponent<ScrollerObject>().id = i;
            entry.transform.SetParent(entryTemplate.transform.parent, false); //the false is to control its position.

            if (contents[i].get_mpDrain() > unitMp)
            {
                entry.GetComponent<Button>().interactable = false; //<- i don't think this works
                //something that would work is changing the color of the box though.
            }
        }


    }

    //populates the scroller with moves
    public void populate_moves(PlayerMove[] contents)
    {
        //clear the list (we keep the first child, because that's the template and we need that)
        GameObject tester = gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject; //<-- this is Content
        
        for (var i = tester.transform.childCount - 1; i >= 1; i--)
        {
            Destroy(tester.transform.GetChild(i).gameObject);
        }

        //populate the list
        for (int i = 0; i < contents.Length; i++)
        {
            //generates a new button
            GameObject entry = Instantiate(entryTemplate) as GameObject;
            //also, set an identifier to 'i'
            entry.SetActive(true);

            entry.GetComponent<ScrollerObject>().set_text(contents[i].get_title());
            entry.GetComponent<ScrollerObject>().id = i;
            entry.transform.SetParent(entryTemplate.transform.parent, false); //the false is to control its position.
        }
    }

    //populates the scroller with passives
    public void populate_passives(Passive[] contents)
    {
        //clear the list (we keep the first child, because that's the template and we need that)
        GameObject tester = gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject; //<-- this is Content

        for (var i = tester.transform.childCount - 1; i >= 1; i--)
        {
            Destroy(tester.transform.GetChild(i).gameObject);
        }

        //populate the list
        for (int i = 0; i < contents.Length; i++)
        {
            //generates a new button
            GameObject entry = Instantiate(entryTemplate) as GameObject;
            //also, set an identifier to 'i'
            entry.SetActive(true);

            entry.GetComponent<ScrollerObject>().set_text(contents[i].title);
            entry.GetComponent<ScrollerObject>().id = i;
            entry.transform.SetParent(entryTemplate.transform.parent, false); //the false is to control its position.
        }
    }

    //populates the scroller with defend moves
    public void populate_defend(DefendMove[] contents)
    {
        //clear the list (we keep the first child, because that's the template and we need that)
        GameObject tester = gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject; //<-- this is Content

        for (var i = tester.transform.childCount - 1; i >= 1; i--)
        {
            Destroy(tester.transform.GetChild(i).gameObject);
        }

        //populate the list
        for (int i = 0; i < contents.Length; i++)
        {
            //generates a new button
            GameObject entry = Instantiate(entryTemplate) as GameObject;
            //also, set an identifier to 'i'
            entry.SetActive(true);

            entry.GetComponent<ScrollerObject>().set_text(contents[i].get_title());
            entry.GetComponent<ScrollerObject>().id = i;
            entry.transform.SetParent(entryTemplate.transform.parent, false); //the false is to control its position.
        }
    }

    //populates the scroller with gear
    public void populate_gear(Gear[] contents, int type)
    {
        //clear the list before refilling it
        GameObject tester = gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject; //<-- this is Content

        for (var i = tester.transform.childCount - 1; i >= 1; i--)
        {
            Destroy(tester.transform.GetChild(i).gameObject);
        }

        //populate the list
        for (int i = 0; i < contents.Length; i++)
        {
            if (contents[i] != null)
            {
                if (type == -1 || contents[i].get_type() == type)
                {
                    //generates a new button
                    GameObject entry = Instantiate(entryTemplate) as GameObject;
                    //also, set an identifier to 'i'
                    entry.SetActive(true);

                    entry.GetComponent<ScrollerObject>().set_text(contents[i].get_name());
                    entry.GetComponent<ScrollerObject>().id = i;
                    entry.transform.SetParent(entryTemplate.transform.parent, false); //the false is to control its position.
                }
            }
   
        }
    }
    //populates loot in inventory view.
    public void populate_loot( (Loot item, int quantity)[] contents )
    {
        //clear the list before refilling it
        GameObject tester = gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject; //<-- this is Content

        for (var i = tester.transform.childCount - 1; i >= 1; i--)
        {
            Destroy(tester.transform.GetChild(i).gameObject);
        }

        //populate the list
        for (int i = 0; i < contents.Length; i++)
        {

            if ( contents[i].quantity > 0)
            {
                //generates a new button
                GameObject entry = Instantiate(entryTemplate) as GameObject;
                entry.SetActive(true);

                entry.GetComponent<ScrollerObject>().set_text(contents[i].item.get_title() + " (Q:" + contents[i].quantity  + ")");
                entry.GetComponent<ScrollerObject>().id = i;
                entry.transform.SetParent(entryTemplate.transform.parent, false); //the false is to control its position.  
            }
            
                                
        }
    }
    //populates key items in inventory view
    public void populate_key(KeyItem[] contents)
    {
        //clear the list before refilling it
        GameObject tester = gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject; //<-- this is Content

        for (var i = tester.transform.childCount - 1; i >= 1; i--)
        {
            Destroy(tester.transform.GetChild(i).gameObject);
        }

        //populate the list
        for (int i = 0; i < contents.Length; i++)
        {
            if (contents[i] != null)
            {
                
                //generates a new button
                GameObject entry = Instantiate(entryTemplate) as GameObject;
                //also, set an identifier to 'i'
                entry.SetActive(true);

                entry.GetComponent<ScrollerObject>().set_text(contents[i].get_title());
                entry.GetComponent<ScrollerObject>().id = i;
                entry.transform.SetParent(entryTemplate.transform.parent, false); //the false is to control its position.
                
            }

        }
    }



}
