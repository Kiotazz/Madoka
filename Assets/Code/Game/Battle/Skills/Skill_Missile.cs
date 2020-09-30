using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Missile : SkillBase
{
    public enum Type
    {
        Direct,
        Bullet,
    }

    [CustomLabel("发射单位")]
    public GameObject MisslePrefab;
    [CustomLabel("弹道类型")]
    public Type MissleType;
    [CustomLabel("飞行速度")]
    public float MissileSpeed = 30;
    [CustomLabel("飞行音效")]
    public AudioClip FlySE;
    [CustomLabel("最小发射数量")]
    public int MissleMinCount = 1;
    [CustomLabel("最大发射数量")]
    public int MissleMaxCount = 1;
    [CustomLabel("发射单位生存时间")]
    public int MissleLifeTime = 0;

    [CustomLabel("发射特效")]
    public GameObject ReleaseFX;
    [CustomLabel("发射特效挂点")]
    public Transform ReleaseFXPoint;
    [CustomLabel("发射音效")]
    public AudioClip ReleaseSE;


    protected override void OnInit()
    {
        if (!ReleaseFXPoint) ReleaseFXPoint = transform;
    }

    protected override CastResult OnCast()
    {
        for (int i = 0, length = Random.Range(MissleMinCount, MissleMaxCount + 1); i < length; ++i)
        {
            if (ReleaseFX) EffectPlayer.PlayOnTransform(ReleaseFX, ReleaseFXPoint);
            if (ReleaseSE) AudioSystem.Instance.PlayOnTransform(ReleaseSE, ReleaseFXPoint);
            Missle missle = Instantiate(MisslePrefab, transform.position, transform.rotation).GetOrAddComponent<Missle>();
            switch (LockType)
            {
                case SkillLockType.Position:
                    switch (MissleType)
                    {
                        case Type.Bullet:
                            missle.InitBullet(TargetPos, MissileSpeed, Settlement);
                            break;
                        default:
                            missle.Init(TargetPos, MissileSpeed, Settlement);
                            break;
                    }
                    break;
                case SkillLockType.NoLock:
                    switch (MissleType)
                    {
                        case Type.Bullet:
                            //missle.InitBullet(TargetPos, MissileSpeed, Settlement);
                            break;
                        default:
                            missle.gameObject.AddComponent<SphereCollider>().isTrigger = true;
                            missle.gameObject.AddComponent<Rigidbody>().isKinematic = true;
                            missle.InitFlyFront(Master.transform.forward, MissileSpeed, SkillMaxRange, (hitObj, missleScript) =>
                            {
                                if (hitObj)
                                {
                                    InteractiveObj target = hitObj.GetComponent<InteractiveObj>();
                                    if (target)
                                    {
                                        if (target.IsAlive && target.IsEnemy(Master.Camp))
                                        {
                                            Settlement(target);
                                            Destroy(missleScript.gameObject);
                                        }
                                        return;
                                    }
                                    Q_SceneObj sceneObj = hitObj.GetComponent<Q_SceneObj>();
                                    if (sceneObj)
                                    {
                                        switch (sceneObj.objType)
                                        {
                                            case Q_SceneObj.Type.Skill:
                                            case Q_SceneObj.Type.Item:
                                            case Q_SceneObj.Type.NeedItem:
                                                break;
                                            default:
                                                return;
                                        }
                                    }
                                }
                                if (HitFX) EffectPlayer.PlayAtPos(HitFX, missleScript.transform.position);
                                if (HitSE) AudioSystem.Instance.PlayAtPos(HitSE, missleScript.transform.position);
                                missleScript.Alive = false;
                                Destroy(missleScript.gameObject);
                            });
                            break;
                    }
                    break;
                default:
                    if (Target)
                        missle.Init(Target.transform, MissileSpeed, (tsf) => Settlement(tsf.GetComponent<InteractiveObj>()));
                    else
                        missle.Init(TargetPos, MissileSpeed, Settlement);
                    break;
            }
            missle.SetLifeTime(MissleLifeTime);
            if (FlySE) AudioSystem.Instance.PlayOnTransform(FlySE, missle.transform);
        }
        return CastResult.Success;
    }
}
