using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetworkVisualizer : MonoBehaviour
{
    public GameObject neuralConnection;
    public LineRenderer[,] neuralConnections;
    public MeshRenderer[] inputNodes;
    public MeshRenderer[] outputNodes;

    public SimulationManager simManager;
    public SquareBehavior currentLead; // The current leading square
    public Color inputNodeColor;
    public Color outputForceColor;
    public Color outputJumpColor;
    public Color outputWalkColor;

    // Use this for initialization
    void Start()
    {
        simManager = FindObjectOfType<SimulationManager>();
        inputNodeColor = inputNodes[0].material.color;
        outputForceColor = outputNodes[0].material.color;
        outputJumpColor = outputNodes[9].material.color;
        outputWalkColor = outputNodes[10].material.color;

        InitiateVisual();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentLead != null)
        {
            UpdateNeuralNetwork();
            MoveLinks();
        }
    }

    public void UpdateNeuralNetwork()
    {
        for (int x = 0; x < neuralConnections.GetLength(0); x++)
        {
            inputNodeColor.a = currentLead.inputs[x];
            inputNodes[x].material.color = inputNodeColor;

            for (int y = 0; y < neuralConnections.GetLength(1); y++)
            {
                if (x == 0) // Only set output nodes alpha once
                {
                    if (y < currentLead.jumpForceOutputs.Length) // Set alpha for jump force nodes
                    {
                        outputForceColor.a = currentLead.jumpForceOutputs[y];
                        outputNodes[y].material.color = outputForceColor;
                    }
                    else if (y == currentLead.jumpForceOutputs.Length) // Set alpha for trigger jump
                    {
                        if (currentLead.output10 >= 0.5f) // If this node is triggered
                        {
                            outputJumpColor.a = 1;
                            outputNodes[y].material.color = outputJumpColor;
                        }
                        else
                        {
                            outputJumpColor.a = 0.5f;
                            outputNodes[y].material.color = outputJumpColor;
                        }
                    }
                    else // Set alpha for trigger walk
                    {
                        if (currentLead.output11 >= 0.5f) // If this node is triggered
                        {
                            outputWalkColor.a = 1;
                            outputNodes[y].material.color = outputWalkColor;
                        }
                        else
                        {
                            outputWalkColor.a = 0.5f;
                            outputNodes[y].material.color = outputWalkColor;
                        }
                    }

                    //neuralConnections[x, y].startWidth = Mathf.Clamp(Mathf.Abs(currentLead.basicLayer[x, y]) / simManager.maxWeightValue, 0.1f, 1); // Set line width
                    //neuralConnections[x, y].endWidth = Mathf.Clamp(Mathf.Abs(currentLead.basicLayer[x, y]) / simManager.maxWeightValue, 0.1f, 1);
                    neuralConnections[x, y].startWidth = Mathf.Abs(currentLead.basicLayer[x, y]) / simManager.maxWeightValue; // Set line width
                    neuralConnections[x, y].endWidth = Mathf.Abs(currentLead.basicLayer[x, y]) / simManager.maxWeightValue;
                    if (currentLead.basicLayer[x, y] >= 0) // Set line color, positive weight give green color, negative weight give red color
                    {
                        neuralConnections[x, y].material.SetColor("_EmissionColor", Color.green);
                    }
                    else
                    {
                        neuralConnections[x, y].material.SetColor("_EmissionColor", Color.red);
                    }
                }
            }
        }
    }

    public void MoveLinks()
    {
        for (int x = 0; x < neuralConnections.GetLength(0); x++)
        {
            for (int y = 0; y < neuralConnections.GetLength(1); y++)
            {
                neuralConnections[x, y].SetPosition(0, inputNodes[x].transform.position);
                neuralConnections[x, y].SetPosition(1, outputNodes[y].transform.position);
            }
        }
    }

    public void InitiateVisual()
    {
        neuralConnections = new LineRenderer[inputNodes.Length, outputNodes.Length];
        for (int x = 0; x < neuralConnections.GetLength(0); x++)
        {
            for (int y = 0; y < neuralConnections.GetLength(1); y++)
            {
                neuralConnections[x, y] = Instantiate(neuralConnection).GetComponent<LineRenderer>();
            }
        }
    }
}
