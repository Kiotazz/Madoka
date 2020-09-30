using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Search_Around : SearchEnemyBase
{
    [CustomLabel("索敌范围")]
    public float fRange = 20;

    protected override void OnInit()
    {
        base.OnInit();
    }

    public override void DoUpdate(float deltaTime)
    {
        foreach (var obj in BattleManager.Instance.dicInteractiveObjs.Values)
            if (obj.IsEnemy(Master.Camp) && IsTargetInSearchArea(obj))
                Master.AddFightingEnemey(obj);
    }

    public override bool IsTargetInSearchArea(InteractiveObj target)
    {
        return target.transform.position.SqrDistanceWith(transform.position) <= fRange * fRange;
    }
}
