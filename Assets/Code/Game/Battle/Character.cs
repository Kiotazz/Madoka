using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using DG.Tweening;

public class Character : InteractiveObj
{
    public enum CharaStatus
    {
        Idle,
        Move,
    }

    public enum CharaAction
    {
        OnSkill,
    }

    [CustomLabel("移动速度")]
    public float fMoveSpeed = 3.5f;
    [CustomLabel("追击距离")]
    public float fChasingRange = 0;
    [CustomLabel("摔死高度")]
    public float fDeadHeight = -1500;

    NavMeshAgent navMeshAgent;
    Vector3 vecMainTargetPos;
    Vector3 vecCurTargetPos;
    Vector3 vecChasingStartPos;
    float fChasingAttackRange = 0;
    protected HashSet<CharaAction> actions = new HashSet<CharaAction>();

    public CharaStatus status { get; protected set; } = CharaStatus.Idle;
    public BattleTeam myTeam { get; protected set; }
    public bool IsChasing { get; protected set; } = false;
    public bool IsForceMove { get; protected set; } = false;

    public NormalEvent OnArriveDestination { get; protected set; } = new NormalEvent();

    protected override void Init()
    {
        navMeshAgent = gameObject.GetOrAddComponent<NavMeshAgent>();
        fChasingRange = Mathf.Max(fChasingRange, OutBattleRange);
        vecMainTargetPos = vecChasingStartPos = vecCurTargetPos = transform.position;

        float range = float.MaxValue;
        for (int i = 0, length = listSkills.Count; i < length; ++i)
            if (listSkills[i].SkillMaxRange < range)
                range = listSkills[i].SkillMaxRange;
        fChasingAttackRange = range == float.MaxValue ? range : (range + fAttackMinRange) / 2;
    }

    public void SetTeam(BattleTeam team)
    {
        myTeam = team;
    }

    protected override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        Role.SetMoveSpeed(navMeshAgent.velocity.magnitude);
        if (status == CharaStatus.Move && actions.Count < 1 && navMeshAgent.remainingDistance < 0.3f)
        {
            BackToNormalStatus();
            return;
        }
        if (!BattleManager.Instance.IsBattleBegin || !IsAlive) return;
        if (transform.position.y < fDeadHeight)
            Death();
        if (status == CharaStatus.Idle)
            ChaseTarget();
    }

    public void MoveTo(Vector3 pos)
    {
        ForceTarget = CurrentTarget = null;
        dicFightingObjs.Clear();
        IsForceMove = true;
        IsChasing = false;
        SetMoveSpeed(fMoveSpeed);
        Weak_MoveTo(vecMainTargetPos = pos);
    }

    public void Weak_MoveTo(Vector3 pos)
    {
        NavMeshHit meshHit;
        if (NavMesh.SamplePosition(pos, out meshHit, 10, 1))
        {
            if (meshHit.position.SqrDistanceWith(transform.position) < 1) return;
            vecCurTargetPos = meshHit.position;
            status = CharaStatus.Move;
            BackToNormalStatus();
        }
    }

    public void SetMoveSpeed(float speed)
    {
        navMeshAgent.speed = speed;
    }

    public void AttackTarget(InteractiveObj tar, float offsetAngle)
    {
        ForceTarget = tar;
        Vector3 vecTargetPos = tar.transform.position;
        if (vecTargetPos.SqrDistanceWith(transform.position) <= fAttackMinRange * fAttackMinRange) return;
        float distance = fAttackMinRange *= Mathf.Sign(transform.position.x - vecTargetPos.x);
        vecTargetPos.x += distance * Mathf.Cos(offsetAngle);
        vecTargetPos.z += distance * Mathf.Sin(offsetAngle);
        MoveTo(ForceTarget.ColliderSelf.ClosestPoint(vecTargetPos) + (vecTargetPos - ForceTarget.transform.position) * 0.9f);
    }

    public override InteractiveObj FindNextEnemy()
    {
        InteractiveObj enemy = base.FindNextEnemy();
        if (enemy || !myTeam) return enemy;
        for (int i = 0, length = myTeam.MembersCount; i < length; ++i)
        {
            enemy = myTeam.GetMember(i).CurrentTarget;
            if (enemy) return enemy;
        }
        return enemy;
    }

    public void BackToNormalStatus(bool autoIdle = true)
    {
        if (!IsAlive) return;
        if (CurrentTarget) transform.LookAt(CurrentTarget.BeAtkPoint);
        Vector3 euler = transform.eulerAngles;
        euler.x = euler.z = 0;
        transform.eulerAngles = euler;
        if (actions.Count > 0) return;
        Role.transform.localRotation = Quaternion.identity;
        if (autoIdle && vecCurTargetPos.SqrDistanceWith(transform.position) < 1)
        {
            status = CharaStatus.Idle;
            IsForceMove = false;
            OnArriveDestination.Invoke();
        }
        else if (status == CharaStatus.Move)
        {
            navMeshAgent.SetDestination(vecCurTargetPos);
            navMeshAgent.isStopped = false;
        }
        Role.PlayAction(Role.strNormalAnimName);
    }

    public void StopMove()
    {
        navMeshAgent.isStopped = true;
        vecCurTargetPos = transform.position;
        status = CharaStatus.Idle;
        BackToNormalStatus(true);
    }

    public void SetPosition(Vector3 pos)
    {
        StopMove();
        navMeshAgent.enabled = false;
        transform.position = pos;
        navMeshAgent.nextPosition = pos;
        navMeshAgent.enabled = true;
    }

    protected override void OnDamage(Damage damage, InteractiveObj source)
    {
        base.OnDamage(damage, source);
        Role.PlayAction(Role.strDamageAnimName, 0.1f);
        if (!ForceTarget && !IsForceMove && source && source != WorldInteractObj.Instance)
        {
            CurrentTarget = source;
            OnFightBegin.Invoke();
            ChaseTarget();
        }
    }

    void ChaseTarget()
    {
        if (!CurrentTarget)
        {
            if (IsChasing) StopChase(true);
            return;
        }
        if (IsForceMove || ForceTarget) return;
        bool chase = false;
        if (IsChasing)
        {
            float sqrDistance = CurrentTarget.transform.position.SqrDistanceWith(transform.position);
            if (!CurrentTarget || sqrDistance > fChasingRange * fChasingRange)
            {
                StopChase(true);
                return;
            }
            chase = sqrDistance > fChasingAttackRange * fChasingAttackRange + 0.5f;
        }
        else
        {
            for (int i = 0, length = listSearchEnemy.Count; i < length; ++i)
                if (chase = listSearchEnemy[i].IsTargetInSearchArea(CurrentTarget))
                    break;
        }
        if (chase)
        {
            if (!IsChasing) vecChasingStartPos = transform.position;
            IsChasing = true;
            SetMoveSpeed(fMoveSpeed);
            Weak_MoveTo(CurrentTarget.transform.position - (CurrentTarget.transform.position - transform.position).normalized * fChasingAttackRange);
        }
    }

    void StopChase(bool backToStartPos)
    {
        if (CurrentTarget) RemoveFightingEnemey(CurrentTarget.ID);
        IsChasing = false;
        if (backToStartPos) Weak_MoveTo(vecMainTargetPos);
    }

    protected override void OnSelfDeath()
    {
        Event_Battle.OnCharacterDead.Invoke(this);
        Role.PlayAction(Role.strDeathAnimName, 0.1f);
        navMeshAgent.isStopped = true;
        IsForceMove = IsChasing = false;
    }

    protected override void OnAttackTarget()
    {
        if (CurrentTarget)
            transform.LookAt(CurrentTarget.BeAtkPoint);
    }

    public override void OnAnimLogicOver(int nameHash)
    {
        if (!IsAlive) return;
        ClearActions();
        BackToNormalStatus(false);
    }

    public override void OnAnimStateExit(int nameHash)
    {
        ClearActions();
        BackToNormalStatus();
    }

    public override void OnSkillStart(SkillBase skill)
    {
        actions.Add(CharaAction.OnSkill);
        navMeshAgent.isStopped = true;
    }

    public override void OnSkillOver(SkillBase skill)
    {
        ClearActions();
        BackToNormalStatus();
    }

    void ClearActions()
    {
        Role.transform.localRotation = Quaternion.identity;
        if (!CurrentSkill || !CurrentSkill.IsWorking)
            actions.Remove(CharaAction.OnSkill);
    }

    public override bool CanCastSkill(SkillBase skill)
    {
        if (status != CharaStatus.Move) return true;
        return skill.TotalTime < 1f;
    }

    public override bool IsFindNextEnemy()
    {
        if (!CurrentTarget || !CurrentTarget.IsAlive || !CurrentTarget.IsEnemy(Camp)) return true;
        return false;
    }
}
