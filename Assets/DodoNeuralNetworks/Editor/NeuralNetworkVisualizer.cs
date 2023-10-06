using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NeuralNetworkVisualizer : EditorWindow
{

    // The selected neural network object
    public NeuralNetworkBehaviour _selectedNNBehaviour;

    // Window transforms
    public Rect _editor;
    private Rect _sideMenu;
    private Rect _toolbar;

    // GUI state
    public Vector2 _offset;
    public Vector2 _uncenteredOffset = new Vector2();
    public float _zoom = 1;
    private bool _dragging = false;
    private bool _showNodeLookup = false;

    private NNVSettings _settings;

    private bool _networkNull = true;

    // Compute shaders
    private ComputeShader _renderNetworkShader;
    private ComputeShader _renderNodeLookupShader;

    // Node position calculation
    private ComputeBuffer _networkSizeBuffer;
    private RenderTexture _nodeLookupTexture;
    // For passing values to the compute shader
    struct GPUNode {
        public float value;
        public float bias;
    };
    private ComputeBuffer _nodeValuesBuffer;

    // Render
    private RenderTexture _networkRender = null;
    // From the render operation
    private ComputeBuffer _nodePositionsBuffer;

    // DEBUG
    private Texture2D _networkTexture = null;

    // For node info interrogation by mouse
    private GPUNode _mouseNodeInfo;
    private bool _mouseOverNode = false;
    private Vector2 _mousePosition;

    // Styles
    
    [MenuItem("Neural Networks/Visualizer")]
    public static void ShowWindow()
    {
        NeuralNetworkVisualizer window = (NeuralNetworkVisualizer)EditorWindow.GetWindow(typeof(NeuralNetworkVisualizer));
        window.titleContent = new GUIContent("Network Visualizer");
        window.minSize = new Vector2(1024, 512);
        window.Init();
        window.Show();
    }

    void Awake () {
        Init();
    }

    void OnEnable () {
        Init();
    }

    void ReleaseAll () {
        if (_networkSizeBuffer != null) _networkSizeBuffer.Release();
        if (_nodeLookupTexture != null) _nodeLookupTexture.Release();
        if (_networkRender != null) _networkRender.Release();
        if (_nodeValuesBuffer != null) _nodeValuesBuffer.Release();
        if (_nodePositionsBuffer != null) _nodePositionsBuffer.Release();
    }

    void OnDestoy () {
        EditorApplication.playModeStateChanged -= InitOnStateChange;
        ReleaseAll();
    }

    void OnDisable () {
        EditorApplication.playModeStateChanged -= InitOnStateChange;
        ReleaseAll();
    }

    void Init () {
        NeuralGUIUtils.Init();
        /*if (NNVSettings.instance.nodeGradient == null) {
            NNVSettings.instance.nodeGradient = MiscUtils.CreateGradient(Color.blue, Color.black, Color.red);
        }
        if (NNVSettings.instance.connectionGradient == null) {
            NNVSettings.instance.connectionGradient = MiscUtils.CreateGradient(new Color(0f,0f,0f,0.5f), new Color(1f,1f,1f,0.1f), new Color(1f,1f,1f,0.5f));
        }*/
        _settings = NNVSettings.instance;
        EditorApplication.playModeStateChanged -= InitOnStateChange;
        EditorApplication.playModeStateChanged += InitOnStateChange;
        if (_selectedNNBehaviour != null) {
            SelectNetworkBehaviour(_selectedNNBehaviour);
        }
        _renderNetworkShader = (ComputeShader)AssetDatabase.LoadAssetAtPath("Assets/DodoNeuralNetworks/Editor/RenderNetwork.compute", typeof(ComputeShader));
        _renderNodeLookupShader = (ComputeShader)AssetDatabase.LoadAssetAtPath("Assets/DodoNeuralNetworks/Editor/RenderNodeLookup.compute", typeof(ComputeShader));
    }

    void InitOnStateChange (PlayModeStateChange state) {
        Init();
    }

    void Update()
    {
        // Components test true for null if they are destroyed
        // but aren't actually null, so will still throw errors
        // if you attempt access. Better to make sure they're null.
        if (_selectedNNBehaviour == null)
        {
            _selectedNNBehaviour = null;
        }
        bool wasOpen = _selectedNNBehaviour != null;
        GameObject sel = Selection.activeTransform?.gameObject;
        if (sel != _selectedNNBehaviour?.gameObject && sel != null)
        {
            if (_selectedNNBehaviour != null) _selectedNNBehaviour.OnNetworkUpdate -= RepaintOnUpdate;
            SelectNetworkBehaviour(sel.GetComponent<NeuralNetworkBehaviour>());
            if (_selectedNNBehaviour != null) _selectedNNBehaviour.OnNetworkUpdate += RepaintOnUpdate;
        }
        bool isOpen = _selectedNNBehaviour != null;
        if (wasOpen != isOpen) Repaint();
        /*foreach (Rect node in _nodePositions) {
            Rect screenNode = NeuralGUIUtils.OffsetZoomRect(node, this);
            if (Event.current != null) {
                if (screenNode.Contains(Event.current.mousePosition)) Repaint();
            }
        }*/
    }

    void SelectNetworkBehaviour (NeuralNetworkBehaviour nnBehaviour) {
        _selectedNNBehaviour = nnBehaviour;
        if (nnBehaviour == null) {
            ReleaseAll();
        } else {
            NeuralNetwork network = _selectedNNBehaviour.network;
            ReleaseAll();
            //Debug.Log("NEW BUFFER SIZE FOR POSITIONS: " + _selectedNNBehaviour.network.nodeCount + ", TOTAL SIZE " + (_selectedNNBehaviour.network.nodeCount * sizeof(float) * 4));
        }
    }

    void RepaintOnUpdate (float[] outputs) {
        Repaint();
    }

    void OnGUI()
    {
        if (_selectedNNBehaviour == null) {
            NeuralGUIUtils.DrawNoSelectionScreen(this);
            _networkNull = true;
            return;
        }

        //RenderNetwork();

        Vector2 screenCenter = new Vector2(position.width / 2, position.height / 2) - (_editor.position / 2);
        _offset = _uncenteredOffset + screenCenter;

        _editor = new Rect(250, 30, position.width - 250, position.height - 30);
        _sideMenu = new Rect(10, 20, 230, position.height - 20);
        _toolbar = new Rect(260, 5, position.width - 260, 25);

        // ----- TOOLBAR --------

        GUILayout.BeginArea(_toolbar);
        GUILayout.BeginHorizontal();

        GUILayout.Label("Offset: " + _offset.ToString() + "   |   Zoom: " + _zoom, GUILayout.Width(_toolbar.width / 3f));
        GUILayout.Label("Mouse position: " + _mousePosition.ToString(), GUILayout.Width(_toolbar.width / 3f));
        //GUILayout.Label(_mouseOverNode ? "Node info: value=" + _mouseNodeInfo.value + ", bias=" + _mouseNodeInfo.bias : "No node selected", GUILayout.Width(_toolbar.width / 3f));

        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        // ----- SIDE MENU ------

        GUILayout.BeginArea(_sideMenu);
        GUILayout.BeginVertical();
        
        EditorGUILayout.LabelField(_selectedNNBehaviour.network.name + " (" + _selectedNNBehaviour.name + ")", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        EditorGUILayout.Space();

        // Use instance here so it saves correctly
        NNVSettings.instance.SetLayerSpacing(EditorGUILayout.FloatField("Layer spacing", _settings.layerSpacing));
        NNVSettings.instance.SetNodeSpacing(EditorGUILayout.FloatField("Node spacing", _settings.nodeSpacing));
        NNVSettings.instance.SetNodeSize(EditorGUILayout.FloatField("Node size", _settings.nodeSize));

        // Color settings - faux gradient
        EditorGUILayout.Space();
        GUILayout.Label("Node gradient");
        GUILayout.BeginHorizontal();
        NNVSettings.instance.SetNodeColorLow(EditorGUILayout.ColorField(_settings.nodeColorLow));
        NNVSettings.instance.SetNodeColorHigh(EditorGUILayout.ColorField(_settings.nodeColorHigh));
        GUILayout.EndHorizontal();
        GUILayout.Label("Connection gradient");
        GUILayout.BeginHorizontal();
        NNVSettings.instance.SetConnectionColorLow(EditorGUILayout.ColorField(_settings.connectionColorLow));
        NNVSettings.instance.SetConnectionColorHigh(EditorGUILayout.ColorField(_settings.connectionColorHigh));
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();

        // Color modes
        NNVSettings.instance.SetNodeColorMode((NNVSettings.NodeColorMode)EditorGUILayout.EnumPopup("Node color mode", _settings.nodeColorMode));
        NNVSettings.instance.SetConnectionColorMode((NNVSettings.ConnectionColorMode)EditorGUILayout.EnumPopup("Connection color mode", _settings.connectionColorMode));

        EditorGUILayout.Space();

        _showNodeLookup = EditorGUILayout.Toggle("Show node lookup", _showNodeLookup);

        if (GUILayout.Button("Reset view")) {
            _offset = new Vector2(0,0);
            _uncenteredOffset = _offset - screenCenter;
            _zoom = 1;
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();

        // ----- EDITOR -----

        GUILayout.BeginArea(_editor, new GUIStyle("Box"));

        NeuralGUIUtils.DrawGrid(10, 0.1f, Color.gray, this); // * zoom
        NeuralGUIUtils.DrawGrid(50, 0.2f, Color.gray, this); // * zoom

        NeuralNetwork network = _selectedNNBehaviour.network;

        if (network != null) {
            
            if (_networkNull) {
                _networkNull = false;
                GUI.changed = true;
            }

            // Legacy CPU rendering for connections - TODO gpu-ify this bitch


            // Get GPU to do position calculations
            RenderNodeLookup();
            // Render positions to an image
            RenderNetwork();

            // Draw the rendered network
            GUI.DrawTexture(new Rect(0, 0, _editor.width, _editor.height), _networkRender, ScaleMode.StretchToFill);
            // Show the node lookup if enabled
            if (_showNodeLookup) GUI.DrawTexture(new Rect(0, 0, 100, 100), _nodeLookupTexture, ScaleMode.StretchToFill);
            
        } else {
            _networkNull = true;
        }

        GUILayout.EndArea();

        // --------------------- PROCESS EVENTS ----------------------------

        ProcessEvents(Event.current);

        if (GUI.changed) Repaint();
    }

    void ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0 && _editor.Contains(e.mousePosition))
                {
                    _dragging = true;
                    GUI.changed = true;
                }
                break;
            case EventType.MouseDrag:
                if (e.button == 0 && (_editor.Contains(e.mousePosition) || _dragging))
                {
                    OnDrag(e.delta);
                }
                break;
            case EventType.MouseUp:
                _dragging = false;
                break;
            case EventType.ScrollWheel:
                OnScroll(e.delta);
                break;
            case EventType.ContextClick:
                //MouseGetNodeInfo(e.mousePosition);
                break;
        }
    }

    /*void MouseGetNodeInfo (Vector2 mousePosition) {
        _mousePosition = mousePosition - _editor.position;
        _networkTexture = new Texture2D(_networkRender.width, _networkRender.height, TextureFormat.ARGB32, false);
        RenderTexture.active = _networkRender;
        _networkTexture.ReadPixels(new Rect(0, 0, _networkRender.width, _networkRender.height), 0, 0);
        _networkTexture.Apply();
        Color nodeColor = _networkTexture.GetPixel(Mathf.RoundToInt(mousePosition.x), Mathf.RoundToInt(mousePosition.y));
        if (nodeColor.a == 0) {
            _mouseOverNode = false;
        } else {
            _mouseOverNode = true;
            _mouseNodeInfo = new GPUNode();
            _mouseNodeInfo.value = nodeColor.r;
            _mouseNodeInfo.bias = nodeColor.b;
        }
        GUI.changed = true;
        Debug.Log("NODE COLOR: " + nodeColor.ToString());
    }*/

    void RenderNodeLookup () {
        if (_selectedNNBehaviour != null) {
            // Initialize position matrix texture
            if (_nodeLookupTexture != null) _nodeLookupTexture.Release();
            _nodeLookupTexture = new RenderTexture (_selectedNNBehaviour.network.size.Length, _selectedNNBehaviour.network.largestLayerSize, 24);
            _nodeLookupTexture.enableRandomWrite = true;
            _nodeLookupTexture.Create();
            // Put network size into buffer
            if (_networkSizeBuffer != null) _networkSizeBuffer.Release();
            _networkSizeBuffer = new ComputeBuffer(_selectedNNBehaviour.network.layerCount, sizeof(int));
            _networkSizeBuffer.SetData(_selectedNNBehaviour.network.size);
            // Get node values and put into buffer
            GPUNode[] nodes = ConvertNodesToGPUData();
            if (_nodeValuesBuffer != null) _nodeValuesBuffer.Release();
            _nodeValuesBuffer = new ComputeBuffer(_selectedNNBehaviour.network.size.Length * _selectedNNBehaviour.network.largestLayerSize, sizeof(float) * 2);
            _nodeValuesBuffer.SetData(nodes);
            // Shader setup
            ComputeShader shader = _renderNodeLookupShader;
            int kernelId = shader.FindKernel("CSMain");
            shader.SetTexture(kernelId, "_Result", _nodeLookupTexture);
            shader.SetBuffer(kernelId, "_NetworkSize", _networkSizeBuffer);
            shader.SetBuffer(kernelId, "_NodeValues", _nodeValuesBuffer);
            shader.SetInt("_LayerCount", _selectedNNBehaviour.network.size.Length);
            shader.SetInt("_LargestLayerSize", _selectedNNBehaviour.network.largestLayerSize);
            // Calculate work group numbers for x and y
            int workGroupsX = Mathf.CeilToInt(_selectedNNBehaviour.network.size.Length / 8f);
            int workGroupsY = Mathf.CeilToInt(_selectedNNBehaviour.network.largestLayerSize / 8f);
            // Run the shader!
            shader.Dispatch(kernelId, workGroupsX, workGroupsY, 1);
        }
    }

    void RenderNetwork () {
        if (_selectedNNBehaviour != null && _nodeLookupTexture != null) { // && _networkSizeBuffer != null) {
            int width = Mathf.RoundToInt(_editor.width);
            int height = Mathf.RoundToInt(_editor.height);
            // Setup render texture
            if (_networkRender != null) _networkRender.Release();
            _networkRender = new RenderTexture( width, height, 24, RenderTextureFormat.ARGB32);
            _networkRender.enableRandomWrite = true;
            _networkRender.Create();
            /*// Setup node position buffer output
            if (_nodePositionsBuffer != null) _nodePositionsBuffer.Release();
            // Must be a rect-shape buffer
            _nodePositionsBuffer = new ComputeBuffer(_selectedNNBehaviour.network.largestLayerSize * _selectedNNBehaviour.network.size.Length, sizeof(float) * 2);*/

            // Setup shader
            ComputeShader shader = _renderNetworkShader;
            int kernelHandle = shader.FindKernel("CSMain");
            int workGroupsX = Mathf.CeilToInt(width / 8f);
            int workGroupsY = Mathf.CeilToInt(height / 8f);
            // Result render
            shader.SetTexture(kernelHandle, "_Result", _networkRender);
            // Network data
            shader.SetTexture(kernelHandle, "_NodeLookupTexture", _nodeLookupTexture);
            shader.SetInt("_NodeCount", _selectedNNBehaviour.network.nodeCount);
            shader.SetInt("_LargestLayerSize", _selectedNNBehaviour.network.largestLayerSize);
            shader.SetBuffer(kernelHandle, "_NetworkSize", _networkSizeBuffer);
            // Spacing and sizing
            shader.SetFloat("_NodeSpacing", _settings.nodeSpacing);
            shader.SetFloat("_LayerSpacing", _settings.layerSpacing);
            shader.SetFloat("_NodeSize", _settings.nodeSize);
            // Positioning
            shader.SetVector("_Offset", _offset);
            shader.SetFloat("_Zoom", _zoom);
            // Color gradients
            shader.SetVector("_NodeLowColor", _settings.nodeColorLow);
            shader.SetVector("_NodeHighColor", _settings.nodeColorHigh);
            shader.SetVector("_ConnectionLowColor", _settings.connectionColorLow);
            shader.SetVector("_ConnectionHighColor", _settings.connectionColorHigh);
            // Color modes
            shader.SetInt("_NodeColorMode", (int)_settings.nodeColorMode);
            shader.SetInt("_ConnectionColorMode", (int)_settings.connectionColorMode);
            shader.Dispatch(kernelHandle, workGroupsX, workGroupsY, 1);
        }
    }

    GPUNode[] ConvertNodesToGPUData () {
        NeuralNetwork network = _selectedNNBehaviour.network;
        int[] size = network.size;
        int layerCount = network.layerCount;
        GPUNode[] nodes = new GPUNode[layerCount * network.largestLayerSize];
        int nodeIdxTracker = 0;
        for (int i = 0; i < layerCount; i++) {
            for (int j = 0; j < network.largestLayerSize; j++) {
                int idx = (i * network.largestLayerSize) + j;
                nodes[idx] = new GPUNode();
                bool occupied = size[i] > j;
                if (occupied) {
                    nodes[idx].value = _selectedNNBehaviour.nodeValues[nodeIdxTracker];
                    nodes[idx].bias = network.nodeBiases[nodeIdxTracker];
                    nodeIdxTracker++;
                }
            }
        }
        return nodes;
    }

    void OnDrag(Vector2 delta)
    {
        _uncenteredOffset += delta;
        GUI.changed = true;
    }

    void OnScroll(Vector2 delta)
    {
        float prevZoom = _zoom;
        _zoom += delta.y * 0.02f;
        _zoom = Mathf.Clamp(_zoom, 0.1f, 5f);
        _uncenteredOffset -= _uncenteredOffset * (_zoom - prevZoom) * 0.5f;
        GUI.changed = true;
    }

}
