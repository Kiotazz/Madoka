using UnityEngine;
using System.Collections.Generic;

public abstract class UILayer : MonoBehaviour
{
    public enum Type
    {
        Window,
        MessageBox,
    }
    public virtual Type type { get { return Type.Window; } }

    public class LoadOperation
    {
        public bool startLoading = false;
        public bool resetSprite = true;
        public bool showKeyDiscrip = true;
    }
    public LoadOperation loadOp { get; private set; }
    public virtual bool loaded { get; protected set; }

    public Vector3 hidePosition { get { return new Vector3(0, 0, 100000f); } }
    protected virtual bool hide
    {
        get { return transform.localPosition == hidePosition; }
        set { transform.localPosition = value ? hidePosition : Vector3.zero; }
    }

    public bool isTopLayer { get { return UISystem.Instance.TopLayer == this; } }

    /// <summary>
    /// 创建此界面
    /// </summary>
    public virtual void Create()
    {
        loaded = false;
        hide = true;
        loadOp = new LoadOperation();
        OnLoad(loadOp);
        if (loadOp.startLoading)
        {
            UISystem.Instance.CreateLoadingBox();
            return;
        }
    }

    /// <summary>
    /// 销毁此界面
    /// </summary>
    public void Destroy()
    {
        OnUnload();
    }

    /// <summary>
    /// 加载工作完成，表示一个Layer完成了加载工作，可以显示了
    /// </summary>
    protected virtual void FinishLoading() { UISystem.Instance.DestroyLoadingBox(); }

    /// <summary>
    /// 开始加载事件，派生类重写此功能，用于实现真正的加载操作
    /// </summary>
    /// <param name="op">
    /// 加载操作上下文。如果指定op.startLoading = true 表示此Layer在创建时就需要加载数据。
    /// 这时，此Layer在创建时不会立即显示出来，而是会先出现LoadingBox，并且在加载结束后调用
    /// FinishLoading()来结束加载工作，然后才会显示出来
    /// </param>
    protected abstract void OnLoad(LoadOperation op);

    /// <summary>
    /// 创建完成事件。如果界面创建时需要加载，则会等待界面加载完毕才会发送此事件。
    /// </summary>
    protected virtual void OnCreated() { }

    /// <summary>
    /// 界面卸载事件。在界面被销毁时，发送此事件，可以在此卸载界面的资源(如果需要手动卸载的话)
    /// </summary>
    protected virtual void OnUnload() { }
    public virtual void OnEscape() { UISystem.Instance.Backward(); }
    protected virtual void OnSelectChange() { }


    public void DoUpdate(float deltaTime)
    {

    }

    public void DoFixedUpdate(float fixedDeltaTime)
    {

    }
}

