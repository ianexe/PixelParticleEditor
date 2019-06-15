using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PaletteSaveWindow : PopupWindowContent
{
    string palette_name;
    Material palette;
    List<Material> palette_list;

    public void SetPalette(Material mat)
    {
        palette = mat;
    }

    public void SetList(List<Material> mat)
    {
        palette_list = mat;
    }

    public override Vector2 GetWindowSize()
    {
        return new Vector2(150, 50);
    }

    public override void OnGUI(Rect rect)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Name: ");
        palette_name = EditorGUILayout.TextField(palette_name);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Save Palette"))
        {
            OnClickSavePalette();
            editorWindow.Close();
        }
    }

    void OnClickSavePalette()
    {
        if (string.IsNullOrEmpty(palette_name))
        {
            EditorUtility.DisplayDialog("Unable to save palette", "Please specify a valid name.", "Close");
            return;
        }

        else
        {
            SaveMaterial();
        }
    }

    void SaveMaterial()
    {
        Material to_export = new Material(palette);
        AssetDatabase.CreateAsset(to_export, "Assets/Pixel Particle Editor/Materials/Palettes/" + palette_name + ".mat");

        palette_list.Add(to_export);
    }
}
