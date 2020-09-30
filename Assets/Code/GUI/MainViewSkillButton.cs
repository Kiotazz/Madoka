using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainViewSkillButton : MonoBehaviour
{
    public InteractiveObj Master { get; protected set; }
    SkillBase bindSkill;

    bool _inited = false;

    void InitSkill()
    {
        if (_inited) return;
        _inited = true;
        Master = ThirdPersonPlayer.Instance;
        bindSkill = GetComponentInChildren<SkillBase>();
        if (bindSkill)
        {
            bindSkill.Init(Master);
            Transform tsf = Master.transform.Find("SkillStartPos");
            bindSkill.transform.SetParent(tsf ? tsf : Master.BeAtkPoint, false);
            bindSkill.transform.localPosition = Vector3.zero;
            bindSkill.transform.localRotation = Quaternion.identity;
        }
    }

    private void Update()
    {
        if (bindSkill) bindSkill.DoUpdate(Time.deltaTime);
    }

    public void UseSkill()
    {
        if (!ThirdPersonPlayer.Instance) return;
        InitSkill();
        if (bindSkill) bindSkill.Execute(null);
    }
}
