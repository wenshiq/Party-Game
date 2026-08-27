using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class EnemyEvents
{
    public UnityEvent OnPlayerSpotted;
    public UnityEvent OnPlayerScaped;
    public UnityEvent OnPlayerContact;
    public UnityEvent OnDamage;
    public UnityEvent OnDie;
    public UnityEvent OnRevive;
}
