using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//todo:
// -on start of dialogue: put player into standing animation, not running, as it is sometimes caught
// -manage portraits



//manage portraits:
//four portraits max. at start of dialogue, read dialogue's hasPortrait[] bool array

public class DialogueManager : MonoBehaviour
{
    public bool inputEnabled { get; set; }

    [SerializeField] private GameObject dBox;
    [SerializeField] private Animator animator;
    public bool dialogActive { get; set; } //

    private Queue<string> sentences;
    private Queue<string> names;
    private Queue<int> portraitIndexes;
    private Queue<int> speakers;
    private PortraitManager[] portraitManagers;

    private PlayerMovement thePlayer;

    [SerializeField] private Text nameText;
    [SerializeField] private Text dText;
    [SerializeField] private Image[] portrait;

    [SerializeField] private GameObject[] branchButtons; //buttons used for branching

    public static Dialogue heldDia;

    // Start is called before the first frame update
    void Start()
    {
        thePlayer = FindObjectOfType<PlayerMovement>();
        inputEnabled = false;
        sentences = new Queue<string>();
        names = new Queue<string>();
        portraitIndexes = new Queue<int>();
        speakers = new Queue<int>();
    }

    // Update is called once per frame
    void Update()
    {
        //brings next line of dialogue
        if (dialogActive && Input.GetKeyDown(KeyCode.Space) && inputEnabled)
        {
            DisplayNextSentence();
        }

    }

    void setup_portraits(Dialogue dia)
    {
        //called at the start of the dialogue and shows portraits of speakers in the dialogue
        //Debug.Log("setting up portraits");
        if (dia.get_startingPortrait() == null || dia.get_startingPortrait().Length == 0) return;

        for(int i = 0; i < 4; i++)
        {   
            if (dia.get_startingPortrait()[i] == 0) 
            {
                portrait[i].gameObject.SetActive(false);
            }
            else
            {
                portrait[i].gameObject.SetActive(true);
                portrait[i].sprite = dia.get_portraitManagers()[i].get_portrait(dia.get_startingPortrait()[i]);
            }
        }
    }
    void update_portraits(int speaker, int portraitIndex)
    {
        //called on each new sentence
        
        //method:
        //highlight the speaker
        //change the portrait of speaker (only the portrait of the speaker is allowed to change) or do nothing (if no change scheduled)


        //*note on the highlight: not necessarily a highlight, but more like a focus. the rest of the speakers should be
        //comparatively in shadow. just bring the speaker into focus.
        //the purpose is: draw attention to the speaker so the player can easily tell who spoke and what portrait 
        //changed without having to look over all the portraits.

        //portrait[speaker].color = new Color(highlight) //or whatever

        portrait[speaker].sprite = portraitManagers[speaker].get_portrait(portraitIndex);
    }

    public void StartDialogue(Dialogue dia)
    {
        //assign portraits:
        heldDia = dia;
        inputEnabled = false;
        dBox.SetActive(true);
        dialogActive = true;
        thePlayer.canMove = false;
        animator.SetBool("IsOpen", true);

        sentences.Clear();
        names.Clear();
        portraitIndexes.Clear();
        speakers.Clear();

        setup_portraits(dia);

        //setup portraitManagers real quick. we don't use it in setup_portraits(), don't worry
        portraitManagers = dia.get_portraitManagers();

        int temp = dia.get_sentences().Length;

        if (dia.get_portraitIndexes() == null || dia.get_portraitIndexes().Length == 0)
        {
            for (int i = 0; i < temp; i++)
            {
                names.Enqueue(dia.get_names()[i]);
                sentences.Enqueue(dia.get_sentences()[i]);
            }
        }
        else
        {
            for (int i = 0; i < temp; i++)
            {
                names.Enqueue(dia.get_names()[i]);
                sentences.Enqueue(dia.get_sentences()[i]);
                portraitIndexes.Enqueue(dia.get_portraitIndexes()[i]);
                speakers.Enqueue(dia.get_speakers()[i]);
            }
        }
       
        DisplayNextSentence();
    }

    void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        
        string name = names.Dequeue();
        string sentence = sentences.Dequeue();

        if (portraitIndexes.Count != 0)
        {
            int portraitIndex = portraitIndexes.Dequeue();
            int speaker = speakers.Dequeue();
            update_portraits(speaker, portraitIndex);
        }
        nameText.text = name;
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {      
        dText.text = "";
        foreach(char letter in sentence.ToCharArray())
        {
            dText.text += letter;
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        inputEnabled = true;
    }

    public void branch_selected(int branchId)
    {
        //called when a branch button is clicked. hides branch buttons.
        //starts up a new dialogue :)

        //convert branchid to dia's jump id
        for (int i = 0; i < 4; i++)
        {
            branchButtons[i].gameObject.SetActive(false);
        }
        WorldManager.branch_dialogue(heldDia.get_jumpId(branchId));
    }

    void EndDialogue()
    {
        if (heldDia.get_hasBranches())
        {
            for (int i = 0; i < heldDia.get_branchOptions().Length; i++)
            {
                branchButtons[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Text>().text = heldDia.get_branchOptions()[i];
                branchButtons[i].gameObject.SetActive(true);
            }
            for (int i = heldDia.get_branchOptions().Length; i < 4; i++)
            {
                branchButtons[i].gameObject.SetActive(false);
            }
        }
        else
        {
            WorldManager.exit_dialogue(heldDia);

            for (int i = 0; i < 4; i++)
            {
                portrait[i].gameObject.SetActive(false);
            }

            dBox.SetActive(false);
            dialogActive = false;
            thePlayer.canMove = true;
            //WorldManager.inDialogue = false;
            //WorldManager.show_owPreviews();

            animator.SetBool("IsOpen", false);
        }

        
    }

}
