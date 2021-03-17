using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundTimer : MonoBehaviour
{
    private int roundNumber = 0;
    [SerializeField] private Text roundCounter;
    [SerializeField] private Text currentTick;
    [SerializeField] private Text limitTick;
    [SerializeField] private Slider progressBar;

    public bool isUnveiled { get; set; } //if true, show all true values of the round stuff.
                                         //if false, show ??? for maxTick and don't update the slider.

    //the default is:
    // -until we're within a certain number of ticks from maxTick, it's hidden how long there is to the next round.

    public void update_slider(int newTick, int maxTick)
    {
        currentTick.text = newTick.ToString();

        if (isUnveiled)
        {
            progressBar.value = newTick;
            progressBar.maxValue = maxTick;
            limitTick.text = maxTick.ToString();

        }
        else
        {
            limitTick.text = "???";
        }

    }

    public void inc_round()
    {
        isUnveiled = false;
        roundNumber += 1;
        roundCounter.text = "Round " + roundNumber.ToString();
    }
    

}
