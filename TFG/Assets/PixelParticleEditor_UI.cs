using UnityEngine;
using UnityEditor;

public enum OPTIONS
{
    PARTICLES,
    FX,
    CAMERA,
    IMPORT,
    EXPORT,
    NULL
}

public class PixelParticleEditor_UI : EditorWindow
{
    Color color;
    Color color2;

    Vector2 scroll_palette;
    Vector2 scroll_camera1;
    Vector2 scroll_camera2;
    Vector2 scroll_layers;
    Vector2 scroll_layers2;
    Vector2 scroll_functions;
    Vector2 scroll_options;

    OPTIONS current_option = OPTIONS.PARTICLES;

    bool layer1;
    bool layer2;

    Camera camera;
    RenderTexture renderTexture;

    Texture2D texture_test;
    float slider_test = 0.0f;

    [MenuItem("Window/Pixel Particle Editor")]
    /*
    public static void ShowWindow()
    {
        GetWindow<PixelParticleEditor_UI>("Pixel Particle Editor");
    }
    */

    static void Init()
    {
        EditorWindow editorWindow = GetWindow(typeof(PixelParticleEditor_UI));
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.Show();
    }

    public void Awake()
    {
        renderTexture = new RenderTexture((int)position.width,
            (int)position.height,
            (int)RenderTextureFormat.ARGB32);
    }

    public void OnEnable()
    {
        camera = Camera.main;
    }

    public void Update()
    {
        if (camera != null)
        {
            camera.targetTexture = renderTexture;
            camera.Render();
            camera.targetTexture = null;
        }
        if (renderTexture.width != position.width ||
            renderTexture.height != position.height)
            renderTexture = new RenderTexture((int)position.width,
                (int)position.height,
                (int)RenderTextureFormat.ARGB32);
    }

    private void OnGUI()
    {
        //Window Code

        EditorGUILayout.BeginHorizontal();

        scroll_palette = EditorGUILayout.BeginScrollView(scroll_palette, GUILayout.Width(175), GUILayout.Height(500));
        DrawPaletteColumn();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();

                scroll_camera1 = EditorGUILayout.BeginScrollView(scroll_camera1, GUILayout.Width(350), GUILayout.Height(350));
        GUI.DrawTexture(new Rect(0.0f, 0.0f, position.width, position.height), renderTexture);

        EditorGUILayout.EndScrollView();

                scroll_camera2 = EditorGUILayout.BeginScrollView(scroll_camera2, GUILayout.Width(350), GUILayout.Height(350));
        GUI.DrawTexture(new Rect(0.0f, 0.0f, position.width, position.height), renderTexture);
        EditorGUILayout.EndScrollView();

                scroll_options = EditorGUILayout.BeginScrollView(scroll_options, GUILayout.Width(100), GUILayout.Height(350));
                DrawOptions();
                EditorGUILayout.EndScrollView();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

                scroll_layers = EditorGUILayout.BeginScrollView(scroll_layers, GUILayout.Width(650), GUILayout.Height(100));
                DrawLayers();
                EditorGUILayout.EndScrollView();

                scroll_layers2 = EditorGUILayout.BeginScrollView(scroll_layers2, GUILayout.Width(150), GUILayout.Height(100));
                DrawLayers2();
                EditorGUILayout.EndScrollView();

            EditorGUILayout.EndHorizontal();

        scroll_functions = EditorGUILayout.BeginScrollView(scroll_functions, GUILayout.Width(600), GUILayout.Height(250));
        DrawProperties();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    void DrawPaletteColumn()
    {
        //Palettes
        GUILayout.BeginVertical();
        //---------------------------------------
        GUILayout.Label("Layer 1 Colors", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        color = EditorGUILayout.ColorField(color);
        EditorGUILayout.ColorField(color);
        EditorGUILayout.ColorField(color);
        GUILayout.EndHorizontal();

        GUILayout.Label("Layer 2 Colors", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        color2 = EditorGUILayout.ColorField(color2);
        EditorGUILayout.ColorField(color2);
        EditorGUILayout.ColorField(color2);
        GUILayout.EndHorizontal();
        //---------------------------------------
        GUILayout.EndVertical();
    }

    void DrawOptions()
    {
        GUILayout.Label("Functions", EditorStyles.boldLabel);
        GUILayout.BeginVertical();

        if (GUILayout.Button("Particles", GUILayout.Width(60), GUILayout.Height(50)))
            current_option = OPTIONS.PARTICLES;

        if (GUILayout.Button("FX", GUILayout.Width(60), GUILayout.Height(50)))
            current_option = OPTIONS.FX;

        if (GUILayout.Button("Camera", GUILayout.Width(60), GUILayout.Height(50)))
            current_option = OPTIONS.CAMERA;

        if (GUILayout.Button("Import", GUILayout.Width(60), GUILayout.Height(50)))
            current_option = OPTIONS.IMPORT;

        if (GUILayout.Button("Export", GUILayout.Width(60), GUILayout.Height(50)))
            current_option = OPTIONS.EXPORT;

        GUILayout.EndVertical();
    }

    void DrawLayers()
    {
        GUILayout.Label("Layers", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        if (GUILayout.Button("1", GUILayout.Width(30), GUILayout.Height(30)))
            current_option = OPTIONS.PARTICLES;
        layer1 = GUILayout.Toggle(layer1,"");
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        if (GUILayout.Button("2", GUILayout.Width(30), GUILayout.Height(30)))
            current_option = OPTIONS.PARTICLES;
        layer2 = GUILayout.Toggle(layer2, "");
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        if (GUILayout.Button("3", GUILayout.Width(30), GUILayout.Height(30)))
            current_option = OPTIONS.PARTICLES;
        layer2 = GUILayout.Toggle(layer2, "");
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        if (GUILayout.Button("4", GUILayout.Width(30), GUILayout.Height(30)))
            current_option = OPTIONS.PARTICLES;
        layer2 = GUILayout.Toggle(layer2, "");
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    void DrawLayers2()
    {
        GUILayout.Label("", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("+", GUILayout.Width(50), GUILayout.Height(40)))
            current_option = OPTIONS.PARTICLES;

        if (GUILayout.Button("-", GUILayout.Width(50), GUILayout.Height(40)))
            current_option = OPTIONS.FX;

        GUILayout.EndHorizontal();
    }

    void DrawProperties()
    {
        GUILayout.Label("Properties", EditorStyles.boldLabel);
        switch (current_option)
        {
            case OPTIONS.PARTICLES:
                DrawPropertiesTemp();
                break;
            case OPTIONS.FX:
                DrawPropertiesTemp();
                break;
            case OPTIONS.CAMERA:
                DrawPropertiesTemp();
                break;
            case OPTIONS.IMPORT:
                DrawPropertiesTemp();
                break;
            case OPTIONS.EXPORT:
                DrawPropertiesExport();
                break;
        }
        
    }

    void DrawPropertiesTemp()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        GUILayout.Label("Texture Test", EditorStyles.label);
        texture_test = (Texture2D)EditorGUILayout.ObjectField(texture_test, typeof(Texture2D), false);

        GUILayout.Label("CheckBox Test", EditorStyles.label);
        layer2 = GUILayout.Toggle(layer2, "");

        GUILayout.Label("Slider Test", EditorStyles.label);
        slider_test = GUILayout.HorizontalSlider(slider_test, 0, 100);
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    void DrawPropertiesExport()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        if (GUILayout.Button("Export PNG", GUILayout.Width(50), GUILayout.Height(40)))
            SaveTexture();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    public void SaveTexture()
    {
        byte[] bytes = CreateTexture2D(renderTexture).EncodeToPNG();
        string path = Application.dataPath + "/test.png";
        System.IO.File.WriteAllBytes(path, bytes);
    }

    Texture2D CreateTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }
}
