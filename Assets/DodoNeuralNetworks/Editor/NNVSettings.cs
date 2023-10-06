using UnityEngine;
using System;
using UnityEditor;

[FilePath("Neural Networks/NNVSettings.settings", FilePathAttribute.Location.PreferencesFolder)]
public class NNVSettings : ScriptableSingleton<NNVSettings> {

    // Spacing and sizing
    public float layerSpacing = 50;
    public float nodeSpacing = 15;
    public float nodeSize = 6;

    // Coloring
    public Color nodeColorHigh = Color.red;
    public Color nodeColorLow = Color.black;
    public Color connectionColorHigh = Color.white;
    public Color connectionColorLow = Color.black;
    
    // Color modes
    public enum NodeColorMode { VALUE=0, BIAS=1 };
    public enum ConnectionColorMode { VALUE=0, WEIGHT=1 };
    public NodeColorMode nodeColorMode = NodeColorMode.VALUE;
    public ConnectionColorMode connectionColorMode = ConnectionColorMode.WEIGHT;

    public void SetLayerSpacing (float layerSpacing) {
        if (this.layerSpacing == layerSpacing) return;
        this.layerSpacing = layerSpacing;
        Save(true);
    }

    public void SetNodeSpacing (float nodeSpacing) {
        if (this.nodeSpacing == nodeSpacing) return;
        this.nodeSpacing = nodeSpacing;
        Save(true);
    }

    public void SetNodeSize (float nodeSize) {
        if (this.nodeSize == nodeSize) return;
        this.nodeSize = nodeSize;
        Save(true);
    }

    public void SetNodeColorHigh (Color nodeColorHigh) {
        if (this.nodeColorHigh == nodeColorHigh) return;
        this.nodeColorHigh = nodeColorHigh;
        Save(true);
    }

    public void SetNodeColorLow (Color nodeColorLow) {
        if (this.nodeColorLow == nodeColorLow) return;
        this.nodeColorLow = nodeColorLow;
        Save(true);
    }

    public void SetConnectionColorHigh (Color connectionColorHigh) {
        if (this.connectionColorHigh == connectionColorHigh) return;
        this.connectionColorHigh = connectionColorHigh;
        Save(true);
    }

    public void SetConnectionColorLow (Color connectionColorLow) {
        if (this.connectionColorLow == connectionColorLow) return;
        this.connectionColorLow = connectionColorLow;
        Save(true);
    }

    public void SetNodeColorMode (NodeColorMode nodeColorMode) {
        if (this.nodeColorMode == nodeColorMode) return;
        this.nodeColorMode = nodeColorMode;
        Save(true);
    }

    public void SetConnectionColorMode (ConnectionColorMode connectionColorMode) {
        if (this.connectionColorMode == connectionColorMode) return;
        this.connectionColorMode = connectionColorMode;
        Save(true);
    }

}