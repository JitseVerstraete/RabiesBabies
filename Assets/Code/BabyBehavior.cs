using System;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

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

    private BabyState _currentState;
    private BabyNeed _currentNeed;
    private float _rabidTimer = 0f;

    private float _needMetTimer = 0f;

    public void Init()
    {
        ChangeState(BabyState.Neutral);
        var enumValues = Enum.GetValues(typeof(BabyNeed));
        _currentNeed = (BabyNeed)enumValues.GetValue(UnityEngine.Random.Range(0, enumValues.Length));
        _rabidTimer = 0f;

        Debug.Log("Baby need " + _currentNeed.ToString());
        
        Physics.Raycast(transform.position + new Vector3(0f, 100f, 0f), Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Floor"));
        transform.position = hit.point;
    }

    private void Update()
    {
        CheckIfNeedMet();

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
                if (_needMetTimer >= _needMetDuration)
                {
                    ChangeState(BabyState.Neutral);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void CheckIfNeedMet()
    {
        //replace this logic with checking if the baby is near the correct station
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeState(BabyState.NeedMet);
        }
    }


    private void ChangeState(BabyState newState)
    {
        if (_currentState == newState)
            return;

        //change visuals here
        switch (newState)
        {
            case BabyState.Neutral:
                Debug.Log("changed to neutral!");
                break;
            case BabyState.Rabid:
                Debug.Log("changed to rabid!");
                break;
            case BabyState.NeedMet:
                Debug.Log("changed to needs met!");
                _rabidTimer = 0f;
                _needMetTimer = 0f;
                break;
        }

        _currentState = newState;
    }

    private void OnDrawGizmos()
    {
        if (_currentState == BabyState.Rabid)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _fightRadius);
        }
    }
}