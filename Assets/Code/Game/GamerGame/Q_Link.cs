using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Q_Link : InteractiveObjExt
{
    static Q_Link Instance;

    [CustomLabel("打招呼时间")]
    public int nHelloDelay = 3500;

    public Character MasterChara { get; protected set; }
    public CharacterFollow Follow { get; protected set; }

    bool isFollow = false;
    float counter = 0;
    bool _start = false;

    protected override void OnInit(InteractiveObj obj)
    {
        MasterChara = obj as Character;
        Follow = obj.GetComponent<CharacterFollow>();
        VedioPlayer.OnVedioFinished.AddListener(() =>
        {
            _start = true;
            MasterChara.Role.PlayAction("Hello", 0);
            Vector3 pos = ThirdPersonPlayer.Instance.transform.position;
            pos.y = MasterChara.transform.position.y;
            MasterChara.transform.LookAt(pos);
            MasterChara.OutBattleRange = 0;
        });
    }

    public override void DoUpdate(float deltaTime)
    {
        if (!_start) return;
        if (!isFollow && (counter += deltaTime) > nHelloDelay / 1000f)
        {
            Follow.Target = ThirdPersonPlayer.Instance;
            ThirdPersonPlayer.Instance.bCanUseItemSkill = true;
            isFollow = true;
            MasterChara.OutBattleRange = 5;
            counter = 0;
        }
    }
}
