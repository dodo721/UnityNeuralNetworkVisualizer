using UnityEngine;
using System;

public static class MiscUtils {

    public static Gradient CreateGradient (Color color1, Color color2) {
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKey;
        GradientAlphaKey[] alphaKey;
        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        colorKey = new GradientColorKey[2];
        colorKey[0].color = color1;
        colorKey[0].time = 0.0f;
        colorKey[1].color = color2;
        colorKey[1].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = color1.a;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = color2.a;
        alphaKey[1].time = 1.0f;

        gradient.SetKeys(colorKey, alphaKey);
        return gradient;
    }

    public static Gradient CreateGradient (Color color1, Color color2, Color color3) {
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKey;
        GradientAlphaKey[] alphaKey;
        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        colorKey = new GradientColorKey[3];
        colorKey[0].color = color1;
        colorKey[0].time = 0.0f;
        colorKey[1].color = color2;
        colorKey[1].time = 0.5f;
        colorKey[2].color = color3;
        colorKey[2].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        alphaKey = new GradientAlphaKey[3];
        alphaKey[0].alpha = color1.a;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = color2.a;
        alphaKey[1].time = 0.5f;
        alphaKey[2].alpha = color3.a;
        alphaKey[2].time = 1.0f;

        gradient.SetKeys(colorKey, alphaKey);
        return gradient;
    }

}