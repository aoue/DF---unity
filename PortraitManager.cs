using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortraitManager : MonoBehaviour
{
    //portrait index - stick to the index as much as possible, for my sanity
    

    //a dialogue has the portraitmanager of every character who's portrait is shown during the dialogue.
    // this isn't demanding, because the portraitmanager will be made into a prefab. anyway, find a few images to change every click
    // is - absolutely NOTHING - for a computer.

    [SerializeField] private Sprite[] allPortraits; //all of the character's portraits. retrieved with an int index.
    public Sprite get_portrait(int index) { return allPortraits[index]; }

    

    //so... a portrait manager is a portrait manager for only one character.
    //i.e. we would have an instance and an entirely filled class - just for friday.
    //but, among classes, try to keep the index of 'happy face', 'sad face', and 'angry face' the same. for my sanity.
    //this gives access to all of a character's faces in any given dialogue.


    //index: (feel free to redesign later)
    //0: leave this image slot disabled.
    //1: neutral
    //2: 


}
