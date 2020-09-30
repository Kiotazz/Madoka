using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Search_Cone : SearchEnemyBase
{
    [CustomLabel("索敌角度")]
    public float fAngle = 1;
    [CustomLabel("索敌距离")]
    public float fRange = 10;

    public override void DoUpdate(float deltaTime)
    {
        foreach (var target in BattleManager.Instance.dicInteractiveObjs.Values)
            if (target && Master.IsEnemy(target.Camp) && IsTargetInSearchArea(target))
                Master.AddFightingEnemey(target);
    }

    public override bool IsTargetInSearchArea(InteractiveObj target)
    {
        return Common.IsPosInConeRange(Master.transform, target.transform.position, fAngle, fRange);
    }
}
