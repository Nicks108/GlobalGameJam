using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


[System.Serializable]

public class RootSpawnEngine : MonoBehaviour
{
    public float barValue; // increase as you grab stuff

    public List<GameObject> pooledObjects;
    public GameObject objectToPool;
    public int amountToPool;

    public float spawnDelay;

    public float SpawnRadius = 3f;

    WaitForSeconds coroutineTimer = new WaitForSeconds(0.02f);
    public UIManager managerRef;



    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        pooledObjects = new List<GameObject>();
        PopulatePool();


        //InvokeRepeating("SpawnObjects", 5, spawnDelay);

        GameObject.Find("UI MANAGER").GetComponent<UIManager>().spawnEvent.AddListener( MassSpawnPlants);

    }

    private void PopulatePool()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            GameObject tmp = Instantiate(objectToPool);
            tmp.SetActive(false);
            tmp.name += i;
            pooledObjects.Add(tmp);
        }
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

    public bool SpawnObjects()
    {
        GameObject plant = GetPooledObject();
        if (plant != null)
        {
            Vector3 tempVector = RandomPointAroundTransform(transform, SpawnRadius);
            tempVector.y = tempVector.y + 2;
            plant.transform.position = tempVector;
            plant.transform.rotation = transform.rotation;
            plant.SetActive(true);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void MassSpawnPlants()
    {
        for (int i = 0; i < 10; i++)
        {
            SpawnObjects();
        }
        //StartCoroutine(PlantSpawner());
    }

    IEnumerator PlantSpawner()
    {
        while(SpawnObjects())
        {
            yield return coroutineTimer;
        }
    }

    private Vector3 RandomPointAroundTransform(Transform Origin, float radius)
    {
        Vector3 randomPos = Random.insideUnitSphere * radius;
        //randomPos += transform.position;
        randomPos.y = 1f;

        Vector3 direction = randomPos; // - transform.position;
        direction.Normalize();

        float dotProduct = Vector3.Dot(Origin.forward, direction);
        float dotProductAngle = Mathf.Acos(dotProduct / Origin.forward.magnitude * direction.magnitude);

        randomPos.x = Mathf.Cos(dotProductAngle) * radius + Origin.position.x;
        randomPos.z = Mathf.Sin(dotProductAngle * (Random.value > 0.5f ? 1f : -1f)) * radius + Origin.position.z;
        return randomPos;
    }
}
