using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(RawImage))]
public class RenderQByUI : MonoBehaviour
{
    public bool excute = false;
    public int RenderQ;
    RawImage pic;
    GameObject child;
    Renderer catchRender;

    [ContextMenu("TestExcute")]
    public void TestExcute()
    {
        foreach (Transform t in transform)
        {
            AddChild(t.gameObject);
        }
    }

    public void AddChild(GameObject c)
    {
        child = c;
        c.transform.SetParent(transform, false);
    }

    void Awake()
    {
        pic = GetComponent<RawImage>();
        if (pic.mainTexture == null)
        {
            pic.texture = new Texture2D(1, 1);
        }
        foreach (Transform t in transform)
        {
            if (t != null)
            {
                AddChild(t.gameObject);
            }
        }
    }

    void Update()
    {
        if (pic != null && pic.materialForRendering != null)
        {
            if (catchRender == null || catchRender.material.renderQueue != pic.materialForRendering.renderQueue)
                changeRenderQ();
            if (!excute)
            {
                changeRenderQ();
                excute = true;
            }
        }
    }

    void changeRenderQ()
    {
        RenderQ = pic.materialForRendering.renderQueue;
        if (!child) return;
        child.SetLayer(LayerMask.NameToLayer("UI"), true);
        Renderer[] renderers = child.GetComponentsInChildren<Renderer>(true);
        for (int i = 0, length = renderers.Length; i < length; ++i)
            (catchRender = renderers[i]).material.renderQueue = RenderQ;
    }

    private void OnDestroy()
    {
        if (pic && pic.texture)
            Destroy(pic.texture);
    }
}
