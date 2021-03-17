using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternSlot : MonoBehaviour
{
    //obj that goes on each slot in pattern.

    //holds coordinates. necessary because inspector only supports one argument.

    [SerializeField] int tileX;
    [SerializeField] int tileY;

    public int get_tileX() { return tileX; }
    public int get_tileY() { return tileY; }

}
