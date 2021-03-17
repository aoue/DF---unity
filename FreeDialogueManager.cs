using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TODO:
// -animator: how should the textbox appear? should it slide in from the side? 
// -manage portraits

//notes:
// -player will not be able to enter dialogue with a character while free dialogue is running

public class FreeDialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject dBox;
    [SerializeField] private Image portrait;
    [SerializeField] private Text nameText;
    [SerializeField] private Text dText;

    private Queue<string> sentences;
    private Queue<string> speakers;

    public static Dialogue heldDia;

    void Start()
    {
        sentences = new Queue<string>();
        speakers = new Queue<string>();
    }

    void setup_portrait()
    {
        //will place character portrait at initial spot
    }
    void update_portrait()
    {
        //updates portrait just before a line is shown
    }

    public void StartDialogue(Dialogue dia)
    {
        heldDia = dia;
        Debug.Log("free dialogue started");
        sentences.Clear();
        speakers.Clear();
        dBox.gameObject.SetActive(true);

        setup_portrait();

        int temp = dia.get_sentences().Length;
        for (int i = 0; i < temp; i++)
        {
            speakers.Enqueue(dia.get_names()[i]);
            sentences.Enqueue(dia.get_sentences()[i]);
        }

        DisplayNextSentence();

    }

    void DisplayNextSentence()
    {
        update_portrait();
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        
        string speaker = speakers.Dequeue();
        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence, speaker));

    }

    IEnumerator TypeSentence(string sentence, string speaker)
    {
        //if has portrait, then show corresponding portrait
        nameText.text = speaker;
        dText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dText.text += letter;
            yield return null;
            //add a waitForSeconds here to adjust display speed
        }
        yield return new WaitForSeconds(3.0f);
        DisplayNextSentence();
    }

    void EndDialogue()
    {
        WorldManager.exit_freeDialogue(heldDia);
        dBox.SetActive(false);       
    }
}
