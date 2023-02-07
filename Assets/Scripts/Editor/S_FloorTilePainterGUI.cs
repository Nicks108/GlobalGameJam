using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[CustomEditor(typeof(S_FloorTilePainter),true)]
public class S_FloorTilePainterGUI : S_ObjectPainterGUI
{
    private S_FloorTilePainter myTarget;
    public void Awake()
    {
        myTarget = (S_FloorTilePainter) target;
    }
    public override void OnDragPaint(Vector3 mouseCentrePointInWorldSpaceVector3)
    {
        Bounds meshBoundsCopy;
        {
            Bounds meshBounds = myTarget.FloorTiles[0].GetComponent<MeshFilter>().sharedMesh.bounds;
            meshBoundsCopy = new Bounds(meshBounds.center, meshBounds.size);
        }

        //meshBoundsCopy.center += meshBoundsCopy.size;

        //Debug.Log("Bounds: "+ meshBoundsCopy.center);

        //if(meshBoundsCopy.Contains(mouseCentrePointInWorldSpaceVector3) )
            //Debug.Log(">>>>>>>>>>>>>>mouse in box");

        //nextObjectIndex++;
        //nextObjectIndex %= myTarget.FloorTiles.Count;

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;

        bool canPlaceTile = false;

        if (Physics.Raycast(ray, out hit))
        {
            canPlaceTile = true;
            if (hit.transform.tag == "Floor")
            {
                //Debug.Log("object under mouse! " + hit.transform.name);
                canPlaceTile = false;
            }
        }
        else
        {
            canPlaceTile = true;
        }

        if (canPlaceTile)
        {
            Bounds NextMeshBounds = GetNextMeshBoundsinGrid(mouseCentrePointInWorldSpaceVector3, meshBoundsCopy);


            GameObject FloorTile = GameObject.Instantiate( myTarget.FloorTiles[Random.Range(0, myTarget.FloorTiles.Count)]);
            FloorTile.transform.position = NextMeshBounds.center - (NextMeshBounds.size/2);
            FloorTile.transform.SetParent((myTarget as S_FloorTilePainter).transform);
            Event.current.Use();
        }


    }

    private static Bounds GetNextMeshBoundsinGrid(Vector3 mouseCentrePointInWorldSpaceVector3, Bounds meshBoundsCopy)
    {
        Bounds NextMeshBounds = new Bounds(meshBoundsCopy.center, meshBoundsCopy.size);

        Vector3 ceillingMouseDistFromCenter = new Vector3();
        ceillingMouseDistFromCenter.x = Mathf.Ceil(mouseCentrePointInWorldSpaceVector3.x);
        ceillingMouseDistFromCenter.z = Mathf.Ceil(mouseCentrePointInWorldSpaceVector3.z);

        Vector3 sizeDive = new Vector3();
        sizeDive.x = Mathf.Ceil(ceillingMouseDistFromCenter.x / NextMeshBounds.size.x) * NextMeshBounds.size.x;
        sizeDive.y = Mathf.Ceil(ceillingMouseDistFromCenter.y / NextMeshBounds.size.y) * NextMeshBounds.size.y;
        sizeDive.z = Mathf.Ceil(ceillingMouseDistFromCenter.z / NextMeshBounds.size.z) * NextMeshBounds.size.z;

        NextMeshBounds.center += sizeDive - meshBoundsCopy.size;
        return NextMeshBounds;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //SerializedObject so = new SerializedObject(target);
        //SerializedProperty list = so.FindProperty("FloorTiles");
        //EditorGUILayout.PropertyField(list, true);
        //so.ApplyModifiedProperties();


        SerializedProperty list = serializedObject.FindProperty("FloorTiles");
        EditorGUILayout.PropertyField(list, true);
        serializedObject.ApplyModifiedProperties();
    }

    public override void PaintMyHandles(Vector3 mouseCentrePointInWorldSpaceVector3)
    {
        base.PaintMyHandles(mouseCentrePointInWorldSpaceVector3);


        Bounds meshBoundsCopy;
        {
            Bounds meshBounds = myTarget.FloorTiles[0].GetComponent<MeshFilter>().sharedMesh.bounds;
            meshBoundsCopy = new Bounds(meshBounds.center, meshBounds.size);
        }

        Bounds NextMeshBounds = GetNextMeshBoundsinGrid(mouseCentrePointInWorldSpaceVector3, meshBoundsCopy);

        Handles.DrawWireCube(NextMeshBounds.center, NextMeshBounds.size);

    }

}
