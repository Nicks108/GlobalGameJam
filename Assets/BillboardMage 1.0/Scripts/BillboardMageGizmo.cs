#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BillboardMageGizmo : MonoBehaviour
{
    //Custom Billboard Renderer Gizmo for allowing selecting BillboardRenderer
    static Color selectedColorTint = new Color(179f/255f, 255f/255f, 234f/255f);
    void OnDrawGizmos()
    {
        var sceneView = SceneView.currentDrawingSceneView;
        if (sceneView != null && sceneView.drawGizmos)
        {
            Vector3 position = transform.position;

            Color tintColor = Selection.activeObject == gameObject ? selectedColorTint : Color.white;

            Gizmos.DrawIcon(position, "BillboardMage/BillboardMage_Gizmo.tiff", true, tintColor);
        }
    }
}
#endif