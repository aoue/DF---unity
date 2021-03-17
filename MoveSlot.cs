using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSlot : MonoBehaviour
{
    //goes on each move or gear slot

    [SerializeField] private int id;
    [SerializeField] private int focusId;
    [SerializeField] private PartyManager pManager;

    public void call_perform_swap()
    {
        //Debug.Log(id);
        //Debug.Log(focusId);
        pManager.perform_swap(id, focusId);
    }


}
