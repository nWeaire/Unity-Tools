using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class GridEditor : EditorWindow {

    public bool Paint;
    public bool Erase;
    public Object GridToEdit;  
    // Add menu named "My Window" to the Window menu
    [MenuItem("Tools/Map Creator Tools/Grid Editor")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        GridEditor window = (GridEditor)EditorWindow.GetWindow(typeof(GridEditor));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Grid Selection", EditorStyles.boldLabel);
        GridToEdit = EditorGUILayout.ObjectField(GridToEdit, typeof(Object), true);
        if (GridToEdit)
        {
            GUILayout.Label("Tools", EditorStyles.boldLabel);
            Paint = EditorGUILayout.ToggleLeft("Paint", Paint);
            Erase = EditorGUILayout.ToggleLeft("Erase", Erase);
        }

        //groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        //myBool = EditorGUILayout.Toggle("Toggle", myBool);
        //myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
        //EditorGUILayout.EndToggleGroup();
    }
}
