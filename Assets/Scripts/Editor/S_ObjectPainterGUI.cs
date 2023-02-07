using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Random = UnityEngine.Random;

[CustomEditor(typeof(S_ObjectPainter))]
public class S_ObjectPainterGUI : Editor
{
    private bool CanPaint = false;
    private float radius = 1;
    private int Density = 1; //used to add more objects. objects added every tick%Desity == 0

    //Brush type selection button values
    int BrushTypeSelected = 0;
    private const string Circle = "Circle";
    private const string Sphere = "Sphere";
    readonly List<string> BrushTypeButtonLabels = new List<string> { Circle, Sphere };

    private int updateCount = 0;

    public override void OnInspectorGUI()
    {
        S_ObjectPainter myTargt = (S_ObjectPainter)target;

        
        BrushTypeSelected = GUILayout.Toolbar(BrushTypeSelected, BrushTypeButtonLabels.ToArray());

        CanPaint = GUILayout.Toggle(CanPaint, "paint objects", "button");
        GUILayout.Label("Radius");
        radius = GUILayout.HorizontalSlider(radius, 0.5f, 10.0f);
        GUILayout.Space(10);

        Density = EditorGUILayout.IntSlider("Density",Density, 1, 10);
        GUILayout.Space(10);


        //SerializedObject so = new SerializedObject(target);
        //so.Update();
        SerializedProperty list = serializedObject.FindProperty("objectsToPaint");
        EditorGUILayout.PropertyField(list, true);
        serializedObject.ApplyModifiedProperties();
    }

    //[DrawGizmo(GizmoType.Selected | GizmoType.Active)]
    //private static void DrawPaintGizmo(S_ObjectPainter scr, GizmoType gizmoType)
    //{
    //    Vector3 worldPosition = scre(Event.current.mousePosition);

    //    Gizmos.DrawSphere(worldPosition, scr.radius);
    //    Debug.Log(worldPosition);

    //}

    
    public void OnSceneGUI()
    {
        if(!CanPaint)
            return;


        //https://stackoverflow.com/questions/54493121/unity-editorwindow-find-mouse-position-in-sceen-view
        //https://docs.unity3d.com/ScriptReference/Plane.html
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        float enter = 0.0f;

        if (plane.Raycast(ray, out enter))
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            Vector3 temp = ray.GetPoint(enter);
            Vector3 centreVector3 = new Vector3(temp.x, 0, temp.z);
            //Get the point that is clicked
            






            //if (Event.current.type == EventType.MouseDown
            //    && Event.current.button == 0)
            //{
            //    mouseMoving = true;
            //    Debug.Log("Mouse down");
            //}
            //else if (Event.current.type == EventType.MouseUp)
            //{
            //    brushMovmentDelta = 0;
            //    mouseMoving = false;
            //    Debug.Log("Mouse up");
            //}



            //draw Handle
           // Vector3 randpoint = new Vector3();

            //if (BrushTypeSelected == BrushTypeButtonLabels.IndexOf(Circle))
            //{
            //    Handles.DrawSolidDisc(centreVector3, Vector3.up, radius);
            //}
            //else if (BrushTypeSelected == BrushTypeButtonLabels.IndexOf(Sphere))
            //{
            //    Handles.SphereHandleCap(0,
            //        centreVector3,
            //        Quaternion.LookRotation(Vector3.forward),
            //        radius * 2, // *2 becouse sphere cap seems to be smaller that aactual raduis in editor (unexplaned Unity Querks YAY!)
            //        EventType.Repaint);
            //}
            //EditorUtility.SetDirty(target);

            PaintMyHandles(centreVector3);
            EditorUtility.SetDirty(target);

            if (Event.current.type == EventType.MouseDrag)
            {
               OnDragPaint(centreVector3);
            }
            
        }


        //https://answers.unity.com/questions/1290911/onscenegui-how-do-you-convert-mouseposition-in-wor.html
        //Vector3 screenPosition = Event.current.mousePosition;
        ////screenPosition.y = Camera.current.pixelHeight - screenPosition.y;
        ////Ray ray = Camera.current.ScreenPointToRay(screenPosition);
        //Ray ray = HandleUtility.GUIPointToWorldRay(screenPosition);
        //RaycastHit hit;
        //// use a different Physics.Raycast() override if necessary
        //if (Physics.Raycast(ray, out hit))
        //{
        //    // do stuff here using hit.point
        //    // tell the event system you consumed the click
        //    Handles.DrawSolidDisc(hit.point, Vector3.up, 1);
        //    Debug.Log("Mouse Pos" + hit.point);
        //    EditorUtility.SetDirty(target);
        //}

    }

    public virtual void PaintMyHandles(Vector3 mouseCentrePointInWorldSpaceVector3)
    {
        Color HandleColor = Color.red;
        HandleColor.a = 0.25f;
        Handles.color = HandleColor;
        if (BrushTypeSelected == BrushTypeButtonLabels.IndexOf(Circle))
        {
            Handles.DrawSolidDisc(mouseCentrePointInWorldSpaceVector3, Vector3.up, radius);
        }
        else if (BrushTypeSelected == BrushTypeButtonLabels.IndexOf(Sphere))
        {
            Handles.SphereHandleCap(0,
                mouseCentrePointInWorldSpaceVector3,
                Quaternion.LookRotation(Vector3.forward),
                radius * 2, // *2 becouse sphere cap seems to be smaller that aactual raduis in editor (unexplaned Unity Querks YAY!)
                EventType.Repaint);
        }
        //EditorUtility.SetDirty(target);
    }

    public virtual void OnDragPaint(Vector3 mouseCentrePointInWorldSpaceVector3)
    {
        //pick spawn position 
        Vector3 randpoint = new Vector3();

        if (BrushTypeSelected == BrushTypeButtonLabels.IndexOf(Circle))
        {
            Vector2 randpoint2D = Random.insideUnitCircle * radius;
            randpoint = new Vector3(randpoint2D.x, 0, randpoint2D.y) + mouseCentrePointInWorldSpaceVector3;
        }
        else if (BrushTypeSelected == BrushTypeButtonLabels.IndexOf(Sphere))
        {
            randpoint = Random.insideUnitSphere * radius + mouseCentrePointInWorldSpaceVector3;
            //randpoint = new Vector3(randpoint.x, randpoint.z, randpoint.y);
        }
        //end pick spawn position

        //Debug.Log("Mouse Pos" + mouseCentrePointInWorldSpaceVector3);
        

        //Debug.Log("randpoint:"+randpoint);
        //Debug.Log("mouseCentrePointInWorldSpaceVector3:" + mouseCentrePointInWorldSpaceVector3);
        //Debug.Log("mouseCentrePointInWorldSpaceVector3:" + mouseCentrePointInWorldSpaceVector3);

        updateCount++;

        int updateCountMod = updateCount % Density;
        if (updateCountMod == 0)
        {
            updateCount = 0;

            S_ObjectPainter myTarget = (target as S_ObjectPainter);
            GameObject NewGameObject = GameObject.Instantiate(myTarget.objectsToPaint[Random.Range(0, myTarget.objectsToPaint.Count)]); ;
            NewGameObject.transform.position = randpoint;

            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0);
            NewGameObject.transform.rotation = randomRotation;
            NewGameObject.transform.SetParent(myTarget.transform);
            Event.current.Use();
        }
    }
}
