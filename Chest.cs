using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] Loot lootItem; //loot contained in chest.
    [SerializeField] Gear gearItem; //gear contained in chest.
    //^^only 1.

    [SerializeField] string msg; //message that appears if chest is interacted with if isOpen is true.
    private static bool opened = false;
    private static bool opening;

    //open trigger
    //
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.name == "Player" && Input.GetKey(KeyCode.Space) && !opening) //Input.GetKeyDown(KeyCode.Space))// && !WorldManager.inDialogue)
        {
            opening = true;
            if (!opened)
            {
                //add loot to world inventory
                //Debug.Log("adding item to inventory");
                
                opened = true;

                //change chest image to an opened, looted chest.
                //show chest contents through worldmanager->examinemanager

                //determine if loot or gear type chest
                if (gearItem == null)
                {
                    WorldManager.open_chest_loot(lootItem);
                    WorldManager.worldInventory.add_loot(lootItem);
                }
                else
                {
                    WorldManager.open_chest_gear(gearItem);
                    WorldManager.worldInventory.add_gear(gearItem);
                }
            }
            else
            {
                //Debug.Log(msg);
                //show msg in notification manager
                WorldManager.add_notification(msg);               
            }
            StartCoroutine(help_opening());
            
        }
    }

    IEnumerator help_opening()
    {
        yield return new WaitForSeconds(1.0f);
        opening = false;
    }

}
