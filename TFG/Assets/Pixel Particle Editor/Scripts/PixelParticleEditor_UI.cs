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
    List<Material> materials;

    RenderTexture renderTexture;
    RenderTexture renderTexture2;

    Texture2D texture_test;
    float slider_test = 0.0f;

    int pixelation = 100;

    ParticleSystem selected_particle;
    float render_time = 5.0f;
    int render_frames = 8;
    int current_frame = 0;
    bool wait_one_frame = false;
    bool render_active = false;
    List<Texture2D> rendered_textures;
    string render_path = "Export";

    [MenuItem("Window/Pixel Particle Editor")]
    /*
    public static void ShowWindow()
    {
        GetWindow<PixelParticleEditor_UI>("Pixel Particle Editor");
    }
    */

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
        particles = new List<ParticleSystem>();
        materials = new List<Material>();
        CreateParticle();

        //Camera Init
        camera = Instantiate(camera, new Vector3(-10000,0,0), Quaternion.identity);
        camera.gameObject.hideFlags = HideFlags.HideInHierarchy;

        //Rendered Texture List Init
        rendered_textures = new List<Texture2D>();
    }

    public void OnEnable()
    {
        SelectParticle(particles[0]);
    }

    public void OnDestroy()
    {
        if (particles[0])
            DestroyImmediate(particles[0].gameObject);

        particles.Clear();

        materials.Clear();

        if (camera)
            DestroyImmediate(camera.gameObject);
    }

    public void Update()
    {
        if (!camera || !particles[0])
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

    void DrawOptions()
    {
        GUILayout.Label("Functions", EditorStyles.boldLabel);
        GUILayout.BeginVertical();

        DrawOptionButton(OPTIONS.PARTICLES, "Particles");
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
            case OPTIONS.FX:
                DrawPropertiesFX();
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
        particle_time = GUILayout.HorizontalSlider(particle_time, 0.0f, 20.0f);
        main.startLifetime = particle_time;

        GUILayout.Label("Particle Speed", EditorStyles.label);
        float particle_speed = main.startSpeed.constant;
        particle_speed = GUILayout.HorizontalSlider(particle_speed, 0.0f, 20.0f);
        main.startSpeed = particle_speed;

        GUILayout.Label("Particle Size", EditorStyles.label);
        float particle_size = main.startSize.constant;
        particle_size = GUILayout.HorizontalSlider(particle_size, 0.0f, 20.0f);
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
        GUILayout.Label("Pixelation", EditorStyles.label);
        int prev_pixelation = pixelation;
        pixelation = EditorGUILayout.IntSlider(pixelation, 256, 8);
        if (pixelation != prev_pixelation)
        {
            renderTexture.Release();
            renderTexture = new RenderTexture(pixelation, pixelation, (int)RenderTextureFormat.ARGB32);
            renderTexture.filterMode = FilterMode.Point;
        }

    }

    void DrawPropertiesExport()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        if (GUILayout.Button("Export\n PNG", GUILayout.Width(60), GUILayout.Height(40)))
            SaveTexture();
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        if (GUILayout.Button("Export\n Sprite Sheet", GUILayout.Width(70), GUILayout.Height(40)))
        {
            render_active = true;
            wait_one_frame = false;
            current_frame = 0;
        }   
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        /*
        if (GUILayout.Button("Simulate", GUILayout.Width(60), GUILayout.Height(40)))
        {
            particles[0].Simulate(render_time);
            //string name = "frametest";
            //SaveTexture2(name);
        }
        */
        render_time = EditorGUILayout.FloatField("Render Time:", render_time);
        render_frames = EditorGUILayout.IntField("Frames to Render:", render_frames);
        //current_frame = EditorGUILayout.IntSlider("Current Frame:", current_frame, 0, render_frames);
        render_path = EditorGUILayout.TextField("File Name:", render_path);

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
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

        if (!AssetDatabase.IsValidFolder("Assets/Particle Render"))
        {
            AssetDatabase.CreateFolder("Assets", "Particle Render");
        }

        string path = Application.dataPath + "/Particle Render/"+ render_path + ".png";
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

        to_return.Play(true);

        return to_return;
    }
}
