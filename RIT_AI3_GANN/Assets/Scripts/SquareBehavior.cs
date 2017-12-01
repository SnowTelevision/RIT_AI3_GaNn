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

    public float mass; // The weight of the square, used to multiplied with the force when push forward
    public float gravity; // The scale of the gravity, used to multiplied with the force when jump
    /// <summary>
    /// Some variebles to be monitored
    /// </summary>
    public GameObject currentPlatform; // The platform the square currently stepped on
    public float currentPlatformSlope; // The slope of the current platform
    public float distanceToEdge; // The distance between the square and the right end of the current platform 
                                 // (If the square is currently not touching the platform, then it will be 0)
    public GameObject nextPlatform; // The next platform to the one the square currently stepped on
    public float nextPlatformLength; // The length of the next platform
    public float nextPlatformAltitudeDiff; // The difference of the y coord between the current platform and the next platform
    public float nextPlatformslope; // The slope of the next platform
    public float currendSpeed; // The current speed of the square

    /// <summary>
    /// Data structure for the neural network
    /// </summary>

    public float[,] basicLayer; // The default layer structure, if this is good enough then this layer won't change and there won't be more layers
                                // The x coord will be the index of the input nodes and the y will for output nodes
    public float input1; // The distance between the square and the right end of the current platform normalized to a range between 0-1
    public float input2; // The length of the next platform normalized to a range between 0-1
    public float input3; // The difference of the y coord between the current platform and the next platform normalized to a range between 0-1
    public float input4; // The current speed of the square normalized to a range between 0-1
    public float output1; // How likely it should jump with the force of jumpForce1
    public float output2; // How likely it should jump with the force of jumpForce2
    public float output3; // How likely it should jump with the force of jumpForce3
    public float output4; // How likely it should jump with the force of jumpForce4
    public float output5; // How likely it should jump with the force of jumpForce5
    public float output6; // How likely it should jump with the force of jumpForce6
    public float output7; // How likely it should jump with the force of jumpForce7
    public float output8; // How likely it should jump with the force of jumpForce8
    public float output9; // How likely it should jump with the force of jumpForce9
    public float output10; // Node to trigger jump (with the jumpforce that has the highest output value
    public float output11; // Node to trigger forward impulse

    // Use this for initialization
    void Start()
    {
        basicLayer = new float[4 + 1, 11];
        mass = GetComponent<Rigidbody2D>().mass;
        gravity = GetComponent<Rigidbody2D>().gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        currendSpeed = GetComponent<Rigidbody2D>().velocity.x;
        //distanceToEdge = currentPlatform.transform.position.x + currentPlatform.transform.localScale.x * 0.5f - transform.position.x; // Calculate 
                                                                                                                                      //how far is the square away from the right end of the current platform it stepped on

        GetComponent<Rigidbody2D>().AddForce(transform.right * moveForce * mass, ForceMode2D.Impulse);
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        GetComponent<Rigidbody2D>().AddForce(transform.up * jumpForce9 * mass * gravity, ForceMode2D.Impulse);
        print("jump");
    }
}
