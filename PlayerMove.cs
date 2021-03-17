using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    //Anatomy of a move:
    [SerializeField] private string title; 
    [SerializeField] private string descr0;
    [SerializeField] private string descr1;
    [SerializeField] private string descr2;
    [SerializeField] private string descr3;

    [SerializeField] private bool isHeal;
    [SerializeField] private bool isFront;
    [SerializeField] private bool appliesStatus;
    [SerializeField] private bool hasCustomHeal;
    [SerializeField] private bool ignoreHealMax;

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
    [SerializeField] private int mpDrain; //amount of mp the move costs to use
    

    public int[] get_targetArea()
    {
        int[] returnArray = new int[] {xsize, ysize};
        return returnArray;
    }
    public string get_title() { return title; }
    public string get_descr0() { return descr0; }
    public string get_descr1() { return descr1; }
    public string get_descr2() { return descr2; }
    public string get_descr3() { return descr3; }
    public bool get_isHeal() { return isHeal; }
    public bool get_isFront() { return isFront; }
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
    public int get_clearType() { return clearType; }
    public int get_clearX() { return clearX; }
    public int get_clearY() { return clearY; }
    public int get_mpDrain() { return mpDrain; }
 
    //on return false, the move is not selectable
    public bool check_pre(int unit_x, int unit_y, PlayerMap pMap, int mp)
    {
        //called in show_moves() to see if the move is available to be used
        //MP CHECK
        if (mp < mpDrain)
        {
            return false;
        }

        //check if translation would move unit off of grid
        if (clearType == 0)
        {
            return true;
        }

        if ( !(unit_x + clearX >= 0) || !(unit_x + clearX < 5) || !(unit_y + clearY >= 0) || !(unit_y + clearY < 4)) //<-- this line.{
        {
            return false;
        }
        return true;
    }

    //on return false, the move is not selectable
    public bool check_post(int unit_x, int unit_y, PlayerMap pMap, int mp)
    {
        //called in exert() to make sure the move can still be used.
        //MP CHECK
        if (mp < mpDrain)
        {
            return false;
        }

        //check if translation would move unit off of grid or if unit would bump someone during their traversal
        if (clearType == 0)
        {
            return true;
        }
             
        if((unit_x + clearX >= 0) && (unit_x + clearX < 5) && (unit_y + clearY >= 0) && (unit_y + clearY < 4)) //<-- this line.
        {
            //hop: needs only end tile clear
            if (clearType == 1 && pMap.search_unit(unit_y + clearY, unit_x + clearX) == null)  //works as intended. tested, good to go.
            {
                return true;                
            }
            else //full path: needs every time in path to be clear
            {
                //NOTE:
                //units ALWAYS move vertical part first, followed by horizontal part.

                if (clearType == 2)
                {
                    //VERTICAL PORTION
                    if (clearY > 0) //if we're moving forwards
                    {
                        for (int i = unit_y + 1; i <= unit_y + clearY; i++) //vertical part
                        {
                            if (pMap.search_unit(i, unit_x) != null)
                            {
                                return false;
                            }

                        }
                    }
                    else if (clearY < 0) //if we're moving backwards
                    {
                        for (int i = unit_y - 1; i >= unit_y + clearY; i--) //vertical part
                        {
                            if (pMap.search_unit(i, unit_x) != null)
                            {
                                return false;
                            }

                        }
                    }
                    

                    //HORIZONTAL PORTION
                    if(clearX > 0) //we're moving to the right
                    {
                        for (int i = unit_x + 1; i <= unit_x + clearX; i++)
                        {
                            if (pMap.search_unit(unit_y + clearY, i) != null)
                            {
                                return false;
                            }
                        }
                    }
                    else if (clearX < 0) //we're moving to the left
                    {
                        for (int i = unit_x - 1; i >= unit_x + clearX; i--)
                        {
                            if (pMap.search_unit(unit_y + clearY, i) != null)
                            {
                                return false;
                            }
                        }
                    }
                    
                    
                    return true;
                }
                
            }
        }
        return false;
    }

    //TODO: a function that highlights the tiles a move would pass over. will be called when a move is passed over.
    // e.g. the player expected a move to be usable and mouses over to see where the move bumps or whatever.

    //tilegrid[4, 3].GetComponent<SpriteRenderer>().color = new Color(35f / 255f, 222f / 255f, 0f / 255f);
    //^this highlights a tile. use for testing.

    public virtual void apply_status_heal(PlayerUnit self, PlayerUnit[] targets, CombatLogic cLog) { }
    public virtual void apply_status_attack(PlayerUnit self, Enemy[] targets, CombatLogic cLog, bool all_ooa) { }
    public virtual int calc_custom_heal(PlayerUnit unit)
    {
        return -1;
    }

}
