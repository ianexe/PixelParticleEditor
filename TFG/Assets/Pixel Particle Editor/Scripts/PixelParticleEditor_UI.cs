using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

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

    public Camera camera;
    public ParticleSystem particle_prefab;
    List<ParticleSystem> particles;

    RenderTexture renderTexture;
    RenderTexture renderTexture2;

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
        //Pixel Render Texture
        renderTexture = new RenderTexture((int)100,
            (int)100,
            (int)RenderTextureFormat.ARGB32);
        renderTexture.filterMode = FilterMode.Point;

        //HD Render Texture
        renderTexture2 = new RenderTexture((int)350,
            (int)350,
            (int)RenderTextureFormat.ARGB32);

        //Particle Init
        ParticleSystem particle_to_instantiate = Instantiate(particle_prefab, new Vector3(-10000, -10, 10), Quaternion.identity);
        particles = new List<ParticleSystem>();
        particles.Add(particle_to_instantiate);

        //Camera Init
        camera = Instantiate(camera, new Vector3(-10000,0,0), Quaternion.identity);
    }

    public void OnEnable()
    {
        Selection.activeObject = particles[0].gameObject;
    }

    public void OnDestroy()
    {
        DestroyImmediate(particles[0].gameObject);
        particles.Clear();
        
        DestroyImmediate(camera.gameObject);
    }

    public void Update()
    {
        if (camera != null)
        {
            camera.targetTexture = renderTexture;
            camera.Render();
            camera.targetTexture = renderTexture2;
            camera.Render();
            camera.targetTexture = null;
        }
        /*
        if (renderTexture.width != position.width ||
            renderTexture.height != position.height)
            renderTexture = new RenderTexture((int)position.width,
                (int)position.height,
                (int)RenderTextureFormat.ARGB32);
                */
    }

    private void OnGUI()
    {
        //Window Code

        EditorGUILayout.BeginHorizontal();

        //Palette Column
        scroll_palette = EditorGUILayout.BeginScrollView(scroll_palette, GUILayout.Width(175), GUILayout.Height(500));
        DrawPaletteColumn();
        EditorGUILayout.EndScrollView();


        EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();

                //Particle HD Render
                scroll_camera1 = EditorGUILayout.BeginScrollView(scroll_camera1, GUILayout.Width(350), GUILayout.Height(350));
                GUI.DrawTexture(new Rect(0.0f, 0.0f, 350, 350), renderTexture2);
                EditorGUILayout.EndScrollView();

                //Particle Pixel Render
                scroll_camera2 = EditorGUILayout.BeginScrollView(scroll_camera2, GUILayout.Width(350), GUILayout.Height(350));
                GUI.DrawTexture(new Rect(0.0f, 0.0f, 350, 350), renderTexture);
                EditorGUILayout.EndScrollView();
                
                //Option Column
                scroll_options = EditorGUILayout.BeginScrollView(scroll_options, GUILayout.Width(100), GUILayout.Height(350));
                DrawOptions();
                EditorGUILayout.EndScrollView();

            EditorGUILayout.EndHorizontal();

            //Layer Bar
            EditorGUILayout.BeginHorizontal();

                scroll_layers = EditorGUILayout.BeginScrollView(scroll_layers, GUILayout.Width(650), GUILayout.Height(100));
                DrawLayers();
                EditorGUILayout.EndScrollView();

                scroll_layers2 = EditorGUILayout.BeginScrollView(scroll_layers2, GUILayout.Width(150), GUILayout.Height(100));
                DrawLayers2();
                EditorGUILayout.EndScrollView();

            EditorGUILayout.EndHorizontal();

            //Properties Bar
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

        foreach (ParticleSystem particle in particles)
        {
            GUILayout.BeginVertical();

            Color original_color = GUI.color;
            if (Selection.activeObject != particle.gameObject)
            {
                GUI.color = new Color(0.5f, 0.5f, 0.5f);
            }

            if (GUILayout.Button(""+(particles.IndexOf(particle)+1), GUILayout.Width(30), GUILayout.Height(30)))
                Selection.activeObject = particle.gameObject;

            GUI.color = original_color;

            bool active = particle.gameObject.activeSelf;
            active = GUILayout.Toggle(active, "");
            if (active != particle.gameObject.activeSelf)
            {
                particle.gameObject.SetActive(active);
                if (active)
                    particle.Play();
            }

            GUILayout.EndVertical();
        }

        GUILayout.EndHorizontal();
    }

    void DrawLayers2()
    {
        GUILayout.Label("", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("+", GUILayout.Width(50), GUILayout.Height(40)))
        {
            ParticleSystem new_particle = Instantiate(particle_prefab, new Vector3(-10000, -10, 10), Quaternion.identity, particles[0].gameObject.transform);
            particles.Add(new_particle);
            Selection.activeObject = new_particle.gameObject;
            new_particle.Play(true);
        }

        if (GUILayout.Button("-", GUILayout.Width(50), GUILayout.Height(40)))
        {
            if (particles.Count > 1)
            {
                if (Selection.activeObject == particles[particles.Count - 1].gameObject)
                    Selection.activeObject = particles[particles.Count - 2].gameObject;

                DestroyImmediate(particles[particles.Count - 1].gameObject);
                particles.RemoveAt(particles.Count - 1);
            }
        }   

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
        Texture2D tex = new Texture2D(renderTexture.height, renderTexture.width, TextureFormat.ARGB32, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }
}
