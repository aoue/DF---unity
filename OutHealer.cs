using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutHealer : MonoBehaviour
{
    //out of battle healer.
    //uses healing moves out of battle.
    //opened by pressing the 'h' key.

    // -can use any healing move a unit knows, not just equipped moves. show in a scroll view.
    //healing moves: (all shown in descr box)
    // -targets: either 1 or all
    // -power: move's power
    // -cost: an mp cost
    // -description:

    //leftmost column: (hover to lock scroller of moves). (click to lock scroller of moves)
    //4 boxes, using unit's portraits 
    [SerializeField] private Button[] actorUnits;

    //x buttons in a scrollview
    [SerializeField] private GameObject MoveScroller; //the scroller.

    //move descr box and text in that box
    [SerializeField] private GameObject moveDescrBox;
    [SerializeField] private Text moveDescrText;

    //4 boxes using unit's portraits. used to select target of heals.
    //for 1 target move: heal the clicked unit on click and highlight hovered unit on mouseover.
    //for 4 target moves: heal all units on click and highlight all units on mouseover.
    [SerializeField] private Button[] targetUnits;

    //variers:
    private PlayerMove[] validMoveList; //a filtered move list from locked actor. all the moves that are outofbattle heal moves.
    private int lockedActor; //what unit to return to on lock.
    private int lockedMove; //what move to return to on lock.
    private bool targetsAll; //true if all units are affected by locked move
    private bool canPerformHeal; //true if a move is locked and usable. safety for the system.
    private PlayerUnit[] party; //the party. imported from worldManager when opened.
    private bool showing; //true if this screen is on.

    void Update()
    {
        if (showing && Input.GetKeyDown(KeyCode.H))
        {
            close_outHealer();
        }
    }

    void update_bars()
    {
        //set targetunits[i]'s hp and mp bars.
        for (int i = 0; i < party.Length; i++)
        {
            Slider hpSlider = targetUnits[i].transform.GetChild(0).GetComponent<Slider>();
            Slider mpSlider = targetUnits[i].transform.GetChild(1).GetComponent<Slider>();

            hpSlider.maxValue = party[i].get_hpMax_actual();
            hpSlider.value = party[i].get_hp();

            mpSlider.maxValue = party[i].get_mpMax_actual();
            mpSlider.value = party[i].get_mp();
        }       
    }
    public void open_outHealer(PlayerUnit[] wParty)
    {
        //called from WorldManager.
        showing = true;
        party = wParty;


        //set up actors
        lockedActor = -1;
        for(int i = 0; i < party.Length; i++)
        {
            //assign images to these guys. (both actors and targets)
            actorUnits[i].gameObject.GetComponent<Image>().sprite = party[i].get_tilePortrait();
            actorUnits[i].gameObject.SetActive(true);
            

            targetUnits[i].gameObject.GetComponent<Image>().sprite = party[i].get_tilePortrait();
            targetUnits[i].gameObject.SetActive(true);
            targetUnits[i].interactable = false;           
        }
        update_bars();
        for (int i = party.Length; i < 4; i++)
        {
            actorUnits[i].gameObject.SetActive(false);
            targetUnits[i].gameObject.SetActive(false);
        }


        //set up move scroller: (make empty)
        MoveScroller.GetComponent<PopulateScroller>().clear();
        lockedMove = -1;
        canPerformHeal = false;
        //how to make it empty? happens automatically?

        //set up moveDescrBox
        moveDescrText.text = "dummy text - you shouldn't ever see this."; //dummy text
        moveDescrBox.gameObject.SetActive(false);

        gameObject.SetActive(true);
    }
    public void close_outHealer()
    {
        //closes outHealer. (through worldmanager)
        gameObject.SetActive(false);
        showing = false;
        WorldManager.isPaused = false;
        WorldManager._instance.show_owPreviews();
    }



    public void hover_actor(int actorIndex)
    {
        //called when a button in actorUnits in hovered.
        //fills scrollview with actor's moves.

        //this function can wait to be filled out.
    }
    public void click_actor(int actorIndex)
    {
        //called when a button in actorUnits is clicked
        //sets lockedActor and move scroller.

        //clear move scroller.
        moveDescrBox.gameObject.SetActive(false);
        MoveScroller.GetComponent<PopulateScroller>().clear();

        toggle_targets_off();
        lockedActor = actorIndex;

        //entryView.GetComponent<PopulateScroller>().populate_entries(topicList[chp]);

        //create healing move list from locked unit:
        List<PlayerMove> moveList = new List<PlayerMove>();
        moveList.AddRange(party[lockedActor].get_frontList());
        moveList.AddRange(party[lockedActor].get_backList());
        moveList.AddRange(party[lockedActor].get_reserveFrontList());
        moveList.AddRange(party[lockedActor].get_reserveBackList());

        moveList.RemoveAll(r => !r.get_isOutOfBattleHeal());
        validMoveList = moveList.ToArray();

        
        MoveScroller.GetComponent<PopulateScroller>().populate_outH_moves(validMoveList, party[lockedActor].get_mp());
    }


    public void hover_move(int moveIndex)
    {
        //called when a move button in the scroller is hovered.
        //sets moveDescrText

        //move used is equal to:
        //validMoveList[moveIndex];
    }
    public void click_move(int moveIndex)
    {
        //called when a move button in the scroller is clicked.
        //
        lockedMove = moveIndex;
        canPerformHeal = false;
        //move used is equal to:
        //;

        if (validMoveList[lockedMove].get_mpDrain() <= party[lockedActor].get_mp())
        {
            canPerformHeal = true;
            toggle_targets_on();
            
            //set targetsAll          
        }

        //enable/fill text of descr box.
        if (validMoveList[lockedMove].get_xsize() > 1 || validMoveList[lockedMove].get_ysize() > 1)
        {
            targetsAll = true;
        }
        else
        {
            targetsAll = false;
        }
        moveDescrText.text = validMoveList[lockedMove].get_title() + "\n"
            + "STR" + validMoveList[lockedMove].get_power() + "\n"
            + validMoveList[lockedMove].get_descr3() + "\nTargets";
        if (targetsAll)
        {
            moveDescrText.text += "All.";
        }
        else
        {
            moveDescrText.text += "1.";
        }


        moveDescrBox.gameObject.SetActive(true);
    }

    void toggle_targets_on()
    {
        for ( int i = 0; i < party.Length; i++ )
        {
            targetUnits[i].interactable = true;
            targetUnits[i].gameObject.SetActive(true);
        }
    }
    void toggle_targets_off()
    {
        for (int i = 0; i < party.Length; i++)
        {
            targetUnits[i].interactable = false;
            //targetUnits[i].gameObject.SetActive(false);
        }
    }

    public void hover_target(int targetIndex)
    {
        //called when a button in targetUnits in hovered
        // depending on targetsAll, will highlight just target or all targets.


    }
    public void click_target(int targetIndex)
    {
        //called when a button in targetUnits in hovered.
        //performs an appropriate heal and subtracts mp from actor.

        //updates moves to see if actor still retains enough mp to use the same move again.

        if (targetsAll)
        {
            //then heal all units in party
            foreach(PlayerUnit unit in party)
            {
                //calc heal using brain.
                int heal = calc_heal(party[lockedActor], unit, validMoveList[lockedMove]);
                unit.modify_hp_heal(unit.get_hp() + heal, false);
                Debug.Log("healed " + unit.get_nom() + "for " + heal.ToString());
            }

        }
        else
        {
            //then heal only the clicked unit

            //calc heal using brain.
            int heal = calc_heal(party[lockedActor], party[targetIndex], validMoveList[lockedMove]);
            party[targetIndex].modify_hp_heal(party[targetIndex].get_hp() + heal, false);
            Debug.Log("healed " + party[targetIndex].get_nom() + "for " + heal.ToString());
        }
        int drain = validMoveList[lockedMove].get_mpDrain();
        party[lockedActor].modify_mp(party[lockedActor].get_mp() - drain);
        update_bars();
        click_move(lockedMove);

    }
    
    int calc_heal(PlayerUnit unit, PlayerUnit target, PlayerMove move)
    {
        int atk = 0;
        int def = 0;

        if (move.get_attackType() == 0)
        {
            //uses physical.
            atk = unit.get_physa_actual();
            def = target.get_physd_actual();
        }
        else
        {
            //uses magical.
            atk = unit.get_maga_actual();
            def = target.get_magd_actual();
        }

        float spread = UnityEngine.Random.Range(0.85f, 1.0f);
        int heal = System.Convert.ToInt32(((atk * (move.get_power() + unit.get_level())) / def) * spread);

        target.status.trigger_healed(target);
        unit.status.trigger_heal(unit);

        return heal;

    }

}
