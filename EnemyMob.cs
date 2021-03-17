using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//some notes about defeated enemies and planned behaviour:
// when leaving a dungeon, all mobs have their isDefeated set to true (except raid bosses, or plot enemies, or whatever)
// using destroy for defeated gameObjects, instead of SetActive(false)-ing them. probably better performance-wise.

public class EnemyMob : MonoBehaviour
{
    private Rigidbody2D rb;

    [SerializeField] private int identifier;
    [SerializeField] private int moveSpeed;
    [SerializeField] private float walkTime;
    [SerializeField] private float waitTime;
    private float walkCounter; //tells us when a walk is done

    //private int walkDirection;
    private GameObject huntedPlayer;
    Vector2 destination;

    [SerializeField] private bool hasWalkZone;
    //[SerializeField] private Collider2D walkZone;
    [SerializeField] private PatrolArea walkZone;
    private Vector2 minWalkPoint;
    private Vector2 maxWalkPoint;

    private bool inPursuit;
    private bool isMoving; 
    private bool canMove;
    private bool keepChecking;


    private bool isDefeated { get; set; } //stops enemies from respawning immediately after mob is fought. reset when entering dungeon.

    public void set_to_defeated() { isDefeated = true; }
    public int get_identifier() { return identifier; }

    //making mobs disappear once defeated
    void Awake()
    {
        //we'll probably have to instantiate the mob.

        //Instantiate(this.gameObject);

        //if the mob has already been defeated, do not spawn them in on reload
        //Debug.Log(isDefeated);

        if (isDefeated)
        {
            Destroy(gameObject);
        }

        //^^probably about to become unnecessary
    }
    

    public void defeated()
    {
        //called by worldmanager when the mob is killed to delete the mob
        Debug.Log("defeated()");
        Destroy(gameObject);
    }

    //mostly walking stuff
    void Start()
    {
        canMove = true;
        rb = GetComponent<Rigidbody2D>();
        
        if (hasWalkZone)
        {
            keepChecking = true;
            isMoving = false;
            minWalkPoint = walkZone.GetComponent<Collider2D>().bounds.min;
            maxWalkPoint = walkZone.GetComponent<Collider2D>().bounds.max;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }
    void FixedUpdate()
    {
        if (!WorldManager.isPaused && !WorldManager.inDialogue)
        {
            if (inPursuit)
            {
                walkCounter -= Time.deltaTime;
                if (walkCounter < 0) //then end pursuit
                {
                    inPursuit = false;
                    keepChecking = true;
                    //Debug.Log("chase finishes...");
                }

                Vector2 newPos = Vector2.MoveTowards(transform.position, huntedPlayer.transform.position, Time.fixedDeltaTime * moveSpeed);
                rb.MovePosition(newPos);   
            }
            else if (hasWalkZone && keepChecking)
            {
                if (isMoving)
                {
                    walkCounter -= Time.deltaTime;
                    if (walkCounter < 0)
                    {
                        isMoving = false;
                    }
                    
                    Vector2 newPosition = Vector2.MoveTowards(transform.position, destination, Time.fixedDeltaTime * moveSpeed);
                    rb.MovePosition(newPosition);                    
                }
                else
                {
                    keepChecking = false;
                    StartCoroutine(beginChoosingNewDestination());
                }
            }
        }       
    }

    public void begin_pursuit(Collider2D other)
    {
        keepChecking = false;
        inPursuit = true;
        walkCounter = 15f;
        //called from patrol area when player enters.
        // this function makes mob chase player for a given time
        huntedPlayer = other.gameObject;
    }

    IEnumerator beginChoosingNewDestination()
    {        
        yield return new WaitForSeconds(waitTime);
        ChooseDirection();
    }
    void ChooseDirection()
    {
        //Debug.Log("chooseDirection() called");
        //comes up with a new spot within unit's walk zone.
        walkCounter = walkTime;
        keepChecking = true;
        isMoving = true;
        float xNum = Random.Range(minWalkPoint.x, maxWalkPoint.x);
        float yNum = Random.Range(minWalkPoint.y, maxWalkPoint.y);

        destination = new Vector2(xNum, yNum);
    }

    
    void OnTriggerEnter2D(Collider2D other)
    {
        //only call a battle if it's the player.
        if (other.CompareTag("Player"))
        {
            isDefeated = true;
            other.gameObject.SetActive(false);

            WorldManager.load_battle(other.gameObject, gameObject, SceneManager.GetActiveScene().name);
            //WorldManager.get_dungeonManager.load_encounter();
            

        }
    }
    


}
