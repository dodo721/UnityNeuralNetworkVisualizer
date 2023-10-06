using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.Linq;

public static class NeuralGUIUtils
{

    private static Texture2D arrowCapTex = null;
    private static Texture2D arrowCapTex_t = null;
    private static Texture2D whiteLineTex_t = null;
    private static Texture2D _staticRectTexture;
    private static GUIStyle _nothingOpenStyle = null;
    private static GUIStyle _staticRectStyle;

    /// <summary>
    ///  Initial set up method.
    /// </summary>
    /// <remarks>
    ///  Must be called before any useage.
    /// </remarks>
    public static void Init()
    {
        arrowCapTex = EditorGUIUtility.Load("Assets/NeuralNetwork/Editor/Editor Resources/arrow-icon-14-16.png") as Texture2D;
        arrowCapTex_t = EditorGUIUtility.Load("Assets/NeuralNetwork/Editor/Editor Resources/arrow-icon-14-16_t.png") as Texture2D;
        whiteLineTex_t = EditorGUIUtility.Load("Assets/NeuralNetwork/Editor/Editor Resources/white_line_t.png") as Texture2D;

        _nothingOpenStyle = new GUIStyle();
        _nothingOpenStyle.border = new RectOffset(12, 12, 12, 12);
        _nothingOpenStyle.padding = new RectOffset(12, 12, 12, 12);
        _nothingOpenStyle.alignment = TextAnchor.MiddleCenter;
        _nothingOpenStyle.fontSize = 20;

        _staticRectTexture = new Texture2D( 1, 1 );
        _staticRectStyle = new GUIStyle();
    }

    public static Vector2 OffsetZoomPosition (Vector2 position, NeuralNetworkVisualizer editor) {
        return (position / editor._zoom) + editor._offset;
    }

    public static Rect OffsetZoomRect (Rect rect, NeuralNetworkVisualizer editor) {
        return new Rect(OffsetZoomPosition(rect.position, editor), rect.size / editor._zoom);
    }

    /// <summary>
    ///  Draws a line.
    /// </summary>
    /// <param name="start">The start position.</param>
    /// <param name="end">The end position.</param>
    public static void DrawRawLine(Vector2 start, Vector2 end, Color color)
    {
        Handles.color = color;
        Handles.DrawLine(new Vector3(start.x, start.y, 0), new Vector3(end.x, end.y, 0));
    }

    /// <summary>
    ///  Draws a line, transformed.
    /// </summary>
    /// <param name="start">The start position.</param>
    /// <param name="end">The end position.</param>
    public static void DrawLine(Vector2 start, Vector2 end, Color color, NeuralNetworkVisualizer editor)
    {
        Handles.color = color;
        Vector2 start_t = OffsetZoomPosition(start, editor);
        Vector2 end_t = OffsetZoomPosition(end, editor);
        Handles.DrawLine(new Vector3(start_t.x, start_t.y, 0), new Vector3(end_t.x, end_t.y, 0));
    }
    
    public static void DrawRect( Rect position, Color color, NeuralNetworkVisualizer editor, GUIContent content=null )
    {
        _staticRectTexture.SetPixel( 0, 0, color );
        _staticRectTexture.Apply();
 
        _staticRectStyle.normal.background = _staticRectTexture;

        //Rect offsetZoomed = OffsetZoomRect(position, editor);

        if (content == null) content = GUIContent.none;
 
        GUI.Box( position, content, _staticRectStyle );
    }

    public static void DrawArrow(Rect start, Vector2 end)
    {
        Vector3 startPos = new Vector3(start.x + start.width / 2, start.y + start.height / 2, 0);
        Vector3 endPos = new Vector3(end.x, end.y, 0);
        Handles.color = Color.white;
        Handles.DrawAAPolyLine(5f, startPos, endPos);
        float xDis = endPos.x - startPos.x;
        float yDis = endPos.y - startPos.y;
        float arrowAngle = Mathf.Rad2Deg * Mathf.Atan2(yDis, xDis);
        GUIUtility.RotateAroundPivot(arrowAngle, end);
        GUI.DrawTexture(new Rect(endPos.x - 6, endPos.y - 10 / 2, 10, 10), arrowCapTex, ScaleMode.StretchToFill);
        GUIUtility.RotateAroundPivot(-arrowAngle, end);
    }

    public static void DrawNoSelectionScreen (NeuralNetworkVisualizer window)
    {
        GUILayout.BeginArea(new Rect(0, 0, window.position.width, window.position.height));
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("No neural network selected", _nothingOpenStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
    }

    /// <summary>
    ///  Draws a grid to the node editor.
    /// </summary>
    /// <param name="gridSpacingUnzoomed">The spacing between grid lines.</param>
    /// <param name="gridOpacity">The opacity to draw lines with.</param>
    /// <param name="gridColor">The color to draw lines with.</param>
    /// <param name="editor">The <c>NodeMachineEditor</c> to draw to.</param>
    public static void DrawGrid(float gridSpacingUnzoomed, float gridOpacity, Color gridColor, NeuralNetworkVisualizer editor)
    {
        float gridSpacing = gridSpacingUnzoomed / editor._zoom;

        int widthDivs = Mathf.CeilToInt(editor._editor.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(editor._editor.height / gridSpacing);

        Handles.BeginGUI();
        Color color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        Vector2 newOffset = new Vector3(editor._offset.x % gridSpacing, editor._offset.y % gridSpacing);

        for (int i = 0; i < widthDivs; i++)
        {
            DrawRawLine(new Vector2(gridSpacing * i, -gridSpacing) + newOffset, new Vector2(gridSpacing * i, editor.position.height) + newOffset, color);
        }

        for (int j = 0; j < heightDivs; j++)
        {
            DrawRawLine(new Vector2(-gridSpacing, gridSpacing * j) + newOffset, new Vector2(editor.position.width, gridSpacing * j) + newOffset, color);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

}