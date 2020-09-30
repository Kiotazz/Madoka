using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_Dot : SkillEffectBase
{
    [CustomLabel("Buff优先级")]
    public int nBuffOrder = 0;
    [CustomLabel("持续时间")]
    public int nDuration = 0;
    [CustomLabel("首次生效时间")]
    public int nFirstEffectTime = 0;
    [CustomLabel("首次生效伤害")]
    public int nEffectDamage = 0;
    [CustomLabel("生效间隔")]
    public int nInterval = 0;
    [CustomLabel("结束伤害")]
    public int nEndDamage = 0;

    [CustomLabel("持续特效")]
    public GameObject objContinueFX;
    [CustomLabel("持续音效")]
    public AudioClip acContinueSE;

    [CustomLabel("每次生效特效")]
    public GameObject objContinueEffectFX;
    [CustomLabel("每次生效音效")]
    public AudioClip acContinueEffectSE;

    [CustomLabel("结束特效")]
    public GameObject objEndEffectFX;
    [CustomLabel("结束音效")]
    public AudioClip acEndEffectSE;

    protected override void OnExecute(InteractiveObj self, InteractiveObj target)
    {
        target.DoDamage(new Damage(EffectType, self.CalAtkDamage(EffectBaseValue)), self);
        EffectPlayer.PlayOnTransform(EffectFX, target.BeAtkPoint);
        AudioSystem.Instance.PlayOnTransform(EffectSE, target.BeAtkPoint);

        Debuff_Dot dot = new Debuff_Dot();
        dot.Init(nBuffOrder, nDuration, BuffBase.Type.Debuff, self, target);
        dot.InitDot(nFirstEffectTime, nDuration, EffectType, self.CalAtkDamage(nEffectDamage), self.CalAtkDamage(nEndDamage));
        dot.InitDotResources(objContinueFX, acContinueSE, objContinueEffectFX, acContinueEffectSE, objEndEffectFX, acEndEffectSE);
        target.AddBuff(dot);
    }
}

public class Debuff_Dot : BuffBase
{
    public int nEffectInterval = 1000;
    public int nEndDamage = 0;

    public GameObject objContinueEffectFX;
    public AudioClip acContinueEffectSE;

    public GameObject objEndEffectFX;
    public AudioClip acEndEffectSE;

    EffectPlayer effectContinue;
    AudioPlayer audioContinue;

    public int RaimainingTime { get; protected set; } = -1;

    int nNextEffectTime = 0;
    WorldSetting.Effect eEffectType = WorldSetting.Effect.Fire;
    int nDamage = 0;

    public void InitDot(int firstEffectTime, int interval, WorldSetting.Effect effectType, int damage, int endDamage)
    {
        nEffectInterval = interval;
        eEffectType = effectType;
        nDamage = damage;
        nEndDamage = endDamage;
        RaimainingTime = nDuration;
        nNextEffectTime = nDuration - firstEffectTime;
    }

    public void InitDotResources(GameObject continueFX, AudioClip continueAC, GameObject effectFX, AudioClip effectAC, GameObject endFX, AudioClip endAC)
    {
        effectContinue = EffectPlayer.PlayOnTransform(continueFX, Target.BeAtkPoint);
        audioContinue = AudioSystem.Instance.PlayOnTransform(continueAC, Target.BeAtkPoint);
        objContinueEffectFX = effectFX;
        acContinueEffectSE = effectAC;
        objEndEffectFX = endFX;
        acEndEffectSE = endAC;
    }

    public override void DoUpdate(float deltaTime)
    {
        if (RaimainingTime < 0) return;
        if ((RaimainingTime -= (int)deltaTime * 1000) < nNextEffectTime)
        {
            Target.DoDamage(new Damage(eEffectType, nDamage), Master);
            nNextEffectTime = RaimainingTime - nEffectInterval;
            EffectPlayer.PlayOnTransform(objContinueEffectFX, Target.BeAtkPoint);
            AudioSystem.Instance.PlayOnTransform(acContinueEffectSE, Target.BeAtkPoint);
        }
    }

    protected override void OnRemove(bool isDead)
    {
        if (effectContinue) effectContinue.Recycle();
        if (audioContinue) audioContinue.Recycle();
        effectContinue = null;
        audioContinue = null;
        Target.DoDamage(new Damage(eEffectType, nEndDamage), Master);
        EffectPlayer.PlayOnTransform(objEndEffectFX, Target.BeAtkPoint);
        AudioSystem.Instance.PlayOnTransform(acEndEffectSE, Target.BeAtkPoint);
    }
}