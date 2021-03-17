using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    //trimenu inventory manager

    [SerializeField] private TriMenu triMenu;

    //from left to right
    [SerializeField] private GameObject itemView;   //items in currently viewed category

    //first preview box
    [SerializeField] private Text itemDescription1;
    [SerializeField] private Text itemTitleDescription1;

    //second preview box
    [SerializeField] private Text itemDescription2;
    [SerializeField] private Text itemTitleDescription2;

    [SerializeField] private Inventory inven; //worldinventory, imported from worldmanager

    private int prevCatId;


    public void close()
    {
        gameObject.SetActive(false);
        triMenu.gameObject.SetActive(true);
    }

    public void load_mainView(Inventory wInven)
    {
        //loads up keyItems by default.

        inven = wInven;


        prevCatId = 0;
        //show_category(0);

    }

    public void show_category(int catId)
    {
        //called when pressing a category button.
        //catID:
        //0: armour
        //1: weapons
        //2: accessories
        //3: loot
        //4: key
        
        //if ( catId != prevCatId)
        //{
            //reset previews
        //}
        prevCatId = catId;
        
        switch (catId)
        {
            case 0:
                itemView.GetComponent<PopulateScroller>().populate_gear(inven.reserveArmour, -1);
                break;

            case 1:
                itemView.GetComponent<PopulateScroller>().populate_gear(inven.reserveWeapon, -1);
                break;

            case 2:
                itemView.GetComponent<PopulateScroller>().populate_gear(inven.reserveAccessory, -1);
                break;

            case 3:
                itemView.GetComponent<PopulateScroller>().populate_loot(inven.lootList);
                break;

            case 4:
                itemView.GetComponent<PopulateScroller>().populate_key(inven.keyItems);
                break;
        }
    }

    public void show_item(int id)
    {
        //can retrieve target item's category by category using prevCatId
        //and specific item by using id. it's the same as item's id.    


        //additional information to be added:
        // -found in: list dungeons. e.g. <<Cherespoir Jowler Den>>
        // -dropped by: if known, write, if unknown, write ?. e.g. jowler, jowler runt, and 1 unknown
        // -use in crafting recipes. e.g. Used in 11 discovered and 3 undiscovered ones crafting recipes
        // -infobits that characters may say.
        // -if you pay the researchers at cherespoir lab an extra fee, they will study and reveal some of this stuff.


        switch (prevCatId)
        {
            case 0:
                itemTitleDescription1.text = inven.reserveArmour[id].get_flavour();
                itemDescription1.text = inven.reserveArmour[id].get_name();
                break;

            case 1:
                itemTitleDescription1.text = inven.reserveWeapon[id].get_flavour();
                itemDescription1.text = inven.reserveWeapon[id].get_name();
                break;

            case 2:
                itemTitleDescription1.text = inven.reserveAccessory[id].get_flavour();
                itemDescription1.text = inven.reserveAccessory[id].get_name();
                break;

            case 3:
                itemTitleDescription1.text = inven.lootList[id].item.get_title();
                itemDescription1.text = inven.lootList[id].item.get_flavour();
                break;

            case 4:
                itemTitleDescription1.text = inven.keyItems[id].get_title();
                itemDescription1.text = inven.keyItems[id].get_flavour();
                break;
        }

    }

}
