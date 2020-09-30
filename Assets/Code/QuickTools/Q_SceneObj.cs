using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Q_SceneObj : MonoBehaviour
{
    public enum Type
    {
        Skill,
        Item,
        GuidePoint,
        TriggerPoint,
        NeedItem,
        LoadScene,
        None,
        Vedio,
        PickUpItem,
        SuccessPoint,
    }

    [CustomLabel("类型")]
    public Type objType = Type.Skill;
    [CustomLabel("参数")]
    public string strNeedItemName;
    [CustomLabel("使用时触发")]
    public EventWorker onItemUsed;

    public Collider ColliderSelf { get; protected set; }

    void Start()
    {
        ColliderSelf = GetComponent<Collider>();

        if (!ColliderSelf)
        {
            Debug.LogError("SceneObj:" + name + "必须设置碰撞盒!");
            ColliderSelf = gameObject.AddComponent<BoxCollider>();
        }

        switch (objType)
        {
            case Type.GuidePoint:
            case Type.LoadScene:
            case Type.SuccessPoint:
                ColliderSelf.isTrigger = true;
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        OnTrigger(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnTrigger(collision.collider);
        if (!BattleManager.Instance.IsBattleBegin) return;
        Character target = collision.collider.GetComponent<Character>();
        if (!target) return;
        switch (objType)
        {
            case Type.PickUpItem:
                target.StopMove();
                CharacterPatrol patrol = target.GetComponent<CharacterPatrol>();
                if (patrol) patrol.Pause = true;
                Rigidbody body = GetComponent<Rigidbody>();
                if (!body || body.velocity.sqrMagnitude < 0.9f) return;
                Vector3 tarPos = target.transform.position + body.velocity * 3;
                target.transform.DOMove(tarPos, 1).onComplete = () =>
                {
                    CharacterPatrol patrolP = target.GetComponent<CharacterPatrol>();
                    if (patrolP) patrolP.Pause = false;
                    target.AddFightingEnemey(ThirdPersonPlayer.Instance);
                };
                break;
        }
    }

    void OnTrigger(Collider other)
    {
        if (!BattleManager.Instance.IsBattleBegin) return;
        switch (objType)
        {
            case Type.Skill:
                InteractiveObj target = other.GetComponent<InteractiveObj>();
                if (!target || target == ThirdPersonPlayer.Instance || !target.IsEnemy(ThirdPersonPlayer.Instance.Camp)) return;
                SkillEffectBase[] skillEffects = GetComponentsInChildren<SkillEffectBase>(true);
                for (int i = 0, length = skillEffects.Length; i < length; ++i)
                    skillEffects[i].Execute(WorldInteractObj.Instance, target);
                UseObj();
                break;
            case Type.TriggerPoint:
                Q_SceneObj obj = other.GetComponent<Q_SceneObj>();
                if (obj && obj.objType == Type.PickUpItem)
                    UseObj();
                break;
            case Type.PickUpItem:
                Rigidbody body = GetComponent<Rigidbody>();
                if (body && body.velocity.sqrMagnitude > 0.9f)
                    UseObj();
                break;
        }
    }

    public void UseObj()
    {
        onItemUsed?.DoTriggerEvents();
    }
}
