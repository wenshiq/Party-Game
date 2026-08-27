using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[Serializable]
public class EntityStateManagerEvents 
{
    public UnityEvent onChange;

    public UnityEvent<Type> onEnter;

    public UnityEvent<Type> onExit;

}
