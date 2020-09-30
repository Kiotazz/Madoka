using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QuickToolBase : MonoBehaviour
{
    [CustomLabel("激活后立即开始工作")]
    public bool bWorkOnStart = true;

    public bool IsWorking { get; protected set; } = false;

    private void Start()
    {
        if (bWorkOnStart) StartWork();
        OnStart();
    }

    public void StartWork() { IsWorking = true; OnStartWork(); }
    public void StopWork() { IsWorking = false; OnStopWork(); }

    protected virtual void OnStart() { }
    protected virtual void OnStartWork() { }
    protected virtual void OnStopWork() { }
}
