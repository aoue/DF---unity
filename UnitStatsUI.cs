using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitStatsUI : MonoBehaviour
{
    //manages the sidebar with all the stats.

    //sidebar ui stuff
    [SerializeField] private UnitStatsDisplay[] pl;

    [SerializeField] private UnitStatsDisplay[] el;

    public UnitStatsDisplay[] get_pl() { return pl; }
    public UnitStatsDisplay[] get_el() { return el; }

    //^^make those two arrays, for ease of indexing with pl and el
    //will also need a thing to only show the ones in use, just like with the move buttons


    //void Start() { };






}
