using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[System.Serializable]
public class S_ObjectPainter : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> objectsToPaint = new List<GameObject>();
}
