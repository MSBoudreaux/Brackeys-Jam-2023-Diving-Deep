using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public enum PickupBoost
    {
        Health,
        MaxHealth,
        Damage,
        Range,
        MiningSpeed,
        Light,
        Score,
        AngyMode
    }

    public PickupBoost myStat;
    public int value;
}