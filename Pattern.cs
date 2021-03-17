using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TODO:
// -unit's sprites
// -panel on the left there shows a rundown of the unit that is selected. include: portrait, stats (without breakdown), move distribution
// -strategy/plan idk think of a word

//unit's sprites:
// -haven't tested multiple units in the party
// -sprite not overwriting the correct tile for some reason. try copying exactly from somewhere it does work. hard to tell rn, wait until real images

// strategy. 
//highlight the tiles that give an effect with a special colour
// different strategies that give a bonus/debuff to units. e.g. +phys atk, -phys defense
// do colour locked bonuses? would not be hard to implement. pass in a var that corresponds to it, then, in battlebrain, check what
// [cont.] strategy is active, then check if either unit is on a state-important tile. nothing to it and it's another layer.
// can be bought, found, unlocked, given as quest rewards, etc. can allow for some cool builds.

public class Pattern : MonoBehaviour
{
    [SerializeField] private GameObject triMenu;
    [SerializeField] private GameObject unitPreviewArea;
    [SerializeField] private Text unitPreviewAreaText;

    [SerializeField] private Image[] col0;
    [SerializeField] private Image[] col1;
    [SerializeField] private Image[] col2;
    [SerializeField] private Image[] col3;
    [SerializeField] private Image[] col4;
    public Image[,] tilegrid { get; set; }

    private PlayerUnit selectedUnit;

    PlayerUnit[,] grid = new PlayerUnit[4, 5]
    {
        {null, null, null, null, null},
        {null, null, null, null, null},
        {null, null, null, null, null},
        {null, null, null, null, null}
    };

    void Awake()
    {
        //setup the tilegrid array. the tiles need to be matched so that they can be coloured appropriately.
        selectedUnit = null;
        tilegrid = new Image[5, 4];
        for (int c = 0; c < 4; c++)
        {
            tilegrid[0, c] = col0[c];
            tilegrid[1, c] = col1[c];
            tilegrid[2, c] = col2[c];
            tilegrid[3, c] = col3[c];
            tilegrid[4, c] = col4[c];
        }
    }

    public void close()
    {
        gameObject.SetActive(false);
        triMenu.gameObject.SetActive(true);
    }

    public void load_mainView(PlayerUnit[] party)
    {
        //called on startup

        //draw each unit's tilePortrait on their appropriate tile
        foreach(PlayerUnit unit in party)
        {
            Debug.Log(unit.get_nom() + "at y=" + unit.get_y() + " x=" + unit.get_x());
            tilegrid[unit.get_x(), unit.get_y()].gameObject.GetComponent<Image>().sprite = unit.get_tilePortrait();
            grid[unit.get_y(), unit.get_x()] = unit;
        }
    }

    public void hightlight_tile(PatternSlot tile)
    {
        //when a tile is moused over, if there is no unit, then highlight it
        if (grid[tile.get_tileX(), tile.get_tileY()] == null)
        {
            tilegrid[tile.get_tileY(), tile.get_tileX()].color = new Color(0 / 255f, 0 / 255f, 0 / 255f); //change colour
        }
        else
        {
            preview_unit(grid[tile.get_tileX(), tile.get_tileY()]);
        }
    }
    public void unhighlight_tile(PatternSlot tile)
    {
        if(grid[tile.get_tileX(), tile.get_tileY()] == null)
        {
            tilegrid[tile.get_tileY(), tile.get_tileX()].color = new Color(255f / 255f, 255f / 255f, 255f / 255f); //make white
        }
        else
        {
            if (selectedUnit != null)
            {
                preview_unit(selectedUnit);
            }
        }
    }
    public void click_tile(PatternSlot tile)
    {
        //if the tile WITHOUT a unit on it is clicked
        if (grid[tile.get_tileX(), tile.get_tileY()] == null) //if tile has no unit on it
        {
            if(selectedUnit != null)
            {
                //set old tile to blank
                tilegrid[selectedUnit.get_x(), selectedUnit.get_y()] = null; //or whatever. full white.
                grid[selectedUnit.get_y(), selectedUnit.get_x()] = null; //remove unit from grid

                //set new tile to unit's tileportrait
                tilegrid[tile.get_tileX(), tile.get_tileY()].sprite = selectedUnit.get_tilePortrait();
                selectedUnit.set_x(tile.get_tileY());
                selectedUnit.set_y(tile.get_tileX());
                grid[selectedUnit.get_x(), selectedUnit.get_y()] = selectedUnit;
                Debug.Log("placed " + selectedUnit.get_nom() + " at y=" + selectedUnit.get_y() + " x=" + selectedUnit.get_x());

            }
        }
        //if the tile has a unit on it
        else
        {
            selectedUnit = grid[tile.get_tileX(), tile.get_tileY()];
            preview_unit(selectedUnit);
            Debug.Log("selected " + selectedUnit.get_nom() + "at y=" + selectedUnit.get_y() + " x=" + selectedUnit.get_x());

        }
    }

    public void preview_unit(PlayerUnit unit)
    {
        //fills the big left slot with some info of the selected unit, to remind the player who the unit is/what their role is

        //relevant info to be show:
        // -portrait
        // -hp, mp, physa, physd, maga, magd (values without breakdown)
        // -moves? or at least move distribution

        unitPreviewArea.SetActive(true);
        //unitPreviewArea.gameObject.GetComponent<Image>() = selectedUnit.get_portrait(); //or something like this.
        unitPreviewAreaText.text = unit.get_nom() + "\nLvl " + unit.get_level() + "\n" 
            + unit.get_hp() + "/" + unit.get_hpMax_actual() + "\n" + unit.get_mp() + "/" + unit.get_mpMax_actual();

    }

    public void assign_strategy()
    {
        //click the buttons arrayed on the bottom that each have a strategy's name on them.
        //hover over each button to fill the description box with the strategy's description

        //assigns worldmanager.strategy to something
    }
}
