using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTile : MonoBehaviour
{

    //what this needs to do:
    // -detect if it's being hovered over and call a function
    // -detect if it's being clicked and call a function
    // -respect being enabled/disabled
    [SerializeField] private GameObject damageText;
    private EnemyMap em;
    private SpriteRenderer sprite;
    [SerializeField] private int x;
    [SerializeField] private int y;
    private DamageNumber affHold; //holds aff mult text so it can be deleted later with a toggle 

    public bool isOrigin { get; set; }

    void Start()
    {
        em = FindObjectOfType<EnemyMap>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void OnMouseEnter()
    {
        if (em.get_battle().allowPreview && em.search_enemy(x, y) != null) //y, x? needs testing
        {
            em.get_battle().showStatPreviewWindow_e(em.search_enemy(x, y));
        }

        if (em.allowHover)
        {
            //call coloring function. makes and other affected tiles red.
            //sprite.color = new Color(250f / 255f, 100f / 255f, 100f / 255f);
            em.highlight_helper(x, y);
        }
        //also: everytime, even if allowHover is false, hovering a tile will show its unit's status profile. 
    }
    void OnMouseExit()
    {
        if (em.get_battle().allowPreview && em.search_enemy(x, y) != null)
        {
            em.get_battle().hide_statPrev();
            if (em.search_enemy(x, y).state == unitState.PREPARING)
            {
                em.get_battle().e_undoStatPreviewWindowHighlight(em.search_enemy(x, y).nextX, em.search_enemy(x, y).nextY, em.search_enemy(x, y).nextMove.get_isHeal(), em.search_enemy(x, y).nextMove.get_targetArea());
            }

        }

        if (em.allowHover)
        {
            //reset tile's color
            em.highlight_helper_off(x, y);
        }
        //hide the unit's status profile
    }

    void OnMouseUp()
    {
        //Method
        // -give position back to the unit so it will know where to do the attack when it's ready.
        // -tell enemymap to stop allowing hover.

        //
        //return posArray;
        //int[] posArray = {x, y};

        //
        if (em.allowHover)
        {
            OnMouseExit();
            if (em.mustUseOrigin && isOrigin)
            {
                isOrigin = false;
                em.disallow_hover();
                em.get_battle().log_attack(x, y);
            }
            else if (!em.mustUseOrigin)
            {
                switch (em.type)
                {
                    case 0:
                        em.disallow_hover();

                        int tryX = x;
                        int tryY = y;
                        if (x + em.targetArea[1] > 4)
                        {
                            tryX = 4 - em.targetArea[1];
                        }
                        if (y + em.targetArea[0] > 5)
                        {
                            tryY = 5 - em.targetArea[0];
                        }

                        em.get_battle().log_attack(tryX, tryY);
                        break;
                    case 2:
                        if (x <= 2 && y <= 3 && x >= 1 && y >= 1)
                        {
                            em.disallow_hover();
                            em.get_battle().log_attack(x, y);
                        }
                        break;
                }



                
            }

            
        }    
    }

    public void show_damage_text(string damage)
    {
        //will show received string next to the unit
        var dmgNum = Instantiate(damageText, transform.position, Quaternion.identity, transform);
        dmgNum.GetComponent<DamageNumber>().set_destroyTime(1.0f);

        //we want it to be in front of THE UNIT'S SPRITE.
        //dmgNum.GetComponent<Renderer>().sortingLayerName = "Combat Sc";
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
        if ( moveIsHeal)
        {
            dmgNum.GetComponent<TextMesh>().text = affText;
        }
        else
        {
            dmgNum.GetComponent<TextMesh>().text = "x" + affText;
        }     
    }
    public void delete_affText()
    {
        if (affHold != null) affHold.destroyNow();
    }

    public void highlight_translation()
    {
        Color theColor = new Color(0f / 255f, 0f / 255f, 200f / 255f); //blue or something
        sprite.color = theColor;
    }
    public void tile_to_white()
    {
        Color theColor = new Color(255f / 255f, 255f / 255f, 255f / 255f); //white or something
        sprite.color = theColor;
    }
}
