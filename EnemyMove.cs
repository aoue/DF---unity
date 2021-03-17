using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--- ENEMY TARGETING LEGEND ---
//hostile target:
// 0: hit random target
// 1: hit enemy closest to the front
// 2: hit enemy closest to the back
// 3: hit enemy with lowest hp
// 4: hit enemy with highest hp
// 5: hit enemy with lowest pdef
// 6: hit enemy with lowest mdef
// 7: hit enemy weak to move's element
// 8: aoe, hits most targets

//friendly target:
// 0: random
// 1: least hp value
// 2: least hp percentage
// 3: most debuffed

public class EnemyMove : MonoBehaviour
{
    //Anatomy of a move:
    [SerializeField] private string title ;
    [SerializeField] private bool isHeal;
    [SerializeField] private bool iff; //if true, affects other map. if false, affects own map.

    //weighting and targeting
    [SerializeField] private bool appliesStatus; //does the move apply status?
    [SerializeField] private bool hasCustomHeal;
    [SerializeField] private bool ignoreHealMax;
    [SerializeField] private int weight; //higher means the move is less likely to be chosen. 
    [SerializeField] private int targeting; //int corresponding to a type of target prioritization
    [SerializeField] private int[] stamina_drain; //how much stamina the unit loses using this move. range

    [SerializeField] private int strikes = 1; //how many times the attack hits. usually 1.
    [SerializeField] private int hit;
    [SerializeField] private int power;
    [SerializeField] private int preDelay; //delay before using the move
    [SerializeField] private int recDelay; //delay after using the move
    [SerializeField] private int xsize;
    [SerializeField] private int ysize;
    [SerializeField] private int highlightType; //for non rectangular stuff
    [SerializeField] private int attackType; //0: attacker uses physical attack.   1: attacker uses magical attack.
    [SerializeField] private int defenseType; //0: defender uses physical. 1: defender uses magic.
    [SerializeField] private int ele; //element

    //clearance and movement stuff
    [SerializeField] private int clearX; //x movement
    [SerializeField] private int clearY; //y movement
    [SerializeField] private int clearType; //0:no movement    1:hop    2:full path

    public int[] get_targetArea()
    {
        int[] returnArray = new int[] { xsize, ysize };
        return returnArray;
    }
    //check clearance
    public bool check_clearance(int unit_x, int unit_y, EnemyMap eMap)
    {
        //called once in show_moves: if returns false, then the move is not interactable
        //called again in exert(), if the space is occupied, then the move fails.

        //MOVEMENT CHECK
        if (clearType == 0)
        {
            return true;
        }

        if ((unit_x + clearX >= 0) && (unit_x + clearX < 5) && (unit_y + clearY >= 0) && (unit_y + clearY < 4))
        {
            //hop: needs only end tile clear
            if (clearType == 1 && eMap.search_enemy(unit_y + clearY, unit_x + clearX) == null)  //works as intended. tested, good to go.
            {
                return true;
            }
            else //full path: needs every time in path to be clear
            {
                //NOTE:
                //units ALWAYS move vertical first, followed by horizontal.

                if (clearType == 2)
                {
                    //VERTICAL PORTION
                    if (clearY > 0) //if we're moving backwards
                    {
                        for (int i = unit_y + 1; i <= unit_y + clearY; i++) //vertical part
                        {
                            if (eMap.search_enemy(i, unit_x) != null)
                            {
                                return false;
                            }

                        }
                    }
                    else
                    {
                        if (clearY < 0) //if we're moving forwards
                        {
                            for (int i = unit_y - 1; i >= unit_y + clearY; i--) //vertical part
                            {
                                if (eMap.search_enemy(i, unit_x) != null)
                                {
                                    return false;
                                }

                            }
                        }
                    }

                    //HORIZONTAL PORTION
                    if (clearX > 0) //we're moving to the right
                    {
                        for (int i = unit_x + 1; i <= unit_x + clearX; i++)
                        {
                            if (eMap.search_enemy(unit_y + clearY, i) != null)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        if (clearX < 0) //we're moving to the left
                        {
                            for (int i = unit_x - 1; i >= unit_x + clearX; i--)
                            {
                                if (eMap.search_enemy(unit_y + clearY, i) != null)
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    return true;
                }

            }
        }
        return false;
    }

    public bool get_iff() { return iff; }
    public string get_title() { return title; }
    public bool get_isHeal() { return isHeal; }
    public bool get_appliesStatus() { return appliesStatus; }
    public bool get_hasCustomHeal() { return hasCustomHeal; }
    public bool get_ignoreHealMax() { return ignoreHealMax; }
    public int get_strikes() { return strikes; }
    public int get_hit() { return hit; }
    public int get_power() { return power; }
    public int get_preDelay() { return preDelay; }
    public int get_recDelay() { return recDelay; }
    public int get_xsize() { return xsize; }
    public int get_ysize() { return ysize; }
    public int get_highlightType() { return highlightType; }
    public int get_attackType() { return attackType; }
    public int get_defenseType() { return defenseType; }
    public int get_ele() { return ele; }
    public int get_targeting() { return targeting; }
    public int get_weight() { return weight; }
    public int[] get_stamina_drain() { return stamina_drain; }
    public int get_clearType() { return clearType; }
    public int get_clearX() { return clearX; }
    public int get_clearY() { return clearY; }

    public virtual void apply_status_heal(Enemy self, Enemy[] targets, CombatLogic cLog) { }
    public virtual void apply_status_attack(Enemy self, PlayerUnit[] targets, CombatLogic cLog, bool all_ooa) { }
    public virtual int calc_custom_heal(Enemy unit)
    {
        return -1;
    }

    //function must pass for enemy to select move
    public virtual bool check_conditions(Enemy self, PlayerMap pMap, EnemyMap eMap, PlayerUnit[] pl, Enemy[] el) { return true; }

}
