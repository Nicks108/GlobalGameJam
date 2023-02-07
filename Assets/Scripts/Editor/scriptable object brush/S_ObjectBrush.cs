using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[CreateAssetMenu]
public class S_ObjectBrush : GridBrushBase
{
    public GameObject prefab1;
    public GameObject prefab2;

    public override void Paint(GridLayout grid, GameObject brushTarget, Vector3Int position)
    {
        bool evenCell = Mathf.Abs(position.y + position.x) % 2 > 0;

        GameObject toBeInatanciated = evenCell ? prefab1 : prefab2;

        if (toBeInatanciated != null)
        {
            GameObject newInstance = Instantiate(toBeInatanciated, grid.CellToWorld(position), Quaternion.identity);
            newInstance.transform.SetParent(brushTarget.transform);
        }
    }

}
