using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class RootSpawnEngine : MonoBehaviour
{
    public float barValue; // increase as you grab stuff
    public static RootSpawnEngine sharedInstance;
    public List<GameObject> pooledObjects;
    public GameObject objectToPool;
    public int amountToPool;

    float spawnTick, runtimeVal;
    bool beginSpawning;
    private void Awake()
    {
        sharedInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        pooledObjects = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < amountToPool; i++)
        {
            tmp = Instantiate(objectToPool);
            tmp.SetActive(false);
            pooledObjects.Add(tmp);
        }
        runtimeVal = 0.05f;
        beginSpawning = false;
    }

    public GameObject GetPooledObject()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            if(!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }
        return null;
    }

    public void SpawnObjects()
    {
        Vector3 spawnBox = GOReferences.GOReferencesInstance.go_player.transform.localScale;
        Vector3 position = new Vector3(Random.Range(0.8f,1.0f) * spawnBox.x, Random.Range(0.8f, 1.0f) * spawnBox.y, Random.Range(0.8f, 1.0f) * spawnBox.z);
        position = GOReferences.GOReferencesInstance.go_player.transform.TransformPoint(position - spawnBox / 2);
        GameObject plant = GetPooledObject();
        if (plant != null)
        {
            plant.transform.position = position;
            plant.transform.rotation = GOReferences.GOReferencesInstance.go_player.transform.rotation;
            plant.SetActive(true);
        }
        else
        {
            beginSpawning = false;
        }
    }

    public void BeginSpawning()
    {
        beginSpawning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            beginSpawning = true;
        }
        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            beginSpawning = false;
        }
        if (beginSpawning)
        {
            if(Time.time > spawnTick)
            {
                spawnTick = Time.time + runtimeVal;
                SpawnObjects();
            }
        }
    }
}
