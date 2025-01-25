using System;
using System.Collections.Generic;
using UnityEngine;

public class NeedStation : MonoBehaviour
{
    [SerializeField] private BabyNeed _fulfillsNeed;
    [SerializeField] private List<Transform> _babyHoldPoints;
    private List<BabyBehavior> _heldBabies = new List<BabyBehavior>();
    [SerializeField] private Transform _babyDropPoint;

    private Transform babyDropPoint
    {
        get => _babyDropPoint;
    }

    public BabyNeed need
    {
        get => _fulfillsNeed;
    }

    public bool hasRoom
    {
        get
        {
            for (int i = 0; i < _heldBabies.Count; i++)
            {
                if (_heldBabies[i] == null)
                {
                    return true;
                }
            }

            return false;
        }
    }

    private int firstFreeSlot
    {
        get
        {
            for (int i = 0; i < _babyHoldPoints.Count; i++)
            {
                if (_heldBabies[i] == null)
                {
                    return i;
                }
            }

            return -1;
        }
    }

    private void Awake()
    {
        _heldBabies = new List<BabyBehavior>();
        for (int i = 0; i < _babyHoldPoints.Count; i++)
        {
            _heldBabies.Add(null);
        }
    }


    public void AddBaby(BabyBehavior baby)
    {
        if (hasRoom)
        {
            _heldBabies[firstFreeSlot] = baby;
            baby.transform.position = _babyHoldPoints[firstFreeSlot].position;
            baby.transform.rotation = _babyHoldPoints[firstFreeSlot].rotation;
        }
    }

    public void RemoveBaby(BabyBehavior baby)
    {
        if (_heldBabies.Contains(baby))
        {
            _heldBabies[_heldBabies.FindIndex(b => b == baby)] = null;
            baby.transform.position = babyDropPoint.position;
            baby.transform.rotation = babyDropPoint.rotation;
            
            Physics.Raycast(transform.position + new Vector3(0f, 100f, 0f), Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Floor"));
            transform.position = hit.point;
        }
    }       
}