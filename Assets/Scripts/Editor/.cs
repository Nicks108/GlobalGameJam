using UnityEngine;
using UnityEditor;

public class  : EditorWindow
{
    [MenuItem("Tools/NTTools/")]
    private static void Init()
    {
         window = () EditorWindow.GetWindow(typeof());
        window.Show();
    }

    private void OnGUI()
    {
    }
}
