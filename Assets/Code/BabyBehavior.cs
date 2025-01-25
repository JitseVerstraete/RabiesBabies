using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BabyState
{
    Neutral,
    Rabid,
    NeedMet
}

public enum BabyNeed
{
    Hungry,
    Bored,
    Diaper
}

public class BabyBehavior : MonoBehaviour
{
    [SerializeField] private float _gracePeriod = 5f;
    [SerializeField] private float _needMetDuration = 10f;
    [SerializeField] private float _fightRadius = 2f;
    [SerializeField] private LineRenderer _dangerRadiusLine = null;
    [SerializeField] private SphereCollider _dangerRadiusCollider = null;

    [Space(5)] 
    [SerializeField] private GameObject _needIconParent;
    [SerializeField] private Image _needIconRef;
    [SerializeField] private Sprite _hungerIcon;
    [SerializeField] private Sprite _boredIcon;
    [SerializeField] private Sprite _diaperIcon;
    
    private BabyState _currentState;
    private BabyNeed _currentNeed;
    private float _rabidTimer = 0f;

    private float _needMetTimer = 0f;

    private NeedStation _attachedStation = null;

    private BabyMovement _movement;

    private void Awake()
    {
        _movement = GetComponent<BabyMovement>();
    }

    public void Init()
    {
        ChangeState(BabyState.Neutral);
        SetNeed(GetRandomNeed());
        _rabidTimer = 0f;

        Debug.Log("Baby need " + _currentNeed.ToString());

        Physics.Raycast(transform.position + new Vector3(0f, 100f, 0f), Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Floor"));
        transform.position = hit.point;

        InitializeDangerCircle();
        _dangerRadiusLine.gameObject.SetActive(false);

        _dangerRadiusCollider.radius = _fightRadius;
    }

    private void Update()
    {
        switch (_currentState)
        {
            case BabyState.Neutral:
                _rabidTimer += Time.deltaTime;
                if (_rabidTimer >= _gracePeriod)
                {
                    ChangeState(BabyState.Rabid);
                }

                break;
            case BabyState.Rabid:
                //check radius for other babies
                //check if close to the required NeedStation
                break;
            case BabyState.NeedMet:
                _needMetTimer += Time.deltaTime;
                transform.rotation = Quaternion.Euler(0f, 360f * _needMetTimer / _needMetDuration, 0f);
                
                if (_needMetTimer >= _needMetDuration)
                {
                    ChangeState(BabyState.Neutral);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
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
            case BabyState.Rabid:
                break;
            case BabyState.NeedMet:
                transform.position = _attachedStation.babyDropPoint.position;
                transform.rotation = Quaternion.identity;
                _attachedStation = null;
                _movement.ChangeState(BabyMovement.MovementState.FREE);
                _needIconParent.SetActive(true);
                SetNeed(GetRandomNeed(_currentNeed));
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
            case BabyState.Rabid:
                Debug.Log("changed to rabid!");
                _dangerRadiusLine.gameObject.SetActive(true);
                break;
            case BabyState.NeedMet:
                Debug.Log("changed to needs met!");
                _movement.ChangeState(BabyMovement.MovementState.NONE);
                _needIconParent.SetActive(false);
                _rabidTimer = 0f;
                _needMetTimer = 0f;
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


    private void OnTriggerEnter(Collider other)
    {
        GameObject otherObject = other .attachedRigidbody?.gameObject;
        if (otherObject &&  otherObject.CompareTag("NeedStation"))
        {
            NeedStation collidedStation = otherObject.GetComponent<NeedStation>();
            if (collidedStation.need == _currentNeed)
            {
                ChangeState(BabyState.NeedMet);
                _attachedStation = collidedStation;
                transform.position = collidedStation.babyHoldPoint.position;
            }
        }
        
        if (IsInLayerMask(other.gameObject, LayerMask.GetMask("DangerRadius")))
        {
            Debug.Log("entered other baby fight circle");
        }
    }
    
    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return ((1 << obj.layer) & layerMask) != 0;
    }
    
}