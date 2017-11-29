using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    /// <summary>
    /// This script controls the entire simulation, storing some data which will be shared through the simulation
    /// </summary>

    public float simSpeed; // The speed the simulation runs
    public float cycleDuration; // The time each generation runs (etc. 10 sec)
    public int fps; // What's the targeting fps for the simulation?

    public static float randomSeedThisSim; // The seed for the default rand() for this simulation
    public static SquareBehavior[] squares; // The array that stores the current generation of squares
    public static int platformCount; // Count for how many platforms has been generated

    public int genNum; // The current number of generations for this simulation

    void Awake()
    {
        Random.InitState(0);
        Application.targetFrameRate = fps;
    }

    // Use this for initialization
    void Start()
    {
        platformCount = 1;
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = simSpeed;
    }
}
