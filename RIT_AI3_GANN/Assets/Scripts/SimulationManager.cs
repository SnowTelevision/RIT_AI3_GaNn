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
    public static SquareBehavior leadSquare; // The square that is currently taking the lead in this simulation
    public static GameObject mainCamera; // The camera
    public static Vector3 cameraLocalPosi; // The camera local position
    public SquareBehavior[] tempArray; // Array use to combine lastSquares and bestSquares

    public SquareBehavior[] bestSquares; // The best squares since the first generation
    public int genNum; // The current number of generations for this simulation
    public float lastGenTime;
    public static float sLastGenTime; // The start time of the current generation

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
        genNum = 0;
        sCrossOverRate = crossOverRate;
        sMutationRate = mutationRate;
        squares = new SquareBehavior[populationEachGen];
        lastSquares = new SquareBehavior[populationEachGen];
        bestSquares = new SquareBehavior[Mathf.RoundToInt(populationEachGen * 0.2f)];
        sMinWeightValue = minWeightValue;
        sMaxWeightValue = maxWeightValue;
        mainCamera = FindObjectOfType<Camera>().gameObject;
        cameraLocalPosi = mainCamera.transform.localPosition;

        NewGen();
    }

    // Update is called once per frame
    void Update()
    {
        //Time.timeScale = simSpeed;
        //print(Mathf.RoundToInt(1.0f / Time.deltaTime) + ", " + Application.targetFrameRate + ", " + QualitySettings.vSyncCount);
        if (Time.time - sLastGenTime >= cycleDuration)
        {
            NewGen();
        }

        foreach (SquareBehavior square in squares) // Find out which square is the leading square
        {
            if (square.travelDist > leadSquare.travelDist)
            {
                leadSquare = square;
                mainCamera.transform.parent = leadSquare.transform;
                mainCamera.transform.localPosition = cameraLocalPosi;
            }
        }
    }

    public void NewGen() // The method to start a new generation
    {
        sLastGenTime = Time.time;
        lastGenTime = sLastGenTime;
        mainCamera.transform.localPosition = cameraLocalPosi;
        mainCamera.transform.parent = null;
        platformCount = 1;
        genNum++;
        Random.InitState(0);

        for (int i = bestSquares.Length; i < lastSquares.Length; i++) // Wipe out the previous stored squares
        {
            if (lastSquares[i] != null)
            {
                Destroy(lastSquares[i].gameObject);
            }
        }

        lastSquares = FindObjectsOfType<SquareBehavior>(); // Fill in the array storing the last generation with the current finished generation
        for (int i = 0; i < lastSquares.Length; i++)
        {
            CalculateFitnessScore(lastSquares[i]);

            for (int j = 0; j < bestSquares.Length; j++) // See if the square is among the best square of all time
            {
                if (bestSquares[j] == null)
                {
                    bestSquares[j] = Instantiate(lastSquares[i].gameObject).GetComponent<SquareBehavior>();
                    bestSquares[j].gameObject.SetActive(false);
                    break;
                }
                else if (lastSquares[i].fitnessScore > bestSquares[j].fitnessScore)
                {
                    if (bestSquares[j].fitnessScore == lastSquares[i].fitnessScore) // If there is already an instance of this square in the best squares
                    {
                        break;
                    }

                    Destroy(bestSquares[j].gameObject);
                    bestSquares[j] = Instantiate(lastSquares[i].gameObject).GetComponent<SquareBehavior>();
                    bestSquares[j].basicLayer = new float[4 + 1, 11];
                    bestSquares[j].basicLayer = lastSquares[i].basicLayer;
                    bestSquares[j].gameObject.SetActive(false);
                    break;
                }
            }

            lastSquares[i].gameObject.SetActive(false);
        }

        // Add the best squares and the last squares together
        tempArray = new SquareBehavior[lastSquares.Length + bestSquares.Length];
        bestSquares.CopyTo(tempArray, 0);
        lastSquares.CopyTo(tempArray, bestSquares.Length);
        lastSquares = new SquareBehavior[lastSquares.Length + bestSquares.Length];
        tempArray.CopyTo(lastSquares, 0);

        Instantiate(platform, Vector3.zero, Quaternion.identity); // Instantiate the new first platform

        for (int i = 0; i < populationEachGen; i++) // Instantiate new squares
        {
            squares[i] = Instantiate(square, Vector3.up * 10, Quaternion.identity).GetComponent<SquareBehavior>();
        }
        leadSquare = squares[0];
        mainCamera.transform.parent = leadSquare.transform;
        mainCamera.transform.localPosition = cameraLocalPosi;
    }

    public void CalculateFitnessScore(SquareBehavior square)
    {
        square.fitnessScore = square.travelDist * travelDistanceScore +
                              square.travelPlat * travelPlatformScore +
                              square.timeStayedOnPlatform * squareOnPlatformScore;
    }
}
