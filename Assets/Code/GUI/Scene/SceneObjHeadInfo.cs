using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneObjHeadInfo : MonoBehaviour
{
    public Text txtBlood;
    public Slider sldBlood;

    RectTransform rectTsfSelf;
    CanvasRenderer rendererSelf;

    public InteractiveObj Master { get; private set; }

    long maxHP = 0;
    long curHP = 0;

    bool _bActive = true;
    public bool IsActive
    {
        get { return _bActive; }
        set
        {
            if (_bActive == value) return;
            gameObject.SetActive(_bActive = value);
        }
    }

    public void Init(InteractiveObj master)
    {
        Master = master;
        rectTsfSelf = GetComponent<RectTransform>();
        rendererSelf = GetComponent<CanvasRenderer>();
        rectTsfSelf.SetParent(UISystem.Instance.sceneuiRoot.transform, false);
        rectTsfSelf.localScale = new Vector3(0.5f, 0.5f);
    }

    public void DoUpdate(float deltaTime)
    {
        if (!(IsActive = NeedShowBloodUI(Master.tsfBloodUIPoint.position)) || !rectTsfSelf) return;
        rectTsfSelf.position = Camera.main.WorldToScreenPoint(Master.tsfBloodUIPoint.position);
        if (maxHP == Master.MaxHP && curHP == Master.HP) return;
        txtBlood.text = Master.HP + "/" + Master.MaxHP;
        sldBlood.value = (float)(Master.HP / (double)Master.MaxHP);
    }

    public bool NeedShowBloodUI(Vector3 worldPos)
    {
        if (!Camera.main || !Master || !Master.IsAlive || Master.fBloodUIShowDistance <= 0) return false;
        Transform camTransform = Camera.main.transform;
        float distance = 0;
        if (ThirdPersonPlayer.Instance)
            distance = ThirdPersonPlayer.Instance.transform.position.SqrDistanceWith(Master.tsfBloodUIPoint.position);
        else
            distance = camTransform.position.SqrDistanceWith(Master.tsfBloodUIPoint.position);
        if (distance > Master.fBloodUIShowDistance * Master.fBloodUIShowDistance)
            return false;
        Vector2 viewPos = Camera.main.WorldToViewportPoint(worldPos);
        Vector3 dir = (worldPos - camTransform.position).normalized;
        float dot = Vector3.Dot(camTransform.forward, dir);//判断物体是否在相机前面
        return dot > 0 && viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
