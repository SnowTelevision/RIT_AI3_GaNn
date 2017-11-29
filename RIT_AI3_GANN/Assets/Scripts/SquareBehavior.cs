using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareBehavior : MonoBehaviour
{
    /// <summary>
    /// This contains the neural network and the behavior controlled by the neural network 
    /// as well as some extra info about this particular square
    /// </summary>

    public float moveForce; // The force applied to the square each time it decided to move forward
    public int jumpForce1; // 
    public int jumpForce2; // 
    public int jumpForce3; // 
    public int jumpForce4; // 
    public int jumpForce5; // 
    public int jumpForce6; // 
    public int jumpForce7; // 
    public int jumpForce8; // 
    public int jumpForce9; // 

    public float travelDist; // How far the square travelled
    public float travelPlat; // How many platform the square travelled 
                             // (the platform will only be counted if the square actually lands on it,
                             // if the square jumped over a platform and landed on the platform ahead,
                             // then this number will only increased by 1, which exclude the platform it jumped over)

    /// <summary>
    /// Some variebles to be monitored
    /// </summary>
    public GameObject currentPlatform; // The platform the square currently stepped on
    public float currentPlatformSlope; // The slope of the current platform
    public float distanceToEdge; // The distance between the square and the right end of the current platform
    public GameObject nextPlatform; // The next platform to the one the square currently stepped on
    public float nextPlatformLength; // The length of the next platform
    public float nextPlatformAltitudeDiff; // The difference of the y coord between the current platform and the next platform
    public float nextPlatformslope; // The slope of the next platform
    public float currendSpeed; // The current speed of the square

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        currendSpeed = GetComponent<Rigidbody>().velocity.x;
        distanceToEdge = currentPlatform.transform.position.x + currentPlatform.transform.localScale.x * 0.5f - transform.position.x; // Calculate 
                                                             //how far is the square away from the right end of the current platform it stepped on
        
    }
}
