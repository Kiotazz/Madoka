using UnityEngine;
using System.Collections;
using System;

public abstract class UIWindow : UILayer
{
    public bool isTopWindow { get { return UISystem.Instance.TopWindow == this; } }
    public bool isRootWindow { get { return UISystem.Instance.RootWindow == this; } }

    protected virtual void OnReshow() { }
    protected virtual void OnHide() { }

    public sealed override void Create()
    {
        base.Create();
        if (loadOp.startLoading)
        {
            if (isRootWindow)
                hide = false;
            return;
        }
        FinishLoading();
    }

    protected void Hide()
    {
        hide = true;
        OnHide();
    }

    public void Reshow()
    {
        hide = false;
        OnReshow();
    }

    protected sealed override void FinishLoading()
    {
        if (!isTopWindow)
            return;
        UISystem.Instance.DestroyLoadingBox();
        hide = false;
        if(!loaded)
        {
            if (!isRootWindow)
            {
                UISystem.Instance.SecondWindow.Hide();
            }
            loaded = true;
            OnCreated();
        }
    }
}
