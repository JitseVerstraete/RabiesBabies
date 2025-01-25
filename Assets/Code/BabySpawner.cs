using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BabySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject _objectToSpawn;        
    [SerializeField] private List<Transform> spawnPoints;      
    [SerializeField] private float spawnInterval = 2.0f;       
    [SerializeField] private int maxObjects = 10;
    [SerializeField] private List<Color> colors;              
    
    private bool _canSpawn = false;
    
    private List<GameObject> _spawnedBabys = new List<GameObject>();
    private int _babyCounter = 0;

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

    public void SetCanSpawn(bool canSpawn)
    {
        _canSpawn = canSpawn;

        var movementState = canSpawn ? BabyMovement.MovementState.FREE : BabyMovement.MovementState.NONE;
        Time.timeScale = canSpawn ? 1.0f : 0.0f;
        
        foreach (var spawnedBaby in _spawnedBabys.Where(spawnedBaby => spawnedBaby))
        {
            spawnedBaby.GetComponent<BabyMovement>().ChangeState(movementState);
        }
    }
    
    private IEnumerator SpawnObjects()
    {
        while (_spawnedBabys.Count < maxObjects)
        {
            yield return new WaitForSeconds(spawnInterval);
            if (_spawnedBabys.Count < maxObjects && _canSpawn)
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
        behavior.Init(colors[_babyCounter++ % colors.Count]);
        _spawnedBabys.Add(spawnedObject);
    }

    public void ResetSpawner()
    {
        _canSpawn = false;
        // destroy all objects in list that are not null
        foreach (var spawnedBaby in _spawnedBabys.Where(spawnedBaby => spawnedBaby))
        {
            Destroy(spawnedBaby);
        }

        _spawnedBabys.Clear();
    }
}