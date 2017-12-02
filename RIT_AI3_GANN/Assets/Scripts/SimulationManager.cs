using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    /// <summary>
    /// This script controls the entire simulation, storing some data which will be shared through the simulation
    /// </summary>

    //public float simSpeed; // The speed the simulation runs
    public float cycleDuration; // The time each generation runs (etc. 10 sec)
    public int fps; // What's the targeting fps for the simulation?
    public float travelDistanceScore; // Multiplier for the square's travel distance
    public float travelPlatformScore; // Multiplier for the platform the square travelled
    public float squareOnPlatformScore; // Multiplier for the time the square is touching the platforms
    public int populationEachGen; // How many squares do we create for each generation
    public GameObject square; //
    public GameObject platform; //
    public float crossOverRate; // The rate for a value to exchange with another parent's
    public float mutationRate; // The rate for a value to mutate to a random number
    public int minWeightValue; // The minimum weight value
    public int maxWeightValue; // The maximum weight value

    public static float sCrossOverRate;
    public static float sMutationRate;
    public static int sMinWeightValue;
    public static int sMaxWeightValue;
    public static float randomSeedThisSim; // The seed for the default rand() for this simulation
    public static SquareBehavior[] squares; // The array that stores the current generation of squares
    public static SquareBehavior[] lastSquares; // The array that stores the last generation of squares
    public static int platformCount; // Count for how many platforms has been generated

    public int genNum; // The current number of generations for this simulation
    public static float lastGenTime; // The start time of the current generation

    void Awake()
    {
        Random.InitState(0);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
        //Application.targetFrameRate = -1;
        //print(Application.targetFrameRate);
    }

    // Use this for initialization
    void Start()
    {
    //    Random.InitState(0);
    //    QualitySettings.vSyncCount = 0;
    //    Application.targetFrameRate = fps;
        platformCount = 1;
        genNum = 0;
        sCrossOverRate = crossOverRate;
        sMutationRate = mutationRate;
        squares = new SquareBehavior[populationEachGen];
        lastSquares = new SquareBehavior[populationEachGen];

        NewGen();
    }

    // Update is called once per frame
    void Update()
    {
        //Time.timeScale = simSpeed;
        //print(Mathf.RoundToInt(1.0f / Time.deltaTime) + ", " + Application.targetFrameRate + ", " + QualitySettings.vSyncCount);

        if(Time.time - lastGenTime >= cycleDuration)
        {
            NewGen();
        }
    }

    public void NewGen() // The method to start a new generation
    {
        lastGenTime = Time.time;

        lastSquares = FindObjectsOfType<SquareBehavior>();
        for (int i = 0; i < lastSquares.Length; i++)
        {
            CalculateFitnessScore(lastSquares[i]);
            lastSquares[i].gameObject.SetActive(false);
        }

        Instantiate(platform, Vector3.zero, Quaternion.identity);

        for(int i = 0; i < populationEachGen; i++)
        {
            squares[i] = Instantiate(square, transform.position, Quaternion.identity).GetComponent<SquareBehavior>();
        }
    }

    public void CalculateFitnessScore(SquareBehavior square)
    {
        square.fitnessScore = square.travelDist * travelDistanceScore +
                              square.travelPlat * travelPlatformScore +
                              square.timeStayedOnPlatform * squareOnPlatformScore;
    }
}
