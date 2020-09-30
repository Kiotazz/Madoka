using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_RoundAOE : SkillEffectBase
{
    [CustomLabel("生效半径")]
    public float Range = 1;

    protected override void OnExecute(InteractiveObj self, InteractiveObj target)
    {
        OnExecute(self, target.transform.position);
    }

    protected override void OnExecute(InteractiveObj self, Vector3 pos)
    {
        RaycastHit[] hits = Physics.SphereCastAll(pos, Range, Vector3.up, float.MaxValue, layerEnemey);
        for (int i = 0, length = hits.Length; i < length; ++i)
        {
            InteractiveObj target = hits[i].collider.GetComponent<InteractiveObj>();
            if (target && self.IsEnemy(target.Camp))
                target.DoDamage(new Damage(EffectType, self.CalAtkDamage(EffectBaseValue)), self);
        }
    }
}
