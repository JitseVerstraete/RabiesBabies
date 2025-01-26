using System.Collections;
using System.Collections.Generic;
using Code;
using UnityEngine;
using UnityEngine.UI;

public class BabySpawner : MonoBehaviour
{
    private static BabySpawner _instance;
    public static BabySpawner instance => _instance;

    [Header("Spawn Settings")] [SerializeField]
    private GameObject _objectToSpawn;

    [SerializeField] private GameObject _ambulance;
    [SerializeField] private Transform _ambulanceHiddenPos;
    [SerializeField] private Transform _ambulanceShownPos;
    [SerializeField] private Transform _launchOrigin;
    [SerializeField] private ParticleSystem _launchParticles;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private Transform startSpawnPoint;
    [SerializeField] private float spawnInterval = 2.0f;
    [SerializeField] private int maxObjects = 10;
    [SerializeField] private List<Color> colors;
    [SerializeField] private AnimationCurve _verticalCurve;
    [SerializeField] private AnimationCurve _horizontalCurve;
    [SerializeField] private float _animationDuration = 3f;
    [SerializeField] private float _animHeight = 15f;

    [SerializeField] private GameObject _landingIconRoot = null;
    [SerializeField] private Image _landingIcon = null;
    [SerializeField] private float _rotationSpeed = 30f;
    [SerializeField] private AnimationCurve _scaleCurve;
    [SerializeField] private float _minScale = 0f;
    [SerializeField] private float _maxScale = 1f;
    [SerializeField] private AnimationCurve _opacityCurve;

    private float _ambulanceMoveDuration = 1.5f;


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

        _ambulance.transform.position = _ambulanceHiddenPos.position;
        _landingIconRoot.SetActive(false);
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
        //roll up ambulance
        SoundManager.Instance.PlaySound("ambulance");
        float ambulanceMoveTimer = 0f;
        while (ambulanceMoveTimer < _ambulanceMoveDuration)
        {
            ambulanceMoveTimer += Time.deltaTime;

            _ambulance.transform.position = Vector3.Lerp(_ambulanceHiddenPos.position, _ambulanceShownPos.position, ambulanceMoveTimer / _ambulanceMoveDuration);
            yield return null;
        }
        SoundManager.Instance.StopSound("ambulance");

        SoundManager.Instance.PlaySound("cannon_shot");
        _launchParticles.Play();

        yield return new WaitForSeconds(0.2f);
        //spawn and shoot the baby 
        int randomIndex = Random.Range(0, spawnPoints.Count);
        Vector3 targetPoint = spawnPoints[randomIndex].position;

        GameObject spawnedObject = Instantiate(_objectToSpawn, _launchOrigin.position, _launchOrigin.rotation);
        BabyBehavior behavior = spawnedObject.GetComponent<BabyBehavior>();
        behavior.PreInit(colors[_babyCounter++ % colors.Count]);

        StartCoroutine(RollBackAmbulance());

        _landingIconRoot.transform.position = targetPoint;
        _landingIconRoot.SetActive(true);
        Color col = _landingIcon.color;
        col.a = 0;
        _landingIcon.color = col;
        _landingIconRoot.transform.localScale = Vector3.zero;
        
        float animTimer = 0f;
        while (animTimer < _animationDuration)
        {
            animTimer += Time.deltaTime;
            float vertical = _animHeight * Mathf.Lerp(_launchOrigin.position.y, targetPoint.y, animTimer / _animationDuration);
            Vector3 mainMovement = Vector3.Lerp(_launchOrigin.position, targetPoint, _horizontalCurve.Evaluate(animTimer / _animationDuration));
            spawnedObject.transform.position = mainMovement + new Vector3(0f, vertical, 0f);
            
            _landingIconRoot.transform.localScale = Vector3.Lerp(new Vector3(_minScale, _minScale, _minScale), new Vector3(_maxScale, _maxScale, _maxScale), _scaleCurve.Evaluate(animTimer / _animationDuration));
            Color currentColor = _landingIcon.color;
            float newAlpha = Mathf.Lerp(0, 1, _opacityCurve.Evaluate(animTimer / _animationDuration));
            Debug.Log(animTimer / _animationDuration);
            currentColor.a = newAlpha;
            _landingIcon.color = currentColor;
            yield return null;
        }
        
        //acutally Initialize it and put it functionally in the level
        SoundManager.Instance.PlaySound("fall_ground");
        behavior = spawnedObject.GetComponent<BabyBehavior>();
        behavior.Init();
        _spawnedBabys.Add(behavior);
        yield return new WaitForSeconds(0.3f);
        
        _landingIconRoot.SetActive(false);
        col = _landingIcon.color;
        col.a = 1;
        _landingIcon.color = col;
        _landingIconRoot.transform.localScale = Vector3.zero;


        FindFirstObjectByType<SecurityScreens>().AddBaby(behavior);
        yield return null;
    }

    public void SpawnDirect(Transform spawnPoint)
    {
        GameObject spawnedObject = Instantiate(_objectToSpawn, spawnPoint.position, spawnPoint.rotation);
        BabyBehavior behavior = spawnedObject.GetComponent<BabyBehavior>();
        behavior.PreInit(colors[_babyCounter++ % colors.Count]);
        behavior.Init();
        _spawnedBabys.Add(behavior);

        FindFirstObjectByType<SecurityScreens>().AddBaby(behavior);
    }


    private IEnumerator RollBackAmbulance()
    {
        yield return new WaitForSeconds(0.3f);

        //roll back ambulance
        float ambulanceMoveTimer = 0f;
        while (ambulanceMoveTimer < _ambulanceMoveDuration)
        {
            ambulanceMoveTimer += Time.deltaTime;

            _ambulance.transform.position = Vector3.Lerp(_ambulanceShownPos.position, _ambulanceHiddenPos.position, ambulanceMoveTimer / _ambulanceMoveDuration);
            yield return null;
        }
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