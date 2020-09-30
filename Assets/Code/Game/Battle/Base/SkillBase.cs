using UnityEngine;

public abstract class SkillBase : MonoBehaviour
{
    [System.Serializable]
    public class SkillCostData
    {
        [CustomLabel("资源类型")]
        public WorldSetting.Energy type;
        [CustomLabel("资源量")]
        public float value;
    }

    public enum Status
    {
        Idle,
        Casting,
        Delay,
        Wait,
        ExWait,
    }

    public enum SkillLockType
    {
        Self,
        Friend,
        Enemy,
        FriendOrEnemy,
        Position,
        NoLock,
    }

    public enum CastResult
    {
        Success,
        InCD,
        IsCasting,
        NoEnergy,
        OutOfRange,
        Interrupt,
        CannotCast,
        NoTarget,
        Forbid,
    }

    [CustomLabel("释放顺序")]
    public int CastOrder = 0;
    [CustomLabel("不可同时释放")]
    public bool IsForbidOtherSkills = true;
    [CustomLabel("可手动释放")]
    public bool CanCastByPlayer = true;

    [CustomLabel("名字")]
    public string Name;
    [CustomLabel("描述")]
    public string Discription;
    [CustomLabel("图标")]
    public UnityEngine.UI.Image Icon;
    [CustomLabel("锁定方式")]
    public SkillLockType LockType = SkillLockType.Position;
    [CustomLabel("CD")]
    public int SkillCD = 0;
    [CustomLabel("最近射程")]
    public float SkillMinRange = 0;
    [CustomLabel("最远射程")]
    public float SkillMaxRange = 1;
    [Header("技能消耗")]
    public SkillCostData[] SkillCost;

    [CustomLabel("吟唱时间")]
    public int CastTime = 0;
    [CustomLabel("吟唱特效")]
    public GameObject CastFX;
    [CustomLabel("吟唱特效挂点")]
    public Transform CastFXPoint;
    [CustomLabel("吟唱音效")]
    public AudioClip CastSE;

    [CustomLabel("释放延时")]
    public int SkillDelay = 0;
    [CustomLabel("释放动作名")]
    public string CastActionName;

    [CustomLabel("击中特效")]
    public GameObject HitFX;
    [CustomLabel("击中音效")]
    public AudioClip HitSE;

    [CustomLabel("技能后摇")]
    public int SkillWait = 0;


    public InteractiveObj Master { get; private set; }
    public Commander MasterCommander { get; private set; }
    //public SkillData Data { get; private set; }
    public bool IsCommanderSkill { get { return MasterCommander; } }
    public InteractiveObj Target { get; private set; }
    public Vector3 TargetPos { get; private set; }
    public Status SkillStatus { get; protected set; } = Status.Idle;
    public bool IsWorking { get { return SkillStatus != Status.Idle || ForbidOther; } }
    public bool ForbidOther { get; protected set; }
    public float LastCastTime { get; protected set; }
    public bool InCD { get { return (Time.timeSinceLevelLoad - LastCastTime) * 1000f < SkillCD; } }
    public int TotalTime { get { return CastTime + SkillDelay + SkillWait; } }

    float fTimeCounter = 0;
    EffectPlayer effectCastFx;
    AudioPlayer castPlayer;

    public void Init(InteractiveObj master)
    {
        Master = master;
        LastCastTime = -SkillCD / 1000f;
        OnInit();
    }
    public void Init(Commander master)
    {
        MasterCommander = master;
        LastCastTime = -SkillCD / 1000f;
        OnInit();
    }
    protected virtual void OnInit() { }

    public CastResult Execute(InteractiveObj target)
    {
        Target = target;
        if (Target) TargetPos = Target.BeAtkPoint.position;
        CastResult result = OnExucete();
        if (result != CastResult.Success)
            Target = null;
        return result;
    }

    public CastResult Execute(Vector3 pos)
    {
        Target = null;
        TargetPos = pos;
        CastResult result = OnExucete();
        return result;
    }

    protected virtual CastResult CanExecute() { return CastResult.Success; }
    protected virtual CastResult PreExecute()
    {
        switch (LockType)
        {
            case SkillLockType.Self:
                Target = Master;
                TargetPos = Master.transform.position;
                break;
            case SkillLockType.Friend:
                break;
            case SkillLockType.Enemy:
                break;
            case SkillLockType.FriendOrEnemy:
                break;
            case SkillLockType.Position:
            case SkillLockType.NoLock:
                return CastResult.Success;
        }
        return Target ? CastResult.Success : CastResult.NoTarget;
    }

    protected CastResult OnExucete()
    {
        if (SkillStatus == Status.Casting) return CastResult.IsCasting;
        if (InCD) return CastResult.InCD;
        CastResult result = PreExecute();
        if (result != CastResult.Success) return result;
        result = CanExecute();
        if (result != CastResult.Success) return result;
        if (!IsInRange()) return CastResult.OutOfRange;
        for (int i = 0, length = SkillCost.Length; i < length; ++i)
            if (!Master.HasSkillEnergy(SkillCost[i].type, SkillCost[i].value))
                return CastResult.NoEnergy;

        ClearCastObjs();
        Master.OnSkillStart(this);
        if (CastTime > 0)
        {
            SkillStatus = Status.Casting;
            fTimeCounter = 0;
            if (CastFX) effectCastFx = EffectPlayer.PlayOnTransform(CastFX, CastFXPoint ? CastFXPoint : transform);
            if (CastSE) castPlayer = AudioSystem.Instance.PlayOnTransform(CastSE, CastFXPoint ? CastFXPoint : transform);
            Master.Role.PlayAction(CastActionName, 0.1f);
        }
        else
        {
            PrepareCast();
        }
        return result;
    }

    public void DoUpdate(float deltaTime)
    {
        if (SkillStatus == Status.Casting && (fTimeCounter += deltaTime) * 1000 > CastTime)
        {
            ClearCastObjs();
            PrepareCast();
        }
        else if (SkillStatus == Status.Delay && (fTimeCounter += deltaTime) * 1000 > SkillDelay)
        {
            Cast();
        }
        else if (SkillStatus == Status.Wait && (fTimeCounter += deltaTime) * 1000 > SkillWait)
        {
            SkillStatus = Status.Idle;
            Master.OnSkillOver(this);
        }
        OnUpdate(deltaTime);
    }
    protected virtual void OnUpdate(float deltaTime) { }

    bool IsInRange()
    {
        if (LockType == SkillLockType.NoLock) return true;
        float sqrDistance = TargetPos.SqrDistanceWith(transform.position);
        return sqrDistance >= SkillMinRange * SkillMinRange && sqrDistance <= SkillMaxRange * SkillMaxRange;
    }

    protected CastResult PrepareCast()
    {
        if (!IsInRange()) return CastResult.OutOfRange;
        CastResult result = CanExecute();
        if (result != CastResult.Success) return result;
        for (int i = 0, length = SkillCost.Length; i < length; ++i)
            if (!Master.TryChangeSkillEnergy(SkillCost[i].type, SkillCost[i].value))
                return CastResult.NoEnergy;
        if (SkillDelay > 0)
        {
            SkillStatus = Status.Delay;
            fTimeCounter = 0;
        }
        else
        {
            result = Cast();
        }
        Master.Role.PlayAction(CastActionName);
        LastCastTime = Time.timeSinceLevelLoad;
        return result;
    }

    protected CastResult Cast()
    {
        CastResult result = OnCast();
        if (SkillWait > 0)
        {
            SkillStatus = Status.Wait;
            fTimeCounter = 0;
        }
        else
        {
            SkillStatus = Status.Idle;
            Master.OnSkillOver(this);
        }
        Target = null;
        return result;
    }

    protected abstract CastResult OnCast();

    void ClearCastObjs()
    {
        if (effectCastFx) effectCastFx.Recycle();
        if (castPlayer) castPlayer.Recycle();
        effectCastFx = null;
        castPlayer = null;
    }

    protected virtual void Settlement()
    {
        if (LockType == SkillLockType.Position)
            Settlement(TargetPos);
        else if (Target)
            Settlement(Target);
    }

    protected virtual void Settlement(InteractiveObj target)
    {
        if (!target) return;
        if (HitFX) EffectPlayer.PlayOnTransform(HitFX, target.BeAtkPoint);
        if (HitSE) AudioSystem.Instance.PlayAtPos(HitSE, target.BeAtkPoint.position);
        SkillEffectBase[] effects = GetComponentsInChildren<SkillEffectBase>(true);
        for (int i = 0, length = effects.Length; i < length; ++i)
            effects[i].Execute(Master, target);
    }

    protected virtual void Settlement(Vector3 pos)
    {
        if (HitFX) EffectPlayer.PlayAtPos(HitFX, pos);
        if (HitSE) AudioSystem.Instance.PlayAtPos(HitSE, pos);
        SkillEffectBase[] effects = GetComponentsInChildren<SkillEffectBase>(true);
        for (int i = 0, length = effects.Length; i < length; ++i)
            effects[i].Execute(Master, pos);
    }

    public bool Interrupt()
    {
        if (!OnInterrupt()) return false;
        SkillStatus = Status.Idle;
        Master.OnSkillOver(this);
        fTimeCounter = 0;
        Target = null;
        return true;
    }

    protected virtual bool OnInterrupt() { return true; }
}

public struct Damage
{
    public WorldSetting.Effect type;
    public int value;
    public Damage(WorldSetting.Effect type, int value) { this.type = type; this.value = value; }
}

public class SkillData
{
    public int id;
    public string name;
    public float cd;
    public WorldSetting.Energy costType;
    public int cost;
}
