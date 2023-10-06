using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NeuralNetworkBehaviour : MonoBehaviour
{

    public NeuralNetwork network;
    public float[] networkInputs;
    public float[] networkOutputs;

    [HideInInspector]
    public float[] nodeValues;

    public delegate void NetworkUpdate (float[] outputs);
    public event NetworkUpdate OnNetworkUpdate;

    void Awake () {
        //InitializeNetwork(networkSize);
        if (Application.isPlaying && network == null) {
            Debug.LogWarning("No network assigned to behave with!", this);
        }
        LoadNetwork();
    }

    public void UpdateNetwork (float[] inputs) {
        networkInputs = inputs;
        SetInputs(inputs);
        UpdateNetwork();
        networkOutputs = GetOutputs();
        OnNetworkUpdate?.Invoke(networkOutputs);
    }

    public void LoadNetwork () {
        networkInputs = new float[network.InputCount()];
        networkOutputs = new float[network.OutputCount()];
        nodeValues = new float[network.nodeCount];
    }

    void OnValidate () {
        if (network != null) {
            LoadNetwork();
        } else {
            networkInputs = new float[0];
            networkOutputs = new float[0];
            nodeValues = new float[0];
        }
    }

    public void UpdateNetwork () {

        // OPTIMISED
        int nodeIdx = 0;
        int connectionIdx = 0;
        for (int layer = 0; layer < network.layerCount; layer++) {
            for (int node = 0; node < network.size[layer]; node++) {
                // Get offset node position in linear array
                float totalInput = 0f;
                // Go through connections (looking back)
                if (layer > 0) {
                    int prevLayerStartNodeIdx = nodeIdx - network.size[layer - 1];
                    for (int connection = 0; connection < network.size[layer - 1]; connection++) {
                        int prevNodeIdx = prevLayerStartNodeIdx + connection;
                        float val = nodeValues[prevNodeIdx];
                        float weight = network.connectionWeights[connectionIdx];
                        totalInput += val * weight;
                        //Debug.Log(nodeIdx + ", " + connectionIdx);
                        connectionIdx++;
                    }
                    totalInput += network.nodeBiases[nodeIdx];
                    nodeValues[nodeIdx] = network.ThresholdFunction(totalInput);
                }
                nodeIdx++;
            }
        }
    }

    public void SetInputs (float[] inputValues) {

        // OPTIMISED
        if (inputValues.Length != network.size[0]) throw new Exception("Input array size must match the size of the network's first layer!");
        for (int i = 0; i < inputValues.Length; i++) {
            nodeValues[i] = inputValues[i];
        }
    }

    public float[] GetOutputs () {
        /*NeuralLayer outputs = layers[layers.Length - 1];
        float[] outputValues = new float[outputs.nodes.Length];
        for (int i = 0; i < outputValues.Length; i++) {
            outputValues[i] = outputs.nodes[i].value;
        }*/
        float[] outputValues = new float[network.size[network.layerCount - 1]];
        for (int i = 0; i < network.size[network.layerCount - 1]; i ++) {
            outputValues[i] = nodeValues[nodeValues.Length - i - 1];
        }
        outputValues.Reverse();
        return outputValues;
    }
}
