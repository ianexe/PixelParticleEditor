﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public enum OPTIONS
{
    PARTICLES,
    RENDER,
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
    Vector2 scroll_palette2;
    Vector2 scroll_camera1;
    Vector2 scroll_camera2;
    Vector2 scroll_layers;
    Vector2 scroll_layers2;
    Vector2 scroll_functions;
    Vector2 scroll_options;

    OPTIONS current_option = OPTIONS.PARTICLES;

    //bool layer1;
    //bool layer2;

    public Camera camera;
    public ParticleSystem particle_prefab;
    public GameObject preset_prefab_0;
    public GameObject preset_prefab_1;
    public GameObject preset_prefab_2;

    List<ParticleSystem> particles = new List<ParticleSystem>();
    List<Material> materials = new List<Material>();
    List<Material> palettes = new List<Material>();

    RenderTexture renderTexture;
    RenderTexture renderTexture2;

    //Texture2D texture_test;
    //float slider_test = 0.0f;

    int palette_list = 0;

    int pixelation = 100;
    int selected_dithering = 0;
    List<Texture> dithering_textures;
    float antialiasing_value = 0.0f;

    ParticleSystem selected_particle;

    GameObject import_object = null;
    GameObject import_preset = null;
    int selected_preset = 0;

    float render_time = 5.0f;
    int render_frames = 8;
    int current_frame = 0;
    bool wait_one_frame = false;
    bool render_active = false;
    List<Texture2D> rendered_textures;
    string render_path = "Export";
    string export_name = "Pixel Particle";

    [MenuItem("Tools/Pixel Particle Editor")]

    static void Init()
    {
        EditorWindow editorWindow = GetWindow<PixelParticleEditor_UI>("Pixel Particle Editor");
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.minSize = new Vector2(950, 650);
        editorWindow.maxSize = new Vector2(950, 650);
        editorWindow.maximized = true;
        editorWindow.Show();
    }

    public void Awake()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        //Pixel Render Texture
        renderTexture = new RenderTexture((int)pixelation,
            (int)pixelation,
            (int)RenderTextureFormat.ARGB32);
        renderTexture.filterMode = FilterMode.Point;

        //HD Render Texture
        renderTexture2 = new RenderTexture((int)350,
            (int)350,
            (int)RenderTextureFormat.ARGB32);

        //Particle Init
        CreateParticle();

        //Camera Init
        camera = Instantiate(camera, new Vector3(-10000,0,0), Quaternion.identity);
        camera.gameObject.hideFlags = HideFlags.HideInHierarchy;

        camera.GetComponent<CameraPalette>().palette_material = new Material(camera.GetComponent<CameraPalette>().palette_material);

        //Palette Init
        string[] palette_guids = AssetDatabase.FindAssets("t:material", new[] { "Assets/Pixel Particle Editor/Materials/Palettes" });

        foreach (string guid in palette_guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material to_add = AssetDatabase.LoadAssetAtPath<Material>(path);
            palettes.Add(to_add);
        }

        //Rendered Texture List Init
        rendered_textures = new List<Texture2D>();

        //Dithering Texture Array Init
        dithering_textures = new List<Texture>();
        string[] texture_guids = (AssetDatabase.FindAssets("t:texture", new[] { "Assets/Pixel Particle Editor/Materials/Palettes" }));
        foreach (string guid in texture_guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Texture to_add = AssetDatabase.LoadAssetAtPath<Texture>(path);
            dithering_textures.Add(to_add);
        }
    }

    public void OnEnable()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        SelectParticle(particles[0]);
    }

    public void OnDestroy()
    {
        if (particles.Count > 0)
            DestroyImmediate(particles[0].gameObject);

        particles.Clear();

        materials.Clear();

        if (camera)
            DestroyImmediate(camera.gameObject);
    }

    public void Update()
    {
        if (!camera || !particles[0] || EditorApplication.isPlayingOrWillChangePlaymode)
            Close();

        if (camera != null)
        {
            camera.targetTexture = renderTexture;
            camera.Render();
            camera.targetTexture = renderTexture2;
            camera.Render();
            camera.targetTexture = null;
        }

        if (render_active)
        {
            float current_time = render_time / render_frames * current_frame;
            particles[0].Simulate(current_time);
            current_frame++;

            if (wait_one_frame == false)
            {
                wait_one_frame = true;
            }
                
            else
            {
                //string name = "frame" + (current_frame-1);
                //SaveTexture2(name);
                SaveFrameToList();

                if (current_frame >= render_frames+1)
                {
                    SaveSpritesheet();
                    particles[0].Play();
                    SelectParticle(selected_particle);
                    render_active = false;
                }
            } 
        }

    }

    private void OnFocus()
    {
        if (selected_particle != null)
            SelectParticle(selected_particle);
    }

    private void OnGUI()
    {
        //Window Code

        EditorGUILayout.BeginHorizontal();

        //Palette Column
        EditorGUILayout.BeginVertical();

        scroll_palette = EditorGUILayout.BeginScrollView(scroll_palette, GUILayout.Width(175), GUILayout.Height(450));
        DrawPaletteColumn();
        EditorGUILayout.EndScrollView();

        scroll_palette2 = EditorGUILayout.BeginScrollView(scroll_palette2, GUILayout.Width(175), GUILayout.Height(250));
        DrawPaletteColumn2();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();


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
            scroll_functions = EditorGUILayout.BeginScrollView(scroll_functions, GUILayout.Width(750), GUILayout.Height(250));
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
        foreach (ParticleSystem particle in particles)
        {
            GUILayout.Label("Layer " + (particles.IndexOf(particle) + 1) + " Color", EditorStyles.boldLabel);

            int blending;

            if (particle.GetComponent<Renderer>().sharedMaterial.shader == Shader.Find("Unlit/PixelShader"))
                blending = 0;

            else if (particle.GetComponent<Renderer>().sharedMaterial.shader == Shader.Find("Unlit/PixelShaderAdditive"))
                blending = 1;

            else
                blending = 2;

            string[] blending_options = new string[]
            {
            "Normal",
            "Additive",
            "Subtractive"
            };

            blending = EditorGUILayout.Popup(blending, blending_options);

            switch (blending)
            {
                case 0:
                    if (particle.GetComponent<Renderer>().sharedMaterial.shader != Shader.Find("Unlit/PixelShader"))
                        particle.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("Unlit/PixelShader");
                    break;
                case 1:
                    if (particle.GetComponent<Renderer>().sharedMaterial.shader != Shader.Find("Unlit/PixelShaderAdditive"))
                        particle.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("Unlit/PixelShaderAdditive");
                    break;
                case 2:
                    if (particle.GetComponent<Renderer>().sharedMaterial.shader != Shader.Find("Unlit/PixelShaderSubtractive"))
                        particle.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("Unlit/PixelShaderSubtractive");
                    break;
            }

            GUILayout.BeginHorizontal();
            var main = particle.main;
            Color color = main.startColor.color;
            color = EditorGUILayout.ColorField(color);
            main.startColor = color;

            GUILayout.EndHorizontal();
        }
        //---------------------------------------
        GUILayout.EndVertical();
    }

    void DrawPaletteColumn2()
    {
        GUILayout.BeginVertical();

        GUILayout.Label("Custom Palette", EditorStyles.boldLabel);

        Material palette_material = camera.GetComponent<CameraPalette>().palette_material;

        List<string> palette_options = new List<string>();

        foreach (Material palette in palettes)
        {
            palette_options.Add(palette.name);
        }
        
        palette_list = EditorGUILayout.Popup(palette_list, palette_options.ToArray());

        GUILayout.BeginHorizontal();

        string button_text;
        if (!camera.GetComponent<CameraPalette>().enabled)
            button_text = "Enable";
        else
            button_text = "Disable";

        if (GUILayout.Button(button_text, GUILayout.Width(50), GUILayout.Height(20)))
        {
            camera.GetComponent<CameraPalette>().enabled = !camera.GetComponent<CameraPalette>().enabled;
            if (camera.GetComponent<CameraBlur>().enabled)
                camera.GetComponent<CameraBlur>().enabled = false;
        }

        if (GUILayout.Button("Save", GUILayout.Width(50), GUILayout.Height(20)))
        {
            Rect rect = new Rect(8, 0, 0, 70);
            PaletteSaveWindow popup = new PaletteSaveWindow();
            popup.SetPalette(palette_material);
            popup.SetList(palettes);
            PopupWindow.Show(rect, popup);
        }

        if (GUILayout.Button("Load", GUILayout.Width(50), GUILayout.Height(20)))
        {

            Color lightest = palettes[palette_list].GetColor("_Ligtest");
            Color light = palettes[palette_list].GetColor("_Ligt");
            Color dark = palettes[palette_list].GetColor("_Dark");
            Color darkest = palettes[palette_list].GetColor("_Darkest");

            palette_material.SetColor("_Ligtest", lightest);
            palette_material.SetColor("_Ligt", light);
            palette_material.SetColor("_Dark", dark);
            palette_material.SetColor("_Darkest", darkest);
        }

        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        Color c_lightest = palette_material.GetColor("_Ligtest");
        c_lightest = EditorGUILayout.ColorField(c_lightest);
        palette_material.SetColor("_Ligtest", c_lightest);

        EditorGUILayout.Space();

        Color c_light = palette_material.GetColor("_Ligt");
        c_light = EditorGUILayout.ColorField(c_light);
        palette_material.SetColor("_Ligt", c_light);

        EditorGUILayout.Space();

        Color c_dark = palette_material.GetColor("_Dark");
        c_dark = EditorGUILayout.ColorField(c_dark);
        palette_material.SetColor("_Dark", c_dark);

        EditorGUILayout.Space();

        Color c_darkest = palette_material.GetColor("_Darkest");
        c_darkest = EditorGUILayout.ColorField(c_darkest);
        palette_material.SetColor("_Darkest", c_darkest);

        GUILayout.EndVertical();
    }

        void DrawOptions()
    {
        GUILayout.Label("Functions", EditorStyles.boldLabel);
        GUILayout.BeginVertical();

        DrawOptionButton(OPTIONS.PARTICLES, "Particles");
        DrawOptionButton(OPTIONS.RENDER, "Render");
        DrawOptionButton(OPTIONS.FX, "FX");
        DrawOptionButton(OPTIONS.CAMERA, "Camera");
        DrawOptionButton(OPTIONS.IMPORT, "Import");
        DrawOptionButton(OPTIONS.EXPORT, "Export");
        
        GUILayout.EndVertical();
    }

    void DrawOptionButton(OPTIONS option, string button_text)
    {
        Color original_color = GUI.color;
        Color grey = new Color(0.5f, 0.5f, 0.5f);
        if (current_option != option)
        {
            GUI.color = grey;
        }
        if (GUILayout.Button(button_text, GUILayout.Width(60), GUILayout.Height(50)))
            current_option = option;

        GUI.color = original_color;
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

            if (GUILayout.Button("" + (particles.IndexOf(particle) + 1), GUILayout.Width(30), GUILayout.Height(30)))
                SelectParticle(particle);

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
            CreateParticle();
        }

        if (GUILayout.Button("-", GUILayout.Width(50), GUILayout.Height(40)))
        {
            if (particles.Count > 1)
            {
                if (Selection.activeObject == particles[particles.Count - 1].gameObject)
                    SelectParticle(particles[particles.Count - 2]);

                DestroyImmediate(particles[particles.Count - 1].gameObject);
                particles.RemoveAt(particles.Count - 1);

                DestroyImmediate(materials[materials.Count - 1]);
                materials.RemoveAt(materials.Count - 1);
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
                DrawPropertiesParticle();
                break;
            case OPTIONS.RENDER:
                DrawPropertiesRender();
                break;
            case OPTIONS.FX:
                DrawPropertiesFX();
                break;
            case OPTIONS.CAMERA:
                DrawPropertiesCamera();
                break;
            case OPTIONS.IMPORT:
                DrawPropertiesImport();
                break;
            case OPTIONS.EXPORT:
                DrawPropertiesExport();
                break;
        }
        
    }

    void DrawPropertiesCamera()
    {
        GUILayout.Label("Move Camera", EditorStyles.label);
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(GUILayout.Width(30));
        GUILayout.Space(30);
        if (GUILayout.Button("<", GUILayout.Width(30), GUILayout.Height(30)))
        {
            camera.transform.Translate(1, 0, 0);
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.Width(30));
        if (GUILayout.Button("^", GUILayout.Width(30), GUILayout.Height(30)))
        {
            camera.transform.Translate(0, -1, 0);
        }

        GUILayout.Space(30);

        if (GUILayout.Button("v", GUILayout.Width(30), GUILayout.Height(30)))
        {
            camera.transform.Translate(0, 1, 0);
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        GUILayout.Space(30);
        if (GUILayout.Button(">", GUILayout.Width(30), GUILayout.Height(30)))
        {
            camera.transform.Translate(-1, 0, 0);
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    void DrawPropertiesRender()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();

        ParticleSystemRenderer particle_renderer;
        particle_renderer = selected_particle.GetComponent<ParticleSystemRenderer>();
        var main = selected_particle.main;

        var particle_gradient = selected_particle.colorOverLifetime;
        bool particle_button = particle_gradient.enabled;
        particle_button = GUILayout.Toggle(particle_button, " Color Gradient");
        if (particle_button != particle_gradient.enabled)
            particle_gradient.enabled = particle_button;

        Gradient gradient = particle_gradient.color.gradient;
        gradient = EditorGUILayout.GradientField(gradient);
        particle_gradient.color = gradient;

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        GUILayout.Label("Render Mode", EditorStyles.label);

        GUILayout.Space(2);

        string[] rendermode_popup = new string[]
        {
            "Billboard",
            "Stretched Billboard"
        };

        int rendermode;
        if (particle_renderer.renderMode == ParticleSystemRenderMode.Billboard)
            rendermode = 0;
        else
            rendermode = 1;

        rendermode = EditorGUILayout.Popup(rendermode, rendermode_popup);
        

        switch (rendermode)
        {
            case 0:
                particle_renderer.renderMode = ParticleSystemRenderMode.Billboard;
                break;

            case 1:
                particle_renderer.renderMode = ParticleSystemRenderMode.Stretch;
                break;
        }

        if (rendermode == 1)
        {
            GUILayout.Space(2);

            GUILayout.Label("Length Scale", EditorStyles.label);
            float length = particle_renderer.lengthScale;
            length = EditorGUILayout.Slider(length, -10.0f, 10.0f);
            particle_renderer.lengthScale = length;
        }
    
        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        GUILayout.Label("Sort Mode", EditorStyles.label);

        GUILayout.Space(2);

        string[] sortmode_popup = new string[]
        {
            "None",
            "Youngest In Front",
            "Oldest In Front"
        };

        int sortmode;
        if (particle_renderer.sortMode == ParticleSystemSortMode.None)
            sortmode = 0;
        else if (particle_renderer.sortMode == ParticleSystemSortMode.YoungestInFront)
            sortmode = 1;
        else
            sortmode = 2;

        sortmode = EditorGUILayout.Popup(sortmode, sortmode_popup);

        switch (sortmode)
        {
            case 0:
                particle_renderer.sortMode = ParticleSystemSortMode.None;
                break;

            case 1:
                particle_renderer.sortMode = ParticleSystemSortMode.YoungestInFront;
                break;
            case 2:
                particle_renderer.sortMode = ParticleSystemSortMode.OldestInFront;
                break;
        }

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    void DrawPropertiesParticle()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();


        ParticleSystemRenderer particle_renderer;
        particle_renderer = selected_particle.GetComponent <ParticleSystemRenderer>();
        Texture texture_select = particle_renderer.sharedMaterial.mainTexture;

        GUILayout.Label("Texture", EditorStyles.label);
        texture_select = (Texture)EditorGUILayout.ObjectField(texture_select, typeof(Texture), false);

        if (texture_select != particle_renderer.sharedMaterial.mainTexture)
        {
            particle_renderer.sharedMaterial.mainTexture = texture_select;
        }

        var main = selected_particle.main;

        GUILayout.Label("Simulation Speed", EditorStyles.label);
        float simulation_speed = main.simulationSpeed;
        simulation_speed = EditorGUILayout.Slider(simulation_speed, 0.0f, 20.0f);
        main.simulationSpeed = simulation_speed;

        GUILayout.Label("Time Delay", EditorStyles.label);
        float particle_delay = main.startDelay.constant;
        particle_delay = EditorGUILayout.Slider(particle_delay, 0.0f, 20.0f);
        main.startDelay = particle_delay;

        bool particle_loop = main.loop;
        particle_loop = GUILayout.Toggle(particle_loop, " Loop");
        main.loop = particle_loop;


        if (GUILayout.Button("Reset Animation", GUILayout.Width(120), GUILayout.Height(30)))
        {
            particles[0].Simulate(0.0f);
            particles[0].Play();
        }

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        GUILayout.Label("Particle Lifetime", EditorStyles.label);
        float particle_time = main.startLifetime.constant;
        particle_time = EditorGUILayout.Slider(particle_time, 0.0f, 20.0f);
        main.startLifetime = particle_time;

        GUILayout.Label("Particle Speed", EditorStyles.label);
        float particle_speed = main.startSpeed.constant;
        particle_speed = EditorGUILayout.Slider(particle_speed, 0.0f, 20.0f);
        main.startSpeed = particle_speed;

        GUILayout.Label("Particle Size", EditorStyles.label);
        float particle_size = main.startSize.constant;
        particle_size = EditorGUILayout.Slider(particle_size, 0.0f, 20.0f);
        main.startSize = particle_size;

        GUILayout.Label("Size Increment", EditorStyles.label);
        var size_ol = selected_particle.sizeOverLifetime;
        int size_increment;
        AnimationCurve curve1 = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
        AnimationCurve curve2 = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 0.0f);
        AnimationCurve curve3 = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
        AnimationCurve curve4 = AnimationCurve.EaseInOut(0.0f, 1.0f, 1.0f, 0.0f);
        if (!size_ol.enabled)
            size_increment = 0;
        else
        {
            if (size_ol.size.curve.Equals(curve1))
                size_increment = 1;

            else if (size_ol.size.curve.Equals(curve2))
                size_increment = 2;

            else if(size_ol.size.curve.Equals(curve3))
                size_increment = 3;

            else
                size_increment = 4;
        }
            
        string[] size_options = new string[] 
        {
            "None",
            "Linear Increase",
            "Linear Decrease",
            "Ease-in-out Increase",
            "Ease-in-out Decrease"
        };
        size_increment = EditorGUILayout.Popup(size_increment, size_options);

        switch (size_increment)
        {
            case 0:
                size_ol.enabled = false;
                break;
            case 1:
                size_ol.enabled = true;
                size_ol.size = new ParticleSystem.MinMaxCurve(1.0f, curve1);
                break;
            case 2:
                size_ol.enabled = true;
                size_ol.size = new ParticleSystem.MinMaxCurve(1.0f, curve2);
                break;
            case 3:
                size_ol.enabled = true;
                size_ol.size = new ParticleSystem.MinMaxCurve(1.0f, curve3);
                break;
            case 4:
                size_ol.enabled = true;
                size_ol.size = new ParticleSystem.MinMaxCurve(1.0f, curve4);
                break;
            default:
                Debug.LogError("Unrecognized Option");
                break;
        }

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        GUILayout.Label("Gravity", EditorStyles.label);
        float gravity = main.gravityModifier.constant;
        gravity = EditorGUILayout.Slider(gravity, -1.0f, 1.0f);
        main.gravityModifier = gravity;

        GUILayout.Label("Max Particles", EditorStyles.label);
        int max_particles = main.maxParticles;
        max_particles = EditorGUILayout.IntSlider(max_particles, 0, 2000);
        main.maxParticles = max_particles;

        GUILayout.Label("Particles per Second", EditorStyles.label);
        float particles_second = selected_particle.emission.rateOverTime.constant;
        particles_second = EditorGUILayout.Slider(particles_second, 0.0f, 2000.0f);
        var emitter = selected_particle.emission;
        emitter.rateOverTime = particles_second;

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        bool particle_shape = selected_particle.shape.enabled;
        particle_shape = GUILayout.Toggle(particle_shape, " Shape");
        var shape = selected_particle.shape;
        shape.enabled = particle_shape;

        string[] shape_options = new string[]
        {
            "Sphere",
            "Hemisphere",
            "Cone",
            "Circle"
        };

        int shape_value;
        if (shape.shapeType == ParticleSystemShapeType.Sphere)
            shape_value = 0;

        else if (shape.shapeType == ParticleSystemShapeType.Hemisphere)
            shape_value = 1;

        else if (shape.shapeType == ParticleSystemShapeType.Cone)
            shape_value = 2;

        else
            shape_value = 3;

        shape_value = EditorGUILayout.Popup(shape_value, shape_options);

        switch (shape_value)
        {
            case 0:
                shape.shapeType = ParticleSystemShapeType.Sphere;
                break;

            case 1:
                shape.shapeType = ParticleSystemShapeType.Hemisphere;
                break;

            case 2:
                shape.shapeType = ParticleSystemShapeType.Cone;

                float cone_angle = shape.angle;
                cone_angle = GUILayout.HorizontalSlider(cone_angle, 0.0f, 90.0f);
                shape.angle = cone_angle;
                break;

            case 3:
                shape.shapeType = ParticleSystemShapeType.Circle;
                break;

            default:
                Debug.LogError("Unrecognized Option");
                break;
        }

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        Vector3 particle_position = shape.position;
        particle_position = EditorGUILayout.Vector3Field("Position:", particle_position);
        shape.position = particle_position;

        Vector3 particle_rotation = shape.rotation;
        particle_rotation = EditorGUILayout.Vector3Field("Rotation:", particle_rotation);
        shape.rotation = particle_rotation;

        Vector3 particle_scale = shape.scale;
        particle_scale = EditorGUILayout.Vector3Field("Scale:", particle_scale);
        shape.scale = particle_scale;

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    void DrawPropertiesFX()
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUILayout.Label("Pixelation", EditorStyles.label);
        int prev_pixelation = pixelation;
        pixelation = EditorGUILayout.IntSlider(pixelation, 256, 8);
        if (pixelation != prev_pixelation)
        {
            renderTexture.Release();
            renderTexture = new RenderTexture(pixelation, pixelation, (int)RenderTextureFormat.ARGB32);
            renderTexture.filterMode = FilterMode.Point;
        }
        GUILayout.EndVertical();

        GUILayout.Space(30);

        GUILayout.BeginVertical();
        Material palette_material = camera.GetComponent<CameraPalette>().palette_material;

        //GUILayout.Label("Dithering", EditorStyles.label);
        bool dithering_button = palette_material.shader == Shader.Find("PixelParticle/Pixel Palette 2");
        dithering_button = GUILayout.Toggle(dithering_button,  " Dithering");
        if (dithering_button != (palette_material.shader == Shader.Find("PixelParticle/Pixel Palette 2")))
        {
            if (dithering_button == true)
            {
                palette_material.shader = Shader.Find("PixelParticle/Pixel Palette 2");
                palette_material.SetTexture("_DitherTex", dithering_textures[selected_dithering]);
            }
            else
            {
                palette_material.shader = Shader.Find("PixelParticle/Pixel Palette");
            }
        }

        int prev_dithering = selected_dithering;
        string[] dither_options = new string[]
        {
            "2x2",
            "4x4",
            "8x8"
        };
        selected_dithering = EditorGUILayout.Popup(selected_dithering, dither_options);
        if (prev_dithering != selected_dithering && dithering_button)
        {
            palette_material.SetTexture("_DitherTex", dithering_textures[selected_dithering]);
        }

        Rect rect = new Rect(330, 80, 70, 70);
        GUI.DrawTexture(rect, dithering_textures[selected_dithering]);

        GUILayout.EndVertical();

        GUILayout.Space(30);

        GUILayout.BeginVertical();

        Material antialiasing_material = camera.GetComponent<CameraBlur>().blur_material;

        bool antialiasing_button = camera.GetComponent<CameraBlur>().enabled == true;
        antialiasing_button = GUILayout.Toggle(antialiasing_button, " Antialiasing");
        if (antialiasing_button != (camera.GetComponent<CameraBlur>().enabled == true))
        {
            camera.GetComponent<CameraBlur>().enabled = antialiasing_button;
        }

        if (antialiasing_button && camera.GetComponent<CameraPalette>().enabled)
            camera.GetComponent<CameraPalette>().enabled = false;

        antialiasing_value = EditorGUILayout.Slider(antialiasing_value, 0.0f, 0.015f);
        if (antialiasing_button)
        {
            antialiasing_material.SetFloat("_BlurSize", antialiasing_value);
            antialiasing_material.SetFloat("_Samples", 100.0f);
        }
        
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    void DrawPropertiesImport()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical("Box");

        if (GUILayout.Button("Import\nPreset", GUILayout.Width(90), GUILayout.Height(40)))
        {
            if(import_preset != null && import_preset.GetComponent<ParticleSystem>())
            {
                if (EditorUtility.DisplayDialog("Warning", "Do you want to overwrite the current Particle?", "Yes", "No"))
                {
                    DestroyImmediate(particles[0].gameObject);
                    particles.Clear();
                    materials.Clear();

                    CreateCustomParticle(import_preset);

                    export_name = import_preset.name;
                }
            }

            else
            {
                EditorUtility.DisplayDialog("Error","Selected GameObject does not contain a Particle System.","Accept");
            }

        }
        GUILayout.Space(20.0f);
        string[] preset_options = new string[]
        {
            "Fire",
            "Energy Ball",
            "Glitch"
        };
        selected_preset = EditorGUILayout.Popup(selected_preset, preset_options);

        switch (selected_preset)
        {
            case 0:
                import_preset = preset_prefab_0;
                break;
            case 1:
                import_preset = preset_prefab_1;
                break;
            case 2:
                import_preset = preset_prefab_2;
                break;
        }

        GUILayout.EndVertical();

        GUILayout.BeginVertical("Box");

        if (GUILayout.Button("Import\nGame Object", GUILayout.Width(90), GUILayout.Height(40)))
        {
            if (import_object != null && import_object.GetComponent<ParticleSystem>())
            {
                if (EditorUtility.DisplayDialog("Warning", "Do you want to overwrite the current Particle?", "Yes", "No"))
                {
                    DestroyImmediate(particles[0].gameObject);
                    particles.Clear();
                    materials.Clear();

                    CreateCustomParticle(import_object);

                    export_name = import_object.name;
                }
            }

            else
            {
                EditorUtility.DisplayDialog("Error", "Selected GameObject does not contain a Particle System.", "Accept");
            }

        }
        GUILayout.Space(20.0f);
        import_object = (GameObject)EditorGUILayout.ObjectField("Import Object:", import_object, typeof(GameObject), false);

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    void DrawPropertiesExport()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical("Box");
        if (GUILayout.Button("Export\nSprite Sheet", GUILayout.Width(90), GUILayout.Height(40)))
        {
            render_active = true;
            wait_one_frame = false;
            current_frame = 0;
        }
        GUILayout.Space(20.0f);
        render_time = EditorGUILayout.FloatField("Render Time:", render_time, GUILayout.Width(361));
        render_frames = EditorGUILayout.IntField("Frames to Render:", render_frames, GUILayout.Width(361));
        render_path = EditorGUILayout.TextField("File Name:", render_path, GUILayout.Width(361));

        GUILayout.EndVertical();

        GUILayout.BeginVertical("Box");
        if (GUILayout.Button("Export\nGame Object", GUILayout.Width(90), GUILayout.Height(40)))
        {
            SaveGameObject(export_name);
        }
        GUILayout.Space(20.0f);
        export_name = EditorGUILayout.TextField("Game Object Name:", export_name);
        GUILayout.Space(36.0f);
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    public void SaveGameObject(string name)
    {
        if (particles[0] != null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Particle Export"))
            {
                AssetDatabase.CreateFolder("Assets", "Particle Export");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Particle Export/Game Objects"))
            {
                AssetDatabase.CreateFolder("Assets/Particle Export", "Game Objects");
            }

            List<Material> export_materials = new List<Material>();

            foreach (Material material in materials)
            {
                export_materials.Add(new Material(material));
                AssetDatabase.CreateAsset(export_materials[materials.IndexOf(material)], "Assets/Particle Export/Game Objects/" + name + "_Material" + materials.IndexOf(material) + ".mat");
            }

            GameObject to_export = Instantiate(particles[0].gameObject, new Vector3(0, -90, 0), Quaternion.identity);
            ParticleSystemRenderer export_renderer = to_export.GetComponent<ParticleSystemRenderer>();
            export_renderer.material = export_materials[0];

            for (int i = 0; i < to_export.transform.childCount; i++)
            {
                export_renderer = to_export.transform.GetChild(i).GetComponent<ParticleSystemRenderer>();
                export_renderer.material = export_materials[i+1];
            }

            export_materials.Clear();

            PrefabUtility.SaveAsPrefabAsset(to_export, "Assets/Particle Export/Game Objects/" + name + ".prefab");

            DestroyImmediate(to_export);
        }
    }

    public void SaveTexture()
    {
        byte[] bytes = CreateTexture2D(renderTexture).EncodeToPNG();
        string path = Application.dataPath + "/test.png";
        System.IO.File.WriteAllBytes(path, bytes);
    }

    public void SaveFrameToList()
    {
        Texture2D new_texture = CreateTexture2D(renderTexture);
        rendered_textures.Add(new_texture);
    }

    public void SaveSpritesheet()
    {
        Texture2D texture = new Texture2D(pixelation*render_frames, pixelation);

        for (int i = 0; i < render_frames; i++)
        {
            for (int x = 0; x < pixelation; x++)
            {
                for (int y = 0; y < pixelation; y++)
                {
                    Color to_render = rendered_textures[i].GetPixel(x, y);
                    texture.SetPixel(x + pixelation*i, y, to_render);
                }
            }
        }

        byte[] bytes = texture.EncodeToPNG();

        if (!AssetDatabase.IsValidFolder("Assets/Particle Export"))
        {
            AssetDatabase.CreateFolder("Assets", "Particle Export");
        }

        if (!AssetDatabase.IsValidFolder("Assets/Particle Export/Sprite Sheets"))
        {
            AssetDatabase.CreateFolder("Assets/Particle Export", "Sprite Sheets");
        }

        string path = Application.dataPath + "/Particle Export/Sprite Sheets/" + render_path + ".png";
        System.IO.File.WriteAllBytes(path, bytes);

        AssetDatabase.Refresh();

        rendered_textures.Clear();
    }

    Texture2D CreateTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(renderTexture.height, renderTexture.width, TextureFormat.ARGB32, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    void SelectParticle(ParticleSystem to_select)
    {
        Selection.activeObject = to_select.gameObject;
        selected_particle = to_select;
    }

    ParticleSystem CreateParticle()
    {
        ParticleSystem to_return;

        if (particles.Count <= 0)
            to_return = Instantiate(particle_prefab, new Vector3(-10000, -10, 10), Quaternion.identity);

        else
            to_return = Instantiate(particle_prefab, new Vector3(-10000, -10, 10), Quaternion.identity, particles[0].gameObject.transform);

        to_return.transform.Rotate(-90, 0, 0);
        to_return.gameObject.hideFlags = HideFlags.HideInHierarchy;
        to_return.randomSeed = (uint)(Random.Range(-2147483648, 2147483648));
        particles.Add(to_return);
        SelectParticle(to_return);

        ParticleSystemRenderer particle_renderer;
        particle_renderer = to_return.GetComponent<ParticleSystemRenderer>();
        particle_renderer.sortingFudge = particles.IndexOf(to_return) * -10;

        particle_renderer.sharedMaterial = new Material(particle_prefab.GetComponent<Renderer>().sharedMaterial);
        materials.Add(particle_renderer.sharedMaterial);

        to_return.Play(true);

        return to_return;
    }

    ParticleSystem CreateCustomParticle(GameObject to_import)
    {
        ParticleSystem to_return;
        ParticleSystem import_particle = to_import.GetComponent<ParticleSystem>();

        to_return = Instantiate(import_particle, new Vector3(-10000, -10, 10), Quaternion.identity);

        to_return.transform.Rotate(-90, 0, 0);
        to_return.gameObject.hideFlags = HideFlags.HideInHierarchy;
        particles.Add(to_return);
        SelectParticle(to_return);

        ParticleSystemRenderer particle_renderer;
        particle_renderer = to_return.GetComponent<ParticleSystemRenderer>();
        particle_renderer.sortingFudge = particles.IndexOf(to_return) * -10;

        particle_renderer.sharedMaterial = new Material(import_particle.GetComponent<Renderer>().sharedMaterial);
        materials.Add(particle_renderer.sharedMaterial);

        for (int i = 0; i < to_import.transform.childCount; i++)
        {
            if (to_return.transform.GetChild(i).GetComponent<ParticleSystem>())
            {
                ParticleSystem import_child = to_return.transform.GetChild(i).GetComponent<ParticleSystem>();
                particles.Add(import_child);

                particle_renderer = to_return.transform.GetChild(i).GetComponent<ParticleSystemRenderer>();
                particle_renderer.sortingFudge = particles.IndexOf(import_child) * -10;

                particle_renderer.sharedMaterial = new Material(import_child.GetComponent<Renderer>().sharedMaterial);
                materials.Add(particle_renderer.sharedMaterial);
            }
        }

            to_return.Play(true);

        return to_return;
    }
}
