using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyManager : MonoBehaviour
{
    //this is the party manager. it allows the player to manage equipped gear and moves.

    //HOVERS
    //three functions:
    // -hover over move to see preview
    // -hover over gear to see preview
    // -hover over stat line to see description of stat, what its used for

    [SerializeField] private TriMenu triMenu;

    private int focus; //int used for disambiguation
    public int get_focus() { return focus; }
    private Inventory inven;
    public static PlayerUnit lastUnit { get; set; } //each time we show a unit, set lastUnit to that unit
    public static int lastUnitIndex { get; set; } //index of lastUnit in party
    private Image portrait;

    [SerializeField] private GameObject[] focusButtons;
    [SerializeField] private Text title;
    [SerializeField] private GameObject[] gearDisplay;
    [SerializeField] private GameObject[] specialMoveDisplay ;
    [SerializeField] private GameObject[] frontMoveDisplay ;
    [SerializeField] private GameObject[] backMoveDisplay ;
    [SerializeField] private Text statBlock; 
    [SerializeField] private Text previewText;
    [SerializeField] private GameObject scroller ;
    private bool allowFocusSwitch;

    //allows swapping to take place:
    private bool allowArmour;
    private bool allowWeapon;
    private bool allowAccessory;
    private bool allowPassive;
    private bool allowDefend;
    private bool allowFrontMoves;
    private bool allowBackMoves;

    private int incumbentId;

    void Awake()
    {
        //gameObject.blocksRaycasts = false;
        allowFocusSwitch = true;
    }

    // F U N C T I O N S
    public void close_party_viewer()
    {
        return_from_equipment();
        gameObject.SetActive(false);
        triMenu.gameObject.SetActive(true);
    }
    public void load_inventory(Inventory newInven)
    {
        inven = newInven;
    }
    public void load_party_unit(PlayerUnit unit)
    {
        previewText.text = "";
        lastUnit = unit;
        //fill in info:

        //portrait = lastUnit.portrait
        title.text = lastUnit.get_nom();

        //scrollview
        scroller.transform.GetChild(0).gameObject.GetComponent<Text>().text = ""; //by default

        //gear
        gearDisplay[0].transform.GetChild(0).gameObject.GetComponent<Text>().text = lastUnit.get_armour().get_name();
        gearDisplay[1].transform.GetChild(0).gameObject.GetComponent<Text>().text = lastUnit.get_weapon().get_name();
        gearDisplay[2].transform.GetChild(0).gameObject.GetComponent<Text>().text = lastUnit.get_acc().get_name();


        //passive and defend
        //specialMoveDisplay[0].transform.GetChild(0).gameObject.GetComponent<Text>().text = lastUnit.passiveEquipped.title;
        specialMoveDisplay[1].transform.GetChild(0).gameObject.GetComponent<Text>().text = lastUnit.get_defendEquipped().get_title();

        //frontmoves
        int counter = 0;
        while (counter < lastUnit.get_frontList().Length)
        {
            frontMoveDisplay[counter].SetActive(true);
            frontMoveDisplay[counter].transform.GetChild(0).gameObject.GetComponent<Text>().text = lastUnit.get_frontList()[counter].get_title();
            counter++;
        }
        while (counter < frontMoveDisplay.Length)
        {
            frontMoveDisplay[counter].SetActive(false);
            counter++;
        }

        //backmoves
        counter = 0;
        while (counter < lastUnit.get_backList().Length)
        {
            backMoveDisplay[counter].SetActive(true);
            backMoveDisplay[counter].transform.GetChild(0).gameObject.GetComponent<Text>().text = lastUnit.get_backList()[counter].get_title();
            counter++;
        }
        while (counter < backMoveDisplay.Length)
        {
            backMoveDisplay[counter].SetActive(false);
            counter++;
        }

        highlight_focusButtons();
        load_stat_block();

    }
    public void load_stat_block()
    {
        //add in legend showing what each colour comes from
        //add in colours to the text
        //base + arm (in blue) + wep (in red) + acc (in green)

        statBlock.text = "LVL" + " " + lastUnit.get_level() 
                       + "\nXP: " + lastUnit.xp + "/" + lastUnit.get_focus().nextLevelExp
                       + "\nHP: " + lastUnit.get_hp() + "/" + lastUnit.get_hpMax_actual() + " ( " + lastUnit.get_hpMax() + " + " + lastUnit.get_armour().get_hp() + " + " + lastUnit.get_weapon().get_hp() + " + " + lastUnit.get_acc().get_hp() + " )"
                       + "\nMP: " + lastUnit.get_mp() + "/" + lastUnit.get_mpMax_actual() + " ( " + lastUnit.get_mpMax() + " + " + lastUnit.get_armour().get_mp() + " + " + lastUnit.get_weapon().get_mp() + " + " + lastUnit.get_acc().get_mp() + " )"
                       + "\nP-ATK: " + lastUnit.get_physa_actual() + " ( " + lastUnit.get_physa() + " + " + lastUnit.get_armour().get_physa() + " + " + lastUnit.get_weapon().get_physa() + " + " + lastUnit.get_acc().get_physa() + " )"
                       + "\nP-DEF: " + lastUnit.get_physd_actual() + " ( " + lastUnit.get_physd() + " + " + lastUnit.get_armour().get_physd() + " + " + lastUnit.get_weapon().get_physd() + " + " + lastUnit.get_acc().get_physd() + " )"
                       + "\nM-ATK: " + lastUnit.get_maga_actual() + " ( " + lastUnit.get_maga() + " + " + lastUnit.get_armour().get_maga() + " + " + lastUnit.get_weapon().get_maga() + " + " + lastUnit.get_acc().get_maga() + " )"
                       + "\nM-DEF: " + lastUnit.get_magd_actual() + " ( " + lastUnit.get_magd() + " + " + lastUnit.get_armour().get_magd() + " + " + lastUnit.get_weapon().get_magd() + " + " + lastUnit.get_acc().get_magd() + " )"
                       + "\nHIT: " + lastUnit.get_hit_actual() + " ( " + lastUnit.get_hit() + " + " + lastUnit.get_armour().get_hit() + " + " + lastUnit.get_weapon().get_hit() + " + " + lastUnit.get_acc().get_hit() + " )"
                       + "\nDODGE: " + lastUnit.get_dodge_actual() + " ( " + lastUnit.get_dodge() + " + " + lastUnit.get_armour().get_dodge() + " + " + lastUnit.get_weapon().get_dodge() + " + " + lastUnit.get_acc().get_dodge() + " )"
                       + "\nSPD: " + lastUnit.get_speed_actual() + " ( " + lastUnit.get_speed() + " + " + lastUnit.get_armour().get_speed() + " + " + lastUnit.get_weapon().get_speed() + " + " + lastUnit.get_acc().get_speed() + " )";                      
    }
    public void load_preview(string inStr)
    {
        //for each equipped move and move in scroller, when hovered, put move's info in the preview area.
        //rank:
        // 0: front, get_frontlist()
        // 1: back, get_backlist()
        // 2: reserve, get_reservefrontlist / get_reservebacklist

        //convert inStr to: mark and type
        int type = System.Convert.ToInt32(inStr[0]) - 48;
        int mark = System.Convert.ToInt32(inStr[2]) - 48;
        PlayerMove[] rankList;
        Gear g = null;
        switch (type)
        {
            default:
                Debug.Log("failed to preview in partyManager. " + type + mark);
                return;
            case 0:
                rankList = lastUnit.get_frontList();
                break;
            case 1:
                rankList = lastUnit.get_backList();
                break;
            case 2:
                if (focus == 5)
                {
                    rankList = lastUnit.get_reserveFrontList().ToArray();
                }
                else if (focus == 6)
                {
                    rankList = lastUnit.get_reserveBackList().ToArray();
                }
                else
                {
                    Debug.Log("failed");
                    return;
                }
                break;
            case 3:
                if (focus == 0) //armour
                {
                    g = inven.reserveArmour[mark];
                }
                else if (focus == 1) //weapon
                {
                    g = inven.reserveWeapon[mark];
                }
                else //acc
                {
                    g = inven.reserveAccessory[mark];
                }
                rankList = lastUnit.get_frontList();
                break;
            case 4:
                //equipped armour
                g = lastUnit.get_armour();
                rankList = lastUnit.get_frontList();
                break;
            case 5:
                //equipped weapon
                g = lastUnit.get_weapon();
                rankList = lastUnit.get_frontList();
                break;
            case 6:
                //equipped accessory
                g = lastUnit.get_acc();
                rankList = lastUnit.get_frontList();
                break;
        }
        
        if (type < 3)
        {
            previewText.text = "<b>" + rankList[mark].get_title() + "</b>\nMP " + rankList[mark].get_mpDrain() + ", " + rankList[mark].get_descr0() + "\n"
                            + rankList[mark].get_descr1() + ", " + rankList[mark].get_descr2()
                            + "\n<i>" + rankList[mark].get_descr3() + "</i>";
        }
        else 
        {
            string buildpreview = "";
            buildpreview = "<b>" + g.get_name() + "</b>\n" + g.get_flavour();
            if (g.get_hp() != 0) buildpreview +="\nHP: " + g.get_hp();
            if (g.get_mp() != 0) buildpreview +="\nMP: " + g.get_mp();
            if (g.get_physa() != 0) buildpreview +="\nP-ATK: " + g.get_physa();
            if (g.get_physd() != 0) buildpreview +="\nP-DEF: " + g.get_physd();
            if (g.get_maga() != 0) buildpreview +="\nM-ATK: " + g.get_maga();
            if (g.get_magd() != 0) buildpreview +="\nM-DEF: " + g.get_magd();
            if (g.get_hit() != 0) buildpreview +="\nHIT: " + g.get_hit();
            if (g.get_dodge() != 0) buildpreview +="\nDODGE: " + g.get_dodge();
            if (g.get_speed() != 0) buildpreview += "\nSPD: " + g.get_speed();
            previewText.text = buildpreview;
        }
    }


    public void highlight_focusButtons()
    {
        //changes the colour to yellow - meaning select one
        foreach (GameObject obj in focusButtons)
        {
            obj.gameObject.GetComponent<Image>().color = new Color(251f / 255f, 243f / 255f, 113f / 255f);
            obj.gameObject.GetComponent<Button>().interactable = true;
        }
    }
    public void remove_highlight_focusButtons()
    {
        //changes the colour back to the previous blue
        foreach (GameObject obj in focusButtons)
        {
            obj.gameObject.GetComponent<Image>().color = new Color(218f / 255f, 248f / 255f, 255f / 255f);
            obj.gameObject.GetComponent<Button>().interactable = false;
        }
    }

    // moving on to the next unit:
    public void change_unit_view(int change)
    {
        //the prev button sets change = -1
        //the next button sets change = 1

        if(lastUnitIndex + change >= WorldManager.party.Length)
        {
            lastUnitIndex = 0;
            change = 0;
        }
        else
        {
            if(lastUnitIndex + change < 0)
            {
                lastUnitIndex = WorldManager.party.Length - 1;
                change = 0;
            }
        }

        lastUnitIndex += change;

        lastUnit = WorldManager.party[lastUnitIndex];
        return_from_equipment();
        load_party_unit(lastUnit);
        load_stat_block();
}


    //M A N A G I N G    G E A R 
    public void return_from_equipment()
    {
        highlight_focusButtons();
        allowFocusSwitch = true;
        allowArmour = false;
        allowWeapon = false;
        allowAccessory = false;
        allowPassive = false;
        allowDefend = false;
        allowFrontMoves = false;
        allowBackMoves = false;
        scroller.GetComponent<PopulateScroller>().clear();

        //also return colour of potentially highlighted slots to normal.
        foreach (GameObject obj in frontMoveDisplay)
        {
            obj.gameObject.GetComponent<Image>().color = new Color(218f / 255f, 248f / 255f, 255f / 255f);
        }
        foreach (GameObject obj in backMoveDisplay)
        {
            obj.gameObject.GetComponent<Image>().color = new Color(218f / 255f, 248f / 255f, 255f / 255f);
        }
        gearDisplay[0].gameObject.GetComponent<Image>().color = new Color(218f / 255f, 248f / 255f, 255f / 255f);
        gearDisplay[1].gameObject.GetComponent<Image>().color = new Color(218f / 255f, 248f / 255f, 255f / 255f);
        gearDisplay[2].gameObject.GetComponent<Image>().color = new Color(218f / 255f, 248f / 255f, 255f / 255f);
        specialMoveDisplay[0].gameObject.GetComponent<Image>().color = new Color(218f / 255f, 248f / 255f, 255f / 255f);
        specialMoveDisplay[1].gameObject.GetComponent<Image>().color = new Color(218f / 255f, 248f / 255f, 255f / 255f);

    }
    public void manage_equipment(int id)
    {
        //method:
        // -use focus to go to the next function, which is specific for a single type of this gear
        // -bring id to this function. Then, by using the id, we can select the right object in the list.       

        allowFocusSwitch = false;
        remove_highlight_focusButtons();

        incumbentId = id;

        //highlight the correct target slots
        switch (focus)
        {
            case 0:
                //armour
                gearDisplay[0].gameObject.GetComponent<Image>().color = new Color(110f / 255f, 180f / 255f, 255f / 255f);
                allowArmour = true;
                break;
            case 1:
                //weapon
                gearDisplay[1].gameObject.GetComponent<Image>().color = new Color(110f / 255f, 180f / 255f, 255f / 255f);
                allowWeapon = true;
                break;
            case 2:
                //accessory.
                gearDisplay[2].gameObject.GetComponent<Image>().color = new Color(110f / 255f, 180f / 255f, 255f / 255f);
                allowAccessory = true;
                break;
            case 3:
                //passive
                specialMoveDisplay[0].gameObject.GetComponent<Image>().color = new Color(110f / 255f, 180f / 255f, 255f / 255f);
                allowPassive = true;
                break;
            case 4:
                //defend
                specialMoveDisplay[1].gameObject.GetComponent<Image>().color = new Color(110f / 255f, 180f / 255f, 255f / 255f);
                allowDefend = true;
                break;
            case 5:
                //front move
                foreach(GameObject obj in frontMoveDisplay)
                {
                    obj.gameObject.GetComponent<Image>().color = new Color(110f / 255f, 180f / 255f, 255f / 255f);                    
                }
                allowFrontMoves = true;
                break;
            case 6:
                //back move
                foreach (GameObject obj in backMoveDisplay)
                {
                    obj.gameObject.GetComponent<Image>().color = new Color(110f / 255f, 180f / 255f, 255f / 255f);
                }
                allowBackMoves = true;
                break;
        }
    }
    public void perform_swap(int targetId, int focusId)
    {
        //targetId = index of move in targetlist that's going to be swapped out. (only useful for frontMoves,backMoves)
        //incumbedId = index of move in reserver list that's going to be swapped in

        //we already know the focus from earlier
        switch (focusId)
        {
            case 0:
                //armour
                swap_armour();
                break;
            case 1:
                //weapon
                swap_weapon();
                break;
            case 2:
                //accessory.
                swap_accessory();
                break;
            case 3:
                //passive
                swap_passive();
                break;
            case 4:
                //defend
                swap_defend();
                break;
            case 5:
                //front move                
                swap_frontmoves(targetId);
                break;
            case 6:
                //back move
                swap_backmoves(targetId);
                break;
        }
    }


    // ARMOUR
    public void focus_armour()
    {
        if (allowFocusSwitch)
        {
            focus = 0;
            scroller.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Armour";
            scroller.GetComponent<PopulateScroller>().populate_gear(inven.reserveArmour, lastUnit.get_equipTypes()[0]);
        }
    }
    public void swap_armour()
    {
        Debug.Log("swap_armour() called");
        if (allowArmour)
        {
            Debug.Log("swap_armour() called and allowArmour = true");
            //change what gear is equipped
            lastUnit.set_armour(inven.reserveArmour[incumbentId]);

            //now, undo the highlight
            gearDisplay[0].gameObject.GetComponent<Image>().color = new Color(218f / 255f, 248f / 255f, 255f / 255f);
            

            //now, update frontMoves
            load_party_unit(lastUnit);
            //and scrollview
            scroller.GetComponent<PopulateScroller>().populate_gear(inven.reserveArmour, lastUnit.get_equipTypes()[0]);

            //finally, reset
            allowArmour = false;
            allowFocusSwitch = true;
        }
    }
    // WEAPON
    public void focus_weapon()
    {
        if (allowFocusSwitch)
        {
            focus = 1;
            scroller.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Weapons";
            scroller.GetComponent<PopulateScroller>().populate_gear(inven.reserveWeapon, lastUnit.get_equipTypes()[1]);
        }
    }
    public void swap_weapon()
    {
        if (allowWeapon)
        {
            //then we need to swap the moves
            lastUnit.set_weapon(inven.reserveWeapon[incumbentId]);

            //now, undo the highlight
            gearDisplay[1].gameObject.GetComponent<Image>().color = new Color(218f / 255f, 248f / 255f, 255f / 255f);
            
            //now, update frontMoves
            load_party_unit(lastUnit);
            //and scrollview
            scroller.GetComponent<PopulateScroller>().populate_gear(inven.reserveWeapon, lastUnit.get_equipTypes()[1]);

            //finally, reset
            allowWeapon = false;
            allowFocusSwitch = true;
        }
    }
    // ACCESSORY
    public void focus_accessory()
    {
        if (allowFocusSwitch)
        {
            focus = 2;
            scroller.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Accessories";
            scroller.GetComponent<PopulateScroller>().populate_gear(inven.reserveAccessory, lastUnit.get_equipTypes()[2]);
        }
    }
    public void swap_accessory()
    {
        if (allowAccessory)
        {
            //then we need to swap the moves
            lastUnit.set_acc(inven.reserveAccessory[incumbentId]);

            //now, undo the highlight
            gearDisplay[2].gameObject.GetComponent<Image>().color = new Color(218f / 255f, 248f / 255f, 255f / 255f);

            //now, update frontMoves
            load_party_unit(lastUnit);
            //and scrollview
            scroller.GetComponent<PopulateScroller>().populate_gear(inven.reserveAccessory, lastUnit.get_equipTypes()[2]);

            //finally, reset
            allowAccessory = false;
            allowFocusSwitch = true;
        }
    }
    //PASSIVE
    public void focus_passive()
    {
        if (allowFocusSwitch)
        {
            focus = 3;
            scroller.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Passive";
            scroller.GetComponent<PopulateScroller>().populate_passives(lastUnit.get_reservePassive());
        }
    }
    public void swap_passive()
    {
        if (allowPassive)
        {
            //then we need to swap the moves
            Passive temp = lastUnit.get_passiveEquipped();
            lastUnit.set_passiveEquipped(lastUnit.get_reservePassive()[incumbentId]);
            lastUnit.get_reservePassive()[incumbentId] = temp;

            //now, undo the highlight
            specialMoveDisplay[0].gameObject.GetComponent<Image>().color = new Color(218f / 255f, 248f / 255f, 255f / 255f);

            //now, update frontMoves
            load_party_unit(lastUnit);
            //and scrollview
            scroller.GetComponent<PopulateScroller>().populate_passives(lastUnit.get_reservePassive());

            //finally, reset
            allowPassive = false;
            allowFocusSwitch = true;
        }
    }
    // DEFEND
    public void focus_defend()
    {
        if (allowFocusSwitch)
        {
            focus = 4;
            scroller.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Defend";
            scroller.GetComponent<PopulateScroller>().populate_defend(lastUnit.get_reserveDefend());
        }
    }
    public void swap_defend()
    {
        if (allowDefend)
        {
            //then we need to swap the moves
            DefendMove temp = lastUnit.get_defendEquipped();
            lastUnit.set_defendEquipped(lastUnit.get_reserveDefend()[incumbentId]);
            lastUnit.get_reserveDefend()[incumbentId] = temp;

            //now, undo the highlight
            specialMoveDisplay[1].gameObject.GetComponent<Image>().color = new Color(218f / 255f, 248f / 255f, 255f / 255f);

            //now, update frontMoves
            load_party_unit(lastUnit);
            //and scrollview
            scroller.GetComponent<PopulateScroller>().populate_defend(lastUnit.get_reserveDefend());

            //finally, reset
            allowDefend = false;
            allowFocusSwitch = true;
        }
    }
    // FRONT MOVES
    public void focus_frontmoves()
    {
        if (allowFocusSwitch)
        {
            focus = 5;
            scroller.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Front Moves";
            scroller.GetComponent<PopulateScroller>().populate_moves(lastUnit.get_reserveFrontList().ToArray());
        }
    }
    public void swap_frontmoves(int targetId)
    {
        if (allowFrontMoves)
        {
            //check if the spot in lastUnit.frontList is null
            if (lastUnit.get_frontList()[targetId] == null) //== noneMove?
            {
                //then just insert the move
                lastUnit.get_frontList()[targetId] = lastUnit.get_reserveFrontList()[incumbentId];
                //remove lastUnit.reserveFrontList[incumbentId]
            }
            else
            {
                //then we need to swap the moves
                PlayerMove temp = lastUnit.get_frontList()[targetId];
                lastUnit.get_frontList()[targetId] = lastUnit.get_reserveFrontList()[incumbentId];
                lastUnit.get_reserveFrontList()[incumbentId] = temp;
            }

            //now, undo the highlight
            foreach (GameObject obj in frontMoveDisplay)
            {
                obj.gameObject.GetComponent<Image>().color = new Color(218f / 255f, 248f / 255f, 255f / 255f);
            }

            //now, update frontMoves
            load_party_unit(lastUnit);
            //and scrollview
            scroller.GetComponent<PopulateScroller>().populate_moves(lastUnit.get_reserveFrontList().ToArray());

            //finally, reset
            allowFrontMoves = false;
            allowFocusSwitch = true;
        }
    }
    // BACK MOVES
    public void focus_backmoves()
    {
        if (allowFocusSwitch)
        {
            focus = 6;
            scroller.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Back Moves";
            scroller.GetComponent<PopulateScroller>().populate_moves(lastUnit.get_reserveBackList().ToArray());
        }
    }
    public void swap_backmoves(int targetId)
    {
        if (allowBackMoves)
        {
            //check if the spot in lastUnit.frontList is null
            if (lastUnit.get_backList()[targetId] == null) //== noneMove?
            {
                //then just insert the move
                lastUnit.get_backList()[targetId] = lastUnit.get_reserveBackList()[incumbentId];
                //remove lastUnit.reserveFrontList[incumbentId]
            }
            else
            {
                //then we need to swap the moves
                PlayerMove temp = lastUnit.get_backList()[targetId];
                lastUnit.get_backList()[targetId] = lastUnit.get_reserveBackList()[incumbentId];
                lastUnit.get_reserveBackList()[incumbentId] = temp;
            }

            //now, undo the highlight
            foreach (GameObject obj in backMoveDisplay)
            {
                obj.gameObject.GetComponent<Image>().color = new Color(218f / 255f, 248f / 255f, 255f / 255f);
            }

            //now, update frontMoves
            load_party_unit(lastUnit);
            //and scrollview
            scroller.GetComponent<PopulateScroller>().populate_moves(lastUnit.get_reserveBackList().ToArray());

            //finally, reset
            allowBackMoves = false;
            allowFocusSwitch = true;
        }
    }












}
