using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class S_FloorTilePainter : S_ObjectPainter
{
    [SerializeField]
    public List<GameObject> FloorTiles = new List<GameObject>();
}
