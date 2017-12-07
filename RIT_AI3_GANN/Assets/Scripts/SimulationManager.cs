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
    public SquareBehavior[] lastSquares; // The array that stores the last generation of squares
    public static int platformCount; // Count for how many platforms has been generated
    public static SquareBehavior leadSquare; // The square that is currently taking the lead in this simulation
    public static GameObject mainCamera; // The camera
    public static Vector3 cameraLocalPosi; // The camera local position
    public NeuralNetworkVisualizer neuralNetworkVisual; // The visualization of the neural network of the leading square
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
        neuralNetworkVisual = FindObjectOfType<NeuralNetworkVisualizer>();

        NewGen();
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = simSpeed;
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
                neuralNetworkVisual.currentLead = leadSquare;
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

        for (int i = 0; i < lastSquares.Length; i++) // Wipe out the previous stored squares
        {
            if (lastSquares[i] != null)
            {
                Destroy(lastSquares[i].gameObject);
            }
        }

        if (genNum == 1) // If it's the first gen
        {
            lastSquares = new SquareBehavior[populationEachGen];
        }
        else
        {
            lastSquares = new SquareBehavior[populationEachGen];
            squares.CopyTo(lastSquares, 0);
            SelectBestSquares();

            for (int i = 0; i < bestSquares.Length; i++) // Replace some last generation squares by the best squares
            {
                Destroy(lastSquares[i].gameObject);

                lastSquares[i] = Instantiate(bestSquares[i].gameObject).GetComponent<SquareBehavior>();

                lastSquares[i].basicLayer = new float[4 + 1, 11];
                CopyNeuralLayer(bestSquares[i].basicLayer, lastSquares[i].basicLayer);

                lastSquares[i].gameObject.SetActive(false);
            }
        }

        Instantiate(platform, Vector3.zero, Quaternion.identity); // Instantiate the new first platform

        for (int i = 0; i < populationEachGen; i++) // Instantiate new squares
        {
            squares[i] = Instantiate(square, Vector3.up * 10, Quaternion.identity).GetComponent<SquareBehavior>();
            squares[i].id = i;
        }

        leadSquare = squares[0];
        mainCamera.transform.parent = leadSquare.transform;
        mainCamera.transform.localPosition = cameraLocalPosi;
        neuralNetworkVisual.currentLead = leadSquare;
    }

    public void SelectBestSquares() // See if any square in the current squares is better than any in the best squares
    {
        for (int i = 0; i < squares.Length; i++)
        {
            CalculateFitnessScore(squares[i]);

            if (genNum == 2) // If it's the second gen
            {
                for (int j = 0; j < bestSquares.Length; j++) // Fill up the bestSquares for the first time
                {
                    if (bestSquares[j] == null)
                    {
                        bestSquares[j] = Instantiate(squares[i].gameObject).GetComponent<SquareBehavior>();

                        bestSquares[j].basicLayer = new float[4 + 1, 11];
                        CopyNeuralLayer(squares[i].basicLayer, bestSquares[j].basicLayer);

                        bestSquares[j].gameObject.SetActive(false);
                        squares[i].gameObject.SetActive(false);
                        break;
                    }
                }

                continue;
            }

            int worstBest = 0; // The index for the lowest score among the best squares
            for (int j = 0; j < bestSquares.Length; j++) // Find out the current worst score in best squares
            {
                if (bestSquares[j].fitnessScore <= bestSquares[worstBest].fitnessScore)
                {
                    worstBest = j;
                }
            }

            if (squares[i].fitnessScore > bestSquares[worstBest].fitnessScore)
            {
                bool similar = false;
                for (int j = 0; j < bestSquares.Length; j++) // See if there is already a square with similar score in it
                {
                    if (Mathf.RoundToInt(bestSquares[j].fitnessScore) == Mathf.RoundToInt(squares[i].fitnessScore))
                    {
                        similar = true;
                    }
                }
                if (similar)
                {
                    continue;
                }

                Destroy(bestSquares[worstBest].gameObject);

                bestSquares[worstBest] = Instantiate(squares[i].gameObject).GetComponent<SquareBehavior>();

                bestSquares[worstBest].basicLayer = new float[4 + 1, 11];
                CopyNeuralLayer(squares[i].basicLayer, bestSquares[worstBest].basicLayer);

                bestSquares[worstBest].layerSample = new float[bestSquares[worstBest].basicLayer.GetLength(0)]; // Show some sample neural links
                for (int x = 0; x < bestSquares[worstBest].layerSample.Length; x++)
                {
                    bestSquares[worstBest].layerSample[x] = bestSquares[worstBest].basicLayer[x, 0];
                }

                bestSquares[worstBest].gameObject.SetActive(false);
            }
            
            squares[i].gameObject.SetActive(false);
        }
    }

    public void CopyNeuralLayer(float[,] copyFrom, float[,] copyTo) // Copy neural layer data
    {
        for (int x = 0; x < copyTo.GetLength(0); x++)
        {
            for (int y = 0; y < copyTo.GetLength(1); y++)
            {
                copyTo[x, y] = copyFrom[x, y];
            }
        }
    }

    public void CalculateFitnessScore(SquareBehavior square)
    {
        square.fitnessScore = square.travelDist * travelDistanceScore +
                              square.travelPlat * travelPlatformScore +
                              square.timeStayedOnPlatform * squareOnPlatformScore;
        //square.fitnessScore = Mathf.Pow(square.travelDist, 2) * travelDistanceScore +
        //                      square.travelPlat * travelPlatformScore +
        //                      square.timeStayedOnPlatform * squareOnPlatformScore;
    }
}
