using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code;
using UnityEngine;

public class BabySpawner : MonoBehaviour
{
    private static BabySpawner _instance;
    public static BabySpawner instance => _instance;
    
    [Header("Spawn Settings")] [SerializeField]
    private GameObject _objectToSpawn;

    [SerializeField] private Transform _launchOrigin;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private float spawnInterval = 2.0f;
    [SerializeField] private int maxObjects = 10;
    [SerializeField] private List<Color> colors;
    
    private bool _canSpawn = false;

    private List<BabyBehavior> _spawnedBabys = new List<BabyBehavior>();
    public List<BabyBehavior> spawnedBabys => _spawnedBabys;
    private int _babyCounter = 0;

    private void Start()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }
        
        
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
        Time.timeScale = canSpawn ? 1.0f : 0.0f;
    }

    private IEnumerator SpawnObjects()
    {
        while (_spawnedBabys.Count < maxObjects)
        {
            yield return new WaitForSeconds(spawnInterval);
            if (_spawnedBabys.Count < maxObjects && _canSpawn)
            {
                SoundManager.Instance.SpeedUpMusic("backgroundMusic");
                StartCoroutine(SpawnAtRandomPoint());
            }
        }
    }

    private IEnumerator SpawnAtRandomPoint()
    {
        int randomIndex = Random.Range(0, spawnPoints.Count);
        Transform spawnPoint = spawnPoints[randomIndex];

        GameObject spawnedObject = Instantiate(_objectToSpawn, spawnPoint.position, spawnPoint.rotation);
        BabyBehavior behavior = spawnedObject.GetComponent<BabyBehavior>();
        behavior.Init(colors[_babyCounter++ % colors.Count]);
        _spawnedBabys.Add(behavior);

        FindFirstObjectByType<SecurityScreens>().AddBaby(behavior);
        
        yield return null;
    }

    public void ResetSpawner()
    {
        _canSpawn = false;
        // destroy all objects in list that are not null
        foreach (var spawnedBaby in _spawnedBabys)
        {
            Destroy(spawnedBaby.gameObject);
        }

        _spawnedBabys.Clear();
    }
}