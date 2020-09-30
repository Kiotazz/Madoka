using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_Heal : SkillEffectBase
{
    protected override void OnExecute(InteractiveObj self, InteractiveObj target)
    {
        Heal();
    }

    protected override void OnExecute(InteractiveObj self, Vector3 pos)
    {
        Heal();
    }

    void Heal()
    {
        ThirdPersonPlayer.Instance.DoHeal(EffectBaseValue);
    }
}
