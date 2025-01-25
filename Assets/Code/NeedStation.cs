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
        }
    }

    public void RemoveBaby(BabyBehavior baby)
    {
        if (_heldBabies.Contains(baby))
        {
            _heldBabies[_heldBabies.FindIndex(b => b == baby)] = null;
            baby.transform.position = babyDropPoint.position;
        }
    }       
}