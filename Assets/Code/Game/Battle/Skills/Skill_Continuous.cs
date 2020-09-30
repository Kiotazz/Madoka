using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Continuous : SkillBase
{
    [CustomLabel("引导时间")]
    public int ContinueTime = 2000;
    [CustomLabel("首次生效时间")]
    public int FirstEffectTime = 0;
    [CustomLabel("生效间隔")]
    public int EffectInterval = 1000;

    public int RaimainingTime { get; protected set; } = -1;

    int NextEffectTime = 0;

    protected override CastResult OnCast()
    {
        RaimainingTime = ContinueTime;
        NextEffectTime = ContinueTime - FirstEffectTime;
        ForbidOther = true;
        return CastResult.Success;
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (RaimainingTime < 0) return;
        if ((RaimainingTime -= (int)deltaTime * 1000) < NextEffectTime)
        {
            Settlement();
            NextEffectTime = RaimainingTime - EffectInterval;
        }
        if (RaimainingTime < 0)
            ForbidOther = false;
    }

    protected override bool OnInterrupt()
    {
        return base.OnInterrupt();
    }
}
