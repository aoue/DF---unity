using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

//when a unit uses a move that inflicts a status on a unit:
// we call unit.enter_statusName() //pretend it's unit.enter_adrenaline()
// in unit.enter_adrenaline(), adrenaline's stuff is only applied if adrenaline = -1, otherwise, it just refreshes its duration.
// in unit.exit_adrenaline(), undo the buffs enter_adrenaline() gave the unit in the pa and hp department. (by reducing hp only if over max, obv)
//in combatlogic, be sure to properly decrement all the unit's stance trackers together.

public class EnemyStances
{
    //a unit needs a place where the icons representing all the different status they are under can be shown.
    // especially important for DOTS.

    //manages stat buffs/debuffs
    public int hp_regen { get; set; }
    public int mp_regen { get; set; }

    //these guys affect the get_??_actual() methods 
    public double pa { get; set; } //physa
    public double pd { get; set; } //physd
    public double ma { get; set; } //maga
    public double md { get; set; } //magd
    public double dodge { get; set; } //dodge
    public double hit { get; set; } //hit
    public double speed { get; set; } //speed

    //battle stances, like kindara, shatter point, adrenaline
    //stance trackers : int=duration : -1=not set.
    public int defend { get; set; }
    public int adrenaline { get; set; }
    public int shatter { get; set; }
    public int kindara { get; set; }
    public int howl { get; set; }
    public int growl { get; set; }
    public int mountain { get; set; }
    public int suppression { get; set; }

    //stance helpers
    private int ad_loss;

    //and DOTs, HOTs
    public int[] bleed = new int[2];
    public int[] poison = new int[2]; //duration, intensity
    public int regen { get; set; }
    public int dot { get; set; } //value that is added to the unit's hp each tick

    public void set_to_default()
    {
        //stat mods
        pa = 1;
        pd = 1;
        ma = 1;
        md = 1;
        dodge = 1;
        hit = 1;
        dot = 0;
        speed = 1;

        //dots
        dot = 0;
        poison[0] = -1;
        poison[1] = 0;
        bleed[0] = -1;
        bleed[1] = 0;
        regen = -1;

        //battle stances should all be initialized to -1
        defend = -1;
        adrenaline = -1;
        shatter = -1;
        kindara = -1;
        howl = -1;
        growl = -1;
        mountain = -1;
        suppression = -1;
        // todo
    }

    // dots. take dot dmg each tick
    public int take_dots(int hp)
    {
        return (hp + dot);
    }
    //returns string that is shown on the right of the unitStatDisplay
    public string get_status_summary()
    {
        string rtnString = "";

        //combat status
        if (defend >= 0) { rtnString += "DEF | "; }
        if (adrenaline >= 0) { rtnString += "Adrenaline - " + adrenaline + " ticks | "; }
        if (shatter >= 0) { rtnString += "Shatter | "; }
        if (kindara >= 0) { rtnString += "Kindara - " + kindara + " charges | "; }
        if (howl >= 0) { rtnString += "Howl - " + howl + " ticks | "; }
        if (growl >= 0) { rtnString += "Growl - " + howl + " ticks | "; }
        if (mountain >= 0) { rtnString += "Mountain | "; }
        if (suppression >= 0) { rtnString += "Suppression - " + suppression + " charges | "; }

        //dots
        if (poison[0] >= 0) { rtnString += "Poison " + poison[1] + " | "; }
        if (bleed[0] >= 0) { rtnString += "Bleed " + bleed[1] + " | "; }

        return rtnString;
    }
    // returns string that is shown in unit window. gives info about unit's status effects
    public string get_status_info()
    {
        string rtnString = "";
        //goes through each status effect. if affected, add line to returnstring

        //combat status
        if (defend >= 0) { rtnString += "DEF\n"; }
        if (adrenaline >= 0) { rtnString += "Adrenaline - " + adrenaline + "ticks\n"; }
        if (shatter >= 0) { rtnString += "Shatter\n"; }
        if (kindara >= 0) { rtnString += "Kindara - " + kindara + " charges\n"; }
        if (howl >= 0) { rtnString += "Howl - " + howl + " ticks\n"; }
        if (growl >= 0) { rtnString += "Growl - " + growl + " ticks\n"; }
        if (mountain >= 0) { rtnString += "Mountain\n"; }
        if (suppression >= 0) { rtnString += "Suppression - " + suppression + " charges\n"; }

        //dots
        if (poison[0] >= 0) { rtnString += "Poison " + poison[1] + "\n"; }
        if (bleed[0] >= 0) { rtnString += "Bleed " + bleed[1] + "\n"; }

        return rtnString;
    }
    public string get_multiplier_info()
    {
        string rtnString = "\n";

        if (pa != 1) { rtnString += "(x" + pa + ")"; }
        rtnString += "\n";
        if (pd != 1) { rtnString += "(x" + pd + ")"; }
        rtnString += "\n";
        if (ma != 1) { rtnString += "(x" + ma + ")"; }
        rtnString += "\n";
        if (md != 1) { rtnString += "(x" + md + ")"; }
        rtnString += "\n";
        if (hit != 1) { rtnString += "(x" + hit + ")"; }
        rtnString += "\n";
        if (dodge != 1) { rtnString += "(x" + dodge + ")"; }

        return rtnString;
    }

    //When a certain situation arises that triggers dots of a certain type, calls one of the functions below
    //method of trigger function:
    // -called at the end of the action. i.e. trigger_defend() is called after the attack has already been resolved
    // -for each stance of that type: check if charge number > 0. if yes, decrement. if == 0, then: exit_stance()


    //trigger each tick
    public void trigger_tick(Enemy self)
    {
        if (bleed[0] > 0)
        {
            bleed[0]--;
        }
        else if (bleed[0] == 0)
        {
            exit_bleed();
        }

        if (poison[0] > 0)
        {
            poison[0]--;
        }
        else if (poison[0] == 0)
        {
            exit_poison();
        }

        if (regen > 0)
        {
            regen--;
        }
        else if (regen == 0)
        {
            exit_regen();
        }

        if ( growl > 0)
        {
            growl--;
        }
        else if ( growl == 0)
        {
            exit_growl();
        }

        if (adrenaline > 0)
        {
            adrenaline--;
        }
        else if (adrenaline == 0)
        {
            exit_adrenaline(self);
        }
    }
    //trigger when the unit makes an attack
    public void trigger_attack(Enemy self)
    {
        if (howl > 0)
        {
            howl--;
        }
        else if (howl == 0)
        {
            exit_howl();
        }

        if (suppression > 0)
        {
            suppression--;
        }
        else if (suppression == 0)
        {
            exit_suppression();
        }
    }
    //trigger when the unit is attacked
    public void trigger_attacked(Enemy self)
    {
        if (mountain == 0)
        {
            exit_mountain();
        }

        if (shatter == 0)
        {
            exit_shatter();
        }
    }
    //trigger when the unit heals
    public void trigger_heal(Enemy self)
    {

    }
    //trigger when the unit is healed
    public void trigger_healed(Enemy self)
    {

    }
    //trigger when the unit moves
    public void trigger_move(Enemy self)
    {

    }

    //DOTS enter-exit pairs
    public void enter_bleed(int intensity, int duration)
    {
        if (intensity > bleed[1])
        {
            dot -= intensity - bleed[1];

            if (bleed[0] < duration)
            {
                bleed[0] = duration;
            }
        }
        else
        {
            bleed[0] = duration;

        }
        bleed[1] = intensity;
    }
    public void exit_bleed()
    {
        bleed[0] = -1;
        dot += bleed[1];
        bleed[1] = 0;
    }

    public void enter_poison(int intensity, int duration)
    {
        if (intensity > poison[1])
        {
            dot -= intensity - poison[1];

            if (poison[0] < duration)
            {
                poison[0] = duration;
            }
        }
        else
        {
            poison[0] = duration;

        }
        poison[1] = intensity;
    }
    public void exit_poison()
    {
        poison[0] = -1;
        dot += poison[1];
        poison[1] = 0;
    }
    public void enter_regen() { }
    public void exit_regen() { }

    //STANCES enter-exit pairs
    public void enter_defend() { }
    public void exit_defend() { }

    public void enter_adrenaline(Enemy self, int duration)
    {
        //if unapplied, do stuff
        if (adrenaline == -1)
        {
            ad_loss = (self.get_hpMax_actual() * 5) + (10 * self.get_level());

            //heal self
            self.modify_hp_heal(self.get_hp() + ad_loss, true);
        }

        //refresh duration
        adrenaline = duration;

    }
    public void exit_adrenaline(Enemy self)
    {
        if (self.get_hp() > self.get_hpMax_actual())
        {
            self.modify_hp_attack(Math.Max(self.get_hpMax_actual(), self.get_hp() - ad_loss));
        }
        adrenaline = -1;
    }

    public void enter_shatter() { }
    public void exit_shatter() { }

    public void enter_kindara() { }
    public void exit_kindara() { }

    public void enter_howl(int charges)
    {
        if ( howl == -1)
        {
            pa += 0.25;
        }

        howl = charges;   
    }
    public void exit_howl()
    {
        pa -= 0.25;
    }

    public void enter_growl(int duration)
    {
        if (growl == -1)
        {
            pd -= 0.25;
        }

        growl = duration;
    }
    public void exit_growl()
    {
        pd += 0.25;
    }

    public void enter_suppression(int duration)
    {
        if (growl == -1)
        {
            pa -= 0.1;
            ma -= 0.1;
        }

        suppression = duration;
    }
    public void exit_suppression()
    {
        pa += 0.1;
        ma += 0.1;
        suppression = -1;
    }

    public void enter_mountain() { }
    public void exit_mountain() { }

}
