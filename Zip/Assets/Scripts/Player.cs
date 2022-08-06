using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    //For Movement
    Rigidbody rb;
    public float speed;
    private Vector3 movement;
    Vector3 movementInputs;

    //For Camera
    private Camera cam;
    public float sensitivity;
    Vector2 mouseInputs;

    //For zipline
    public bool isInterrupted;
    public LayerMask zipLayer;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        HandleInput();
        HandleCamera();
        RaycastHit hit;
        if(Physics.Raycast(cam.transform.position,cam.transform.forward,out hit,2,  zipLayer))
        {
            //If you can access the zipline press "E"
            if (Input.GetKeyDown(KeyCode.E))
            {
                //If statement for either point
                if (hit.collider.tag == "Point1")
                {
                    isInterrupted = true;
                    StartCoroutine(ZipLine(hit.collider.GetComponent<LineRenderer>(),true));
                }
                else if (hit.collider.tag == "Point2")
                {
                    isInterrupted = true;
                    StartCoroutine(ZipLine(hit.collider.GetComponent<LineRenderer>(),false));
                }
            }
            
        }
        //Stop riding the zipline mid ride
        if(isInterrupted && Input.GetKeyDown(KeyCode.Space))
        {
            StopAllCoroutines();
            rb.useGravity = true;
            isInterrupted = false;
        }
    }

    private void FixedUpdate()
    {
        if(!isInterrupted)
            HandleMovement();
    }

    void HandleInput()
    {
        //Simple inputs
        movementInputs.x = Input.GetAxis("Horizontal");
        movementInputs.z = Input.GetAxis("Vertical");
        mouseInputs.x -= Input.GetAxisRaw("Mouse Y") * sensitivity;
        mouseInputs.y += Input.GetAxisRaw("Mouse X") * sensitivity;
    }
    void HandleMovement()
    {
        //Basic movement
        movement = transform.right * movementInputs.x + transform.forward * movementInputs.z;
        movement.Normalize();
        movement *= speed;
        movement += new Vector3(0, rb.velocity.y, 0);
        rb.velocity = movement;
    }

    void HandleCamera()
    {
        //Basic Camera
        cam.transform.localEulerAngles = new Vector3(mouseInputs.x, 0, 0);
        transform.eulerAngles = new Vector3(0, mouseInputs.y, 0);
    }
    IEnumerator ZipLine(LineRenderer line, bool isPointOne)
    {
        Vector3 start = Vector3.zero;
        Vector3 des = Vector3.zero;

        //Deciding the start point and the destination
        if (isPointOne)
        {
            start = line.GetPosition(0) + new Vector3(0, -1f, 0);
            des = line.GetPosition(1) + new Vector3(0, -1f, 0);
        }
        else
        {
            start = line.GetPosition(0) + new Vector3(0, -1f, 0);
            des = line.GetPosition(1) + new Vector3(0, -1f, 0);
        }
        
        float dist = Vector3.Distance(start, des);
        //Calculating the time it would take to finish the ride
        float duration = dist / speed;
        float currentDuration = 0;
        //Disable gravity (VERY IMPORTANT)
        rb.useGravity = false;

        //Going through the ride
        while (currentDuration < duration)
        {
            float t = currentDuration / duration;
            //A simple smoothstep function
            t = t * t * (3f - 2f * t);
            //Moving
            transform.position = Vector3.Lerp(start, des, t);
            currentDuration += Time.deltaTime;
            
            yield return null;
        }
        //Set position to the actual destination to be precise
        transform.position = des;

        //Ending function and go back to default settings
        rb.useGravity = true;
        isInterrupted = false;
        
    }
}
