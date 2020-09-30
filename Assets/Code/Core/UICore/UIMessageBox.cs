using UnityEngine;
using System.Collections;
using System;

public class UIMessageBox : UILayer 
{
    public override Type type { get { return Type.MessageBox; } }
    public bool isTopMessageBox {  get { return UISystem.Instance.TopMessageBox == this; } }

    public override bool loaded { get { return true; } protected set { } }

	protected System.Action<int> m_pfunCallback;

    public sealed override void Create()
    {
        base.Create();
        Show();
    }

    public void Show()
    {
        if (isTopMessageBox)
        {
            hide = false;
            OnCreated();
        }
    }

    protected override void OnLoad(LoadOperation op)
    {        
    }
    protected sealed override void FinishLoading()
    {
    }
    public void SetCallback(System.Action<int> cb)
	{
		m_pfunCallback = cb;
	}
	
	public virtual void Cancel()
	{
		UISystem.Instance.DestroyMessageBox(this);
		if(m_pfunCallback != null)
			m_pfunCallback(0);
		m_pfunCallback = null;
	}

    public virtual void Confirm()
    {
        UISystem.Instance.DestroyMessageBox(this);
        if (m_pfunCallback != null)
            m_pfunCallback(1);
        m_pfunCallback = null;
    }

    protected virtual void Return(int ret)
    {
        UISystem.Instance.DestroyMessageBox(this);
        if (m_pfunCallback != null)
            m_pfunCallback(ret);
        m_pfunCallback = null;
    }
}
