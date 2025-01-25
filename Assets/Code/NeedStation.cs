using UnityEngine;

public class NeedStation : MonoBehaviour
{
   [SerializeField] private BabyNeed _fulfillsNeed;
   [SerializeField] private Transform _babyHoldPoint;
   [SerializeField] private Transform _babyDropPoint;
   public BabyNeed need { get => _fulfillsNeed; }
   public Transform babyHoldPoint { get => _babyHoldPoint; }
   public Transform babyDropPoint { get => _babyDropPoint; }
}
