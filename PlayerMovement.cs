using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float movementSpeed;
    private Rigidbody2D rbody;

    private PlayerRenderer renderer;

    public bool canMove { get; set; }

    private void Awake()
    {
        movementSpeed = 3.0f;
        canMove = true;
        rbody = GetComponent<Rigidbody2D>();
        renderer = GetComponentInChildren<PlayerRenderer>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!canMove || WorldManager.isPaused)
        {
            rbody.velocity = Vector2.zero;
            renderer.SetDirection(rbody.velocity);
            return;
        }

        
        //if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        //{
        //    movementSpeed = 10.0f;
        //    Debug.Log("shift key was pressed");
        //}
        //else
        //{
        //    movementSpeed = 3.0f;
        //}
        

        /*
        Vector2 currentPos = rbody.position;
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector2 inputVector = new Vector2(horizontalInput, verticalInput);
        inputVector = Vector2.ClampMagnitude(inputVector, 1);
        Vector2 movement = inputVector * movementSpeed;
        Vector2 newPos = currentPos + movement * Time.fixedDeltaTime;
        renderer.SetDirection(movement);
        rbody.MovePosition(newPos);
        */

        //wasd
        float horizontalInput = 0f;
        float verticalInput = 0f;
        Vector2 currentPos = rbody.position;

        //wasd
        if (Input.GetKey(KeyCode.A))
            horizontalInput = -1.0f;
        else if (Input.GetKey(KeyCode.D))
            horizontalInput = 1.0f;

        if (Input.GetKey(KeyCode.W))
        {
            if (horizontalInput == 0)
            {
                verticalInput = 1.0f;
            }
            else
            {
                verticalInput = 0.5f;
            }
        }
        else if (Input.GetKey(KeyCode.S))
        {
            if (horizontalInput == 0)
            {
                verticalInput = -1.0f;
            }
            else
            {
                verticalInput = -0.5f;
            }
        }
            
        //wasd over

        Vector2 inputVector = new Vector2(horizontalInput, verticalInput);
        inputVector = Vector2.ClampMagnitude(inputVector, 1);
        Vector2 movement = inputVector * movementSpeed;
        Vector2 newPos = currentPos + movement * Time.fixedDeltaTime;
        renderer.SetDirection(movement);
        rbody.MovePosition(newPos);
    }
}
