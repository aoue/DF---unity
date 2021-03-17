using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//an explanation of the weighting/stamina system:
// to use a move, enemy must have clearance, and roll less than weight. call the roll 'chance'
//chance = random(1, 100). moves have a weight. if weight+stamina > chance, the move is picked to be used.
//when a move is picked, drain stamina right away.
//when entering recovery, regen stamina right away.



public class Enemy : MonoBehaviour
{
    [SerializeField] private Sprite critPortrait;
    [SerializeField] private int xSize; //unit's thiccness
    [SerializeField] private int ySize; //^

    public UnitStatsDisplay stats { get; set; } //responsible for only this one unit's stats
    public EnemyStances status { get; set; } //stances
    [SerializeField] private string nom;
    
    public int delay{ get; set; }
    public int fullDelay{ get; set; }

    public unitState state { get; set; }

    //combat stats
    [SerializeField] private int placementType; //0: dont care, 1:front, 2:back
    [SerializeField] private Loot[] drops;
    [SerializeField] private int[] dropchances;
    [SerializeField] private int exp; 
    [SerializeField] private int aiTier; //0: nothing, 1: cancel moves, 2: dodge
    [SerializeField] private int hpMax;
    [SerializeField] private int hp;

    
    public bool ooa{ get; set; } //out of action.
    [SerializeField] private int level;
    [SerializeField] private int aff;
    [SerializeField] private int physa;
    [SerializeField] private int physd;
    [SerializeField] private int maga;
    [SerializeField] private int magd;
    [SerializeField] private int hit;
    [SerializeField] private int dodge;
    [SerializeField] private int speed;

    [SerializeField] private Gear armour;
    [SerializeField] private Gear weapon;
    [SerializeField] private Gear acc;

    [SerializeField] private int[] staminaRegen;
    [SerializeField] private int staminaMax;
    private int stamina;
    public EnemyMove nextMove { get; set; }
    public int nextX{ get; set; }
    public int nextY{ get; set; }

    //map stuff
    [SerializeField] private int x;
    [SerializeField] private int y;

    [SerializeField] private Passive passiveEquipped; //a single passive that is equipped
    [SerializeField] private EnemyMove[] frontList; //size varies with the
    [SerializeField] private EnemyMove[] backList; //unit we're dealing with.
    

    public Passive get_passiveEquipped() { return passiveEquipped; }

    void Awake()
    {
        //passiveEquipped = new Passive();
        
        status = new EnemyStances();
        //status.set_to_default();

    }

    //combat values being set stuff
    public void modify_hp_heal(int newHp, bool ignoreMax = false)
    {
        if (ignoreMax)
        {
            hp = newHp;
        }
        else
        {
            hp = Math.Min(Math.Max(0, newHp), get_hpMax_actual());
        }
    }
    public void modify_hp_attack(int newHp)
    {
        //hp = Math.Min(Math.Max(0, newHp), get_hpMax_actual());
        //unit is taking damage, so hp cannot go below 0
        hp = Math.Max(0, newHp);

        check_dead();
    }
    public void check_dead()
    {
        if (hp == 0)
        {
            ooa = true;

            /*
            //passive stuff
            if (passiveEquipped.type == 5)
            {
                passiveEquipped.e_exert(this);
            }
            if (armour.get_hasPassive() && armour.get_passive().type == 5)
            {
                armour.get_passive().e_exert(this);
            }
            if (acc.get_hasPassive() && acc.get_passive().type == 5)
            {
                acc.get_passive().e_exert(this);
            }
            */
        }
    }

    //UI and delay stuff
    public void update_stats()
    {
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

        do
        {
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
    public void generate_start_delay(bool isReinforcement = false, int reinforceTick = -1)
    {
        stats.get_nom().text = nom;
        stats.update_moveNamePreview("Recovering");
        hp = get_hpMax_actual();
        //makes the delay that the unit starts the battle with. depends on speed and randomness.
        //unit will be ready to act when it reaches 0. the lower the number, the longer the unit will have to wait.
        Color color = new Color(253f / 255f, 248f / 255f, 11f / 255f); //yellow
        stats.update_color(color); //make it yellow

        //set stamina to half of max
        stamina = staminaMax / 2;

        status.set_to_default();
        state = unitState.RECOVERING;

        if (isReinforcement)
        {
            fullDelay = calc_delay(UnityEngine.Random.Range(7, 13));
            stats.update_slider((reinforceTick % 100) + fullDelay, true);                      
        }
        else
        {
            delay = 0;
            //fullDelay = 2;
            //fullDelay = 40;
            //fullDelay = 90;
            fullDelay = ((100 - get_speed_actual()) / 5) + calc_delay(UnityEngine.Random.Range(0, 6));
        }

        
    }
    public void enter_recovery()
    {
        //recovery delay is not currently affected by unit speed
        stats.update_moveNamePreview("Recovering");

        Color color = new Color(253f / 255f, 248f / 255f, 11f / 255f); //yellow
        stats.update_color(color); //make it yellow
        state = unitState.RECOVERING;
        delay = 0;
        fullDelay = nextMove.get_recDelay();
        update_stats();
        stats.update_slider(fullDelay, false);

        regen_stamina();
    }
    public void enter_prepare()
    {
        stats.update_moveNamePreview(nextMove.get_title());
        Color color = new Color(28f / 255f, 77f / 255f, 253f / 255f); //blue        
        stats.update_color(color); //make it blue
        state = unitState.PREPARING;
        delay = 0;
        fullDelay = calc_delay(nextMove.get_preDelay());
        update_stats();
        stats.update_slider(fullDelay, false);
    }

    //AI
    public void e_choose_move(PlayerMap pMap, EnemyMap eMap, PlayerUnit[] pl, Enemy[] el)
    {
        //METHOD
        // -pick move using weighting system.
        // -choose target/location of target using move's prioritization.
        // -log move

        nextMove = pick_move(eMap, pMap, pl, el);

        if (nextMove.get_isHeal())
        {
            //move is a heal, so pick target on enemymap
            pick_friendly_target(nextMove.get_targeting(), eMap, el, nextMove.get_xsize(), nextMove.get_ysize()); //finds coordinates of tile through pMap's tilegrid object.
        }
        else
        {
            //move is an attack, so pick target on playermap
            pick_hostile_target(nextMove.get_targeting(), pMap, pl, nextMove.get_xsize(), nextMove.get_ysize()); //finds coordinates of tile through pMap's tilegrid object.
        }

        enter_prepare(); //set up delay and stuff.
    }
    EnemyMove pick_move(EnemyMap eMap, PlayerMap pMap, PlayerUnit[] pl, Enemy[] el)
    {
        //pick move section
        int chance = UnityEngine.Random.Range(1, 101); //high weight means move is more likely to be used
        EnemyMove[] moveList;
        int chosen = 0;
        if ( y < 2) //if we're in front
        {
            moveList = frontList;
        }
        else //if we're in back
        {
            moveList = backList;
        }

        //cycle through moveList until we find a suitable move.
        //requirements for a move to be eligible
        // -move's weight <= generated weight
        // -move must pass clearance

        for (int i = 0; i < moveList.Length; i++)
        {
            //move is chosen iff:
            // -stamina and weighting allows
            // -clearance allows
            // -move's unique check_conditions() function also allows
            if ((moveList[i].get_weight() + stamina) > chance && moveList[i].check_clearance(x, y, eMap) && moveList[i].check_conditions(this, pMap, eMap, pl, el))
            {
                chosen = i;
                break;
            }
        }

        //movelists need to be designed so that the unit can always choose a move, no matter the circumstances

        drain_stamina(moveList[chosen].get_stamina_drain()); //drain stamina right now

        //return move
        return moveList[chosen];
    }
    void pick_hostile_target(int targeting, PlayerMap pMap, PlayerUnit[] pl, int aoe_x, int aoe_y)
    {
        //check chosen move's targeting prioritization. picks a unit's tile and returns the x, y of their tile.
        //LEGEND:
        // 0: hit random target 
        // 1: hit enemy closest to the front 
        // 2: hit enemy closest to the back 
        // 3: hit enemy with lowest hp 
        // 4: hit enemy with highest hp 
        // 5: hit enemy with lowest pdef 
        // 6: hit enemy with lowest mdef 
        // 7: hit enemy weak to move's element
        // 8: aoe, most targets 
        int chosen;
        int chosenIndex;
        List<PlayerUnit> f_targets = new List<PlayerUnit>();
        switch (targeting)
        {
            case 0:
                // 0: hits a random target.
                bool find = true;
                while (find)
                {
                    chosen = UnityEngine.Random.Range(0, pl.Length);
                    if (pl[chosen].ooa == false)
                    {
                        nextX = pl[chosen].get_y();
                        nextY = pl[chosen].get_x();
                        
                        find = false;
                    }
                }
                break;
            case 1:
                // 1: hits enemy nearest to the front. if multiple units equally close to the front, randomly pick between them.
                for (int c = 3; c >= 0; c--)                             
                {
                    for (int r = 4; r >= 0; r--)
                    {
                        if (pMap.search_unit(c, r) != null)
                        {
                            if (!f_targets.Contains(pMap.search_unit(c, r)) && pMap.search_unit(c, r).ooa == false)
                            {
                                f_targets.Add(pMap.search_unit(c, r));
                            }
                        }
                    }

                    //after we've finished a row, check if there's any units in f_targets
                    if (f_targets.Count > 0)
                    {
                        chosen = UnityEngine.Random.Range(0, f_targets.Count);

                        nextX = f_targets[chosen].get_y();
                        nextY = f_targets[chosen].get_x();
                        break;
                    }
                }
                break;
            case 2:
                // 2: hits enemy nearest to the back. if multiple units equally close to the back, randomly pick between them.
                for (int c = 0; c < 4; c++)
                {
                    for (int r = 0; r < 5; r++)
                    {
                        if (pMap.search_unit(c, r) != null)
                        {
                            if (!f_targets.Contains(pMap.search_unit(c, r)) && pMap.search_unit(c, r).ooa == false)
                            {
                                f_targets.Add(pMap.search_unit(c, r));
                            }
                        }
                    }
                    //after we've finished a row, check if there's any units in f_targets
                    if (f_targets.Count > 0)
                    {
                        chosen = UnityEngine.Random.Range(0, f_targets.Count);

                        nextX = f_targets[chosen].get_y();
                        nextY = f_targets[chosen].get_x();
                        break;
                    }
                }
                break;
            case 3:
                // 3: hit enemy with lowest hp
                chosen = pl[0].get_hp();

                for (int i = 1; i < pl.Length; i++)
                {
                    if ( pl[i].ooa == false && pl[i].get_hp() <= chosen)
                    {
                        if (chosen == pl[i].get_hp())
                        {
                            f_targets.Add(pl[i]);
                        }
                        else
                        {
                            f_targets.Clear();
                            f_targets.Add(pl[i]);
                        }
                        chosen = pl[i].get_hp();
                    }
                }
                chosen = UnityEngine.Random.Range(0, f_targets.Count);
                nextX = f_targets[chosen].get_y();
                nextY = f_targets[chosen].get_x();
                break;
            case 4:
                // 4: hit enemy with highest hp
                chosen = pl[0].get_hp();

                for (int i = 1; i < pl.Length; i++)
                {
                    if (pl[i].ooa == false && pl[i].get_hp() >= chosen)
                    {
                        if (chosen == pl[i].get_hp())
                        {
                            f_targets.Add(pl[i]);
                        }
                        else
                        {
                            f_targets.Clear();
                            f_targets.Add(pl[i]);
                        }
                        chosen = pl[i].get_hp();
                    }
                }
                chosen = UnityEngine.Random.Range(0, f_targets.Count);
                nextX = f_targets[chosen].get_y();
                nextY = f_targets[chosen].get_x();
                break;
            case 5:
                // 5: hit enemy with lowest pdef
                chosen = pl[0].get_physd_actual();
                f_targets.Add(pl[0]);

                for (int i = 1; i < pl.Length; i++)
                {
                    if (pl[i].ooa == false && pl[i].get_physd_actual() <= chosen)
                    {
                        if (chosen == pl[i].get_physd_actual())
                        {
                            f_targets.Add(pl[i]);
                        }
                        else
                        {
                            f_targets.Clear();                            
                            f_targets.Add(pl[i]);
                        }
                        chosen = pl[i].get_physd_actual();
                    }
                }
                chosen = UnityEngine.Random.Range(0, f_targets.Count);
                nextX = f_targets[chosen].get_y();
                nextY = f_targets[chosen].get_x();
                break;
            case 6:
                // 6: hit enemy with lowest mdef
                chosen = pl[0].get_magd_actual();
                f_targets.Add(pl[0]);

                for (int i = 1; i < pl.Length; i++)
                {
                    if (pl[i].ooa == false && pl[i].get_magd_actual() <= chosen)
                    {
                        if (chosen == pl[i].get_magd_actual())
                        {
                            f_targets.Add(pl[i]);
                        }
                        else
                        {
                            f_targets.Clear();                           
                            f_targets.Add(pl[i]);
                        }
                        chosen = pl[i].get_magd_actual();
                    }
                }
                chosen = UnityEngine.Random.Range(0, f_targets.Count);
                nextX = f_targets[chosen].get_y();
                nextY = f_targets[chosen].get_x();
                break;
            case 7:
                // 7: hit enemy weakest to move's element

                double chosend = WorldManager.get_eleMod(pl[0].get_aff(), nextMove.get_ele());
                f_targets.Add(pl[0]);

                for (int i = 1; i < pl.Length; i++)
                {
                    if (pl[i].ooa == false && WorldManager.get_eleMod(pl[i].get_aff(), nextMove.get_ele()) <= chosend)
                    {
                        if (chosend == WorldManager.get_eleMod(pl[i].get_aff(), nextMove.get_ele()))
                        {                        
                            f_targets.Add(pl[i]);
                        }
                        else
                        {
                            f_targets.Clear();
                            f_targets.Add(pl[i]);
                        }
                        chosend = WorldManager.get_eleMod(pl[i].get_aff(), nextMove.get_ele());
                    }
                }
                chosen = UnityEngine.Random.Range(0, f_targets.Count);
                nextX = f_targets[chosen].get_y();
                nextY = f_targets[chosen].get_x();
                break;
            case 8:
                // 8: aoe, most targets
                List<PlayerUnit> max_f_targets = new List<PlayerUnit>();
                int currentTargetNumber = 0;
                int maxTargetNumber = 0;
                //method:
                //for each tile:
                //  gen list of affected targets 
                //  take running max targetlist.
              
                for (int c = 0; c < 5 - aoe_y; c++)
                {
                    for (int r = 0; r < 6 - aoe_x; r++)
                    {
                        //generate aoe area for each tile
                        currentTargetNumber = 0;
                        for (int i = c; i < c + aoe_y; i++)
                        {
                            for (int j = r; j < r + aoe_x; j++)
                            {
                                //if tile is occupied
                                if (pMap.search_unit(i, j) != null)
                                {
                                    //and if unit on tile is not already in f_targets and is not ooa
                                    if (!f_targets.Contains(pMap.search_unit(i, j)) && pMap.search_unit(i, j).ooa == false)
                                    {
                                        f_targets.Add(pMap.search_unit(i, j));
                                        currentTargetNumber++;
                                    }
                                }
                            }
                        }
                        if (currentTargetNumber > 0 && currentTargetNumber >= maxTargetNumber)
                        {                           
                            
                            if (currentTargetNumber == maxTargetNumber)
                            {                                
                                //50% chance to replace if same target count,
                                chosen = UnityEngine.Random.Range(0, 2); //can be 0 or 1
                                //Debug.Log(chosen);
                                if (chosen == 0)
                                {
                                    max_f_targets = f_targets;
                                    nextX = c;
                                    nextY = r;

                                } //else, do nothing                                                       
                            }
                            else
                            {
                                max_f_targets = f_targets;
                                maxTargetNumber = currentTargetNumber;
                                nextX = c;
                                nextY = r;
                            }
                        }

                        f_targets.Clear();

                    }
                }
                break;
        }
        //Debug.Log(nom + ":attack logged at x,y = " + nextX + "," + nextY);
    }
    void pick_friendly_target(int targeting, EnemyMap eMap, Enemy[] el, int aoe_x, int aoe_y)
    {
        //pick unit from eMap based on targeting legend.
        //legend:
        // 0: random target
        // 1: least hp value
        // 2: least hp percentage
        // 3: most debuffed
        // 4: aoe, most targets

        List<Enemy> f_targets = new List<Enemy>();
        double chosen;
        int chosenIndex;

        switch (targeting)
        {
            case 0:
                // 0: random target
                bool find = true;
                while (find)
                {
                    chosenIndex = UnityEngine.Random.Range(0, el.Length);
                    if (el[chosenIndex].ooa == false)
                    {
                        nextX = el[chosenIndex].get_y();
                        nextY = el[chosenIndex].get_x();
                        find = false;
                    }
                }
                break;
            case 1:
                // 1: least hp
                chosenIndex = 0;
                chosen = el[0].get_hp();

                for (int i = 1; i < el.Length; i++)
                {
                    if (el[i].ooa == false && el[i].get_hp() <= chosen)
                    {
                        chosen = el[i].get_hp();
                        chosenIndex = i;
                    }

                }
                nextX = el[chosenIndex].get_y();
                nextY = el[chosenIndex].get_x();
                break;
            case 2:
                // 2: least proportional hp
                chosenIndex = 0;
                chosen = el[0].get_hp() / el[0].get_hpMax_actual();

                for (int i = 1; i < el.Length; i++)
                {
                    if (el[i].ooa == false && (el[i].get_hp() / el[i].get_hpMax_actual()) <= chosen)
                    {
                        chosen = el[i].get_hp() / el[i].get_hpMax_actual();
                        chosenIndex = i;
                    }
                }
                nextX = el[chosenIndex].get_y();
                nextY = el[chosenIndex].get_x();

                break;
            case 3:
                // most debuffed
                
                // todo
                //add a isDebuffed identifier to enemies, then, target the enemy with the highest score.
                //a move with this targeting type will be a move that applies the dispel status

                break;
            case 4:
                // 8: aoe, most targets
                List<Enemy> max_f_targets = new List<Enemy>();
                int currentTargetNumber = 0;
                int maxTargetNumber = 0;
                //method:
                //for each tile:
                //  gen list of affected targets 
                //  take running max targetlist.

                for (int c = 0; c < 5 - aoe_y; c++)
                {
                    for (int r = 0; r < 6 - aoe_x; r++)
                    {
                        //generate aoe area for each tile
                        currentTargetNumber = 0;
                        for (int i = c; i < c + aoe_y; i++)
                        {
                            for (int j = r; j < r + aoe_x; j++)
                            {
                                //if tile is occupied
                                if (eMap.search_enemy(i, j) != null)
                                {
                                    //and if unit on tile is not already in f_targets and is not ooa
                                    if (!f_targets.Contains(eMap.search_enemy(i, j)) && eMap.search_enemy(i, j).ooa == false)
                                    {
                                        f_targets.Add(eMap.search_enemy(i, j));
                                        currentTargetNumber++;
                                    }
                                }
                            }
                        }
                        if (currentTargetNumber > 0 && currentTargetNumber >= maxTargetNumber)
                        {

                            if (currentTargetNumber == maxTargetNumber)
                            {
                                //50% chance to replace if same target count,
                                chosen = UnityEngine.Random.Range(0, 2); //can be 0 or 1
                                //Debug.Log(chosen);
                                if (chosen == 0)
                                {
                                    max_f_targets = f_targets;
                                    nextX = c;
                                    nextY = r;

                                } //else, do nothing                                                       
                            }
                            else
                            {
                                max_f_targets = f_targets;
                                maxTargetNumber = currentTargetNumber;
                                nextX = c;
                                nextY = r;
                            }
                        }

                        f_targets.Clear();

                    }
                }
                break;
        }
    }
    void drain_stamina(int[] drain)
    {
        stamina -=  Math.Max(0, UnityEngine.Random.Range(drain[0], drain[1]));
    }
    void regen_stamina()
    {
        stamina += Math.Min(staminaMax, UnityEngine.Random.Range(staminaRegen[0], staminaRegen[1]));
    }

    /*to add:
    - prep cancelling
    - map dodging
    */
    public void checkUp_tick(PlayerMap pMap, EnemyMap eMap, BattleBrain brain)
    {
        //checks up on the unit each tick.
        /*looking for:
            - moves that will hit no one -> enter recovery.
            - if self is about to be hit and own nextMove has a low weight -> if a sidestep is possible -> enter prep to dodge.
        */

        if ( state == unitState.RECOVERING ) return;

        //AI TIER 1 - cancel prep
        if (aiTier < 1) return;

        if ( nextMove.get_iff() == true)
        {
            if (brain.f_check_targets(pMap, this) == false)
            {
                //our move would affect no one, so cancel it and enter recovery.
                cancel_move();                
            }
        }
        else
        {
            if (brain.e_check_targets(eMap, this) == false)
            {
                //our move would affect no one, so cancel it and enter recovery.
                cancel_move();
            }
        }

        //AI TIER 2
        // -sidestep/map dodges
        //if (aiTier < 2) return;

        //AI TIER 3
        // -?
        //if (aiTier < 3) return;
    }
    void cancel_move()
    {
        //a form of enter_recovery() that is called when the unit checks for targets in its predicted target area and finds none.

        stats.update_moveNamePreview("Recovering");
        Color color = new Color(253f / 255f, 248f / 255f, 11f / 255f); //yellow
        stats.update_color(color); //make it yellow
        
         delay = 0;
        fullDelay = calc_delay(nextMove.get_recDelay() / 2); //or whatever
        state = unitState.RECOVERING;
        update_stats();
        stats.update_slider(fullDelay, false);

        regen_stamina();
    }


    //getting stats_actual
    public int get_hpMax_actual() { return (hpMax + weapon.get_hp() + armour.get_hp() + acc.get_hp()); }
    public int get_physa_actual() { return (int)((physa + weapon.get_physa() + armour.get_physa() + acc.get_physa()) * status.pa); }
    public int get_physd_actual() { return (int)((physd + weapon.get_physd() + armour.get_physd() + acc.get_physd()) * status.pd); }
    public int get_maga_actual() { return (int)((maga + weapon.get_maga() + armour.get_maga() + acc.get_maga()) * status.ma); }
    public int get_magd_actual() { return (int)((magd + weapon.get_magd() + armour.get_magd() + acc.get_magd()) * status.md); }
    public int get_dodge_actual() { return (int)((dodge + weapon.get_dodge() + armour.get_dodge() + acc.get_dodge()) * status.dodge); }
    public int get_hit_actual() { return (int)((hit + weapon.get_hit() + armour.get_hit() + acc.get_hit()) * status.hit); }
    public int get_speed_actual() { return (int)((speed + weapon.get_speed() + armour.get_speed() + acc.get_speed()) * status.speed); }
    public int get_speed() { return speed; }
    public int get_x() { return x; }
    public int get_y() { return y; }
    public int get_xSize() { return xSize; }
    public int get_ySize() { return ySize; }
    public Gear get_armour() { return armour; }
    public Gear get_weapon() { return weapon; }
    public Gear get_acc() { return acc; }
    public int get_hp() { return hp; }
    public int get_level() { return level; }
    public int get_aff() { return aff; }
    public int get_exp() { return exp; }
    public Loot[] get_drops() { return drops; }
    public int[] get_dropChances() { return dropchances; }
    public string get_nom() { return nom; }
    public void set_x(int newX) { x = newX; }
    public void set_y(int newY) { y = newY; }
    public int get_placementType() { return placementType; }
    public int get_stamina() { return stamina; }
    public int get_staminaMax() { return staminaMax; }
}
