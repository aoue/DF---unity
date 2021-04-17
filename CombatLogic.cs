using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

//HELLO; ToDo:
//ctrl+m, ctrl+o to collapse functions

//health bar effects
// -slide when value changes
// -when at max health: special colour/image
// -when above max health: special colour/image

//unit stats on the bottom:
// -containers so the things look better and it's clear where things separate

//criticals:
// -each move has a critical chance, which is added to the unit's critical chance (innate + gear).
// -write down this total chance as a sum in the description part of each move. most moves have a base chance of 0.
// -when a critical is landed, show unit's cut-in

//special target highlights:
//for any shapes that cannot be represented as a rectangle.
//five steps to implement:
// 0. add an int code (called type by the maps and moves) and add it to the index in PlayerMap
// 1. in PlayerMap and EnemyMap, create a case in allowHover()
// 2. in PlayerMap and EnemyMap, implement a case in highlight_helper() and highlight_helper_off()
// 3. in PlayerTile and EnemyTile, implement a case check in mouseUp.
// 4. in BattleBrain, implement a case for finding targets.

//animate unit changing states. different sprites for preparing, recovering, defending, acting, and ooa.

//animate unit moving between tiles. lerp or something? idk.

//patterns: 5/3, 3/5, 4/4
// friday: 4/4 (mc)
// yve: 5/3 (peyton)
// mad: 3/5 (mueler)
// anton: 4/4? 


public enum battleState { START, PROCEED, PLAYERTURN, ENEMYTURN, WON, LOST, WAITING, QUICKMENU, REPOSITION } 

public class CombatLogic : MonoBehaviour
{
    //exp and loot
    private int runningExp; //holds exp to retrieve at battle end.
    private List<Loot> runningLoot = new List<Loot>(); //holds loot to be retrieved at battle end
    [SerializeField] private Text lootSummary; //text that displays all the loot gained at the end of battle

    //help stuff. bools default false
    private bool allowHelp; //when true, pressing the help button will open the help page.
    private bool helpShowing;
    [SerializeField] private GameObject helpImage;

    //running and ending the battle
    [SerializeField] private BattleBrain brain;// = new BattleBrain();
    private int tickLimit;
    private battleState state;
    private int minDelay; //what the lowest delay a unit starts with is.
    private bool showingPostScreen;

    //play by play
    [SerializeField] private GameObject playByPlay;
    [SerializeField] private Text playByPlayText;

    //affinity multiplier text
    bool showingMults = false;

    //stat preview window
    [SerializeField] private GameObject statPreviewWindow; //shows data on targets on mouse over
    public bool allowPreview { get; set; } //if true, allows the player to hover over units on the field and see a preview of their stats
    [SerializeField] private GameObject actingPreviewWindow; //shows data on currentUnit when unit is choosing a move. shows name, hp, and mp.

    //unit acting portrait
    [SerializeField] private GameObject actingUnitImage; //used to show image for acting unit. dimensions: 300w, 750h
    private bool isPortraitLocked; //when true, portrait cannot be shown.


    //universal tick slide obj
    [SerializeField] private Slider tickSlider;

    //reposition
    [SerializeField] private Slider repositionBar; 
    [SerializeField] private Text repositionText; 
    private PlayerUnit roundUnit; //unit selected during round stuff. 
    private int repositionValue; //full at 100 

    //level up
    [SerializeField] private GameObject levelUpCanvas; //has levelUpSlots children
    [SerializeField] private GameObject[] levelUpSlots; //each has a text child, and the first 4 have a portrait.

    //move display
    [SerializeField] private Button[] frontMoves;
    [SerializeField] private Button[] backMoves;
    [SerializeField] private GameObject moveDisplay;
    private Text moveDisplayText;
    private Text moveDescrBox;
    private bool allowButtonHover;
    private bool inTileSelectPhase;
    private Button defendButton;  

    //quick menu
    [SerializeField] private GameObject quickMenu;
    [SerializeField] private Button quickMenuCloseButton;
    [SerializeField] private Button[] quickMenuUnitButtons;
    [SerializeField] private Button quickMenuCancel;
    [SerializeField] private Button quickMenuDefend;
    [SerializeField] private Button quickMenuReposition;
    [SerializeField] private Text[] quickMenuUnitTexts;
    private PlayerUnit quickMenuSelected; //the unit selected in the quick menu
    private bool quickMenuEnabled; //will a right click bring up the quickmenu?
    private bool quickMenuShowing; //is the quick menu on screen

    public PlayerUnit[] reservePl { get; set; }
    public PlayerUnit[] pl { get; set; }
    public Enemy[] el { get; set; }
    [SerializeField] private PlayerMap pMap;
    [SerializeField] private EnemyMap eMap;
    [SerializeField] private UnitStatsUI stats; //will be used displays game stats, not individual unit stats. things like current tick, battle effects, etc
    private int currentTick; //keeps track of tick in round. end of round is 150 ticks, just for now.

    private PlayerUnit currentUnit; //the unit that is currently choosing an action.
    private PlayerMove[] rankList; //used with buttons to correctly use either a unit's frontList or backList
    private int moveID; //the pos in movelist of the move currently chosen.

    private Queue<IEnumerator> exertQueue = new Queue<IEnumerator>(); 
    private Queue<IEnumerator> chooseQueue = new Queue<IEnumerator>();
    private Queue<IEnumerator> orderQueue = new Queue<IEnumerator>();
    private int ordersLeft; //how many orders are left to do this round.
    private int attacksLeft; //how many attacks are left to watch this round.
    private bool doneOrdering; //for keeping the order tab open for each individual unit

    public BattleBrain get_brain() { return brain; }
    public PlayerMap get_pMap() { return pMap; }
    public EnemyMap get_eMap() { return eMap; }

    IEnumerator CoroutineCoordinator()
    {
        while ( exertQueue.Count > 0)
        {
            yield return StartCoroutine(exertQueue.Dequeue());
        }

        while ( chooseQueue.Count > 0)
        {
            yield return StartCoroutine(chooseQueue.Dequeue());
        }

        yield return null;
    }

    //START AND RESUME
    void Awake()
    {
        //initialize combat and set up all the units

        //link battlebrain and this
        brain.cLog = this;

        //move interface setup
        defendButton = moveDisplay.transform.GetChild(0).gameObject.GetComponent<Button>();
        moveDescrBox = moveDisplay.transform.GetChild(1).gameObject.GetComponent<Text>();

        currentTick = 0;
        SetupBattle();
    }
    void Update() //it was fixed update before. change it back if problems. we put it like this for quickmenu 
    {
        //open quickmenu on 'right click'
        if (Input.GetMouseButtonDown(1) && quickMenuEnabled && state == battleState.PROCEED)
        {
            //Debug.Log("right clicked");
                        
            state = battleState.QUICKMENU;
            quickMenu_open();
            
        }

        if (Input.GetMouseButtonDown(1) && !quickMenuShowing && inTileSelectPhase)
        {
            cancel_move_selection();
        }

        //end battle when on postbattlescreen on 'left click'
        if (Input.GetMouseButtonDown(0) && showingPostScreen)
        {
            WorldManager.return_from_battle(); //return to world
        }

        //switch enemy healthbars to affinity multipliers on 'spacebar'
        if ( allowButtonHover && Input.GetKeyDown(KeyCode.Space))
        {
            //Debug.Log("update() aff multipler condition entered");
            show_affMultipliers();
        }


        //continue tick counting after we've been paused. automatic.
        if (!quickMenuShowing && state == battleState.WAITING && ordersLeft == 0 && attacksLeft == 0)
        {
            state = battleState.PROCEED;
            StopAllCoroutines();
            StartCoroutine(Combat_round());
        }

        
    }
    void SetupBattle()
    {
        currentTick = 0;
        repositionValue = 0;
        quickMenuEnabled = false;
        quickMenuShowing = false;
        showingPostScreen = false;

        //reserve = partyReserve //*not yet implemented*
        int smallestFullDelay = 100;
        tickLimit = WorldManager.battleTickLimit;

        el = new Enemy[WorldManager.enemyParty.Length];
        pl = new PlayerUnit[WorldManager.party.Length];


        for (int i = 0; i < pl.Length; i++)
        {           
            //pl[i] = Instantiate(WorldManager.party[i]);
            pl[i] = WorldManager.party[i];
            pl[i].GetComponent<SpriteRenderer>().enabled = true;


            //attach stat displayer and enable it
            stats.get_pl()[i].gameObject.SetActive(true);
            pl[i].stats = stats.get_pl()[i];


            //pl[i].GetComponentInParent<SpriteRenderer>().enabled = true;
            pMap.place_unit(pl[i]);
            pl[i].generate_start_delay();
            //pl[i].update_stats();

            if(pl[i].fullDelay < smallestFullDelay)
            {
                smallestFullDelay = pl[i].fullDelay;
            }
        }

        for (int i = 0; i < el.Length; i++)
        {
            el[i] = Instantiate(WorldManager.enemyParty[i]);

            //set position of el[i], stored in WorldManager.Edeployer
            el[i].set_x(WorldManager.Edeployer[i].xLoc);
            el[i].set_y(WorldManager.Edeployer[i].yLoc);


            stats.get_el()[i].gameObject.SetActive(true);
            el[i].stats = stats.get_el()[i];

            eMap.place_enemy(el[i]);
            el[i].generate_start_delay();
            //el[i].update_stats();
            if (el[i].fullDelay < smallestFullDelay)
            {
                smallestFullDelay = el[i].fullDelay;
            }
        }



        //now: reduction
        //this will chop off all the extra fullDelay from every unit until one unit has [minDelay] delay.
        //use minDelay and smallestFullDelay to calc reduction
        //then, reduce the fullDelay of every unit by reduction

        minDelay = 5; //minDelay should be configurable in the settings, eh? make it a static in WorldManager if yes.
        if (smallestFullDelay > minDelay)
        {
            int reduction = smallestFullDelay - minDelay;
            //now, we
            foreach(PlayerUnit unit in pl)
            {
                unit.fullDelay -= reduction;
                unit.update_stats();
                unit.stats.update_slider(unit.fullDelay, false);
            }
            foreach(Enemy unit in el)
            {
                unit.fullDelay -= reduction;
                unit.update_stats();
                unit.stats.update_slider(unit.fullDelay, false);
            }
        }
        else //we still have to update stats sometime...
        {
            foreach (PlayerUnit unit in pl)
            {
                unit.update_stats();
                unit.stats.update_slider(unit.fullDelay, false);
            }
            foreach (Enemy unit in el)
            {
                unit.update_stats();
                unit.stats.update_slider(unit.fullDelay, false);

            }
        }

        state = battleState.START;
        StartCoroutine(Combat_round());
    }  
    IEnumerator Combat_round()
    {

        if (state == battleState.START)
        {
            yield return new WaitForSeconds(1.0f); //pause before battle begins
            state = battleState.PROCEED;
        }


        while (state == battleState.PROCEED) //after which a new round will start and current tick will be set to 0. 100 ticks??
        {
            //StartCoroutine(Combat_tick());
            
            yield return new WaitForSeconds(0.66f); //pause after each tick

            while (quickMenuShowing == true)
            {
                yield return new WaitForSeconds(0.05f);
            }
            Combat_tick();
        }
    
    }

    //ENDING BATTLE
    void show_advancement()
    {
        //this function is shown at the end of the battle and shows the exp gained/moves learned for each unit and the items gained

        


        //do the actual level up. many things will be flagged to assist the display.
        foreach(PlayerUnit unit in pl)
        {
            unit.get_focus().level_up(unit, runningExp);

            //normalize unit's hp 
            //if ( u )
            unit.post_battle();
        }
        //foreach(PlayerUnit unit in reservePl)
        //{
        //    unit.focus.level_up(unit, brain.runningExp);
        //}


        //show results of level up
        levelUpCanvas.SetActive(true);
        for (int i = 0; i < pl.Length; i++)
        {

            //levelUpSlots[i].transform.GetChild(0).gameObject.GetComponent<Image>() = pl[i].cutIn_portrait;
            levelUpSlots[i].SetActive(true);
            if (pl[i].get_focus().leveledUp)
            {
                levelUpSlots[i].transform.GetChild(1).gameObject.GetComponent<Text>().text =
                pl[i].get_nom() + " Lvl  " + (pl[i].get_level()-1) + "  -> " + (pl[i].get_level()).ToString()
                + "\n Exp:     " + pl[i].get_focus().oldxp + " -> " + pl[i].xp
                + "\n Next:     " + pl[i].get_focus().nextLevelExp;

                //move learned:
                if (pl[i].get_focus().learnedMove)
                {
                    levelUpSlots[i].transform.GetChild(1).gameObject.GetComponent<Text>().text += "\n Learned:  " +
                    pl[i].get_focus().learnedName + "!";
                }
            }
            else
            {
                levelUpSlots[i].transform.GetChild(1).gameObject.GetComponent<Text>().text =
                pl[i].get_nom() + " Lvl  " + pl[i].get_level()
                + "\n Exp:     " + pl[i].get_focus().oldxp + "  -> " + pl[i].xp
                + "\n Next:     " + pl[i].get_focus().nextLevelExp;
            }                     
        }
        //show reserve:
        //for (int i = 0; i < reservePl.Length; i++)
        //{
            //if(reservePl[i].focus.leveledUp)
            //{
                //levelUpSlots[i+4].transform.GetChild(0).gameObject.GetComponent<Text>().text = 
                //reservePl[i].get_name() + lvl -> new lvl;
                //if (reservePl[i].focus.learnedMove){ text += "learned" + pl[i].focus.learnedName;}
            //}

        //}

        //show loot gained:
        if ( runningLoot.Count == 0)
        {
            lootSummary.text = "\n\nNothing was found.";
        }
        else
        {
            lootSummary.text = "Gained:";
            //foreach(Loot item in runningLoot)
            for ( int i = 0; i < runningLoot.Count; i++ )
            {
                //display gained loot
                lootSummary.text += "\n" + runningLoot[i].get_title();

                //also, add gained loot item to worldmanager's inventory.
                WorldManager.worldInventory.add_loot(runningLoot[i]);

                if (i > 9) //we only have so much room on the loot screen.
                {
                    lootSummary.text += "\nAnd " + i + " others.";
                    break;
                }
            }
        }


    }
    void end_battle()
    {
        //Debug.Log(state);
        StopAllCoroutines();

        //moveDisplay.SetActive(false);

        if (state == battleState.WON)
        {
            showingPostScreen = true;
            show_advancement(); //post battle screen
        }
        else if (state == battleState.LOST)//i.e. if(state == battleState.LOST)
        {
            //game over screen with options:
            // -load last save
            // -retry battle? <-- this would require doing an autosave right before the battle. not too bad, really.
            // -return to title screen


            WorldManager.return_from_battle(); //<-- placeholder, so we don't get stuck.
        }
    }
    public bool f_check_battle_over()
    {
        //called whenever a player unit is KO-ed (and only then).
        bool isOver = true;

        foreach (PlayerUnit unit in pl)
        {
            if (unit.ooa == false)
            {
                isOver = false;
                break;
            }
        }
        return isOver;
    }
    public bool e_check_battle_over()
    {
        //called whenever an enemy unit is KO-ed (and only then).
        bool isOver = true;

        foreach (Enemy unit in el)
        {
            if (unit.ooa == false)
            {
                isOver = false;
                break;
            }
        }
        return isOver;
    }
    
    //GAME LOOP
    void inc_repositionValue()
    {
        if (repositionValue == 100)
        {
            repositionText.text = "FULL";
        }
        else
        {
            repositionValue++;            
            repositionText.text = repositionValue.ToString();
        }
        repositionBar.value = repositionValue;

    }
    void handle_wraparound()
    {
        tickSlider.value = 0;

        //next, set the value of each unit's delay to slider to fulldelay - delay.
        foreach(PlayerUnit unit in pl)
        {
            unit.stats.update_slider(unit.fullDelay - unit.delay, true);
        }
        foreach (Enemy unit in el)
        {
            unit.stats.update_slider(unit.fullDelay - unit.delay, true);
        }

    }
    void move_bar()
    {
        if (currentTick % 100 == 0)
        {           
            handle_wraparound();
        }
        else
        {
            tickSlider.value += 1;
        }
    }
    void next_tick()
    {
        inc_repositionValue();
        bool skip = false;

        //DOTs are here too, btw.
        currentTick += 1;
        move_bar();

        foreach (PlayerUnit unit in pl) //update unit's delays
        {
            if (unit.ooa)
            {
                skip = true;
            }
            else if (unit.state != unitState.DEFENDING)
            {
                unit.delay += 1;
                skip = false;
            }

            if (!skip)
            {
                if (unit.status.dot != 0)
                {
                    unit.modify_hp_heal(unit.status.take_dots(unit.get_hp()));
                }
                unit.status.trigger_tick(unit);
                unit.update_stats();
            }
        }
        foreach (Enemy unit in el) //update unit's delays
        {
            if (unit.ooa)
            {
                skip = true;
            }
            else if (unit.state != unitState.DEFENDING)
            {
                unit.delay += 1;
                skip = false;
            }

            if (!skip)
            {
                if (unit.status.dot != 0)
                {
                    unit.modify_hp_heal(unit.status.take_dots(unit.get_hp()));
                }
                unit.status.trigger_tick(unit);
                unit.update_stats();
            }
        }

        if (currentTick == tickLimit) //end the battle
        {           
            if (WorldManager.winOnTimeOut)
            {
                state = battleState.WON;
                end_battle();
            }
            else
            {
                state = battleState.LOST;
                end_battle();
            }
        }

      
    }
    void Combat_tick()
    {
        //Debug.Log("tick " + currentTick);
        
        ordersLeft = 0;
        next_tick();
        quickMenuEnabled = false;
        
        for (int i = 0; i < pl.Length; i++)
        {
            
            if (pl[i].delay == pl[i].fullDelay && pl[i].state != unitState.DEFENDING)
            {
                currentUnit = pl[i];

                if (pl[i].state == unitState.RECOVERING) //unit needs to choose a move
                {
                    ordersLeft += 1;
                    chooseQueue.Enqueue(choose_action(pl[i]));

                }
                else if (pl[i].state == unitState.REPOSITION)
                {
                    chooseQueue.Enqueue(exert_reposition(pl[i]));
                }
                else //unit is ready to use a move
                {                    
                    attacksLeft += 1;
                    exertQueue.Enqueue(exert(pl[i]));
                }                               
            }
        }

        for (int i = 0; i < el.Length; i++)
        {
            el[i].checkUp_tick(pMap, eMap, brain); //<-- move cancels and map dodges.

            if (el[i].delay == el[i].fullDelay && el[i].state != unitState.DEFENDING)
            {
                
                if (el[i].state == unitState.RECOVERING) //unit needs to choose a move
                {
                    //e_choose_action(el[i]);
                    el[i].stats.update_delayText("RDY");
                    ordersLeft += 1;
                    chooseQueue.Enqueue(e_choose_action(el[i]));
                }
                else //unit is ready to use a move
                {
                    attacksLeft += 1;
                    exertQueue.Enqueue(e_exert(el[i]));

                }
            }
            
        }
        
        //run the coroutine queue here.
        StartCoroutine(CoroutineCoordinator());
        if (currentTick > minDelay) quickMenuEnabled = true;
    }   
    IEnumerator exert(PlayerUnit unit)
    {
        state = battleState.PLAYERTURN;
        bool validExert = false;

        //check if unit is still alive
        foreach (PlayerUnit aliveDude in pl)
        {
            if (unit == aliveDude)
            {
                validExert = true;
                break;
            }
        }
        //check if clearance is still ok.
        if (!unit.nextMove.check_post(unit.get_x(), unit.get_y(), pMap, unit.get_mp()))
        {
            pMap.tilegrid[unit.get_x(), unit.get_y()].GetComponent<PlayerTile>().show_damage_text("Blocked");
            //pMap.tilegrid[unit.get_y(), unit.get_x()].GetComponent<PlayerTile>().show_damage_text("Blocked");
            validExert = false;
        }

        if (validExert)
        {
            quickMenuEnabled = false;
            //changes the acting unit's colour so we know who is acting
            Color color = new Color(135f / 255f, 206f / 255f, 235f / 255f); //sky blue
            unit.stats.update_color(color);

            show_f_playbyplay(unit);

            for (int i = 0; i < unit.nextMove.get_strikes(); i++) //multi hit moves
            { 
                if (unit.nextMove.get_isHeal()) //if player -> player
                {
                    brain.f_resolve_heal(unit, pMap);
                }
                else //if player -> enemy        
                {
                    brain.f_resolve_attack(unit, eMap);
                }

                yield return new WaitForSeconds(1.25f);

                //check if the move applies status
                if (unit.nextMove.get_appliesStatus() == true)
                {
                    //apply move's status effects now.
                    if (unit.nextMove.get_isHeal())
                    {
                        unit.nextMove.apply_status_heal(unit, brain.get_f_targets().ToArray(), this);
                    }
                    else
                    {
                        unit.nextMove.apply_status_attack(unit, brain.get_e_targets().ToArray(), this, brain.all_ooa);
                    }
                }

                yield return new WaitForSeconds(1.0f);
            }

            hide_playbyplay();

            //move the unit, if applies.
            if (unit.nextMove.get_clearType() != 0) //i.e. if the unit moves
            {
                brain.f_translate(unit, pMap, unit.nextMove.get_clearX(), unit.nextMove.get_clearY());
                yield return new WaitForSeconds(0.25f);
            }


            
        }
        unit.enter_recovery();
        if (e_check_battle_over() == true) //if all enemy units are out of action
        {
            state = battleState.WON;
            end_battle();
        }
        else
        {
            state = battleState.WAITING;
        }
        attacksLeft -= 1;       
        if (attacksLeft == 0)
        {
            quickMenuEnabled = true;
        }
    }
    IEnumerator e_exert(Enemy unit)
    {
        state = battleState.ENEMYTURN;

        bool validExert = false;
        //check if unit is still alive
        foreach(Enemy aliveDude in el)
        {
            if (unit == aliveDude)
            {
                validExert = true;
                break;
            }
        }
        //check if check_clearance still returns true
        if (!unit.nextMove.check_clearance(unit.get_x(), unit.get_y(), eMap))
        {
            eMap.tilegrid[unit.get_y(), unit.get_x()].GetComponent<EnemyTile>().show_damage_text("Blocked");
            validExert = false;
        }

        if (validExert)
        {
            quickMenuEnabled = false;
            //changes the acting unit's colour so we know who is acting
            Color color = new Color(135f / 255f, 206f / 255f, 235f / 255f); //sky blue
            unit.stats.update_color(color);

            show_e_playbyplay(unit);

            for (int i = 0; i < unit.nextMove.get_strikes(); i++) //multi hit moves
            {
                if (unit.nextMove.get_isHeal()) //if enemy -> enemy
                {
                    brain.e_resolve_heal(unit, eMap);
                }
                else //if enemy -> player
                {
                    brain.e_resolve_attack(unit, pMap);
                }

                yield return new WaitForSeconds(1.25f);

                //check if the move applies status
                if (unit.nextMove.get_appliesStatus() == true)
                {
                    //apply move's status effects now.
                    if (unit.nextMove.get_isHeal())
                    {
                        unit.nextMove.apply_status_heal(unit, brain.get_e_targets().ToArray(), this);
                    }
                    else
                    {
                        unit.nextMove.apply_status_attack(unit, brain.get_f_targets().ToArray(), this, brain.all_ooa);
                    }
                }

                yield return new WaitForSeconds(1.0f);
            }

            if (unit.nextMove.get_strikes() == 0)
            {
                //check if the move applies status
                if (unit.nextMove.get_appliesStatus() == true)
                {
                    //apply move's status effects now.
                    if (unit.nextMove.get_isHeal())
                    {
                        unit.nextMove.apply_status_heal(unit, brain.get_e_targets().ToArray(), this);
                    }
                    else
                    {
                        unit.nextMove.apply_status_attack(unit, brain.get_f_targets().ToArray(), this, brain.all_ooa);
                    }
                }

                yield return new WaitForSeconds(1.0f);
            }
            hide_playbyplay();

            if (unit.nextMove.get_clearType() != 0) //i.e. if the unit moves
            {
                brain.e_translate(unit, eMap, unit.nextMove.get_clearX(), unit.nextMove.get_clearY());
                yield return new WaitForSeconds(0.25f);
            }
            
        }
        unit.enter_recovery();
        if (f_check_battle_over() == true) //if all enemy units are out of action
        {
            state = battleState.LOST;
            end_battle();
        }
        else
        {
            state = battleState.WAITING;
        }
        attacksLeft -= 1;
        if (attacksLeft == 0)
        {
            quickMenuEnabled = true;
        }
    }
    IEnumerator choose_action(PlayerUnit unit)
    {
        state = battleState.PLAYERTURN;

        //check if still valid
        bool validExert = false;
        foreach (PlayerUnit aliveDude in pl)
        {
            if (unit == aliveDude)
            {
                validExert = true;
                break;
            }
        }
        if (validExert)
        {
            yield return new WaitForSeconds(0.33f);
            //METHOD
            //this function only serves to allow the player to choose what move they want to use. just click the move.
            doneOrdering = false;
            currentUnit = unit;
            unit.stats.update_delayText("RDY");
            state = battleState.PLAYERTURN;
            show_buttons();

            //wait in this coroutine until player has chosen move (marked in log_attack)
            while (!doneOrdering)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
        else
        {
            ordersLeft -= 1;
        }
    }
    IEnumerator e_choose_action(Enemy unit)
    {
        state = battleState.ENEMYTURN;

        bool validExert = false;
        //check if unit is still alive
        foreach (Enemy aliveDude in el)
        {
            if (unit == aliveDude)
            {
                validExert = true;
                break;
            }
        }
        if (validExert)
        {
            unit.e_choose_move(pMap, eMap, pl, el); //chooses and logs unit's move.
            //Debug.Log(unit.get_nom() + "has chosen a move");           
            yield return new WaitForSeconds(0.33f);
        }
        ordersLeft -= 1;
        if (ordersLeft == 0) { state = battleState.WAITING; }
    }
    IEnumerator exert_reposition(PlayerUnit unit)
    {
        //repositions the unit to its logged tile.
        //goes into exert_queue()

        //ADD CLEARANCE CHECK

        bool validExert = false;
        foreach (PlayerUnit aliveDude in pl)
        {
            if (unit == aliveDude)
            {
                validExert = true;
                break;
            }
        }
        show_f_playbyplay(unit, true);
        //if ( pMap.search_unit(unit.get_x(), unit.get_y()) != null)
        if ( pMap.search_unit(unit.nextX, unit.nextY) != null)
        {
            pMap.tilegrid[unit.get_y(), unit.get_x()].GetComponent<PlayerTile>().show_damage_text("Blocked");
            validExert = false;
        }

        if (validExert)
        {           
            pMap.remove_unit(unit);

            //what we do:
            unit.set_x(unit.nextY);
            unit.set_y(unit.nextX);
            pMap.stop_reposition_highlight(unit.get_x(), unit.get_y());
            pMap.place_unit(unit);
        }

        unit.enter_recovery(true);
        //unit.stats.update_slider((currentTick % 100) + unit.fullDelay, true);


        yield return new WaitForSeconds(1.0f);
        hide_playbyplay();
    }

    //MOVE INTERFACE
    IEnumerator RevealPortrait()
    {
        //show unit's select portrait 
        //but wait for half a second before doing this.
        //also, needs to be shown with an animation.
        yield return new WaitForSeconds(0.2f);

        //this here:
        //moveSelectPortrait.GetComponent<Image>().sprite = currentUnit.get_moveSelectPortrait();
        //moveSelectPortrait.gameObject.SetActive(true);


        if (!isPortraitLocked)
        {
            actingUnitImage.gameObject.GetComponent<Image>().sprite = currentUnit.get_moveSelectPortrait();
            //^^uncomment line to turn on unit's images. otherwise will just be a white background.


            actingUnitImage.gameObject.SetActive(true);           
        }
    }
    void hidePortrait()
    {
        isPortraitLocked = true;
        actingUnitImage.gameObject.SetActive(false);
    }
    public void show_buttons()
    {
        //display unit's image here. hide in hide_buttons()
        isPortraitLocked = false;
        StartCoroutine(RevealPortrait());

        currentUnit.nextMove = null;
        allowPreview = true;
        allowHelp = true;
        // Portrait stuff
        //StartCoroutine(RevealPortrait());
        quickMenuEnabled = false;
        allowButtonHover = true;
        moveDescrBox.gameObject.SetActive(false);

        //highlight unit's tile
        pMap.select_move_highlight(currentUnit.get_x(), currentUnit.get_y());
        defendButton.interactable = true;

        //Debug.Log(currentUnit.get_nom() + " is choosing a move.");
        //FRONT
        int count = 0;
        while (count < currentUnit.get_frontList().Length)
        {
            frontMoves[count].gameObject.SetActive(true); //shows buttons that can be clicked 
            
            //set move title AND, beneath it, move's prep / rec
            frontMoves[count].transform.GetChild(0).GetComponent<Text>().text = currentUnit.get_frontList()[count].get_title() + "\n" + currentUnit.calc_delay(currentUnit.get_frontList()[count].get_preDelay()) + "/" + currentUnit.get_frontList()[count].get_recDelay();

            if (currentUnit.get_frontList()[count].check_pre(currentUnit.get_x(), currentUnit.get_y(), pMap, currentUnit.get_mp()) == true && currentUnit.get_y() > 1)
            {
                frontMoves[count].interactable = true;
            }
            else
            {
                frontMoves[count].interactable = false;
            }
            count++;
        }
        while (count < frontMoves.Length)
        {
            frontMoves[count].gameObject.SetActive(false); //hides buttons that cannot be clicked
            count++;
        }
        //BACK
        count = 0;
        while (count < currentUnit.get_backList().Length)
        {
            backMoves[count].gameObject.SetActive(true); //shows buttons that can be clicked
            backMoves[count].transform.GetChild(0).GetComponent<Text>().text = currentUnit.get_backList()[count].get_title() + "\n" + currentUnit.calc_delay(currentUnit.get_backList()[count].get_preDelay()) + "/" + currentUnit.get_backList()[count].get_recDelay();

            if (currentUnit.get_backList()[count].check_pre(currentUnit.get_x(), currentUnit.get_y(), pMap, currentUnit.get_mp()) == true && currentUnit.get_y() < 2)
            {
                backMoves[count].interactable = true;
            }
            else
            {
                backMoves[count].interactable = false;
            }
            count++;
        }
        while (count < backMoves.Length)
        {
            backMoves[count].gameObject.SetActive(false); //hides buttons that cannot be clicked
            count++;
        }     
        moveDisplay.SetActive(true);
        showActingUnitWindow(currentUnit);
    }
    public void hide_buttons()
    {
        //display unit's image here. hide in hide_buttons()
        hidePortrait();

        moveDisplay.SetActive(false);
        hideActingUnitWindow();
        stats.gameObject.SetActive(true);
        
        defendButton.interactable = false;

    } 
    public void load_move_descr(int bNum)
    {
        if (showingMults)
        {
            clear_affmults();
        }
        
        //called when move button is moused over
        moveDescrBox.gameObject.SetActive(true);

        if (allowButtonHover)
        {
            //show move description
            moveDescrBox.gameObject.SetActive(true);
            //for defend
            if (bNum == -1)
            {
                if (allowButtonHover)
                {
                    moveDescrBox.text = currentUnit.get_defendEquipped().get_descr0() + " " + currentUnit.get_defendEquipped().get_descr1() + ", " + currentUnit.get_defendEquipped().get_descr2()
                    + "\n<i>" + currentUnit.get_defendEquipped().get_descr3() + "</i>";
                    pMap.restore_all();
                    eMap.restore_all();
                }
                return;
            }

            //if bNum > 4, then it's talking about backlist. subtract 4 from bNum
            int mark = bNum;
            PlayerMove[] rankList;
            if (mark >= 5)
            {
                mark -= 5;
                rankList = currentUnit.get_backList();
            }
            else
            {
                rankList = currentUnit.get_frontList();
            }
            moveDescrBox.text = "MP " + rankList[mark].get_mpDrain() + ", " + rankList[mark].get_descr0() + "\n" + rankList[mark].get_descr1() + ", " + rankList[mark].get_descr2()
            + "\n<i>" + rankList[mark].get_descr3() + "</i>";

            currentUnit.nextMove = rankList[mark]; //just temporarily. will be overwritten in log_move. used for aff mult preview
            if (showingMults) hpbars_for_aff(currentUnit.nextMove.get_isHeal());
            //reset pMap's colouring

            //show move's translation path highlighted
            //^if the unit is, in fact, moving:
            //then, highlight all the tiles the unit will be moving over.
            f_show_translationPreview(currentUnit.get_x(), currentUnit.get_y(), rankList[mark].get_clearX(), rankList[mark].get_clearY(), rankList[mark].get_clearType());       
        }                
    }
    void f_show_translationPreview(int unit_x, int unit_y, int clearX, int clearY, int clearType)
    {
        //version for playerUnits.
        //called on move previews and on unit hovers. lets player see what tiles the unit will be passing over on their next move.
        //be careful we don't highlight a tile that doesn't exist, or is out of bounds.        
        pMap.restore_all();
        if (clearType == 0 || !(unit_x + clearX >= 0) || !(unit_x + clearX < 5) || !(unit_y + clearY >= 0) || !(unit_y + clearY < 4))
            return;
        
        if ( clearType == 1)
        {
            //then only highlight the end tile.
            //if in bounds. if out of bounds, some way to show that too.
            
                pMap.highlight_given(unit_x + clearX, unit_y + clearY);
        }
        else if ( clearType == 2)
        {
            //then highlight vertical tiles, then horizontal tiles, between starting and ending points.

            //VERTICAL PORTION
            if (clearY > 0) //if we're moving forwards
            {
                for (int i = unit_y + 1; i <= unit_y + clearY; i++) //vertical part
                {
                    //pMap.highlight_given(i, unit_x); //<-- here
                    pMap.highlight_given(unit_x, i);
                }
            }
            else if (clearY < 0) //if we're moving backwards
            {
                for (int i = unit_y - 1; i >= unit_y + clearY; i--) //vertical part
                {
                    //pMap.highlight_given(i, unit_x);
                    pMap.highlight_given(unit_x, i);
                }
            }

            //HORIZONTAL PORTION
            if (clearX > 0) //we're moving to the right

            {
                for (int i = unit_x + 1; i <= unit_x + clearX; i++)
                {
                    pMap.highlight_given(i, unit_y + clearY); //was unit_y + clearY, i
                }
            }
            else if (clearX < 0) //we're moving to the left
            {
                for (int i = unit_x - 1; i >= unit_x + clearX; i--)
                {
                    pMap.highlight_given(i, unit_y + clearY); //was unit_y + clearY, i
                }
            }
        }
    }
    void e_show_translationPreview(int unit_x, int unit_y, int clearX, int clearY, int clearType)
    {
        //version for enemy units.
        //be careful we don't highlight a tile that doesn't exist, or is out of bounds.        
        eMap.restore_all();
        if ( clearType == 0 || !(unit_x + clearX >= 0) || !(unit_x + clearX < 5) || !(unit_y + clearY >= 0) || !(unit_y + clearY < 4))
            return;

            
        
        if (clearType == 1)
        {
            //then only highlight the end tile.
            //if in bounds. if out of bounds, some way to show that too.

            eMap.highlight_given(unit_x + clearX, unit_y + clearY);
        }
        else if (clearType == 2)
        {
            //then highlight vertical tiles, then horizontal tiles, between starting and ending points.

            //VERTICAL PORTION
            if (clearY > 0) //if we're moving forwards
            {
                for (int i = unit_y + 1; i <= unit_y + clearY; i++) //vertical part
                {
                    eMap.highlight_given(i, unit_x);
                }
            }
            else if (clearY < 0) //if we're moving backwards
            {
                for (int i = unit_y - 1; i >= unit_y + clearY; i--) //vertical part
                {
                    eMap.highlight_given(i, unit_x);
                }
            }

            //HORIZONTAL PORTION
            if (clearX > 0) //we're moving to the right
            {
                for (int i = unit_x + 1; i <= unit_x + clearX; i++)
                {
                    eMap.highlight_given(unit_y + clearY, i);
                }
            }
            else if (clearX < 0) //we're moving to the left
            {
                for (int i = unit_x - 1; i >= unit_x + clearX; i--)
                {
                    eMap.highlight_given(unit_y + clearY, i);
                }
            }
        }
    }
    public void use_move_general(int bNum)
    {
        inTileSelectPhase = true;
        int[] origin = { currentUnit.get_y(), currentUnit.get_x() };
        moveID = bNum;
        if (moveID >= 5)
        {
            moveID -= 5;
            rankList = currentUnit.get_backList();
        }
        else
        {
            rankList = currentUnit.get_frontList();
        }

        if (rankList[moveID].get_isHeal())
        {           
            pMap.allow_hover(rankList[moveID].get_xsize(), rankList[moveID].get_ysize(), rankList[moveID].get_highlightType(), origin);
        }
        else
        {
            eMap.allow_hover(rankList[moveID].get_xsize(), rankList[moveID].get_ysize(), rankList[moveID].get_highlightType(), origin);
        }
        currentUnit.delay = 0;
        hide_buttons();
    }
    public void log_attack(int x, int y)
    {
        //called when tile is finally clicked and move is locked in.
        allowHelp = false;
        inTileSelectPhase = false;
        hideStatPreviewWindow();
        pMap.restore_old_tile(currentUnit.get_x(), currentUnit.get_y());
        pMap.restore_all();

        allowButtonHover = false;
        if ( showingMults)
        {
            aff_for_hpbars();
        }

        currentUnit.enter_prepare(x, y, moveID);

        //mark eMap tiles as beingTargeted HERE.


        doneOrdering = true;
        ordersLeft -= 1;
        state = battleState.WAITING;
    }
    public void use_defend_move()
    {
        //unit's is put into whatever state specified by their move - immediately
        //todo: should we make the player click the unit's square as a sort of confirmation?
        pMap.restore_all();
        eMap.restore_all();
        currentUnit.enter_defend();
        hide_buttons();
        doneOrdering = true;
        ordersLeft -= 1;
        state = battleState.WAITING;
    }
    public void cancel_move_selection()
    {
        //allows player to return from tile selection phase back to move selection phase.
        //Debug.Log("cancel move selection called");
        inTileSelectPhase = false;
        pMap.allowHover = false;
        eMap.allowHover = false;

        if ( currentUnit.nextMove.get_isHeal()) pMap.restore_all();      
        else eMap.restore_all(); 
      
        aff_for_hpbars();
        currentUnit.nextMove = null;
        show_buttons();
    }

    //ACTING UNIT PREVIEW WINDOW
    void showActingUnitWindow(PlayerUnit unit)
    {
        actingPreviewWindow.SetActive(true);

        // name \n lvl
        actingPreviewWindow.transform.GetChild(0).gameObject.GetComponent<Text>().text = unit.get_nom() + "\nLvl " + unit.get_level();

        //hp/hpmax \n mp/mpmax
        actingPreviewWindow.transform.GetChild(1).gameObject.GetComponent<Text>().text = unit.get_hp() + "/" + unit.get_hpMax_actual() + "\n"
                                                                                     + unit.get_mp() + "/" + unit.get_mpMax_actual();
        //adjust hp slider
        actingPreviewWindow.transform.GetChild(2).gameObject.GetComponent<Slider>().maxValue = unit.get_hpMax_actual();
        actingPreviewWindow.transform.GetChild(2).gameObject.GetComponent<Slider>().value = unit.get_hp();

        //adjust mp slider
        actingPreviewWindow.transform.GetChild(3).gameObject.GetComponent<Slider>().maxValue = unit.get_mpMax_actual();
        actingPreviewWindow.transform.GetChild(3).gameObject.GetComponent<Slider>().value = unit.get_mp();

        //adjust affinity text
        actingPreviewWindow.transform.GetChild(5).gameObject.GetComponent<Text>().text = "Aff: " + unit.get_aff() // or rather, get_aff_name()
                                                                                  + "\nWeapon Ele: " + unit.get_weapon().get_element();
    }
    void hideActingUnitWindow()
    {
        actingPreviewWindow.SetActive(false);
    }

    //QUICK MENU
    public void quickMenu_close()
    {
        //if (currentTick >= minDelay)
        //{
        //    moveDisplay.SetActive(true);
        //}
        hideStatPreviewWindow();
        quickMenu.SetActive(false);
        quickMenuShowing = false;
        allowHelp = false;
        state = battleState.WAITING;
    }
    public void quickMenu_open()
    {
        allowPreview = true;
        allowHelp = true;
        quickMenuShowing = true;
        moveDisplay.SetActive(false);
        quickMenuSelected = null;
        quickMenu.SetActive(true);

        //setActive as many buttons as we have units and load in the text.
        //if the unit is in prep or defending, set to interactable.
        for(int i = 0; i < pl.Length; i++)
        {
            quickMenuUnitButtons[i].interactable = false;
            quickMenuUnitButtons[i].gameObject.SetActive(true);
            quickMenuUnitTexts[i].text = pl[i].get_nom();

            if ((pl[i].state == unitState.PREPARING || pl[i].state == unitState.DEFENDING) && !(pl[i].ooa))
            {               
                quickMenuUnitButtons[i].interactable = true;
            }
        }
        quickMenu_checkCancel();
        quickMenu_checkDefend();
        quickMenu_checkReposition();
    }
    public void quickMenu_MakeSelection(int chosen)
    {               
        quickMenuSelected = pl[chosen];        
        quickMenu_checkCancel();
        quickMenu_checkDefend();
        quickMenu_checkReposition();
    }
    public void quickMenu_selectHover(int chosen)
    {
        //chosen is the index of the unit in pl.
        //highlight the tile the unit is standing on on pointer enter

        pMap.select_move_highlight(pl[chosen].get_x(), pl[chosen].get_y());
    }
    public void quickMenu_deselectHover(int chosen)
    {
        //chosen is the index of the unit in pl
        //restores prev color to the tile on pointer exit
        pMap.restore_old_tile(pl[chosen].get_x(), pl[chosen].get_y());
    }
    public void quickMenu_checkCancel()
    {
        quickMenuCancel.interactable = false;
        if (quickMenuSelected != null) //single unit order
        {
            if (quickMenuSelected.state == unitState.PREPARING || quickMenuSelected.state == unitState.DEFENDING)
            {
                //if the unit is in prep or def, then you can cancel.
                quickMenuCancel.interactable = true;
                quickMenuCancel.GetComponentInChildren<Text>().text = "Selected->Cancel";

            }
        }
        else //check all units
        {
            //if at least one unit can be ordered to cancel, the button is enabled.

            foreach(PlayerUnit unit in pl)
            {
                if(unit.state == unitState.PREPARING || unit.state == unitState.DEFENDING)
                {
                    quickMenuCancel.interactable = true;
                    quickMenuCancel.GetComponentInChildren<Text>().text = "All->Cancel";
                    break;
                }
            }           
        }

        
    }
    public void quickMenu_checkDefend()
    {
        quickMenuDefend.interactable = false;
        if (quickMenuSelected != null) //single unit order
        {
            if (quickMenuSelected.state == unitState.PREPARING)
            {
                //if the unit is in prep, then you can defend.
                quickMenuDefend.interactable = true;
                quickMenuDefend.GetComponentInChildren<Text>().text = "Selected->Defend";
                
            }
        }
        else //all units
        {
            //if at least one unit can be ordered to defend, the button is enabled.
            foreach (PlayerUnit unit in pl)
            {
                if (unit.state == unitState.PREPARING)
                {
                    quickMenuDefend.interactable = true;
                    quickMenuDefend.GetComponentInChildren<Text>().text = "All->Defend";

                    break;
                }
            }
        }
    }
    public void quickMenu_checkReposition()
    {
        if (quickMenuSelected != null && (quickMenuSelected.state == unitState.PREPARING || quickMenuSelected.state == unitState.DEFENDING) && repositionValue == 100) //single unit order
        {
            quickMenuReposition.interactable = true;
            quickMenuReposition.GetComponentInChildren<Text>().text = "Selected->Move";
        }
        else
        {
            quickMenuReposition.interactable = false;
        }        
    }    
    public void quickMenu_exertCancel()
    {
        if(quickMenuSelected != null)
        {
            //we have a unit selected, so make them ready for orders.
            quickMenuSelected.state = unitState.RECOVERING;

            quickMenuSelected.enter_recovery(false, true);


            quickMenuSelected.stats.update_slider((currentTick % 100) + quickMenuSelected.fullDelay, true);
            quickMenuSelected.stats.update_delayText("");

            quickMenuSelected.stats.update_moveNamePreview("Recovering");
            Color color = new Color(253f / 255f, 248f / 255f, 11f / 255f); //yellow
            quickMenuSelected.stats.update_color(color); //make it yellow
            quickMenuSelected.update_stats();
            quickMenuSelected = null;
        }
        else
        {
            //check for each unit if they're eligible to be canceled, and cancel all that are eligible
            foreach (PlayerUnit unit in pl)
            {
                if (unit.state == unitState.PREPARING || unit.state == unitState.DEFENDING)
                {
                    if (unit.state == unitState.DEFENDING)
                    {
                        unit.status.exit_defend();
                    }

                    unit.state = unitState.RECOVERING;

                    unit.enter_recovery(false, true);
                    unit.stats.update_slider((currentTick % 100) + unit.fullDelay, true);
                    unit.stats.update_delayText("");


                    unit.stats.update_moveNamePreview("Recovering");
                    Color color = new Color(253f / 255f, 248f / 255f, 11f / 255f); //yellow
                    unit.stats.update_color(color); //make it yellow
                    unit.update_stats();
                }
            }
        }
        quickMenu_checkCancel();
        quickMenu_checkDefend();
        quickMenu_checkReposition();
    }
    public void quickMenu_exertDefend()
    {
        if (quickMenuSelected != null)
        {
            //we have a unit selected, so put them into defend
            quickMenuSelected.enter_defend();
            quickMenuSelected.update_stats();
            quickMenuSelected = null;
        }
        else
        {
            foreach (PlayerUnit unit in pl)
            {
                if (unit.state == unitState.PREPARING)
                {
                    //clear tile maps, too
                    unit.enter_defend();
                    unit.update_stats();
                }
            }
        }
        pMap.restore_all();
        eMap.restore_all();
        quickMenu_checkCancel();
        quickMenu_checkDefend();
        quickMenu_checkReposition();
    }
    public void quickMenu_exertReposition()
    {
        //can only be clicked if there quickMenuSelected != null       
        quickMenu.SetActive(false);

        //allow player to place units
        open_reposition(quickMenuSelected);
        quickMenuSelected = null;
    }

    //REPOSITION
    public void open_reposition(PlayerUnit chosen)
    {
        //click a tile for the unit to reposition to
        //Debug.Log("open_reposition() called");
        pMap.targetArea[0] = 1;
        pMap.targetArea[1] = 1;
        pMap.mustUseOrigin = false;
        pMap.type = 0;
        roundUnit = chosen;

        //all tiles must be clickable
        for (int c = 0; c < 4; c++)
        {
            for (int r = 0; r < 5; r++)
            {
                pMap.tilegrid[r, c].GetComponent<PlayerTile>().repositionPlace = true;
            }
        }
        //except for the tiles that already have unit on them.
        foreach (PlayerUnit unit in pl)
        {
            pMap.stop_reposition_highlight(unit.get_x(), unit.get_y());
        }
        
    }
    public void reposition_unit(int x, int y)
    {
        //set up the unit to reposition, which it will do when it reaches that fullDelay
        //logs the coords of the tile 
        //Debug.Log("reposition_unit() called");
        roundUnit.state = unitState.REPOSITION;

        repositionValue = 0;

        roundUnit.enter_prepare(x, y, -1);
        roundUnit.stats.update_slider((currentTick % 100) + roundUnit.fullDelay, true);

        for (int c = 0; c < 4; c++)
        {
            for (int r = 0; r < 5; r++)
            {
                pMap.tilegrid[r, c].GetComponent<PlayerTile>().repositionPlace = false;
            }
        }

        //remove highlight from clicked tile
        pMap.restore_old_tile(y, x);

        quickMenu_checkCancel();
        quickMenu_checkDefend();
        quickMenu_checkReposition();
        quickMenu.SetActive(true);
    }

    //STAT PREVIEW WINDOW
    public void showStatPreviewWindow(PlayerUnit unit)
    {
        if (helpShowing) return;

        statPreviewWindow.SetActive(true);       

        if(unit.state == unitState.PREPARING)
        {
            //highlight unit's translation
            f_show_translationPreview(unit.get_x(), unit.get_y(), unit.nextMove.get_clearX(), unit.nextMove.get_clearY(), unit.nextMove.get_clearType());

            if (unit.nextMove.get_isHeal())
            {
                //highlight pmap
                //pMap.type = unit.nextMove.get_highlightType();
                pMap.highlight_helper(unit.nextX, unit.nextY, true, false, unit.nextMove.get_targetArea(), unit.nextMove.get_highlightType());
            }
            else
            {
                //highlight emap
                eMap.type = unit.nextMove.get_highlightType();
                eMap.highlight_helper(unit.nextX, unit.nextY, true, false, unit.nextMove.get_targetArea());
            }
        }
        else if (unit.state == unitState.REPOSITION)
        {
            //highlight target tile in blue
            //pMap.type = 0;
            int[] assistTargetArea = new int[2] { 1, 1 };
            pMap.highlight_helper(unit.nextX, unit.nextY, false, false, assistTargetArea, 0, true);
        }

        // name \n lvl
        statPreviewWindow.transform.GetChild(0).gameObject.GetComponent<Text>().text = unit.get_nom() + "\nLvl " + unit.get_level();

        //hp/hpmax \n mp/mpmax
        statPreviewWindow.transform.GetChild(1).gameObject.GetComponent<Text>().text = unit.get_hp() + "/" + unit.get_hpMax_actual() + "\n"
                                                                                     + unit.get_mp() + "/" + unit.get_mpMax_actual();
        //adjust hp and mp sliders
        statPreviewWindow.transform.GetChild(3).gameObject.GetComponent<Slider>().maxValue = unit.get_hpMax_actual();
        statPreviewWindow.transform.GetChild(3).gameObject.GetComponent<Slider>().value = unit.get_hp();

        //enable and update mp slider
        statPreviewWindow.transform.GetChild(4).gameObject.SetActive(true);
        statPreviewWindow.transform.GetChild(4).gameObject.GetComponent<Slider>().maxValue = unit.get_mpMax_actual();
        statPreviewWindow.transform.GetChild(4).gameObject.GetComponent<Slider>().value = unit.get_mp();
        
        //hide stamina slider
        statPreviewWindow.transform.GetChild(8).gameObject.SetActive(false);
        statPreviewWindow.transform.GetChild(9).gameObject.SetActive(false);

        // physa
        // physd
        // maga
        // magd
        // hit
        // dodge
        // affinity
        statPreviewWindow.transform.GetChild(2).gameObject.GetComponent<Text>().text =
                         "\nP-atk: " + unit.get_physa_actual()
                       + "\nP-def: " + unit.get_physd_actual()
                       + "\nM-atk: " + unit.get_maga_actual()
                       + "\nM-def: " + unit.get_magd_actual()
                       + "\nHit: " + unit.get_hit_actual()
                       + "\nDodge: " + unit.get_dodge_actual()
                       + "\nAff: " + unit.get_aff(); // or rather, get_aff_name()
        statPreviewWindow.transform.GetChild(7).gameObject.GetComponent<Text>().text = unit.status.get_multiplier_info();

        statPreviewWindow.transform.GetChild(6).gameObject.GetComponent<Text>().text = unit.status.get_status_info();
    }
    public void f_undoStatPreviewWindowHighlight(int unit_x, int unit_y, bool healio, int[] newArea)
    {
        statPreviewWindow.SetActive(false);
        //Debug.Log("yeah buddy - 0");
        pMap.restore_all();
        /*
        if (healio) //is the next move a heal
        {
            //unhighlight pmap
            pMap.highlight_helper_off(unit_x, unit_y, false, newArea);
        }
        */
        if (!healio)
        {
            //unhighlight emap
            eMap.highlight_helper_off(unit_x, unit_y, false, newArea);
        }
    }
    public void showStatPreviewWindow_e(Enemy unit)
    {
        if (helpShowing) return;

        statPreviewWindow.SetActive(true);

        if (unit.state == unitState.PREPARING)
        {
            //highlight unit's translation
            e_show_translationPreview(unit.get_x(), unit.get_y(), unit.nextMove.get_clearX(), unit.nextMove.get_clearY(), unit.nextMove.get_clearType());

            if (unit.nextMove.get_isHeal())
            {
                //highlight emap
                eMap.type = unit.nextMove.get_highlightType();
                eMap.highlight_helper(unit.nextX, unit.nextY, false, false, unit.nextMove.get_targetArea());
            }
            else
            {
                //highlight pmap
                Debug.Log("highlighting pmap through unit preview");
                pMap.type = unit.nextMove.get_highlightType();
                pMap.highlight_helper(unit.nextX, unit.nextY, false, false, unit.nextMove.get_targetArea()); //<-- here (2020-12-20)
            }
        }

        //show:
        //defendButton = moveDisplay.transform.GetChild(0).gameObject.GetComponent<Button>();

        // name \n lvl
        statPreviewWindow.transform.GetChild(0).gameObject.GetComponent<Text>().text = unit.get_nom() + "\nLvl " + unit.get_level();

        //hp/hpmax \n mp/mpmax
        statPreviewWindow.transform.GetChild(1).gameObject.GetComponent<Text>().text = unit.get_hp() + "/" + unit.get_hpMax_actual();
        //adjust hp and mp sliders
        statPreviewWindow.transform.GetChild(3).gameObject.GetComponent<Slider>().maxValue = unit.get_hpMax_actual();
        statPreviewWindow.transform.GetChild(3).gameObject.GetComponent<Slider>().value = unit.get_hp();

        //disable mp slider
        statPreviewWindow.transform.GetChild(4).gameObject.SetActive(false);
        
        //enemy only: show stamina slider and stamina title text
        statPreviewWindow.transform.GetChild(8).gameObject.SetActive(true);
        statPreviewWindow.transform.GetChild(9).gameObject.SetActive(true);
        statPreviewWindow.transform.GetChild(9).gameObject.GetComponent<Slider>().maxValue = unit.get_staminaMax();
        statPreviewWindow.transform.GetChild(9).gameObject.GetComponent<Slider>().value = unit.get_stamina();

        // physa
        // physd
        // maga
        // magd
        // hit
        // dodge
        // affinity
        statPreviewWindow.transform.GetChild(2).gameObject.GetComponent<Text>().text =
                         "\nP-atk: " + unit.get_physa_actual()
                       + "\nP-def: " + unit.get_physd_actual()
                       + "\nM-atk: " + unit.get_maga_actual()
                       + "\nM-def: " + unit.get_magd_actual()
                       + "\nHit: " + unit.get_hit_actual()
                       + "\nDodge: " + unit.get_dodge_actual() 
                       + "\nAff: " + unit.get_aff(); // or rather, get_aff_name()

        //stat multiplier text:
        //only shows if multiplier != 1
        statPreviewWindow.transform.GetChild(7).gameObject.GetComponent<Text>().text = unit.status.get_multiplier_info();
        statPreviewWindow.transform.GetChild(6).gameObject.GetComponent<Text>().text = unit.status.get_status_info();
    }
    public void e_undoStatPreviewWindowHighlight(int unit_x, int unit_y, bool healio, int[] newArea)
    {
        statPreviewWindow.SetActive(false);

        eMap.restore_all();
        /*
        if (healio) //is the next move a heal
        {
            eMap.highlight_helper_off(unit_x, unit_y, false, newArea);
        }
        */
        if (!healio)
        {
            pMap.highlight_helper_off(unit_x, unit_y, false, newArea);
        }
    }
    private void hideStatPreviewWindow()
    {
        //unhighlight highlighted tiles
        pMap.restore_all();
        allowPreview = false;
        statPreviewWindow.SetActive(false);
    }
    public void hide_statPrev()
    {
        pMap.restore_all();
        statPreviewWindow.SetActive(false);
    }

    //REMOVING AND ADDING ENEMIES
    public void reinforce_enemy(Enemy reinforcement, int pref = 0)
    {
        //AAAAAAAAAAAHHHHHHHH
        //only 1x1 tile units can be reinforcements.

        //pref dictates where the unit wants to be placed.
        // 0: don't care
        // 1: front
        // 2: back       

        int xStore, yStore; //stores the coordinates where the reinforcement will be placed
        bool canSit;

        //current method would place units on first open tiles. we want more random than that.
        //so, create an int list for xcoords and ycoords and mix it around as you wish.

        //also, create lists with respect to the unit's xsize and ysize
        /*
        int[] xcoords = new int[5 - reinforcement.get_xSize()];
        for (int i = 0; i < xcoords.Length; i++)
        {
            xcoords[i] = i;
        }
        */

        int[] xcoords = new int[5] { 0, 1, 2, 3, 4 };
        int[] ycoords;
        switch (pref)
        {
            default: //i.e. case 0. 
                ycoords = new int[4] { 0, 1, 2, 3 };
                break;
            case 1:
                ycoords = new int[2] { 0, 1 };
                break;
            case 2:
                ycoords = new int[2] { 2, 3 };
                break;
        }


        //shuffle both xcoords and ycoords
        int temp;
        int rnd;
        for (int i = 0; i < xcoords.Length - 1; i++)
        {
            rnd = UnityEngine.Random.Range(i, xcoords.Length);
            temp = xcoords[rnd];
            xcoords[rnd] = xcoords[i];
            xcoords[i] = temp;
            
        }
        for (int i = 0; i < ycoords.Length - 1; i++)
        {
            rnd = UnityEngine.Random.Range(i, ycoords.Length);
            temp = ycoords[rnd];
            ycoords[rnd] = ycoords[i];
            ycoords[i] = temp;
        }

        bool exitNow = false;
        xStore = -1;
        yStore = -1;
        switch (pref)
        {
            default: //can do later. case 0.
                //temp - do not run with
                
                break;

            case 1:              
                Debug.Log("reinforce 1");
                //check each tile in the front two rows
                for (int c = 0; c < 2; c++)
                {
                    if (exitNow) break;
                    for (int r = 0; r < 5; r++)
                    {

                        if (eMap.search_enemy(ycoords[r], xcoords[c]) == null)
                        {
                            //we've found a spot.
                            Debug.Log("spot found");
                            xStore = ycoords[r];
                            yStore = xcoords[c];
                            exitNow = true;
                            break;
                        }
                    }
                }              
                break;
                

            case 2:
                //check each tile in the front two rows
                
                for (int i = 0; i <= 4; i++) //all x tiles in a row
                {
                    if (exitNow) break;

                    for (int j = 2; j <= 3; j++) //move to next row
                    {
                        //now, treat the spot as if we want to sit there. i.e. check if its empty
                        //and if tiles that would be required by unit's size are also empty.

                        if (exitNow) break;

                        canSit = true;
                        for (int xfat = 0; xfat < reinforcement.get_xSize(); xfat++)
                        {
                            if (exitNow) break;

                            for (int yfat = 0; yfat < reinforcement.get_ySize(); yfat++)
                            {
                                if (exitNow) break;

                                if (eMap.search_enemy(xcoords[i] + xfat, ycoords[j] + yfat) != null)
                                {
                                    canSit = false;
                                }
                            }
                        }

                        if (canSit == true)
                        {
                            //we've found a spot.
                            xStore = xcoords[i];
                            yStore = ycoords[j];

                            //exit loop now
                            exitNow = true;
                        }
                    }
                }
                break;
        }
        //if we don't jump to FoundSpot, then it means we couldn't find a spot to place our unit. as such, cut our losses

        if ( xStore == -1) //checking only xStore is sufficient, since they are set together
        {
            Debug.Log("could not find spot");
            return;
        }

        //overwrite el
        Enemy[] replaceList = new Enemy[el.Length + 1];

        for (int i = 0; i < el.Length; i++)
        {
            replaceList[i] = el[i];
        }
        replaceList[replaceList.Length - 1] = Instantiate(reinforcement);
        

        //set up stat bar
        stats.get_el()[replaceList.Length - 1].gameObject.SetActive(true);
        replaceList[replaceList.Length - 1].stats = stats.get_el()[replaceList.Length - 1];

        //place enemy
        replaceList[replaceList.Length - 1].set_x(yStore);
        replaceList[replaceList.Length - 1].set_y(xStore);
        eMap.place_enemy(replaceList[replaceList.Length - 1]);

        //generate starting delay, setup unit
        replaceList[replaceList.Length - 1].generate_start_delay(true, currentTick);
        replaceList[replaceList.Length - 1].update_stats();

        el = replaceList;
    }
    public void remove_enemy(Enemy casualty)
    {
        //add unit's exp to running exp and unit's drops to lootlist:
        runningExp += casualty.get_exp();
        handle_drops(casualty);

        //removes enemy unit from el and stops showing their stats and sprite. also sets their tile(s) to null.

        if ( el.Length > 1)
        {
            Enemy[] replaceList = new Enemy[el.Length - 1];

            int count = 0;
            foreach (Enemy unit in el)
            {
                if (unit != casualty)
                {
                    replaceList[count] = unit;
                    count++;
                }
            }

            el = replaceList;
        }
        

        //disable casualty's ui stuff, hide their corpse, and make their tiles null
        eMap.remove_enemy(casualty);


        casualty.stats.gameObject.SetActive(false);
        Destroy(casualty.gameObject, 1.0f);
    }
    private void handle_drops(Enemy mob)
    {
        //functions runs probability checks and appropriately adds loot to runningLoot list

        if ( mob.get_drops() == null)
        {
            return;
        }

        for(int i = 0; i < mob.get_drops().Length; i++)
        {
            if (mob.get_dropChances()[i] > UnityEngine.Random.Range(1, 100)) //range of 1 to 99. loot w/ chance 100 will always be dropped.
            {
                runningLoot.Add(mob.get_drops()[i]);
            }
        }
    }

    //AFFINITY MULTIPLIER TEXT
    void show_affMultipliers()
    {
        //Debug.Log("hpbars for aff called");
        if (currentUnit.nextMove == null) return;

        //shows aff multiplier while spacebar is being held.
        if (allowButtonHover)
        {
            if (showingMults)
            {
                aff_for_hpbars(); //replace aff mults with hp bars      
                             
            }
            else
            {               
                hpbars_for_aff(currentUnit.nextMove.get_isHeal()); //replace hp bars with aff mults
            }
        }
    }
    void hpbars_for_aff(bool moveIsHeal)
    {
        //Debug.Log("hpbars for aff called");
        showingMults = true;

        if (moveIsHeal)
        {
            foreach (Enemy unit in el)
            {
                unit.stats.get_floatingHp().gameObject.SetActive(false); //disable hp bar
                eMap.tilegrid[unit.get_x(), unit.get_y()].GetComponent<EnemyTile>().show_aff_text("N/A", true);
            }
            return;
        }

        foreach (Enemy unit in el)
        {
            unit.stats.get_floatingHp().gameObject.SetActive(false); //disable hp bar

            //calculate what the aff mod would be:
            int atkAff;
            if (currentUnit.nextMove.get_ele() == -1)
            {
                atkAff = currentUnit.get_weapon().get_element();
            }
            else
            {
                atkAff = currentUnit.nextMove.get_ele();
            }
            double affMod = brain.get_aff_mod(atkAff, unit.get_aff(), unit.get_armour().get_element());
            eMap.tilegrid[unit.get_x(), unit.get_y()].GetComponent<EnemyTile>().show_aff_text(affMod.ToString(), false);
        }
    }
    void aff_for_hpbars()
    {
        //Debug.Log("aff for hpbars called");
        //delete the damage text
        //set hp bar to active
        showingMults = false;
        foreach (Enemy unit in el)
        {
            unit.stats.get_floatingHp().gameObject.SetActive(true); //enable hp bar
            eMap.tilegrid[unit.get_x(), unit.get_y()].GetComponent<EnemyTile>().delete_affText();
        }
    }
    void clear_affmults()
    {
        foreach (Enemy unit in el)
        {
            eMap.tilegrid[unit.get_x(), unit.get_y()].GetComponent<EnemyTile>().delete_affText();
        }
    }

    //PLAY BY PLAY
    void show_e_playbyplay(Enemy unit)
    {
        //anytime a unit uses a move, we display:
        // [b]unit[/b] uses [b]movename[/b]

        playByPlayText.text = unit.get_nom() + " uses " + unit.nextMove.get_title();

        playByPlay.SetActive(true);
    }
    void show_f_playbyplay(PlayerUnit unit, bool isRepositioning = false)
    {
        //anytime a unit uses a move, we display:
        // [unit] uses [movename]

        if ( isRepositioning)
        {
            playByPlayText.text = unit.get_nom() + " repositions";
        }
        else
        {
            playByPlayText.text = unit.get_nom() + " uses " + unit.nextMove.get_title();
        }
        playByPlay.SetActive(true);



    }
    void hide_playbyplay()
    {
        playByPlay.SetActive(false);
    }

    //HELP MENU
    public void toggle_helpScreen()
    {
        if (allowHelp)
        {
            //Debug.Log("help menu shown");
            if (helpShowing)
            {
                hide_helpScreen();
            }
            else
            {
                helpShowing = true;
                helpImage.SetActive(true);
            }            
        }        
    }
    void hide_helpScreen()
    {        
        //Debug.Log("help menu hidden");
        helpShowing = false;
        helpImage.SetActive(false);
    }

}

