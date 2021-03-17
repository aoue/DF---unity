using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutscene : Dialogue
{
    [SerializeField] private Sprite backgroundImage;

    public Sprite get_cg() { return backgroundImage; }
}
