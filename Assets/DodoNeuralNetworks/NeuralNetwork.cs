using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(fileName = "Neural Network")]
public class NeuralNetwork : ScriptableObject
{

    public int[] size = new int[1];

    [HideInInspector]
    public int largestLayerSize;
    [HideInInspector]
    public int nodeCount;
    [HideInInspector]
    public int connectionCount;
    [HideInInspector]
    public int layerCount;
    
    public float[] nodeBiases;
    public float[] connectionWeights;

    /*public struct NodeRep {
        public float value;
        public float bias;
    }

    public struct LayerRep {
        public NodeRep[] nodes;
        public int size;
    }

    public struct NetworkRep {
        public LayerRep[] layers;
        public int size;
    }

    public NetworkRep representation;*/

    int NodeCount () {
        return size.Aggregate((sum, next) => sum+next);
    }

    public int InputCount () {
        return size[0];
    }

    public int OutputCount () {
        return size[size.Length - 1];
    }

    int ConnectionCount () {
        int finalSize = 0;
        for (int i = 0; i < size.Length - 1; i++) {
            finalSize += size[i] * size[i + 1];
        }
        return finalSize;
    }

    /*public NeuralNode GetNodeFromId (int id) {
        int total = 0;
        for (int i = 0; i < size.Length; i++) {
            if (total + size[i] > id) return layers[i].nodes[id - total];
            total += size[i];
        }
        return null;
    }*/

    public void Init () {
        largestLayerSize = LargestLayerSize();
        nodeCount = NodeCount();
        connectionCount = ConnectionCount();
        layerCount = size.Length;

        // OPTIMISED
        nodeBiases = new float[nodeCount];
        connectionWeights = new float[connectionCount];
        // Init nodes
        for (int i = 0; i < nodeCount; i++) {
            nodeBiases[i] = UnityEngine.Random.Range(-1f, 1f);
        }
        // Init connections
        // track connections processed - easier than trying to recalculate every loop
        int startConnectionIdx = 0;
        for (int i = 0; i < size.Length - 1; i++) {
            for (int j = 0; j < size[i] * size[i + 1]; j++) {
                int idx = startConnectionIdx + j;
                connectionWeights[idx] = UnityEngine.Random.Range(-1f, 1f);
            }
            startConnectionIdx += size[i] * size[i + 1];
        }

    }

    int LargestLayerSize () {
        int largestLayer = 0;
        foreach (int layerSize in size) {
            if (layerSize > largestLayer) largestLayer = layerSize;
        }
        return largestLayer;
    }

    public float ThresholdFunction (float input) {
        // Sigmoid
        return 1 / (1 + MathF.Exp(-input));
    }

#if UNITY_EDITOR
    /*void OnValidate () {
        Init();
    }*/
#endif

}
