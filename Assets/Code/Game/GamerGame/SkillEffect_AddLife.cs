using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_AddLife : SkillEffectBase
{
    protected override void OnExecute(InteractiveObj self, InteractiveObj target)
    {
        AddLife();
    }

    protected override void OnExecute(InteractiveObj self, Vector3 pos)
    {
        AddLife();
    }

    void AddLife()
    {
        ThirdPersonPlayer.Instance.MaxHP += EffectBaseValue;
        ThirdPersonPlayer.Instance.DoHeal(EffectBaseValue);
    }
}
