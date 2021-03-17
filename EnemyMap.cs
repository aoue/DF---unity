using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//offset is bottom left square of grid. unity coordinates= ( -4.3, 0.05)
public class EnemyMap : MonoBehaviour
{
    public GameObject[,] tilegrid { get; set; }
    [SerializeField] private CombatLogic battle;

    //^because unity doesn't allow multidimensional arrays in the inspector, we need a creative solution:
    [SerializeField] private GameObject[] col0;
    [SerializeField] private GameObject[] col1;
    [SerializeField] private GameObject[] col2;
    [SerializeField] private GameObject[] col3;
    [SerializeField] private GameObject[] col4;
    public bool allowHover { get; set; } //when true, the tiles can be highlighted and clicked. false, they cannot.
    public int[] targetArea { get; set; } //array of 2 items. they specify x size and y size of the target area.
    public int type { get; set; } //for handling non-rectangular highlights. 0 = rectangular.
    public int[] origin { get; set; } //for highlights that must originate on a certain tile
    public bool mustUseOrigin { get; set; } //if true, can only click tiles marked as origin

    Enemy[,] grid = new Enemy[4, 5]
    {
        {null, null, null, null, null},
        {null, null, null, null, null},
        {null, null, null, null, null},
        {null, null, null, null, null}
    };

    public CombatLogic get_battle()
    {
        return battle;
    }

    public void Start()
    {
        //setup the tilegrid array. the tiles need to be matched so that they can be coloured appropriately.     
        targetArea = new int[2];
        tilegrid = new GameObject[5, 4];
        

        for (int c = 0; c < 4; c++)
        {
            tilegrid[0, c] = col0[c];
            tilegrid[1, c] = col1[c];
            tilegrid[2, c] = col2[c];
            tilegrid[3, c] = col3[c];
            tilegrid[4, c] = col4[c];
        }       
    }
    public void allow_hover(int xsize, int ysize, int highlightType, int[] bringOrigin)
    {
        type = highlightType;
        origin = bringOrigin;
        mustUseOrigin = false;
        switch (type)
        {
            case 0: //rectangular
                targetArea[0] = xsize;
                targetArea[1] = ysize;
                break;
            case 1: //self
                mustUseOrigin = true;
                tilegrid[origin[1], origin[0]].GetComponent<PlayerTile>().isOrigin = true;
                break;
            case 2: // 3x3 centerless cross 
                targetArea[0] = xsize;
                targetArea[1] = ysize;
                break;

        }
        allowHover = true;
    }
    public void disallow_hover()
    {
        allowHover = false;
    }
    public void highlight_helper(int x, int y, bool useRed = true, bool useTargetArea = true, int[] optTargetArea = null)
    {
        Color theColor; ;
        if (useRed) { theColor = new Color(250f / 255f, 100f / 255f, 100f / 255f); }
        else { theColor = new Color(35f / 255f, 222f / 255f, 0f / 255f); }

        //shrink x and y by the target area, if they would have bumped the edge
        
        switch (type)
        {
            case 0: //rectangle
                int[] oldTargetArea = new int[2];
                if (!useTargetArea)
                {
                    oldTargetArea = targetArea;
                    targetArea = optTargetArea;
                }

                x = Math.Min(x, 4 - targetArea[1]);
                y = Math.Min(y, 5 - targetArea[0]);

                for (int c = x; c < Math.Min(x + targetArea[1], 4); c++)
                {
                    for (int r = y; r < Math.Min(y + targetArea[0], 5); r++)
                    {
                        tilegrid[r, c].GetComponent<SpriteRenderer>().color = theColor;
                    }
                }
                if (!useTargetArea) { targetArea = oldTargetArea; }
                break;
            case 1: //self. can only highlight tile x, y
                if (origin[0] == x && origin[1] == y)
                {
                    tilegrid[y, x].GetComponent<SpriteRenderer>().color = theColor;
                }
                break;
            case 2: //3x3 centerless cross
                if (x <= 2 && y <= 3 && x >= 1 && y >= 1)
                {
                    tilegrid[y - 1, x].GetComponent<SpriteRenderer>().color = theColor;
                    tilegrid[y + 1, x].GetComponent<SpriteRenderer>().color = theColor;
                    tilegrid[y, x - 1].GetComponent<SpriteRenderer>().color = theColor;
                    tilegrid[y, x + 1].GetComponent<SpriteRenderer>().color = theColor;
                }
                break;
        }       
    }
    public void highlight_helper_off(int x, int y, bool useTargetArea = true, int[] optTargetArea = null)
    {
        Color theColor = new Color(255f / 255f, 255f / 255f, 255f / 255f);

        //shrink x and y by the target area, if they would have bumped the edge
        
        switch (type)
        {
            case 0: //rectangle
                int[] oldTargetArea = new int[2];
                if (!useTargetArea)
                {
                    oldTargetArea = targetArea;
                    targetArea = optTargetArea;
                }
                x = Math.Min(x, 4 - targetArea[1]);
                y = Math.Min(y, 5 - targetArea[0]);
                for (int c = x; c < Math.Min(x + targetArea[1], 4); c++)
                {
                    for (int r = y; r < Math.Min(y + targetArea[0], 5); r++)
                    {
                        tilegrid[r, c].GetComponent<SpriteRenderer>().color = theColor;
                    }
                }
                if (!useTargetArea) { targetArea = oldTargetArea; }
                break;
            case 1: //self. can only highlight tile x, y
                if (origin[0] == x && origin[1] == y)
                {
                    tilegrid[y, x].GetComponent<SpriteRenderer>().color = theColor;
                }
                break;
            case 2: //3x3 centerless cross
                if (x <= 2 && y <= 3 && x >= 1 && y >= 1)
                {
                    tilegrid[y - 1, x].GetComponent<SpriteRenderer>().color = theColor;
                    tilegrid[y + 1, x].GetComponent<SpriteRenderer>().color = theColor;
                    tilegrid[y, x - 1].GetComponent<SpriteRenderer>().color = theColor;
                    tilegrid[y, x + 1].GetComponent<SpriteRenderer>().color = theColor;
                }
                break;
        }
    }

    public void place_enemy(Enemy unit)
    {
        for (int i = 0; i < unit.get_xSize(); i++)
        {
            for (int j = 0; j < unit.get_ySize(); j++)
            {
                grid[unit.get_y() + i, unit.get_x() + j] = unit;
            }
        }    

        if(unit.get_xSize() == 1 && unit.get_ySize() == 1)
        {
            float newX = (float)(-4.3 + 0.9 * (unit.get_x() - unit.get_y()));
            float newY = (float)(0.025 + 0.5 * (unit.get_x() + unit.get_y()));
            unit.transform.position = new Vector2(newX, newY);
        }
        else
        {
            float newX = (float)(-4.3 + 0.9 * (unit.get_x() - unit.get_y()) + 0.9 * (unit.get_xSize() - unit.get_ySize()));
            float newY = (float)(0.025 + 0.5 * (unit.get_x() + unit.get_y()) + 0.5 * (unit.get_ySize()));
            unit.transform.position = new Vector2(newX, newY);
        }
        //place floating hp bar
        float nX = (float)(198 + 45.75 * (unit.get_x() - unit.get_y()));
        float nY = (float)(130 + 27.5 * (unit.get_x() + unit.get_y()));
        unit.stats.get_floatingHp().transform.localPosition = new Vector2(nX, nY);
        
    }
    public void remove_enemy(Enemy unit)
    {
        for(int i = 0; i < unit.get_xSize(); i++)
        {
            for(int j = 0; j < unit.get_ySize(); j++)
            {
                grid[unit.get_y() + i, unit.get_x() + j] = null;
            }
        }       
    }
    public Enemy search_enemy(int x, int y)
    {
        return grid[x, y];
    }

    public void highlight_given(int x, int y)
    {
        //highlights the given tile                
        tilegrid[x, y].GetComponent<EnemyTile>().highlight_translation();
    }
    public void restore_all()
    {
        //restores all tiles to their lastColor
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                tilegrid[x, y].GetComponent<EnemyTile>().tile_to_white();
            }
        }
    }

}

