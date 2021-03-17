using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationManager : MonoBehaviour
{
    //one of worldmanager's assistant managers.
    //here's what is does:

    //when an event or something happens, shows a text line somewhere to draw the player's attention to this.
    //notifications are text on a narrow strip that shows up in the top right of the screen. show up one at a time until none are left. uses a queue.
    //while it has notifications, display one every 3 seconds or something.
    //notifications include: letter found, 'title' encyclopedia entry progressed, 'title' quest progressed/completed/failed

    private static Queue<string> notes = new Queue<string>();
    private static bool showing;
    private static bool startShowing;

    public void add_note(string note)
    {
        notes.Enqueue(note);
        if (!startShowing)
        {
            StartCoroutine(show_all_notes());
        }
    }

    IEnumerator show_all_notes()
    {


        startShowing = true;
        while (notes.Count > 0)
        {
            if (!showing)
            {
                gameObject.transform.GetChild(0).gameObject.SetActive(true);
                showing = true;
            }

            gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = notes.Dequeue();
            yield return new WaitForSeconds(3.5f);
        }

        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        showing = false;
        startShowing = false;

        yield return null;
    }

}
