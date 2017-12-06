using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SquareBehavior : MonoBehaviour
{
    /// <summary>
    /// This contains the neural network and the behavior controlled by the neural network 
    /// as well as some extra info about this particular square
    /// </summary>

    public float moveForce; // The force applied to the square each time it decided to move forward
    public float[] jumpForces;
    public float baseJumpForce; // The smallest jump force

    /// <summary>
    /// Values to calculate fitness score
    /// </summary>
    public float travelDist; // How far the square travelled
    public float travelPlat; // How many platform the square travelled 
                             // (the platform will only be counted if the square actually lands on it,
                             // if the square jumped over a platform and landed on the platform ahead,
                             // then this number will only increased by 1, which exclude the platform it jumped over)
    public float timeStayedOnPlatform; // The total time that the square is touching the platforms
                                       // We don't want the square to jump when it doesn't need to

    public float mass; // The weight of the square, used to multiplied with the force when push forward
    public float gravity; // The scale of the gravity, used to multiplied with the force when jump
    public int id; // ID to help differentiate the squares
    public SimulationManager simManager; // The simulation manager, used to extract genetic informations

    /// <summary>
    /// Some variebles to be monitored
    /// </summary>
    public MakeNextPlatform currentPlatform; // The platform the square currently stepped on
    public float currentPlatformSlope; // The slope of the current platform
    public float distanceToEdge; // The distance between the square and the right end of the current platform 
                                 // (If the square is currently not touching the platform, then it will be 0)
                                 //public MakeNextPlatform nextPlatform; // The next platform to the one the square currently stepped on
    public float nextPlatformLength; // The length of the next platform
    public float nextPlatformAltitudeDiff; // The difference of the y coord between the current platform and the next platform
    public float nextPlatformslope; // The slope of the next platform
    public float currendSpeed; // The current speed of the square
    public bool canJump; // If the square is currently touching a platform and can jump

    /// <summary>
    /// Data structure for the neural network
    /// </summary>
    public float[,] basicLayer; // The default layer structure, if this is good enough then this layer won't change and there won't be more layers
                                // The x coord will be the index of the input nodes and the y will for output nodes
    public float[] inputs;      // inputs[0]: The distance between the square and the right end of the current platform normalized to a range between 0-1
                                // inputs[1]: The length of the next platform normalized to a range between 0-1
                                // inputs[2]: The difference of the y coord between the current platform and the next platform normalized to a range between 0-1
                                // inputs[3]: The current speed of the square normalized to a range between 0-1
    public float[] jumpForceOutputs; // public float output1; // How likely it should jump with the force of jumpForce1
                                     // public float output2; // How likely it should jump with the force of jumpForce2
                                     // public float output3; // How likely it should jump with the force of jumpForce3
                                     // public float output4; // How likely it should jump with the force of jumpForce4
                                     // public float output5; // How likely it should jump with the force of jumpForce5
                                     // public float output6; // How likely it should jump with the force of jumpForce6
                                     // public float output7; // How likely it should jump with the force of jumpForce7
                                     // public float output8; // How likely it should jump with the force of jumpForce8
                                     // public float output9; // How likely it should jump with the force of jumpForce9
    public float output10; // Node to trigger jump (with the jumpforce that has the highest output value
    public float output11; // Node to trigger forward impulse
    public int jumpForceNeuronSelected; // The selected jump force neuron

    /// <summary>
    /// Data structure for the genetic algorithm
    /// </summary>
    public float fitnessScore; // The fitness score for this square. It will be calculated after the current gen is ended

    // Use this for initialization
    void Start()
    {
        basicLayer = new float[4 + 1, 11];
        mass = GetComponent<Rigidbody2D>().mass;
        gravity = GetComponent<Rigidbody2D>().gravityScale;
        canJump = false;
        travelDist = 0;
        travelPlat = 0;
        timeStayedOnPlatform = 0;
        fitnessScore = 0;
        jumpForceOutputs = new float[9];
        simManager = FindObjectOfType<SimulationManager>();

        jumpForces = new float[9];
        for (int i = 0; i < jumpForces.Length; i++)
        {
            jumpForces[i] = baseJumpForce * (i + 1);
        }

        inputs = new float[4];

        GeneticCrossOver();
        GeneticMutation();
    }

    // Update is called once per frame
    void Update()
    {
        currendSpeed = GetComponent<Rigidbody2D>().velocity.x;

        if (currentPlatform != null) // Calculate how far is the square away from the right end of the current platform it stepped on
        {
            distanceToEdge = currentPlatform.transform.position.x + currentPlatform.transform.localScale.x * 0.5f - transform.position.x;
        }

        NeuralBehavior(); // Run the values through its neural network

    }

    public void NeuralBehavior()
    {
        if (currentPlatform != null)
        {
            inputs[0] = Mathf.Clamp01(distanceToEdge /
                                      currentPlatform.maxLength); // Normalize input1
            inputs[1] = Mathf.Clamp01((currentPlatform.nextPlatform.transform.localScale.x - currentPlatform.minLength) /
                                   (currentPlatform.maxLength - currentPlatform.minLength));
            inputs[2] = Mathf.Clamp01((currentPlatform.nextPlatform.transform.position.y - currentPlatform.transform.position.y + currentPlatform.heightRange) /
                                   (currentPlatform.heightRange * 2f));
            inputs[3] = Mathf.Clamp01(GetComponent<Rigidbody2D>().velocity.x / 10f);
        }

        output10 = 0;
        output11 = 0;

        // Calculate the neuron for trigger jump
        for (int x = 0; x < inputs.Length; x++)
        {
            output10 += inputs[x] * basicLayer[x, 9];
        }
        output10 += basicLayer[basicLayer.GetLength(0) - 1, 9]; // Adding bias
        output10 = (1f / (1f + Mathf.Pow((float)Math.E, output10))); // Plug into the Sigmoid function

        // Calculate the neuron for trigger forward impulse
        for (int x = 0; x < inputs.Length; x++)
        {
            output11 += inputs[x] * basicLayer[x, 10];
        }
        output11 += basicLayer[basicLayer.GetLength(0) - 1, 10]; // Adding bias
        output11 = (1f / (1f + Mathf.Pow((float)Math.E, output11))); // Plug into the Sigmoid function
        jumpForceNeuronSelected = 0;

        if (output10 >= 0.5f)
        {
            for (int y = 0; y < jumpForceOutputs.Length; y++)
            {
                jumpForceOutputs[y] = 0;

                for (int x = 0; x < inputs.Length; x++)
                {
                    jumpForceOutputs[y] += inputs[x] * basicLayer[x, y];
                }
                jumpForceOutputs[y] += basicLayer[basicLayer.GetLength(0) - 1, y]; // Adding bias
                jumpForceOutputs[y] = (1f / (1f + Mathf.Pow((float)Math.E, jumpForceOutputs[y]))); // Plug into the Sigmoid function

                //if (SimulationManager.squares[0].gameObject == gameObject)
                //{
                //    print(jumpForceOutputs[y]);
                //}

                if (jumpForceOutputs[y] >= jumpForceOutputs[jumpForceNeuronSelected])
                {
                    jumpForceNeuronSelected = y;
                }
            }

            if (jumpForceOutputs[jumpForceNeuronSelected] < 0.5f)
            {
                jumpForceNeuronSelected = jumpForceOutputs.Length;
            }
        }

        if (output10 >= 0.5f) // If the jump trigger neuron is triggered
        {
            if (canJump)
            {
                if (jumpForceNeuronSelected < jumpForceOutputs.Length) // If there is a force neuron triggered
                {
                    GetComponent<Rigidbody2D>().AddForce(transform.up * jumpForces[jumpForceNeuronSelected], ForceMode2D.Impulse);
                    //GetComponent<Rigidbody2D>().AddForce(transform.up * 9, ForceMode2D.Impulse);
                }
            }
        }

        if (output11 >= 0.5f) // If the move forward neuron is triggered
        {
            GetComponent<Rigidbody2D>().AddForce(transform.right * moveForce * mass, ForceMode2D.Impulse);
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        //GetComponent<Rigidbody2D>().AddForce(transform.up * jumpForce9 * mass * gravity, ForceMode2D.Impulse);
        //print("Colliding: " + coll.transform.name);
        canJump = true;

        if (currentPlatform != null && coll.gameObject != currentPlatform.gameObject) // If the square landed on a new platform
        {
            travelPlat++;
        }

        currentPlatform = coll.gameObject.GetComponent<MakeNextPlatform>();
    }

    void OnCollisionStay2D(Collision2D coll)
    {
        canJump = true;
        timeStayedOnPlatform += Time.fixedDeltaTime;
        travelDist = transform.position.x; // Distance only count if the square is touching the platform
        currentPlatform = coll.gameObject.GetComponent<MakeNextPlatform>();
    }

    void OnCollisionExit2D(Collision2D coll)
    {
        canJump = false;
    }

    public void GeneticMutation()
    {
        for (int i = 0; i < basicLayer.GetLength(0); i++)
        {
            for (int j = 0; j < basicLayer.GetLength(1); j++)
            {
                if (BetterRandom.betterRandom(0, 100000000) / 100000000f <= SimulationManager.sMutationRate) // Mutate
                {
                    basicLayer[i, j] = BetterRandom.betterRandom(SimulationManager.sMinWeightValue, SimulationManager.sMaxWeightValue);
                }
                else if (simManager.genNum == 1) // If this is the first generation
                {
                    basicLayer[i, j] = BetterRandom.betterRandom(SimulationManager.sMinWeightValue, SimulationManager.sMaxWeightValue);
                }

                print(id + ", (" + i + ", " + j + "): " + basicLayer[i, j]);
            }
        }
    }

    public void GeneticCrossOver()
    {
        if (simManager.genNum == 1) // If this is the first generation
        {
            return;
        }

        int[] parentIndexes = SelectParents();
        basicLayer = simManager.lastSquares[parentIndexes[0]].basicLayer; // Copy one parent's neural network layer to the new one's

        if (basicLayer == null)//parentIndexes[0] < 10)
        {
            print(parentIndexes[0] + ", " + simManager.lastSquares[parentIndexes[0]].fitnessScore);
        }

        for (int i = 0; i < basicLayer.GetLength(0); i++)
        {
            for (int j = 0; j < basicLayer.GetLength(1); j++)
            {
                if (BetterRandom.betterRandom(0, 1000000) / 1000000f <= SimulationManager.sCrossOverRate) // Cross over
                {
                    basicLayer[i, j] = simManager.lastSquares[parentIndexes[1]].basicLayer[i, j];
                }
            }
        }
    }

    public int[] SelectParents()
    {
        int[] parentIndexes = new int[2]; // Index in square array for parent A and B

        float totalFitness = 0;
        for (int i = 0; i < simManager.lastSquares.Length; i++) // Add up total fitness
        {
            totalFitness += simManager.lastSquares[i].fitnessScore;
        }

        // Select first parent
        float parent = BetterRandom.betterRandom(0, Mathf.RoundToInt(totalFitness * 10000)) / 10000f;
        float selector = 0;
        for (int i = 0; i < simManager.lastSquares.Length; i++)
        {
            selector += simManager.lastSquares[i].fitnessScore;
            if (selector >= parent)
            {
                parentIndexes[0] = i;
                break;
            }
        }

        // Select second parent
        parentIndexes[1] = parentIndexes[0];
        while (parentIndexes[1] == parentIndexes[0])
        {
            parent = BetterRandom.betterRandom(0, Mathf.RoundToInt(totalFitness * 10000)) / 10000f;
            selector = 0;

            for (int i = 0; i < simManager.lastSquares.Length; i++)
            {
                selector += simManager.lastSquares[i].fitnessScore;
                if (selector >= parent)
                {
                    parentIndexes[1] = i;
                    break;
                }
            }
        }

        return parentIndexes;
    }
}
