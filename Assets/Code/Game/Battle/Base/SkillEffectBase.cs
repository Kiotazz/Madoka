using UnityEngine;

public class SkillEffectBase : MonoBehaviour
{
    protected const int DefaultEffectRange = 1;

    [CustomLabel("触发延时")]
    public int EffectDelay = 0;
    [CustomLabel("效果类型")]
    public WorldSetting.Effect EffectType;
    [CustomLabel("效果基础数值")]
    public int EffectBaseValue = 0;
    [CustomLabel("触发特效")]
    public GameObject EffectFX;
    [CustomLabel("触发音效")]
    public AudioClip EffectSE;

    protected LayerMask layerEnemey { get { return 1 << LayerMask.NameToLayer("Accessable") | 1 << LayerMask.NameToLayer("Monster"); } }

    public void Execute(InteractiveObj self, InteractiveObj target)
    {
        GameClient.Instance.NextTick(() =>
        {
            if (!target) return;
            EffectPlayer.PlayOnTransform(EffectFX, target.BeAtkPoint);
            AudioSystem.Instance.PlayAtPos(EffectSE, target.BeAtkPoint.position);
            OnExecute(self, target);
        }, EffectDelay / 1000f);
    }

    protected virtual void OnExecute(InteractiveObj self, InteractiveObj target)
    {
        target.DoDamage(new Damage(EffectType, self.CalAtkDamage(EffectBaseValue)), self);
    }

    public void Execute(InteractiveObj self, Vector3 pos)
    {
        GameClient.Instance.NextTick(() =>
        {
            if (!self) return;
            EffectPlayer.PlayAtPos(EffectFX, pos);
            AudioSystem.Instance.PlayAtPos(EffectSE, pos);
            OnExecute(self, pos);
        }, EffectDelay / 1000f);
    }

    protected virtual void OnExecute(InteractiveObj self, Vector3 pos)
    {
        InteractiveObj target = FindTargetByPos(self, pos);
        if (target) OnExecute(self, target);
    }

    protected InteractiveObj FindTargetByPos(InteractiveObj self, Vector3 pos, int range = DefaultEffectRange)
    {
        RaycastHit[] hits = Physics.SphereCastAll(pos, range, Vector3.up, float.MaxValue, layerEnemey);
        for (int i = 0, length = hits.Length; i < length; ++i)
        {
            InteractiveObj target = hits[i].collider.GetComponent<InteractiveObj>();
            if (target && self.IsEnemy(target.Camp))
                return target;
        }
        return null;
    }
}
