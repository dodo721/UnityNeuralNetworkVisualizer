using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(NeuralNetworkBehaviour))]
public class NeuralNetworkEditor : Editor {

    //SerializedProperty networkSize;

    void OnEnable()
    {
        /*actionRegisterSerialized = serializedObject.FindProperty("actionRegisterSerialized");
        centerText = new GUIStyle();
        centerText.alignment = TextAnchor.MiddleCenter;*/
        //networkSize = serializedObject.FindProperty("networkSize");
    }

    public override void OnInspectorGUI()   
    {
        NeuralNetworkBehaviour networkBehaviour = (NeuralNetworkBehaviour) target;

        DrawDefaultInspector();
        EditorGUILayout.Space();
        //EditorGUILayout.PropertyField(networkSize, new GUIContent("Network Size"));

        GUILayout.Label(networkBehaviour.network == null ? "Not initiated" : "Network loaded");

        EditorGUILayout.Space();

        if (GUILayout.Button("Update Network")) {
            networkBehaviour.UpdateNetwork(networkBehaviour.networkInputs);
        }

        if (GUILayout.Button("Reinit network")) {
            networkBehaviour.network.Init();
            networkBehaviour.LoadNetwork();
        }

        serializedObject.ApplyModifiedProperties();
    }

}