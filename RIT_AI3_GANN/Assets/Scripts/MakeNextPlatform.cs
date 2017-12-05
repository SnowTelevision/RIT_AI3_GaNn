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
    public float minDistance; // What's the minimum distance between the new platform and this platform
    public float maxDistance; // What's the maximum distance between the new platform and this platform
    public bool randAngle; // Does the platforms have random angles or all remains flat?
    public float angleRange; // The maximum angle the new platform can tilted to the left or right

    public GameObject nextPlatform; // The new platform that's been generated
    public SimulationManager manager; // The simulation manager
    public float currentGenStartTime; // The time the current generation started

    // Use this for initialization
    void Start()
    {
        manager = FindObjectOfType<SimulationManager>();
        currentGenStartTime = SimulationManager.sLastGenTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentGenStartTime != SimulationManager.sLastGenTime) // If a new generation started
        {
            Destroy(gameObject);
        }

        if (SimulationManager.platformCount <= 2 && nextPlatform == null && currentGenStartTime == SimulationManager.sLastGenTime)
        {
            MakeTheNextPlatform();
        }

        if (nextPlatform != null && nextPlatform.GetComponent<MakeNextPlatform>().nextPlatform == null && currentGenStartTime == SimulationManager.sLastGenTime && SimulationManager.leadSquare.transform.position.x >= transform.position.x - transform.localScale.x * 0.5f)
        {
            nextPlatform.GetComponent<MakeNextPlatform>().MakeTheNextPlatform();
        }
    }

    public void MakeTheNextPlatform()
    {
        Vector3 scale = Vector3.one;
        scale.x = Random.Range(minLength, maxLength); // Randomly change the new platform's length

        Vector3 position = transform.position;
        position.x += transform.localScale.x * 0.5f; // Start counting distance from the right end of the current platform
        position.x += scale.x * 0.5f; // Count in the distance from the left end of the new platform
        position.x += Random.Range(minDistance, maxDistance); // Add a random range of distance between the new platform
        position.y += Random.Range(-heightRange, heightRange);

        Vector3 euler = Vector3.zero;

        nextPlatform = Instantiate(platform, position, Quaternion.identity);
        nextPlatform.transform.localScale = scale;

        if (randAngle)
        {
            nextPlatform.transform.eulerAngles = euler;
        }

        SimulationManager.platformCount++;
    }
}
