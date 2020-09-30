using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SearchEnemyBase : MonoBehaviour
{
    public InteractiveObj Master { get; protected set; }

    public void Init(InteractiveObj master)
    {
        Master = master;
        OnInit();
    }
    protected virtual void OnInit() { }

    public virtual void DoUpdate(float deltaTime) { }
    public abstract bool IsTargetInSearchArea(InteractiveObj target);
}
