using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PopulateField : MonoBehaviour
{
    public GameObject VegToSpawn;
    // Start is called before the first frame update
    void Start()
    {

        List<GameObject> spawnPoints = GetChildrenByTag(transform,"VegSpawnPoint");

        foreach (GameObject SpawnPoint in spawnPoints)
        {
            Instantiate(VegToSpawn, SpawnPoint.transform);
        }
    }

    List<GameObject> GetChildrenByTag(Transform incommingTransform, string Tag)
    {
        List<GameObject> ChildrenWithTag= new List<GameObject>();

        foreach (Transform Child in incommingTransform)
        {
            if(Child.tag == Tag)
                ChildrenWithTag.Add(Child.gameObject);
            if (incommingTransform.childCount > 0)
                ChildrenWithTag.AddRange(GetChildrenByTag(Child, Tag));
        }


        return ChildrenWithTag;
    }

}
