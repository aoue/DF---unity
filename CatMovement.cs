using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CatMovement : MonoBehaviour
{
    private float moveSpeed;
    private Rigidbody2D rb;
    private bool isMoving;
    private float walkTime;
    private float walkCounter;
    private float waitTime;
    private float waitCounter;
    private int walkDirection;
    private Collider2D walkZone;
    private bool hasWalkZone;
    private Vector2 minWalkPoint;
    private Vector2 maxWalkPoint;

    public bool canMove { get; set; }
    private DialogueManager dMan;

    // Start is called before the first frame update
    void Awake()
    {
        canMove = true;
        //dMan = FindObjectOfType<DialogueManager>();
        rb = GetComponent<Rigidbody2D>();
        waitCounter = waitTime;
        walkCounter = walkTime;

        ChooseDirection();

        if(walkZone != null)
        {
            hasWalkZone = true;
            minWalkPoint = walkZone.bounds.min;
            maxWalkPoint = walkZone.bounds.max;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!WorldManager.inDialogue && !WorldManager.isPaused)
        {
            canMove = true;
        }


        if (!canMove && !WorldManager.isPaused)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (isMoving && !WorldManager.isPaused)
        {
            walkCounter -= Time.deltaTime;
            
            //move
            switch (walkDirection)
            {
                case 0:
                    rb.velocity = new Vector2(0, moveSpeed);
                    if(hasWalkZone && transform.position.y > maxWalkPoint.y)
                    {
                        isMoving = false;
                        waitCounter = waitTime;
                    }
                    break;

                case 1:
                    rb.velocity = new Vector2(moveSpeed, 0);
                    if (hasWalkZone && transform.position.x > maxWalkPoint.x)
                    {
                        isMoving = false;
                        waitCounter = waitTime;
                    }
                    break;

                case 2:
                    rb.velocity = new Vector2(0, -moveSpeed);
                    if (hasWalkZone && transform.position.y < minWalkPoint.y)
                    {
                        isMoving = false;
                        waitCounter = waitTime;
                    }
                    break;

                case 3:
                    rb.velocity = new Vector2(-moveSpeed, 0);
                    if (hasWalkZone && transform.position.x < minWalkPoint.x)
                    {
                        isMoving = false;
                        waitCounter = waitTime;
                    }
                    break;
            }

            if (walkCounter < 0)
            {
                isMoving = false;
                waitCounter = waitTime;
            }


        }
        else
        {
            waitCounter -= Time.deltaTime;
            rb.velocity = Vector2.zero;
            
            if(waitCounter < 0)
            {
                ChooseDirection();
            }

        }
    }

    public void ChooseDirection()
    {
        walkDirection = Random.Range(0, 4);
        isMoving = true;
        walkCounter = walkTime;
    }

}
