using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

//planned characters:
// mc/fri
// mueler/madds
// payton/yve
// nai/anton

public enum unitState { PREPARING, RECOVERING, DEFENDING, REPOSITION }
public class PlayerUnit : MonoBehaviour
{
    [SerializeField] private Sprite tilePortrait; //small image used for prebattle deployment in 'pattern'
    [SerializeField] private Sprite moveSelectPortrait; //this is a separate sprite than the unit's vn image set.
    [SerializeField] private Sprite critPortrait; //small image, long, but skinny. just eyes. like the s-craft from sky.
    [SerializeField] private int xSize; //unit's thiccness
    [SerializeField] private int ySize; //^

    public UnitStatsDisplay stats { get; set; } //responsible for only this one unit's stats
    public Stances status { get; set; } //stances

    [SerializeField] private Focus focus;

    public int xp { get; set; }
    [SerializeField] private int level;
    [SerializeField] private string nom;
    private int[] focusInts = new int[5]; //int list representing stat focuses

    public unitState state { get; set; }

    //combat stats
    [SerializeField] private int hpMax;
    [SerializeField] private int hp;
    [SerializeField] private int mpMax;
    [SerializeField] private int mp;
    [SerializeField] private int aff;
    [SerializeField] private int physa;
    [SerializeField] private int physd;
    [SerializeField] private int maga;
    [SerializeField] private int magd;
    [SerializeField] private int hit;
    [SerializeField] private int dodge;
    [SerializeField] private int speed;
    //focus
    [SerializeField] private Gear armour;
    [SerializeField] private Gear weapon;
    [SerializeField] private Gear acc;

    //map stuff
    //public int size;
    [SerializeField] private int x;
    [SerializeField] private int y;

    //pattern is implied by movelists.
    [SerializeField] private int[] equipTypes; //a gear type in each slot: armour, weapon, acc
    [SerializeField] private DefendMove defendEquipped; //a single move that is used to defend.
    [SerializeField] private DefendMove[] reserveDefend; //all the defend moves that are unequipped
    [SerializeField] private Passive passiveEquipped; //a single passive that is equipped
    [SerializeField] private Passive[] reservePassive; //all the passives that are unequipped

    //[SerializeField] private PlayerMove[] reserveFrontList; //all the front/back moves that are unequipped.
    //[SerializeField] private PlayerMove[] reserveBackList; //all the front/back moves that are unequipped.
    //^^replaced with lists
    private List<PlayerMove> reserveFrontList = new List<PlayerMove>();
    private List<PlayerMove> reserveBackList = new List<PlayerMove>();

    [SerializeField] private PlayerMove[] frontList; //size varies with the
    [SerializeField] private PlayerMove[] backList; //unit we're dealing with.

    //registering next move
    public bool ooa { get; set; } //out of action.
    public int nextX { get; set; }
    public int nextY { get; set; }
    public PlayerMove nextMove { get; set; }
    public int delay { get; set; }
    public int fullDelay { get; set; }

    void Awake()
    {
        hp = get_hpMax_actual();
        mp = get_mpMax_actual();
        status = new Stances();
        status.set_to_default();
        focus.focus_setup(this);
        //equipTypes = new int[3];
        //passiveEquipped = new Passive();
        //nextMove = new PlayerMove();
    }

    //some UI stuff
    public void update_stats()
    {
        //shows unit's stats on unit's displayer
        //string nom, int hp, int hpMax, int delay, int fullDelay
        stats.update_stats(hp, get_hpMax_actual(), delay, fullDelay, status.get_status_summary());
    }
    public int calc_delay(int deli)
    {
        //calcs delay based on fullDelay and unit's speed (actual)

        //method:
        //compounding speed. 

        //a move's preparation delay can be cut at most by 50 % at 150 speed
        //the first 25 speed reduces delay by 0.50 % per speed
        //the next 50 speed reduces delay by 0.375 % per speed
        //the next 75 speed reduces delay by 0.25 % per speed
        //max speed reduction is 50 %

        
        int speedTemp = get_speed_actual();
        double speedMod = 0; //speed after compounding
        double compound = 0; //multiplier for speedmod
        double pawn;
        int it = 0; //iteration through the do-while loop

        do {                 
            if (it == 0)
            {
                if (speedTemp > 25) pawn = 25;
                else pawn = speedTemp;
                compound = 0.5;
                it = 1;
            }
            else if (it == 1)
            {
                if (speedTemp > 50) pawn = 50;
                else pawn = speedTemp;
                compound = 0.375;
                it = 2;
            }
            else
            {
                if (speedTemp > 75) pawn = 75;
                else pawn = speedTemp;
                compound = 0.25;
            }
           
            speedMod += (pawn * compound);
            speedTemp -= (int)pawn;
            
        } while (speedTemp > 0);
        
              
        speedMod = Math.Min(speedMod, 50);
        
        return (int)Math.Ceiling(deli * ((double)(100 - speedMod) / 100));
    }

    public void generate_start_delay()
    {
        stats.get_nom().text = nom;
        stats.update_moveNamePreview("Recovering");

        Color color = new Color(253f / 255f, 248f / 255f, 11f / 255f); //yellow
        stats.update_color(color); //make it yellow

        
        state = unitState.RECOVERING;
        delay = 0;
        //fullDelay = 3;
        //fullDelay = 50;
        fullDelay = ((100 - get_speed_actual()) / 5) + calc_delay(UnityEngine.Random.Range(0, 6));

    }
    public void enter_recovery(bool isReposition = false, bool isCancel = false)
    {
        //recovery is not currently affected by unit speed.
        delay = 0;
        if (isReposition)
        {
            fullDelay = 10;
        }
        else if (isCancel)
        {
            fullDelay = 1;
        }
        else if (state == unitState.DEFENDING)
        {
            fullDelay = defendEquipped.get_recDelay();
        }
        else
        {
            fullDelay = nextMove.get_recDelay();
        }
        state = unitState.RECOVERING;

        stats.update_moveNamePreview("Recovering");
        Color color = new Color(253f / 255f, 248f / 255f, 11f / 255f); //yellow
        stats.update_color(color); //make it yellow
        update_stats();

        //drain mp
        if (!isCancel)
        {
            modify_mp(get_mp() - nextMove.get_mpDrain());
        }        

        stats.update_slider(fullDelay, false);
    }
    public void enter_prepare(int targetX, int targetY, int nextID)
    {
        if (nextID == -1) //reposition
        {
            state = unitState.REPOSITION;
            fullDelay = calc_delay(10);
            stats.update_moveNamePreview("Reposition");
        }
        else
        {
            if (y > 1) //front
            {
                nextMove = frontList[nextID];
            }
            else
            {
                nextMove = backList[nextID];              
            }
            state = unitState.PREPARING;
            fullDelay = calc_delay(nextMove.get_preDelay());
            stats.update_moveNamePreview(nextMove.get_title());
            stats.update_slider(fullDelay, false);
        }      

        nextX = targetX;
        nextY = targetY;
        delay = 0;
             
        Color color = new Color(28f / 255f, 77f / 255f, 253f / 255f); //blue        
        stats.update_color(color); //make it blue        
        update_stats();
        //Debug.Log("prep= " + fullDelay);    
    }

    //defend
    public void enter_defend()
    {        
        defendEquipped.exert(this);
    }
    
    //some combat stuff
    public void post_battle()
    {
        status.set_to_default();
        if (hp > get_hpMax_actual()) hp = get_hpMax_actual();
    }
    public void modify_hp_heal(int newHp, bool ignoreMax = false)
    {

        if (ignoreMax)
        {
            hp = Math.Max(0, newHp);
        }
        else
        {
            if (hp <= get_hpMax_actual())
            {
                hp = Math.Min(Math.Max(0, newHp), get_hpMax_actual());
            }
            else
            {
                hp = Math.Min(Math.Max(0, newHp), hp);
            }

            
        }
    }
    public void modify_hp_attack(int newHp)
    {      
        //hp = Math.Min(Math.Max(0, newHp), get_hpMax_actual());
        //unit is taking damage, so hp cannot go below 0
        hp = Math.Max(0, newHp);

        check_dead();        
    }
    public void modify_mp(int newMp)
    {
        mp = Math.Min(newMp, get_mpMax_actual());
    }
    public void check_dead()
    {
        if (hp == 0)
        {
            //check passives to see if really dead
            state = unitState.RECOVERING; //i guess...
            ooa = true;
            status.set_to_default();
            delay = 0;
            stats.go_ooa();
            //change sprite to ooa version

            //change delay bar to all blacked out or something

        }
    }

    //getting stats_actual
    public int get_hpMax_actual() { return (hpMax + weapon.get_hp() + armour.get_hp() + acc.get_hp()); }
    public int get_mpMax_actual() { return (mpMax + weapon.get_mp() + armour.get_mp() + acc.get_mp()); }
    public int get_physa_actual() { return (int)((physa + weapon.get_physa() + armour.get_physa() + acc.get_physa()) * status.pa); }
    public int get_physd_actual() { return (int)((physd + weapon.get_physd() + armour.get_physd() + acc.get_physd()) * status.pd); }
    public int get_maga_actual() { return (int)((maga + weapon.get_maga() + armour.get_maga() + acc.get_maga()) * status.ma); }
    public int get_magd_actual() { return (int)((magd + weapon.get_magd() + armour.get_magd() + acc.get_magd()) * status.md); }
    public int get_dodge_actual() { return (int)((dodge + weapon.get_dodge() + armour.get_dodge() + acc.get_dodge()) * status.dodge); }
    public int get_hit_actual() { return (int)((hit + weapon.get_hit() + armour.get_hit() + acc.get_hit()) * status.hit); }
    public int get_speed_actual() { return (int)((speed + weapon.get_speed() + armour.get_speed() + acc.get_speed()) * status.speed); }

    public DefendMove get_defendEquipped(){return defendEquipped;}
    public DefendMove[] get_reserveDefend(){return reserveDefend;}
    public Passive get_passiveEquipped(){return passiveEquipped;}
    public Passive[] get_reservePassive(){return reservePassive;}
    public PlayerMove[] get_frontList(){return frontList;}
    public PlayerMove[] get_backList(){return backList;}
    public List<PlayerMove> get_reserveFrontList(){return reserveFrontList;}
    public List<PlayerMove> get_reserveBackList(){return reserveBackList;}
    public Sprite get_moveSelectPortrait() {return moveSelectPortrait;}

    public void set_level(int newLevel) { level = newLevel; }
    public void set_defendEquipped(DefendMove defg) { defendEquipped = defg; }
    public void set_passiveEquipped(Passive pasg) { passiveEquipped = pasg; }
    public void set_armour(Gear arm) { armour = arm; }
    public void set_weapon(Gear wep) { weapon = wep; }
    public void set_acc(Gear ac) { acc = ac; }
    
    public Sprite get_tilePortrait() { return tilePortrait; }
    public string get_nom() { return nom; }
    public int get_x() { return x; }
    public int get_y() { return y; }
    public void set_x(int newX) { x = newX; }
    public void set_y(int newY) { y = newY; }
    public Gear get_armour() { return armour; }
    public Gear get_weapon() { return weapon; }
    public Gear get_acc() { return acc; }
    public int get_level() { return level; }
    public int get_aff() { return aff; }
    public int get_xSize() { return xSize; }
    public int get_ySize() { return ySize; }
    public int get_hp() { return hp; }
    public int get_mp() { return mp; }
    public int get_mpMax() { return mpMax; }
    public int get_hpMax() { return hpMax; }
    public int get_physa() { return physa; }
    public int get_physd() { return physd; }
    public int get_maga() { return maga; }
    public int get_magd() { return magd; }
    public int get_hit() { return hit; }
    public int get_dodge() { return dodge; }
    public int get_speed() { return speed; }
    public int[] get_equipTypes() { return equipTypes; }


    //level up stat increases
    public Focus get_focus() { return focus; }
    public int[] get_focusInts() { return focusInts; }
    public void inc_hp(int inc) { hpMax += inc; hp += inc; }
    public void inc_mp(int inc) { mpMax += inc; mp += inc; }
    public void inc_pa(int inc) { physa += inc; }
    public void inc_pd(int inc) { physd += inc; }
    public void inc_ma(int inc) { maga += inc; }
    public void inc_md(int inc) { magd += inc; }
    public void inc_hit(int inc) { hit += inc; }
    public void inc_dodge(int inc) { dodge += inc; }
    public void inc_speed(int inc) { speed += inc; }

}



