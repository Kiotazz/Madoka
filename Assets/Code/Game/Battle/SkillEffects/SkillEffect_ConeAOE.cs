using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_ConeAOE : SkillEffectBase
{
    [CustomLabel("生效角度")]
    public float Angle = 1;
    [CustomLabel("最近距离")]
    public float MinRange = 0;
    [CustomLabel("最远距离")]
    public float MaxRange = 1;

    protected override void OnExecute(InteractiveObj self, InteractiveObj target)
    {
        OnExecute(self, target.transform.position);
    }

    protected override void OnExecute(InteractiveObj self, Vector3 pos)
    {
        foreach (var target in BattleManager.Instance.dicInteractiveObjs.Values)
            if (target && self.IsEnemy(target.Camp) && Common.IsPosInConeRange(self.transform, pos, Angle, MaxRange, MinRange))
                target.DoDamage(new Damage(EffectType, self.CalAtkDamage(EffectBaseValue)), self);
    }
}
