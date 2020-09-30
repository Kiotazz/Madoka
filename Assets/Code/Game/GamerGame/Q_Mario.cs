using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Q_Mario : InteractiveObjExt
{
    static Q_Mario Instance;

    [CustomLabel("打招呼时间")]
    public int nHelloDelay = 3000;
    [CustomLabel("跳跃力")]
    public float fJumpStrength = 800;
    [CustomLabel("保护刷新时间")]
    public int nProtectRefreshTime = 30000;

    public Character MasterChara { get; protected set; }
    public CharacterFollow Follow { get; protected set; }

    bool isFollow = false;
    float counter = 0;
    bool _start = false;

    protected override void OnInit(InteractiveObj obj)
    {
        MasterChara = obj as Character;
        Follow = obj.GetComponent<CharacterFollow>();
        MasterChara.WillNotBeFind = true;
        VedioPlayer.OnVedioFinished.AddListener(() =>
        {
            _start = true;
            MasterChara.Role.PlayAction("Hello", 0);
            Vector3 pos = ThirdPersonPlayer.Instance.transform.position;
            pos.y = MasterChara.transform.position.y;
            MasterChara.transform.LookAt(pos);
        });
    }

    public override void DoUpdate(float deltaTime)
    {
        if (!_start) return;
        if (isFollow)
        {
            if ((counter += deltaTime) > nProtectRefreshTime / 1000f)
            {
                ThirdPersonPlayer.Instance.IsProtected = true;
                counter = 0;
            }
        }
        else if ((counter += deltaTime) > nHelloDelay / 1000f)
        {
            Follow.Target = ThirdPersonPlayer.Instance;
            ThirdPersonPlayer.Instance.bCanUseJumpSkill = true;
            ThirdPersonPlayer.Instance.fJumpStrength = fJumpStrength;
            ThirdPersonPlayer.Instance.IsProtected = true;
            isFollow = true;
            counter = 0;
        }
    }
}
