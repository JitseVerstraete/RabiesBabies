using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BabyState
{
    Neutral,
    RabidWarning,
    Rabid,
    NeedMet,
    Fighting
}

public enum BabyNeed
{
    Hungry,
    Bored,
    Diaper
}

public class BabyBehavior : MonoBehaviour
{
    [SerializeField] private float _gracePeriod = 8f;
    [SerializeField] private float _rabidWarningTime = 3f;
    [SerializeField] private float _needMetDuration = 10f;
    [SerializeField] private float _fightRadius = 2f;
    [SerializeField] private float _fightMargin = 0.5f;
    [SerializeField] private LineRenderer _dangerRadiusLine = null;
    [SerializeField] private SphereCollider _dangerRadiusCollider = null;

    [SerializeField] private GameObject _needIconParent;
    [SerializeField] private Image _needIconRef;
    [SerializeField] private GameObject _stationImageParent;
    [SerializeField] private Image _stationFillImage;
    [SerializeField] private Sprite _hungerIcon;
    [SerializeField] private Sprite _boredIcon;
    [SerializeField] private Sprite _diaperIcon;
    [SerializeField] private LineRenderer _pathLineRenderer;
    [SerializeField] private SkinnedMeshRenderer _skinnedRenderer;
    [SerializeField] private List<GameObject> _foamParticles;
    [SerializeField] private Animator _animator;

    private BabyState _currentState;
    private BabyNeed _currentNeed;
    private float _rabidTimer = 0f;

    private float _needMetTimer = 0f;

    private NeedStation _attachedStation = null;

    private BabyMovement _movement;

    private RenderTexture _renderTexture;

    private BabyBehavior _fightTarget = null;
    private GameObject _linkedFightCloud = null;
    [SerializeField] private float _fightRunSpeed = 6f;

    public RenderTexture RenderTexture => _renderTexture;

    private void Awake()
    {
        _movement = GetComponent<BabyMovement>();
    }

    public void Init(Color color)
    {
        ChangeState(BabyState.Neutral);
        SetNeed(GetRandomNeed());
        _rabidTimer = 0f;

        _stationFillImage.color = color;

        Debug.Log("Baby need " + _currentNeed.ToString());

        Physics.Raycast(transform.position + new Vector3(0f, 100f, 0f), Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Floor"));
        transform.position = hit.point;

        InitializeDangerCircle();
        _dangerRadiusLine.gameObject.SetActive(false);

        _pathLineRenderer.material.color = color;
        _skinnedRenderer.materials[1].color = color;

        // GameObject faceCamView = Instantiate(_faceCamPrefab, FindFirstObjectByType<VerticalLayoutGroup>().transform);
        // RawImage rawImage = faceCamView.GetComponentInChildren<RawImage>(); 
        _renderTexture = new RenderTexture(256, 256, 24);
        GetComponentInChildren<Camera>().targetTexture = _renderTexture;
        // rawImage.texture = renderTexture;

        _needIconParent.SetActive(true);
        _stationImageParent.SetActive(false);
    }

    private void Update()
    {
        switch (_currentState)
        {
            case BabyState.Neutral:
                _rabidTimer += Time.deltaTime;

                if (_gracePeriod - _rabidTimer <= _rabidWarningTime)
                {
                    ChangeState(BabyState.RabidWarning);
                }

                break;
            case BabyState.RabidWarning:
                _rabidTimer += Time.deltaTime;
                if (_rabidTimer >= _gracePeriod)
                {
                    ChangeState(BabyState.Rabid);
                }

                break;
            case BabyState.Rabid:
                //check radius for other babies
                //check if close to the required NeedStation

                float closestDistance = float.MaxValue;
                BabyBehavior closestBaby = null;
                List<BabyBehavior> allBabies = BabySpawner.instance.spawnedBabys;
                foreach (BabyBehavior baby in allBabies)
                {
                    if (baby == this) continue;
                    if (baby._currentState == BabyState.NeedMet) continue;

                    float distance = Vector3.Distance(transform.position, baby.transform.position);
                    if (distance < _fightRadius + _fightMargin && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestBaby = baby;
                    }
                }

                if (closestBaby != null)
                {
                    ChangeState(BabyState.Fighting);
                    GameManager.Instance.SetGameEndCondition();
                    StartCoroutine(BabyBrawlRoutine(closestBaby));
                }

                break;
            case BabyState.NeedMet:
                _needMetTimer += Time.deltaTime;

                _stationFillImage.fillAmount = _needMetTimer / _needMetDuration;

                if (_needMetTimer >= _needMetDuration)
                {
                    ChangeState(BabyState.Neutral);
                }

                break;
            case BabyState.Fighting:

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private IEnumerator PulsingWarning()
    {
        _dangerRadiusLine.gameObject.SetActive(true);

        float alpha = 0;
        float pulseDuration = 0.25f;
        float pulseBreak = 0.1f;
        float alphaPerSecond = 1f / pulseDuration;

        Material material = new Material(_dangerRadiusLine.material);
        _dangerRadiusLine.material = material;

        bool reverse = false;
        while (_currentState != BabyState.Rabid)
        {
            if (alpha > 1)
            {
                reverse = true;
            }
            else if (alpha < 0)
            {
                reverse = false;
                yield return new WaitForSeconds(pulseBreak);
            }

            alpha += alphaPerSecond * Time.deltaTime * (reverse ? -1 : 1);
            material.color = new Color(material.color.r, material.color.g, material.color.b, alpha);

            yield return null;
        }

        material.color = new Color(material.color.r, material.color.g, material.color.b, 1);
    }

    private void ChangeState(BabyState newState)
    {
        if (_currentState == newState)
            return;

        //exit from state stuff
        switch (_currentState)
        {
            case BabyState.Neutral:
                break;
            case BabyState.RabidWarning:
                break;
            case BabyState.Rabid:
                // disable foams and reset blendshape
                _foamParticles.ForEach(go => go.SetActive(false));
                _skinnedRenderer.SetBlendShapeWeight(0, 0);
                _skinnedRenderer.materials[3].SetColor("_Emission", Color.white);
                break;
            case BabyState.NeedMet:
                _animator.SetTrigger("crawl");
                transform.rotation = Quaternion.identity;
                _attachedStation.RemoveBaby(this);
                _attachedStation = null;
                _movement.ChangeState(BabyMovement.MovementState.FREE);
                _needIconParent.SetActive(true);
                _stationImageParent.SetActive(false);

                SetNeed(GetRandomNeed(_currentNeed));
                break;
            case BabyState.Fighting:
                SoundManager.Instance.PlaySound("fight");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }


        //go to state stuff
        switch (newState)
        {
            case BabyState.Neutral:
                Debug.Log("changed to neutral!");
                break;
            case BabyState.RabidWarning:
                StartCoroutine(PulsingWarning());
                break;
            case BabyState.Rabid:
                Debug.Log("changed to rabid!");
                _dangerRadiusLine.gameObject.SetActive(true);
                _foamParticles.ForEach(go => go.SetActive(true));
                _skinnedRenderer.SetBlendShapeWeight(0, 100);
                _skinnedRenderer.materials[3].SetColor("_Emission", Color.red);
                SoundManager.Instance.PlaySound("baby_growl_short");
                break;
            case BabyState.NeedMet:
                Debug.Log("changed to needs met!");
                _animator.SetTrigger("sit");
                _movement.ChangeState(BabyMovement.MovementState.NONE);
                _needIconParent.SetActive(false);
                _dangerRadiusLine.gameObject.SetActive(false);
                _stationImageParent.SetActive(true);
                _rabidTimer = 0f;
                _needMetTimer = 0f;
                break;
            case BabyState.Fighting:
                SoundManager.Instance.PlaySound("fight");
                break;
        }

        _currentState = newState;
    }

    private void SetNeed(BabyNeed need)
    {
        _currentNeed = need;
        switch (_currentNeed)
        {
            case BabyNeed.Hungry:
                _needIconRef.sprite = _hungerIcon;
                break;
            case BabyNeed.Bored:
                _needIconRef.sprite = _boredIcon;
                break;
            case BabyNeed.Diaper:
                _needIconRef.sprite = _diaperIcon;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private BabyNeed GetRandomNeed(BabyNeed exclude)
    {
        BabyNeed newNeed;
        do
        {
            newNeed = GetRandomNeed();
        } while (exclude == newNeed);

        return newNeed;
    }

    private BabyNeed GetRandomNeed()
    {
        var enumValues = Enum.GetValues(typeof(BabyNeed));
        BabyNeed newNeed = (BabyNeed)enumValues.GetValue(UnityEngine.Random.Range(0, enumValues.Length));

        return newNeed;
    }

    private void InitializeDangerCircle()
    {
        _dangerRadiusCollider.radius = _fightRadius;

        List<Vector3> positions = new List<Vector3>();
        int nrPoints = 20;
        for (int i = 0; i < nrPoints; i++)
        {
            float angle = ((i + 1f) / nrPoints) * 2f * Mathf.PI;
            positions.Add(new Vector3(_fightRadius * Mathf.Cos(angle), 0f, _fightRadius * Mathf.Sin(angle)));
        }

        _dangerRadiusLine.positionCount = nrPoints;
        _dangerRadiusLine.SetPositions(positions.ToArray());
    }


    private void OnTriggerStay(Collider other)
    {
        GameObject otherObject = other.attachedRigidbody?.gameObject;
        if (otherObject && otherObject.CompareTag("NeedStation") && _currentState != BabyState.NeedMet)
        {
            NeedStation collidedStation = otherObject.GetComponent<NeedStation>();
            if (collidedStation.need == _currentNeed && collidedStation.hasRoom)
            {
                collidedStation.AddBaby(this);
                ChangeState(BabyState.NeedMet);
                _attachedStation = collidedStation;
            }
        }
    }

    private IEnumerator BabyBrawlRoutine(BabyBehavior otherBaby)
    {
        yield return null;
        //go towards eachother
        _movement.ChangeState(BabyMovement.MovementState.NONE);
        if (_fightTarget == null)
        {
            _fightTarget = otherBaby;
        }

        if (otherBaby._fightTarget == null)
        {
            otherBaby._fightTarget = this;
        }

        float distanceToTarget = 0;
        do
        {
            distanceToTarget = Vector3.Distance(transform.position, _fightTarget.transform.position);

            //also rotate towards target
            transform.rotation = Quaternion.LookRotation(_fightTarget.transform.position - transform.position);

            Vector3 moveDir = (_fightTarget.transform.position - transform.position).normalized * Mathf.Clamp(_fightRunSpeed * Time.deltaTime, 0f, distanceToTarget);
            transform.position += moveDir;

            yield return null;
        } while (distanceToTarget > 0.1f);

        _dangerRadiusCollider.gameObject.SetActive(false);

        _fightTarget._movement.ChangeState(BabyMovement.MovementState.NONE);

        if (_fightTarget._linkedFightCloud != null)
        {
            _linkedFightCloud = _fightTarget._linkedFightCloud;
        }
        else if (_linkedFightCloud == null)
        {
            GameObject fightCloudPrefab = Resources.Load<GameObject>("FightCloud");
            GameObject spawnedCloud = Instantiate(fightCloudPrefab, transform.position, transform.rotation);
            _linkedFightCloud = spawnedCloud;
            Debug.Log("spawning new fight cloud!");
        }
    }

    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return ((1 << obj.layer) & layerMask) != 0;
    }
}