using UnityEngine;
using UnityEditor;

public class SC_BillboardMageWindow : EditorWindow
{
    static bool settingsChanged = false;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Billboard Mage 1.0/Settings")]
    static void Init()
    {
        SC_BillboardMage.LoadSettings(false);
        settingsChanged = SC_BillboardMage.CombineSettings() != SC_BillboardMage.defaultSettings;

        // Get existing open window or if none, make a new one:
        SC_BillboardMageWindow window = (SC_BillboardMageWindow)EditorWindow.GetWindowWithRect(typeof(SC_BillboardMageWindow), new Rect(0, 0, 640, 320), false, "Billboard Mage 1.0 Settings");
        window.Show();
    }

    void OnGUI()
    {
        //GUILayout.Label("Billboard Mage 1.0 Settings", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUI.BeginChangeCheck();

        SC_BillboardMage.disableRealtimeLightingWhenCapture = EditorGUILayout.Toggle(new GUIContent("Omit Lighting:", "If this enabled, all the real-time lights and ambient lighting will be discarded when capturing the billboard images, enable this if you plan to share billboards between different scenes with different lighting."), SC_BillboardMage.disableRealtimeLightingWhenCapture);

        SC_BillboardMage.outputImageMultiplier = (SC_BillboardMage.CaptureResolution)EditorGUILayout.EnumPopup(new GUIContent("Output Resolution (px):", "The size of the captured .png file containing samples, higher resolution will take longer to compute and will take more disk space"), SC_BillboardMage.outputImageMultiplier);

        SC_BillboardMage.samplesPerRow = EditorGUILayout.IntField(new GUIContent("Samples Per Row:", "The output image is divided in grid, this value controls the number of columns per row, each column containing a sample image."), SC_BillboardMage.samplesPerRow);

        SC_BillboardMage.totalRows = EditorGUILayout.IntField(new GUIContent("Total Rows:", "The output image is divided in grid, this value controls the number of rows, each row containing a number of columns controlled by 'Samples Per Row'."), SC_BillboardMage.totalRows);

        SC_BillboardMage.samplePadding = EditorGUILayout.IntField(new GUIContent("Padding (px):", "A padding value for each sample in the grid."), SC_BillboardMage.samplePadding);

        SC_BillboardMage.sampleIsolationLayer = EditorGUILayout.LayerField(new GUIContent("Sample Isolation Layer:", "This is the layer that will be temporarily assigned to the selected object and a sample camera, if some unselected objects appear in the final texture, try picking a different layer that is not in use by any objects in the Scene"), SC_BillboardMage.sampleIsolationLayer);

        SC_BillboardMage.allowMultiSelect = EditorGUILayout.Toggle(new GUIContent("Allow Multi-Select:", "If this is enabled, a separate Billboard Asset will be generated for each selected object (NOTE: If the objects are the same, it's better to generate Billboard Asset for one object then duplicate for the other object to avoid generating unnecessary assets). If you need to group multiple objects, move them inside the same object then generate billboard for that object."), SC_BillboardMage.allowMultiSelect);

        SC_BillboardMage.enableTransparencyFix = EditorGUILayout.Toggle(new GUIContent("Transparency Fix:", "Fix black edges by extending pixel color to a nearby transparent pixels."), SC_BillboardMage.enableTransparencyFix);

        if (EditorGUI.EndChangeCheck())
        {
            Debug.Log("Text input was changed!");
            settingsChanged = SC_BillboardMage.CombineSettings() != SC_BillboardMage.defaultSettings;
        }

        if (settingsChanged)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reset To Default"))
            {
                SC_BillboardMage.LoadSettings(true);
                settingsChanged = false;
            }
            GUILayout.EndHorizontal();
        }
    }

    void OnDestroy()
    {
        SC_BillboardMage.SaveSettings();
    }
}
