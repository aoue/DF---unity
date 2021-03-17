using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    //[SerializeField] private GameObject followTarget;
    private GameObject followTarget;
    private Vector3 targetPosition;
    private float cameraSpeed; //can freeze camera by setting its speed to zero. e.g. for a room that fits in the whole scene.

    public bool cameraFollow { get; set; }



    // Start is called before the first frame update
    void Start()
    {
        //associate it with the right followTarget
        
        cameraSpeed = 2f;
        followTarget = GameObject.Find("Player");
        transform.position = new Vector3(followTarget.transform.position.x, followTarget.transform.position.y, transform.position.z);
        cameraFollow = true;

    }

    // Update is called once per frame
    void Update()
    {
        if (cameraFollow)
        {
            
            targetPosition = new Vector3(followTarget.transform.position.x, followTarget.transform.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, cameraSpeed * Time.deltaTime);         
            //^slide version

            //transform.position = new Vector3(followTarget.transform.position.x, followTarget.transform.position.y, transform.position.z);
            //^move along version
        }
       
    }
}
