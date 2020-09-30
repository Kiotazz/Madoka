using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObjExt : MonoBehaviour
{
    public InteractiveObj Master { get; protected set; }

    public void Init(InteractiveObj obj)
    {
        Master = obj;
        OnInit(obj);
    }

    protected virtual void OnInit(InteractiveObj obj) { }

    public virtual void DoUpdate(float deltaTime) { }
}
