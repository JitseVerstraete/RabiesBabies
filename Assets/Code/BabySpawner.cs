using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject _objectToSpawn;        
    [SerializeField] private List<Transform> spawnPoints;      
    [SerializeField] private float spawnInterval = 2.0f;       
    [SerializeField] private int maxObjects = 10;              
    [SerializeField] private bool loopSpawning = true;         

    private int currentObjectCount = 0;                        

    private void Start()
    {
        if (spawnPoints.Count > 0 && _objectToSpawn != null)
        {
            StartCoroutine(SpawnObjects());
        }
        else
        {
            Debug.LogError("Object to spawn or spawn points are not set!");
        }
    }

    private IEnumerator SpawnObjects()
    {
        while (loopSpawning || currentObjectCount < maxObjects)
        {
            if (currentObjectCount < maxObjects)
            {
                SpawnAtRandomPoint();
                currentObjectCount++;
            }

            // Wait for the interval before spawning the next object
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnAtRandomPoint()
    {
        int randomIndex = Random.Range(0, spawnPoints.Count);
        Transform spawnPoint = spawnPoints[randomIndex];
        
        GameObject spawnedObject = Instantiate(_objectToSpawn, spawnPoint.position, spawnPoint.rotation);
        BabyBehavior behavior = spawnedObject.GetComponent<BabyBehavior>();
        behavior.Init();
    }

    // Optional: Reset spawn count for reusing the spawner
    public void ResetSpawner()
    {
        currentObjectCount = 0;
    }
}