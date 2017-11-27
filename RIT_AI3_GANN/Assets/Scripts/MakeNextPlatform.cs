using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeNextPlatform : MonoBehaviour
{
    /// <summary>
    /// This will generating the next new random platform
    /// </summary>

    public GameObject platform; // The platform prefab
    public float minLength; // The minimum length of the next platform
    public float maxLength; // The maximum length of the next platform
    public float heightRange; // What's the maximum difference of the altitude 
                              // from the new platform to the previous one can be
    public bool randAngle; // Does the platforms have random angles or all remains flat?
    public bool angleRange; // The maximum angle the new platform can tilted to the left or right

    public GameObject nextPlatform; // The new platform that's been generated

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void MakeTheNextPlatform()
    {

    }
}
