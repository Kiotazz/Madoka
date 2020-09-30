using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterFollow : InteractiveObjExt
{
    [CustomLabel("跟随目标")]
    public InteractiveObj Target;
    [CustomLabel("过远距离")]
    public float fDistance = 10;
    [CustomLabel("跟随偏移")]
    public Vector3 vecOffset;

    public Character MasterChara { get; protected set; }

    protected override void OnInit(InteractiveObj obj)
    {
        MasterChara = obj as Character;
        //MasterChara.OnArriveDestination.AddListener(OnCharacterArrive);
        TryFollow();
    }

    public override void DoUpdate(float deltaTime)
    {
        TryFollow();
    }

    void TryFollow()
    {
        if (!MasterChara.IsAlive || !Target) return;
        if (Target.transform.position.SqrDistanceWith(MasterChara.transform.position) > fDistance * fDistance)
        {
            NavMeshHit meshHit;
            if (NavMesh.SamplePosition(Target.transform.position + vecOffset, out meshHit, 10, 1))
            {
                if (Target.ColliderSelf.bounds.Contains(meshHit.position)) return;
                if (MasterChara.CurrentTarget) MasterChara.RemoveFightingEnemey(MasterChara.CurrentTarget.ID);
                MasterChara.SetPosition(meshHit.position);
            }
        }
        else if (!MasterChara.CurrentTarget && !MasterChara.IsChasing && MasterChara.status == Character.CharaStatus.Idle)
        {
            MasterChara.Weak_MoveTo(Target.transform.position + vecOffset);
        }
    }
}
