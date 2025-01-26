using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
            int slot = firstFreeSlot;
            Transform targetTransform = _babyHoldPoints[slot];
            _heldBabies[slot] = baby;
            baby.transform.position = targetTransform.position;
            baby.transform.rotation = targetTransform.rotation;
            PlayNeedSound();
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
            baby.transform.position = hit.point;
        }
    }

    private void PlayNeedSound()
    {
        switch (_fulfillsNeed)
        {
            case BabyNeed.Bored:
                SoundManager.Instance.PlaySound("playing_sand");
                break;
            case BabyNeed.Diaper:
                SoundManager.Instance.PlaySound("pooping");
                break;
            case BabyNeed.Hungry:
                SoundManager.Instance.PlaySound("eating");
                break;
            }
    }
}