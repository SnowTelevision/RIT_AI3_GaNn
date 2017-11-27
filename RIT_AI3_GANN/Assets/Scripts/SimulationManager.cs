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

    public static float randomSeedThisSim; // The seed for the default rand() for this simulation
    public static SquareBehavior[] squares; // The array that stores the current generation of squares

    public int genNum; // The current number of generations for this simulation

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = simSpeed;
    }
}
