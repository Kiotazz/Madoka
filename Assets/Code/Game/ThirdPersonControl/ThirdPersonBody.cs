using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonBody : MonoBehaviour
{
    public ThirdPersonPlayer Master { get; protected set; }
    public Rigidbody RigidSelf { get; protected set; }
    public CapsuleCollider ColliderSelf { get; protected set; }

    public Vector3 Position { get { return RigidSelf.position; } }
    public Quaternion Rotation { get { return RigidSelf.rotation; } }
    public Vector3 Velocity { get { return RigidSelf.velocity; } }
    public bool IsGrounded { get { return Mathf.Abs(RigidSelf.velocity.y) < 0.05f; } }


    public void Init(ThirdPersonPlayer master)
    {
        Master = master;
        RigidSelf = GetComponent<Rigidbody>();
        ColliderSelf = GetComponent<CapsuleCollider>();
        RigidSelf.position = master.transform.position;
        RigidSelf.rotation = master.transform.rotation;
        transform.localScale = master.transform.localScale;
        RigidSelf.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    public void MoveTo(Vector3 pos)
    {
        RigidSelf.MovePosition(pos);
    }

    public void RotateTo(Quaternion rot)
    {
        RigidSelf.MoveRotation(rot);
    }

    public void MoveBy(Vector3 deltaPos)
    {
        RigidSelf.MovePosition(RigidSelf.position + deltaPos);
    }

    public void RotateBy(Quaternion deltaRot)
    {
        RigidSelf.MoveRotation(RigidSelf.rotation * deltaRot);
    }


    private void OnTriggerEnter(Collider other)
    {
        OnTrigger(other);
    }

    private void OnTriggerExit(Collider other)
    {
        Q_SceneObj sceneObj = other.GetComponent<Q_SceneObj>();
        if (!sceneObj) return;
        switch (sceneObj.objType)
        {
            case Q_SceneObj.Type.GuidePoint:
                UISystem.Instance.CloseGuide();
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Master.bCanUseJumpSkill)
        {
            InteractiveObj target = collision.collider.GetComponent<InteractiveObj>();
            if (target && target.IsAlive && target.IsEnemy(Master.Camp))
            {
                Vector3 myCenter = Master.ColliderSelf.bounds.center;
                Vector3 center = target.ColliderSelf.bounds.center;
                float top = target.ColliderSelf.bounds.max.y;
                if (top - Master.ColliderSelf.bounds.min.y < 0.1f)
                {
                    float y = center.y;
                    center.y = myCenter.y;
                    myCenter.y = y;
                    if (Master.ColliderSelf.bounds.Contains(center) || target.ColliderSelf.bounds.Contains(myCenter))
                    {
                        target.DoDamage(new Damage(WorldSetting.Effect.Physical, 1000000), Master);
                        Master.Body.RigidSelf.AddForce(0, 500, 0, ForceMode.Impulse);
                        return;
                    }
                }
            }
        }
        OnTrigger(collision.collider);
    }

    public void OnTrigger(Collider other)
    {
        if (!Master.IsAlive || !BattleManager.Instance.IsBattleBegin) return;
        Q_SceneObj sceneObj = other.GetComponent<Q_SceneObj>();
        if (sceneObj)
        {
            switch (sceneObj.objType)
            {
                case Q_SceneObj.Type.Skill:
                    SkillEffectBase[] skillEffects = sceneObj.GetComponentsInChildren<SkillEffectBase>(true);
                    for (int i = 0, length = skillEffects.Length; i < length; ++i)
                        skillEffects[i].Execute(WorldInteractObj.Instance, Master);
                    sceneObj.UseObj();
                    break;
                case Q_SceneObj.Type.GuidePoint:
                    sceneObj.UseObj();
                    UISystem.Instance.ShowGuideDialog(sceneObj.name);
                    break;
                case Q_SceneObj.Type.TriggerPoint:
                    sceneObj.UseObj();
                    break;
                case Q_SceneObj.Type.LoadScene:
                    sceneObj.UseObj();
                    GameClient.Instance.LoadScene(sceneObj.strNeedItemName);
                    break;
                case Q_SceneObj.Type.Vedio:
                    VedioPlayer.Play(sceneObj.strNeedItemName, null);
                    sceneObj.gameObject.SetActive(false);
                    sceneObj.UseObj();
                    break;
                case Q_SceneObj.Type.SuccessPoint:
                    sceneObj.UseObj();
                    BattleManager.Instance.BattleOver(true);
                    break;
            }
        }
        InteractiveObj target = other.GetComponent<InteractiveObj>();
        if (target && target.IsAlive && target.IsEnemy(ThirdPersonPlayer.Instance.Camp))
            ThirdPersonPlayer.Instance.DoDamage(new Damage(WorldSetting.Effect.Physical, 1), target);
    }
}
