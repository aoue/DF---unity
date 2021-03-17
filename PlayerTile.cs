using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTile : MonoBehaviour
{
    [SerializeField] private GameObject damageText;
    private PlayerMap pm;
    private SpriteRenderer sprite;
    [SerializeField] private int x;
    [SerializeField] private int y;
    private Color lastColor;
    private DamageNumber affHold; //holds aff mult text so it can be deleted later with a toggle 
    public bool reposition { get; set; } //when true, the tiles can be clicked, for use of round end repositioning.
    public bool repositionPlace { get; set; } //when true, the tiles can be clicked, for use of round end repositioning.
    public bool isOrigin { get; set; } //when true, tile is okay to click

    void Start()
    {
        lastColor = new Color(255f / 255f, 255f / 255f, 255f / 255f);
        pm = FindObjectOfType<PlayerMap>();          
        sprite = GetComponent<SpriteRenderer>();
    }

    void OnMouseEnter()
    {
        //show preview stats window and Hide the delay bar stuff

        if (pm.get_battle().allowPreview && pm.search_unit(x, y) != null) //y, x? needs testing
        {
            pm.get_battle().showStatPreviewWindow(pm.search_unit(x, y));
        }

        if (pm.allowHover || repositionPlace)
        {
            //call coloring function. makes and other affected tiles red.
            //sprite.color = new Color(250f / 255f, 100f / 255f, 100f / 255f);
            //Debug.Log("colouring");
            if ( repositionPlace)
            {
                //Debug.Log("repositionplace is true");
                pm.highlight_helper(x, y, useGreen : false, isRepo : true);
            }
            else
            {
                pm.highlight_helper(x, y);
            }

            
        }
        //also: everytime, even if allowHover is false, hovering a tile will show its unit's status profile. 
    }
    void OnMouseExit()
    {
        //hide the preview stats window
        if (pm.get_battle().allowPreview && pm.search_unit(x, y) != null)
        {
            pm.get_battle().hide_statPrev();
            if (pm.search_unit(x,y).state == unitState.PREPARING)
            {
                pm.get_battle().f_undoStatPreviewWindowHighlight(pm.search_unit(x, y).nextX, pm.search_unit(x, y).nextY, pm.search_unit(x, y).nextMove.get_isHeal(), pm.search_unit(x, y).nextMove.get_targetArea());
            }
        }

        if (pm.allowHover || repositionPlace)
        {
            //reset tile's color
            pm.highlight_helper_off(x, y);
        }
        //hide the unit's status profile
    }

    void OnMouseUp()
    {
        //Method
        // -give position back to the unit so it will know where to do the attack when it's ready.
        // -tell enemymap to stop allowing hover.

        if (pm.allowHover)
        {
            OnMouseExit();
            if (pm.mustUseOrigin && isOrigin)
            {
                isOrigin = false;

                pm.disallow_hover();
                pm.get_battle().log_attack(x, y);
            }
            else if (!pm.mustUseOrigin)
            {
                //pm.highlight_helper_off(x, y);

                switch (pm.type)
                {
                    case 0:
                        pm.disallow_hover();

                        int tryX = x;
                        int tryY = y;
                        if (x + pm.targetArea[1] > 4)
                        {
                            tryX = 4 - pm.targetArea[1];
                        }
                        if (y + pm.targetArea[0] > 5)
                        {
                            tryY = 5 - pm.targetArea[0];
                        }

                        pm.get_battle().log_attack(tryX, tryY);
                        break;
                    case 2:
                        if (x <= 2 && y <= 3 && x >= 1 && y >= 1)
                        {
                            pm.disallow_hover();
                            pm.get_battle().log_attack(x, y);
                        }
                        break;
                }
            }           
        }
        else if (repositionPlace)
        {
            pm.get_battle().reposition_unit(x, y);
        }
    }

    public void show_damage_text(string damage)
    {
        //will show received string next to the unit
        var dmgNum = Instantiate(damageText, transform.position, Quaternion.identity, transform);
        dmgNum.GetComponent<DamageNumber>().set_destroyTime(1.0f);
        //we want it to be in front of THE UNIT'S SPRITE.
        //dmgNum.GetComponent<Renderer>().sortingLayerName = "Combat Scene Layer";
        dmgNum.GetComponent<Renderer>().sortingOrder = 1;

        dmgNum.GetComponent<TextMesh>().color = new Color(255f / 255f, 0f / 255f, 0f / 255f);
        dmgNum.GetComponent<TextMesh>().text = damage;
    }
    public void show_heal_text(string damage)
    {
        //will show received string next to the unit
        var dmgNum = Instantiate(damageText, transform.position, Quaternion.identity, transform);
        dmgNum.GetComponent<DamageNumber>().set_destroyTime(1.0f);
        //we want it to be in front of THE UNIT'S SPRITE.
        //dmgNum.GetComponent<Renderer>().sortingLayerName = "Combat Scene Layer";
        dmgNum.GetComponent<Renderer>().sortingOrder = 1;

        dmgNum.GetComponent<TextMesh>().color = new Color(5f / 255f, 195f / 255f, 0f / 255f);
        dmgNum.GetComponent<TextMesh>().text = damage;
    }
    public void show_aff_text(string affText, bool moveIsHeal)
    {
        var dmgNum = Instantiate(damageText, transform.position, Quaternion.identity, transform);
        affHold = dmgNum.GetComponent<DamageNumber>();
        //dmgNum.GetComponent<DamageNumber>().set_destroyTime(0.1f);

        //we want it to be in front of THE UNIT'S SPRITE.
        //dmgNum.GetComponent<Renderer>().sortingLayerName = "Combat Scene Layer";
        dmgNum.GetComponent<Renderer>().sortingOrder = 1;

        dmgNum.GetComponent<TextMesh>().color = new Color(0f / 255f, 0f / 255f, 200f / 255f); //blue
        if (moveIsHeal)
        {
            dmgNum.GetComponent<TextMesh>().text = affText;
        }
        else
        {
            dmgNum.GetComponent<TextMesh>().text = "x" + affText;
        }
    }

    public void unit_is_selected()
    {
        //changes tile colour to sky blue when the unit on this tile is having their move chosen or acting.
       lastColor = sprite.color;
       Color color = new Color(135f / 255f, 206f / 255f, 235f / 255f); //sky blue
       sprite.color = color;
    }

    public void restore_color()
    {
        //restores the tile's colour to whatever is stored in lastColor

        //sprite.color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
        sprite.color = lastColor;


    }
    public void highlight_translation()
    {
        lastColor = sprite.color;
        Color theColor = new Color(0f / 255f, 0f / 255f, 200f / 255f); //blue or something
        sprite.color = theColor;
    }


}
