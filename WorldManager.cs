using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//in charge of managing everything. god. (in both senses) (ctrl-m -> ctrl-o)
public class WorldManager : MonoBehaviour
{
    //SINGLETON STUFF
    public static WorldManager _instance;
    private static bool firstTime = true;
    //public static WorldManager Instance {  get { return _instance; } }

    //OVERWORLD PARTY PREVIEW
    [SerializeField] private GameObject[] owCharsPreviews;
    
    //general state stuff
    //private static bool firstTime = true;
    public static int currentChapter { get; set; } //the game's current chapter. only advances.
    public static int LEVELCAP { get; set; } //max level for any unit.
    public static bool isPaused { get; set; } //a pause for the whole game. 
    public static string dateAndTime { get; set; } //string date and time for display
    public static bool inPortal { get; set; } //marks whether player is standing in a portal.
    
    //BATTLE MANAGEMENT
    public static bool inBattle { get; set; }
    public static string prevScene { get; set; } //(str) name of previous scene. used to return to overworld. set with active scene when called.
    public static GameObject prevPlayer { get; set; } //player obj from previous scene
    public static int battleTickLimit { get; set; } //the maximum number of ticks until the battle ends. -1 for unlimited.
    public static bool winOnTimeOut = false; //if true, then player wins when time elapses. if false, then player loses.
    
    private static double[,] affList =
    {
        { 1, 1, 1, 1, 1, 0.5, 0.5, 1, 1, 1 }, //#normal
        { 1.5, 1.5, 1, 0.5, 1, 2, 0.5, 2, 1, 2 }, //#beast
        { 1, 1, 0.5, 0.5, 1, 2, 2, 1, 1, 1 }, //#ice
        { 1, 2, 2, 0.25, 1, 0.5, 0.5, 2, 0.5, 1 }, //#fire
        { 1.5, 2, 1, 1, 0.25, 0.25, 2, 0.25, 1, 1 }, //#spark
        { 1, 1, 0.5, 1, 1, 1, 0.25, 0.25, 1, 1 }, //#earth
        { 1, 1.5, 1, 0.5, 0.25, 2, 1, 1, 2, 1 },// #metal
        { 1.5, 1, 1.5, 1.5, 1.5, 0.25, 1, 3, 2, 0.25 }, //#vile
        { 0.25, 1, 1, 1, 1, 1, 1, 2, 0, 1 }, //#heroic
        {1, 1, 1, 1, 1, 1, 0.25, 1, 1, 0 } //adaptive


    }; //global affinity/elemental weakness list. access with [defender's aff][move's ele]

    //inventory testing stuff
    [SerializeField] private Gear example_gear;

    //public static double[,] get_affList() { return affList; }
    public static double get_eleMod(int defAff, int ele)
    {
        return affList[defAff, ele];
    }

    //OVERWORLD MANAGEMENT
    [SerializeField] private PortalManager portalManager;

    //CAMERA MANAGEMENT
    [SerializeField] private Camera camera;
    //[SerializeField] private GameObject playerChar;
    private GameObject playerChar;

    //TRI MENU MANAGEMENT
    [SerializeField] private TriMenu triMenu;
    [SerializeField] private PartyManager pManager;
    [SerializeField] private QuestManager qManager;
    [SerializeField] private EncyclopediaManager eManager;
    [SerializeField] private InventoryManager iManager;
    [SerializeField] private Pattern patternManager;

    //DUNGEON MANAGEMENT
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private GameObject dungeonOverlay;
    [SerializeField] private OutHealer hManager;
    private static bool inDungeon = true; //true if in dungeon, false if not. controls a bunch of overlay stuff.

    //EXAMINATION
    [SerializeField] private ExamineManager examiner;

    //DIALOGUE, CUTSCENE MANAGMENT
    public static bool inDialogue { get; set; } //[prevents trimenu opening] and [prevents a new dialogue from starting] when true
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private FreeDialogueManager freeDialogueManager;
    [SerializeField] private CutsceneManager cutsceneManager;
    [SerializeField] private NotificationManager noteManager;
    [SerializeField] private DialogueLibrary diaLibrary;

    public static Inventory worldInventory { get; set; }

    //party stuff
    [SerializeField]
    private PlayerUnit startingUnit, startingUnit2;
    public static PlayerUnit[] partyReserve { get; set; } //all units in the player's party but not deployed in battle
    public static PlayerUnit[] party { get; set; } //all units deployed on the player's side
    public static Enemy[] enemyParty { get; set; } //all units deployed on the enemy's side
    public static (int xLoc, int yLoc)[] deploymentCoordinator; //saves deployment before battles so player doesn't have to reset deployment each time.
    public static (int xLoc, int yLoc)[] Edeployer; //saves deployment from formation from enemy to be placed in battle.

    //party testing helper
    [SerializeField] private PlayerMove pm;

    //JUMPING SCENES W/ PORTALS
    public static void open_portal(int portalKey)
    {
        //called when player moves into a portal's area
        //maybe a slight half second delay would be nice? also an animation of the popup too.      
        inPortal = true;
        _instance.portalManager.show_portal_prompt(portalKey);
        //change status to us being in the portal, so that the next spacebar press warps through.
        WorldManager.prevScene = _instance.portalManager.get_portal(portalKey);
    }
    public static void close_portal(bool isJump=false)
    {
        //called when player moves out of a portal's area or goes through a portal
        inPortal = false;
        _instance.portalManager.hide_portal_prompt();
    }
    public static void jump_portal()
    {
        //called when player goes through a portal. detected here in update, it's a key trigger.
        _instance.hide_owPreviews();
        inPortal = false;
        SceneManager.LoadScene(WorldManager.prevScene);
    }

    //DIALOGUE AND CUTSCENE STARTS
    public static void branch_dialogue(int diaId)
    {
        Dialogue dia = _instance.diaLibrary.retrieve_dialogue(_instance.diaLibrary.get_heldNpcId(), diaId);
        _instance.dialogueManager.StartDialogue(dia);
    }
    public static void begin_dialogue(int npcId, int diaId)
    {
        WorldManager.inDialogue = true;              
        _instance.hide_owPreviews();

        //retrive dialogue from dialogueLibrary based on chapter and id passed in
        Dialogue dia = _instance.diaLibrary.retrieve_dialogue(npcId, diaId);
        _instance.dialogueManager.StartDialogue(dia);       
    }
    public static void begin_freeDialogue(int npcId, int diaId)
    {
        WorldManager.inDialogue = true;
        //retrive dialogue from dialogueLibrary based on chapter and id passed in
        Dialogue dia = _instance.diaLibrary.retrieve_dialogue(npcId, diaId);
        _instance.freeDialogueManager.StartDialogue(dia);
    }
    public static void begin_cutscene(CutsceneHolder ch)
    {
        WorldManager.inDialogue = true;
        _instance.hide_owPreviews();

        Cutscene cs = _instance.cutsceneManager.get_cutscene(ch.get_csType(), ch.get_cutsceneId());
        _instance.cutsceneManager.startCutscene(cs);

        //now, start the dialogue
        _instance.dialogueManager.StartDialogue(cs);
    }
    public static void exit_cutscene()
    {
        WorldManager.inDialogue = false;
        _instance.cutsceneManager.exit_cutscene();
        _instance.show_owPreviews();
    }
    public static void exit_dialogue(Dialogue dia)
    {
        exit_freeDialogue(dia);
        _instance.show_owPreviews();
    }
    public static void exit_freeDialogue(Dialogue dia)
    {
        //from here, do any questie things or something.
        //grab attached dialogue and run its postDialogue function:
        // -does nothing by default
        // -is overloaded when it wants something to be done.
        dia.post_dia();

        //inDialogue = false;
        _instance.help_inDialogue();  
    }
    void help_inDialogue()
    {
        StartCoroutine(disable_inDialogue());
    }
    IEnumerator disable_inDialogue()
    {
        yield return new WaitForSeconds(1.0f);
        inDialogue = false;   
    }

    public static void add_notification(string note)
    {
        _instance.noteManager.add_note(note);
    }

    public static void progress_quest(Quest progressme)
    {
        //check if quest is there.
        _instance.qManager.progress_quest(progressme);
        _instance.examiner.format_quest_progress(progressme);
    }
    public static void add_quest_to_chapter(Quest addme)
    {
        //used to insert a quest into qmanager from the top level
        _instance.qManager.add_quest(addme);
        _instance.examiner.format_quest_pickup(addme);
    }

    //OPENING CHESTS
    public static void open_chest_loot(Loot l)
    {
        //show examine box
        _instance.examiner.format_chest_loot(l);
    }
    public static void open_chest_gear(Gear g)
    {
        //show examine box    
        _instance.examiner.format_chest_gear(g);
    }

    void Awake()
    {        
        if (_instance != null)
        {           
            Destroy(_instance.gameObject);
            //destroy new version of player too
        }
        _instance = this;       
           
        if (firstTime)
        {
            prevPlayer = GameObject.Find("Player");
            DontDestroyOnLoad(prevPlayer);

            firstTime = false;
            dateAndTime = "November 22, 141";
            LEVELCAP = 10;
            currentChapter = 0;
            battleTickLimit = -1;
            qManager.setup();
            worldInventory = new Inventory();
            worldInventory.build_starting_inventory(example_gear);

            //putting the party together here for testing purposes
            party = new PlayerUnit[2];
            deploymentCoordinator = new (int, int)[party.Length];
            Edeployer = new (int, int)[8];

            party[0] = Instantiate(startingUnit);
            party[1] = Instantiate(startingUnit2);

            //testing move switching
            party[0].get_reserveFrontList().Add(pm);
            party[0].get_reserveFrontList().Add(pm);
            party[0].get_reserveFrontList().Add(pm);

                
            for (int i = 0; i < party.Length; i++)
            {
                DontDestroyOnLoad(party[i]);
            }
        }
        else
        {
            //show name of scene we're in
            add_notification(prevScene);
        }

        show_owPreviews();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && inPortal)
        {
            //warp maps
            jump_portal();
        }
        else if (!inBattle && !inDialogue)
        {
            if (Input.GetMouseButtonDown(1))
            {
                //open/close trimenu
                if (!isPaused)
                {
                    isPaused = true;
                    hide_owPreviews();
                    triMenu.open(party, dateAndTime);
                }
                else if (isPaused && triMenu.gameObject.active)
                {
                    isPaused = false;
                    triMenu.gameObject.SetActive(false);
                    show_owPreviews();
                }
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                //open outHealer
                open_healer();
            }
                   
        }
        
    }
    public void open_healer()
    {
        isPaused = true;
        hManager.open_outHealer(party);
    }
    public void open_party(int index)
    {
        triMenu.gameObject.SetActive(false);
        pManager.gameObject.SetActive(true);
        pManager.load_inventory(worldInventory);
        pManager.load_party_unit(party[index]);
    }
    public void open_journal()
    {
        triMenu.gameObject.SetActive(false);
        qManager.gameObject.SetActive(true);
        qManager.load_mainView(currentChapter);
    }
    public void open_pattern()
    {
        triMenu.gameObject.SetActive(false);
        patternManager.gameObject.SetActive(true);
        patternManager.load_mainView(party);
    }
    public void open_encyclopedia()
    {
        triMenu.gameObject.SetActive(false);
        eManager.gameObject.SetActive(true);
        eManager.load_mainView();
    }
    public void open_inventory()
    {
        triMenu.gameObject.SetActive(false);
        iManager.gameObject.SetActive(true);
        iManager.load_mainView(worldInventory);
    }

    public static void load_battle(GameObject prev, GameObject mob, string prScene)
    {
        //jumps to battle scene from overworld
        _instance.save_deployment();

        prevPlayer = prev;
        //DontDestroyOnLoad(prevPlayer);
        prevScene = prScene;

        // disable/hide all mobs on screen       
        _instance.dungeonManager.adjust_aliveList(mob.GetComponent<EnemyMob>());

        //generate and register positions for the enemy party:
        _instance.hide_owPreviews();
        enemyParty = _instance.dungeonManager.generate_encounter();
        SceneManager.LoadScene("CombatScene");
    }
    public static void return_from_battle()
    {
        //only comes through here if player won, otherwise, use a gameover function

        //returns to overworld from battle
        foreach (PlayerUnit unit in party)
        {
            unit.GetComponent<SpriteRenderer>().enabled = false;
        }
        //currently there's a blip as sprites disappear, but a screen animation can hide this.

        //return party units to their pre-battle positions.
        _instance.restore_deployment();

        inBattle = false;

        SceneManager.LoadScene(prevScene);
        prevPlayer.SetActive(true);
        //_instance.show_owPreviews();
    }

    //save and restore deployment
    void save_deployment()
    {
        //saves party units positions before a battle, so that they can be returned to them afterwards
        for (int i = 0; i < party.Length; i++)
        {
            deploymentCoordinator[i].xLoc = party[i].get_x();
            deploymentCoordinator[i].yLoc = party[i].get_y();
        }
    }
    void restore_deployment()
    {
        //returns party units to their pre-battle positions, which are stored in deploymentCoordinator
        //deploymentCoordinator

        //parallel lists are the best
        for (int i = 0; i < party.Length; i++)
        {
            party[i].set_x(deploymentCoordinator[i].xLoc);
            party[i].set_y(deploymentCoordinator[i].yLoc);
        }
    }
    public static void set_formationSpot(int index, int xspot, int yspot)
    {
        Edeployer[index].xLoc = xspot;
        Edeployer[index].yLoc = yspot;
    }

    
    //OVERWORLD PARTY PREVIEW
    public void show_owPreviews()
    {
        //this is always visible, except when we're in dialogue or in cutscene
        //Debug.Log("show ow previews called");
        
        for (int i = 0; i < party.Length; i++)
        {
            owCharsPreviews[i].SetActive(true);
            owCharsPreviews[i].gameObject.GetComponent<Image>().sprite = party[i].get_tilePortrait();
            //update go -> img

            //update go -> hp slider, mp slider
            Slider hpSlider = owCharsPreviews[i].transform.GetChild(0).GetComponent<Slider>();
            Slider mpSlider = owCharsPreviews[i].transform.GetChild(1).GetComponent<Slider>();

            hpSlider.maxValue = party[i].get_hpMax_actual();
            hpSlider.value = party[i].get_hp();

            mpSlider.maxValue = party[i].get_mpMax_actual();
            mpSlider.value = party[i].get_mp();
        }
        for (int i = party.Length; i < 4; i++)
        {
            owCharsPreviews[i].SetActive(false);
        }
        owCharsPreviews[0].transform.parent.gameObject.SetActive(true);

        //update dungeon threat bar (if in dungeon)
        if (inDungeon)
        {
            //update value of dungeon bar
            Slider threatSlider = dungeonOverlay.transform.GetChild(0).GetComponent<Slider>();
            threatSlider.value = dungeonManager.get_threatValue();

            dungeonOverlay.SetActive(true);
        }
    }
    private void hide_owPreviews()
    {
        //hide ow parent obj, which hides all owCharsPreviews by implication
        //Debug.Log("hide ow previews called");
        owCharsPreviews[0].transform.parent.gameObject.SetActive(false);

        //also hides dungeon threat bar (if in dungeon)
        if (inDungeon)
        {
            dungeonOverlay.SetActive(false);
        }      
    }
}
