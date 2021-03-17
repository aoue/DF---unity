using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortalManager : MonoBehaviour
{
    //member of WorldManager that manages jumping from map to map on the overworld.
    //takes in an int from Portal and selects the matching scene and position based on that int.

    private static string[] jumpList = { "Cherespoir - Approach", "Map1"};
    private static string[] descrList = { "jump to cherespoir approach?", "Map1"};

    [SerializeField] private GameObject confirmPopup;
    [SerializeField] private Text confirmText;


    //also, should pass in coordinates for the player to be positioned at upon scene loading.

    public string get_portalDescr(int portalKey)
    {
        return descrList[portalKey];
    } 

    public string get_portal(int portalKey)
    {
        string newSceneString = jumpList[portalKey];

        return newSceneString;
    }

    public void show_portal_prompt(int portalKey)
    {
        confirmText.text = "Enter <" + get_portal(portalKey) + "> ?";
        confirmPopup.gameObject.SetActive(true);      
    }

    public void hide_portal_prompt()
    {
        confirmPopup.gameObject.SetActive(false);

    }


}
