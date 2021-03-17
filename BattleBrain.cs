using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class BattleBrain : MonoBehaviour
{
    //does calculations and stuff for the combat. will handle everything. is an object of combat logic.
    public CombatLogic cLog { get; set; } //for removing dead units from el

    //target lists
    public bool all_ooa { get; set; } //true if all targets were put ooa. checked for some passives.
    List<PlayerUnit> f_targets = new List<PlayerUnit>();
    List<Enemy> e_targets = new List<Enemy>();

    public List<PlayerUnit> get_f_targets() { return f_targets; }
    public List<Enemy> get_e_targets() { return e_targets; }
    
    //Movement functions
    public void f_translate(PlayerUnit unit, PlayerMap pMap, int displaceX, int displaceY)
    {
        //moves a playerunit to a new location. displaceX,Y are how far the unit is move in those directions.
        pMap.remove_unit(unit);
        //pMap.restore_old_tile(unit.get_x(), unit.get_y());
        unit.set_x(Math.Max(Math.Min(unit.get_x() + displaceX, 4), 0)); //must be between 0 and 4, inclusive
        unit.set_y(Math.Max(Math.Min(unit.get_y() + displaceY, 3), 0)); //must be between 0 and 3, inclusive
        pMap.place_unit(unit);

        //pMap.select_move_highlight(unit.get_x(), unit.get_y()); //<--highlights new tile correctly.
        unit.status.trigger_move(unit);
    }
    public void e_translate(Enemy unit, EnemyMap eMap, int displaceX, int displaceY)
    {
        //moves a playerunit to a new location. displaceX,Y are how far the unit is move in those directions.
        eMap.remove_enemy(unit);
        //pMap.restore_old_tile(unit.get_x(), unit.get_y());
        unit.set_x(Math.Max(Math.Min(unit.get_x() + displaceX, 4), 0)); //must be between 0 and 4, inclusive
        unit.set_y(Math.Max(Math.Min(unit.get_y() + displaceY, 3), 0)); //must be between 0 and 3, inclusive
        eMap.place_enemy(unit);

        //pMap.select_move_highlight(unit.get_x(), unit.get_y()); //<--highlights new tile correctly.
        unit.status.trigger_move(unit);
    }

    //Some general functions
    public IEnumerator show_battleNumbers(GameObject floaterBar, float timeHidden)
    {
        //plays nice with damage/heal numbers and floating hp bar
        floaterBar.SetActive(false);
        yield return new WaitForSeconds(timeHidden);
        floaterBar.SetActive(true);
        yield return null;
    }
    void reset_targetLists()
    {
        f_targets.Clear();
        e_targets.Clear();
    }
    float get_spread(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }
    public double get_aff_mod(int ele, int defAff1, int defAff2)
    {
        //takes the average of the mods for attacking element with each of the target's natural affinity and target's gear affinity.
        return ((calc_aff(ele, defAff1) + calc_aff(ele, defAff2)) / 2);
    }
    double calc_aff(int ele, int defAff)
    {
        //ele: move affinity
        //defAff = defender's affinity
        //return affList[defAff, ele];

        return WorldManager.get_eleMod(ele, defAff);
    }

    //checks for targets. for enemy's checkUp ticks.
    public bool f_check_targets(PlayerMap pMap, Enemy unit)
    {
        //returns false if there are no targets left in the area.
        EnemyMove move = unit.nextMove;

        //index:
        //0: rectangular
        //1: 1x1 self
        //2: 3x3 centerless cross
        //3: 3x3 cross
        switch (move.get_highlightType())
        {
            case 0: //standard rectangular
                for (int c = unit.nextX; c < (unit.nextX + move.get_ysize()); c++)
                {
                    for (int r = unit.nextY; r < (unit.nextY + move.get_xsize()); r++)
                    {
                        if (pMap.search_unit(c, r) != null)
                        {
                            return true;
                        }
                    }
                }
                break;
            case 2: //3x3 centerless cross.
                if (pMap.search_unit(unit.nextX - 1, unit.nextY) != null)
                {
                    return true;
                }


                if (pMap.search_unit(unit.nextX + 1, unit.nextY) != null)
                {
                    return true;
                }

                if (pMap.search_unit(unit.nextX, unit.nextY - 1) != null)
                {
                    return true;
                }

                if (pMap.search_unit(unit.nextX, unit.nextY + 1) != null)
                {
                    return true;
                }
                break;
        }
        return false;
    }
    public bool e_check_targets(EnemyMap eMap, Enemy unit)
    {
        //returns false if there are no targets left in the area.
        EnemyMove move = unit.nextMove;

        //index:
        //0: rectangular
        //1: 1x1 self
        //2: 3x3 centerless cross
        //3: 3x3 cross
        switch (move.get_highlightType())
        {
            case 0: //standard rectangular
                for (int c = unit.nextX; c < (unit.nextX + move.get_ysize()); c++)
                {
                    for (int r = unit.nextY; r < (unit.nextY + move.get_xsize()); r++)
                    {
                        if (eMap.search_enemy(c, r) != null)
                        {
                            return true;
                        }
                    }
                }
                break;
            case 1:
                return true;
                
            case 2: //3x3 centerless cross.
                if (eMap.search_enemy(unit.nextX - 1, unit.nextY) != null)
                {
                    return true;
                }


                if (eMap.search_enemy(unit.nextX + 1, unit.nextY) != null)
                {
                    return true;
                }

                if (eMap.search_enemy(unit.nextX, unit.nextY - 1) != null)
                {
                    return true;
                }

                if (eMap.search_enemy(unit.nextX, unit.nextY + 1) != null)
                {
                    return true;
                }
                break;
        }
        return false;
    }

    //=====================================================================

    //ENEMY attacks PLAYER
    public void e_resolve_attack(Enemy unit, PlayerMap pMap)
    {
        reset_targetLists();
        EnemyMove move = unit.nextMove;

        //gather targets
        switch (move.get_highlightType())
        {
            case 0: //standard rectangular
                for (int c = unit.nextX; c < (unit.nextX + move.get_ysize()); c++)
                {
                    for (int r = unit.nextY; r < (unit.nextY + move.get_xsize()); r++)
                    {
                        if (pMap.search_unit(c, r) != null)
                        {
                            if (!f_targets.Contains(pMap.search_unit(c, r)))
                            {
                                f_targets.Add(pMap.search_unit(c, r));
                            }
                        }
                        else
                        {
                            pMap.tilegrid[r, c].GetComponent<PlayerTile>().show_damage_text("miss");
                        }
                    }
                }
                break;
            case 2: //3x3 centerless cross.
                if (pMap.search_unit(unit.nextX - 1, unit.nextY) != null)
                {
                    if (!f_targets.Contains(pMap.search_unit(unit.nextX - 1, unit.nextY)))
                    {
                        f_targets.Add(pMap.search_unit(unit.nextX - 1, unit.nextY));
                    }
                }
                else
                {
                    pMap.tilegrid[unit.nextY, unit.nextX - 1].GetComponent<PlayerTile>().show_damage_text("miss");
                }

                if (pMap.search_unit(unit.nextX + 1, unit.nextY) != null)
                {
                    if (!f_targets.Contains(pMap.search_unit(unit.nextX + 1, unit.nextY)))
                    {
                        f_targets.Add(pMap.search_unit(unit.nextX + 1, unit.nextY));
                    }
                }
                else
                {
                    pMap.tilegrid[unit.nextY, unit.nextX + 1].GetComponent<PlayerTile>().show_damage_text("miss");
                }

                if (pMap.search_unit(unit.nextX, unit.nextY - 1) != null)
                {
                    if (!f_targets.Contains(pMap.search_unit(unit.nextX, unit.nextY - 1)))
                    {
                        f_targets.Add(pMap.search_unit(unit.nextX, unit.nextY - 1));
                    }
                }
                else
                {
                    pMap.tilegrid[unit.nextY - 1, unit.nextX].GetComponent<PlayerTile>().show_damage_text("miss");
                }

                if (pMap.search_unit(unit.nextX, unit.nextY + 1) != null)
                {
                    if (!f_targets.Contains(pMap.search_unit(unit.nextX, unit.nextY + 1)))
                    {
                        f_targets.Add(pMap.search_unit(unit.nextX, unit.nextY + 1));
                    }
                }
                else
                {
                    pMap.tilegrid[unit.nextY + 1, unit.nextX].GetComponent<PlayerTile>().show_damage_text("miss");
                }
                break;
        }

        //now, damage the targets
        //foreach (PlayerUnit target in f_targets)
        if (f_targets.Count > 0) all_ooa = true;
        int runs = f_targets.Count;
        for (int i = 0; i < runs; i++)
        {
            int damage = e_calc_damage(unit, f_targets[0]);

            if ( move.get_appliesStatus())
            {
                StartCoroutine(show_battleNumbers(f_targets[0].stats.get_floatingHp().gameObject, 2.25f));
            }
            else
            {
                StartCoroutine(show_battleNumbers(f_targets[0].stats.get_floatingHp().gameObject, 1));
            }
            


            if (damage == -1)
            {                
                pMap.tilegrid[f_targets[0].get_x(), f_targets[0].get_y()].GetComponent<PlayerTile>().show_damage_text("dodge");
                f_targets.RemoveAt(i);
            }
            else if ( damage > 0 )
            {
                e_do_damage(f_targets[0], damage);
                pMap.tilegrid[f_targets[0].get_x(), f_targets[0].get_y()].GetComponent<PlayerTile>().show_damage_text(damage.ToString());


                //check if enemy was killed
                if (f_targets[0].ooa != true)
                {
                    all_ooa = false;
                }
                else
                {
                    //change sprite to ooa version or something
                    f_targets.RemoveAt(0);
                }

            }

        }        
    }
    int e_calc_damage(Enemy unit, PlayerUnit target)
    {
        EnemyMove move = unit.nextMove;

        if (move.get_power() == 0) return 0;

        int hit = unit.get_hit_actual() + move.get_hit();
        int dodge = target.get_dodge_actual();


        if(UnityEngine.Random.Range(0, 100)+hit <= dodge)
        {
            //dodge is successful
            return -1;
        }

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

        int atkAff;
        if ( move.get_ele() == -1)
        {
            atkAff = unit.get_weapon().get_element();
        }
        else
        {
            atkAff = move.get_ele();
        }

        double affMod = get_aff_mod(atkAff, target.get_aff(), target.get_armour().get_element());
        float spread = get_spread(0.85f, 1.0f);

        int dmg = Convert.ToInt32(((atk * (move.get_power() + unit.get_level())) / def) * affMod * spread);

        

        //stances order:
        //-defender:
        //  -mountain
        //  -shatter point
        //-attacker:
        //  -just decrement 


        if (target.status.mountain > 0)
        {
            if (move.get_attackType() == 0) //if a physical attack, halve damage
            {
                dmg = dmg / 2;
            }
            else //if a magic attack, dispel mountain
            {
                target.status.mountain = 0;
            }
        }
        if (target.status.shatter > 0)
        {
            if (dmg < target.get_hp()) //if attack will not kill the unit
            {
                dmg = 1; //does only 1 dmg
            }
            else
            {
                target.status.shatter = 0;
            }
        }
        target.status.trigger_attacked(target);
        unit.status.trigger_attack(unit);

        return dmg;
    }
    void e_do_damage(PlayerUnit target, int damage)
    {
        target.modify_hp_attack(target.get_hp() - damage);
        target.update_stats();
        //check dead, all that stuff happens here.
    }
    
    //ENEMY heals ENEMY
    public void e_resolve_heal(Enemy unit, EnemyMap eMap)
    {
        reset_targetLists();
        EnemyMove move = unit.nextMove;

        //gather targets
        switch (move.get_highlightType())
        {
            case 0: //standard rectangular.
                for (int c = unit.nextX; c < (unit.nextX + move.get_ysize()); c++)
                {
                    for (int r = unit.nextY; r < (unit.nextY + move.get_xsize()); r++)
                    {
                        if (eMap.search_enemy(c, r) != null)
                        {
                            if (!e_targets.Contains(eMap.search_enemy(c, r)))
                            {
                                e_targets.Add(eMap.search_enemy(c, r));
                            }
                        }
                        else
                        {
                            eMap.tilegrid[r, c].GetComponent<EnemyTile>().show_heal_text("miss");
                        }
                    }
                }
                break;
            case 1:
                //self
                e_targets.Add(unit);
                break;
        }
        //now, heal the targets
        foreach (Enemy target in e_targets)
        {
            //calc heal:
            bool ignoreMax = false;
            int heal = 0;
            if (move.get_hasCustomHeal())
            {
                //Debug.Log("custom heal going");
                StartCoroutine(show_battleNumbers(target.stats.get_floatingHp().gameObject, 2.25f));
                heal = move.calc_custom_heal(unit);
                ignoreMax = move.get_ignoreHealMax();
            }
            else
            {
                StartCoroutine(show_battleNumbers(target.stats.get_floatingHp().gameObject, 1));
                heal = e_calc_heal(unit, target);
            }

            //display heal number
            if (heal != -1)
            {
                e_do_heal(target, heal, ignoreMax);
                eMap.tilegrid[target.get_x(), target.get_y()].GetComponent<EnemyTile>().show_heal_text(heal.ToString());
            }                       
        }

    }
    int e_calc_heal(Enemy unit, Enemy target)
    {
        EnemyMove move = unit.nextMove;

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

        float spread = get_spread(0.85f, 1.0f);

        int heal = Convert.ToInt32(((atk * (move.get_power() + unit.get_level())) / def) * spread);



        //stances order:
        //-healed:
        //  -
        //-heal:
        //  -just decrement 

        target.status.trigger_healed(target);
        unit.status.trigger_heal(unit);


        return heal;
    }
    void e_do_heal(Enemy target, int heal, bool ignoreHealMax)
    {
        target.modify_hp_heal(target.get_hp() + heal, ignoreHealMax);
        target.update_stats();
    }

    //PLAYER attacks ENEMY
    public void f_resolve_attack(PlayerUnit unit, EnemyMap eMap)
    {
        //method:
        //first assemble targets (keeping in mind cases)
        //then deal damage 

        reset_targetLists();
        PlayerMove move = unit.nextMove;
       
        //gather targets
        switch (move.get_highlightType())
        {
            case 0: //standard rectangular.
                for (int c = unit.nextX; c < (unit.nextX + move.get_ysize()); c++)
                {
                    for (int r = unit.nextY; r < (unit.nextY + move.get_xsize()); r++)
                    {
                        if (eMap.search_enemy(c, r) != null)
                        {
                            if (!e_targets.Contains(eMap.search_enemy(c, r)))
                            {
                                e_targets.Add(eMap.search_enemy(c, r));
                            }
                        }
                        else
                        {
                            eMap.tilegrid[r, c].GetComponent<EnemyTile>().show_damage_text("miss");
                        }
                    }
                }
                break;
            case 2: //3x3 centerless cross.
                if ( eMap.search_enemy(unit.nextX-1, unit.nextY) != null)
                {
                    if (!e_targets.Contains(eMap.search_enemy(unit.nextX - 1, unit.nextY)))
                    {
                        e_targets.Add(eMap.search_enemy(unit.nextX - 1, unit.nextY));
                    }
                }
                else
                {
                    eMap.tilegrid[unit.nextY, unit.nextX-1].GetComponent<EnemyTile>().show_damage_text("miss");
                }

                if (eMap.search_enemy(unit.nextX+1, unit.nextY) != null)
                {
                    if (!e_targets.Contains(eMap.search_enemy(unit.nextX + 1, unit.nextY)))
                    {
                        e_targets.Add(eMap.search_enemy(unit.nextX + 1, unit.nextY));
                    }
                }
                else
                {
                    eMap.tilegrid[unit.nextY, unit.nextX + 1].GetComponent<EnemyTile>().show_damage_text("miss");
                }

                if (eMap.search_enemy(unit.nextX, unit.nextY-1) != null)
                {
                    if (!e_targets.Contains(eMap.search_enemy(unit.nextX, unit.nextY - 1)))
                    {
                        e_targets.Add(eMap.search_enemy(unit.nextX, unit.nextY - 1));
                    }
                }
                else
                {
                    eMap.tilegrid[unit.nextY-1, unit.nextX].GetComponent<EnemyTile>().show_damage_text("miss");
                }

                if (eMap.search_enemy(unit.nextX, unit.nextY + 1) != null)
                {
                    if (!e_targets.Contains(eMap.search_enemy(unit.nextX, unit.nextY + 1)))
                    {
                        e_targets.Add(eMap.search_enemy(unit.nextX, unit.nextY + 1));
                    }
                }
                else
                {
                    eMap.tilegrid[unit.nextY + 1, unit.nextX].GetComponent<EnemyTile>().show_damage_text("miss");
                }
                break;
        }

        //now, damage the targets
        if (e_targets.Count > 0) all_ooa = true;
        int runs = e_targets.Count;
        for (int i = 0; i < runs; i++)
        {
            int damage = f_calc_damage(unit, e_targets[0]);
          
            if (move.get_appliesStatus())
            {
                StartCoroutine(show_battleNumbers(e_targets[0].stats.get_floatingHp().gameObject, 2.25f));
            }
            else
            {
                StartCoroutine(show_battleNumbers(e_targets[0].stats.get_floatingHp().gameObject, 1));
            }


            if (damage == -1)
            {
                eMap.tilegrid[e_targets[0].get_x(), e_targets[0].get_y()].GetComponent<EnemyTile>().show_damage_text("dodge");
            }
            else if ( damage > 0 )
            {
                f_do_damage(e_targets[0], damage);
                eMap.tilegrid[e_targets[0].get_x(), e_targets[0].get_y()].GetComponent<EnemyTile>().show_damage_text(damage.ToString());

                //check if enemy was killed
                if (e_targets[0].ooa != true)
                {
                    all_ooa = false;                            
                }
                else
                {
                    cLog.remove_enemy(e_targets[0]);
                    e_targets.RemoveAt(0);
                }
            }
        }
    }
    int f_calc_damage(PlayerUnit unit, Enemy target)
    {
        PlayerMove move = unit.nextMove;

        if (move.get_power() == 0) return 0;

        int hit = unit.get_hit_actual() + move.get_hit();
        int dodge = target.get_dodge_actual();

        if (UnityEngine.Random.Range(0, 100)+hit <= dodge)
        {
            //dodge is successful
            return -1;
        }

        int atk;
        int def;

        //check dodge:
        //  -if success, return

        if (move.get_attackType() == 0)
        {
            //uses physical.
            atk = unit.get_physa_actual();
            def = target.get_physd_actual();

            //testing
            //Debug.Log("atk = " + atk);
            //Debug.Log("def = " + def);

        }
        else
        {
            //uses magical.
            atk = unit.get_maga_actual();
            def = target.get_magd_actual();
        }

        int atkAff;
        if (move.get_ele() == -1)
        {
            atkAff = unit.get_weapon().get_element();
        }
        else
        {
            atkAff = move.get_ele();
        }

        double affMod = get_aff_mod(atkAff, target.get_aff(), target.get_armour().get_element());

        float spread = get_spread(0.85f, 1.0f);

        //damage = damage * ((atk) * (cmove.get_power() + self.get_lvl()) / (dfs)) * aff_mod * spread
        int dmg = Convert.ToInt32(((atk * (move.get_power() + unit.get_level())) / def) * affMod * spread);

        //stances order:
        //-defender:
        //  -mountain
        //  -shatter point
        //-attacker:
        //  -just decrement 
        if (target.status.mountain > 0)
        {
            if (move.get_attackType() == 0) //if a physical attack, halve damage
            {
                dmg = dmg / 2;
            }
            else //if a magic attack, dispel mountain
            {
                target.status.mountain = 0;
            }
        }
        if (target.status.shatter > 0)
        {
            if (dmg < target.get_hp()) //if attack will not kill the unit
            {
                dmg = 1; //does only 1 dmg
            }
            else
            {
                target.status.shatter = 0;
            }
        }
        target.status.trigger_attacked(target);
        unit.status.trigger_attack(unit);

        return dmg;
    }
    void f_do_damage(Enemy target, int damage)
    {
        target.modify_hp_attack(target.get_hp() - damage);
        target.update_stats();
    }

    //PLAYER heals PLAYER
    public void f_resolve_heal(PlayerUnit unit, PlayerMap pMap)
    {
        //method:
        //first assemble targets (keeping in mind cases)
        //then deal damage 
        reset_targetLists();
        PlayerMove move = unit.nextMove;

        //gather targets
        switch (move.get_highlightType())
        {
            case 0: //standard rectangular
                for (int c = unit.nextX; c < (unit.nextX + move.get_ysize()); c++)
                {
                    for (int r = unit.nextY; r < (unit.nextY + move.get_xsize()); r++)
                    {
                        if (pMap.search_unit(c, r) != null)
                        {
                            if (!f_targets.Contains(pMap.search_unit(c, r)))
                            {
                                f_targets.Add(pMap.search_unit(c, r));
                            }
                        }
                        else
                        {
                            pMap.tilegrid[r, c].GetComponent<PlayerTile>().show_damage_text("miss");
                        }
                    }
                }
                break;
            case 1:
                //self
                f_targets.Add(unit);
                break;
            
            case 2: //3x3 centerless cross.
                if (pMap.search_unit(unit.nextX - 1, unit.nextY) != null)
                {
                    if (!f_targets.Contains(pMap.search_unit(unit.nextX - 1, unit.nextY)))
                    {
                        f_targets.Add(pMap.search_unit(unit.nextX - 1, unit.nextY));
                    }
                }
                else
                {
                    pMap.tilegrid[unit.nextY, unit.nextX - 1].GetComponent<PlayerTile>().show_damage_text("miss");
                }

                if (pMap.search_unit(unit.nextX + 1, unit.nextY) != null)
                {
                    if (!f_targets.Contains(pMap.search_unit(unit.nextX + 1, unit.nextY)))
                    {
                        f_targets.Add(pMap.search_unit(unit.nextX + 1, unit.nextY));
                    }
                }
                else
                {
                    pMap.tilegrid[unit.nextY, unit.nextX + 1].GetComponent<PlayerTile>().show_damage_text("miss");
                }

                if (pMap.search_unit(unit.nextX, unit.nextY - 1) != null)
                {
                    if (!f_targets.Contains(pMap.search_unit(unit.nextX, unit.nextY - 1)))
                    {
                        f_targets.Add(pMap.search_unit(unit.nextX, unit.nextY - 1));
                    }
                }
                else
                {
                    pMap.tilegrid[unit.nextY - 1, unit.nextX].GetComponent<PlayerTile>().show_damage_text("miss");
                }

                if (pMap.search_unit(unit.nextX, unit.nextY + 1) != null)
                {
                    if (!f_targets.Contains(pMap.search_unit(unit.nextX, unit.nextY + 1)))
                    {
                        f_targets.Add(pMap.search_unit(unit.nextX, unit.nextY + 1));
                    }
                }
                else
                {
                    pMap.tilegrid[unit.nextY + 1, unit.nextX].GetComponent<PlayerTile>().show_damage_text("miss");
                }
                break;
        }
        //now, heal the targets
        foreach (PlayerUnit target in f_targets)
        {
            
            //calc heal:
            bool ignoreMax = false;
            int heal = 0;
            if (move.get_hasCustomHeal())
            {
                //Debug.Log("custom heal going");
                StartCoroutine(show_battleNumbers(target.stats.get_floatingHp().gameObject, 2.25f));
                heal = move.calc_custom_heal(unit);
                ignoreMax = move.get_ignoreHealMax();
            }
            else
            {
                StartCoroutine(show_battleNumbers(target.stats.get_floatingHp().gameObject, 1));
                heal = f_calc_heal(unit, target);
            }

            //display heal number
            if (heal != -1)
            {
                f_do_heal(target, heal, ignoreMax);
                pMap.tilegrid[target.get_x(), target.get_y()].GetComponent<PlayerTile>().show_heal_text(heal.ToString());
            }
           
        }       
    }
    int f_calc_heal(PlayerUnit unit, PlayerUnit target)
    {
        PlayerMove move = unit.nextMove;
        
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

        float spread = get_spread(0.85f, 1.0f);

        int heal = Convert.ToInt32(((atk * (move.get_power() + unit.get_level())) / def) * spread);

       

        //stances order:
        //-healed:
        //  -
        //-heal:
        //  -just decrement 
        
        target.status.trigger_healed(target);
        unit.status.trigger_heal(unit);


        return heal;

    }
    void f_do_heal(PlayerUnit target, int heal, bool ignoreHealMax)
    {
        target.modify_hp_heal(target.get_hp() + heal, ignoreHealMax);
        target.update_stats();
    }
  
}
