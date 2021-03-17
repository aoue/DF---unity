using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UnitStatsDisplay : MonoBehaviour
{
    [SerializeField] private Text nom;
    [SerializeField] private Text hp;
    //public Text mp;
    [SerializeField] private Text moveNamePreview;
    [SerializeField] private Text delayText;
    [SerializeField] private Slider delay;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Text statusText;

    //floating health bar
    [SerializeField] private Slider floatingHp;


    public Slider get_floatingHp() { return floatingHp; }

    public Text get_nom() { return nom; }

    public void go_ooa()
    {
        //called when unit goes ooa
        Color color = new Color(0f / 255f, 0f / 255f, 0f / 255f); //white for now i guess...
        update_color(color);

        update_moveNamePreview("----");
        update_delayText("-/-");
        update_slider(100, true); //fills delay bar. won't move again until unit is revived
    }

    public void update_status_text(string newStr)
    {
        statusText.text = newStr;
    }

    public void update_stats(int curHp, int hpMax, int delay, int fullDelay, string statusText)
    {
        hp.text = "HP: " + curHp.ToString() + "/" + hpMax.ToString();
        hpSlider.maxValue = hpMax;
        hpSlider.value = curHp;      
        update_status_text(statusText);

        //update floating hp bar too
        floatingHp.maxValue = hpMax;
        floatingHp.value = curHp;

        update_delay(delay, fullDelay);
        //update_slider(delay, fullDelay);
    }
    public void update_moveNamePreview(string updateText)
    {
        moveNamePreview.text = updateText;
    }
    void update_delay(int newDelay, int fullDelay)
    {
        //changes the delay numbers that appear on the right

        if (delayText.text != "-/-")
        {
            delayText.text = newDelay + "/" + fullDelay;
        }
    }
    public void update_slider(int fullDelay, bool wrap)
    {
        //changes the slider's filled portion.

        if (wrap)
        {
            delay.value = fullDelay;
        }
        else
        {
            delay.value += fullDelay;
        }
        
    }

    public void update_color(Color color)
    {
        delay.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = color;
    }

    public void update_delayText(string newText)
    {
        delayText.text = newText;
    }


}
