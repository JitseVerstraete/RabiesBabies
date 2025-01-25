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
    
    private List<GameObject> _spawnedBabys = new List<GameObject>();

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
        while (_spawnedBabys.Count < maxObjects)
        {
            yield return new WaitForSeconds(spawnInterval);
            if (_spawnedBabys.Count < maxObjects)
            {
                SpawnAtRandomPoint();
            }
        }
    }

    private void SpawnAtRandomPoint()
    {
        int randomIndex = Random.Range(0, spawnPoints.Count);
        Transform spawnPoint = spawnPoints[randomIndex];
        
        GameObject spawnedObject = Instantiate(_objectToSpawn, spawnPoint.position, spawnPoint.rotation);
        BabyBehavior behavior = spawnedObject.GetComponent<BabyBehavior>();
        behavior.Init();
        _spawnedBabys.Add(spawnedObject);
    }

    // Optional: Reset spawn count for reusing the spawner
    public void ResetSpawner()
    {
        _spawnedBabys.Clear();
    }
}