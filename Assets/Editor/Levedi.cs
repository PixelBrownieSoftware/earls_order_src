using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Levedi : EditorWindow {

    [MenuItem("Level edit/Save and load")]
    public static void ShowWindow()
    {
        GetWindow(typeof(Levedi));
    }
    private void OnGUI()
    {
        string dir = "";
        if (GUILayout.Button("Load"))
        {
            dir = EditorUtility.OpenFilePanel("Open Json level file", "Assets/Levels/", "");
            if (dir == string.Empty)
                return;
            s_levelloader.load.LoadData(dir);
        }
        if (GUILayout.Button("Save"))
        { dir = EditorUtility.SaveFilePanel("Save Json level file", "Assets/Levels/", "Unnamed.txt", ".txt");
            s_levelloader.load.SaveData(dir);
        }
    }
    private void d()
    {

        string dir = EditorUtility.OpenFilePanel("Open Json level file", "Assets/Levels/", "");
    }

}
