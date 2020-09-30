using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class InteractiveObj : MonoBehaviour
{
    private static int MaxID = 0;

    [CustomLabel("初始血量")]
    public long StartHP = 20;
    [CustomLabel("最高血量")]
    public long MaxHP = 20;
    [CustomLabel("攻击力")]
    public int ATK = 0;
    [CustomLabel("防御力")]
    public int DEF = 0;
    [CustomLabel("阵营")]
    public int Camp = 2;
    [CustomLabel("脱战距离")]
    public float OutBattleRange = 0;
    [CustomLabel("显示血条距离")]
    public float fBloodUIShowDistance = 20;
    [CustomLabel("血条预设体")]
    public GameObject prefabBloodUI;
    [CustomLabel("血条挂载点")]
    public Transform tsfBloodUIPoint;
    [CustomLabel("死亡音效")]
    public AudioClip clipDead;
    [CustomLabel("死亡特效")]
    public GameObject effectDead;

    public LongEvent OnHpChange { get; private set; } = new LongEvent();
    public NormalEvent OnInitOver { get; private set; } = new NormalEvent();
    public NormalEvent OnFightBegin { get; protected set; } = new NormalEvent();
    public NormalEvent OnFightOver { get; protected set; } = new NormalEvent();
    public NormalEvent OnDeath { get; protected set; } = new NormalEvent();

    public int ID { get; protected set; }
    public int RealCamp { get; protected set; }
    public Collider ColliderSelf { get; protected set; }
    public Asset_Role Role { get; protected set; }
    public bool IsTrigger { get; private set; } = false;
    public bool IsFog { get; private set; } = false;
    public bool IsAlive { get; protected set; } = true;
    public SkillBase CurrentSkill { get; protected set; }
    public InteractiveObj CurrentTarget { get; protected set; }
    public InteractiveObj ForceTarget { get; protected set; }
    public Transform BeAtkPoint { get { return Role ? Role.tsfAtkPoint : transform; } }
    public bool WillNotBeFind { get; set; } = false;
    long _selfHP = 0;
    public long HP
    {
        get { return _selfHP; }
        protected set
        {
            if (value > MaxHP) value = MaxHP;
            if (_selfHP == value) return;
            _selfHP = value;
            OnHpChange.Invoke(value);
        }
    }
    public float AttackMinRange
    {
        get
        {
            float range = float.MaxValue;
            for (int i = 0, length = listSkills.Count; i < length; ++i)
                if (listSkills[i].SkillMinRange < range)
                    range = listSkills[i].SkillMinRange;
            return range;
        }
    }
    public float AttackMaxRange
    {
        get
        {
            float range = 0;
            for (int i = 0, length = listSkills.Count; i < length; ++i)
                if (listSkills[i].SkillMaxRange > range)
                    range = listSkills[i].SkillMaxRange;
            return range;
        }
    }
    public bool IsFighting { get { return dicFightingObjs.Count > 0; } }
    public SceneObjHeadInfo UIHeadInfo { get; protected set; }

    protected Dictionary<WorldSetting.Energy, SkillEnergy> dicSkillEnergy = new Dictionary<WorldSetting.Energy, SkillEnergy>();
    protected List<ResumeEnergy> listResumeSkillEnergy = new List<ResumeEnergy>();
    protected List<SkillBase> listSkills = new List<SkillBase>();
    protected List<SearchEnemyBase> listSearchEnemy = new List<SearchEnemyBase>();
    protected Dictionary<int, InteractiveObj> dicFightingObjs = new Dictionary<int, InteractiveObj>();
    protected List<InteractiveObjExt> listExtModules = new List<InteractiveObjExt>();

    protected List<BuffBase> listBuff = new List<BuffBase>();

    private List<BuffBase> listRemoveBuff = new List<BuffBase>();
    private List<int> listRemoveEnemies = new List<int>();

    protected bool bCanClick = true;
    protected float fAttackMinRange = 0;
    protected float fAttackMaxRange = 0;

    void Start()
    {
        ID = ++MaxID;
        RealCamp = Camp;
        HP = StartHP;
        ColliderSelf = gameObject.GetComponent<Collider>();
        if (!ColliderSelf) ColliderSelf = gameObject.AddComponent<BoxCollider>();
        IsTrigger = ColliderSelf.isTrigger;
        Role = gameObject.GetComponentInChildren<Asset_Role>();
        if (Role) Role.Init(this);
        else if (this != WorldInteractObj.Instance) Debug.LogError(gameObject.name + "下没有找到Asset_Role！");

        dicSkillEnergy.Clear();
        SearchEnemyBase[] searchEnemies = GetComponentsInChildren<SearchEnemyBase>(true);
        for (int i = 0, length = searchEnemies.Length; i < length; ++i)
        {
            SearchEnemyBase single = searchEnemies[i];
            single.Init(this);
            listSearchEnemy.Add(single);
        }
        SkillEnergy[] skillResources = GetComponentsInChildren<SkillEnergy>(true);
        for (int i = 0, length = skillResources.Length; i < length; ++i)
        {
            SkillEnergy single = skillResources[i];
            single.Init(this);
            dicSkillEnergy[single.E_type] = single;
        }
        ResumeEnergy[] resumeSkillResources = GetComponentsInChildren<ResumeEnergy>(true);
        for (int i = 0, length = resumeSkillResources.Length; i < length; ++i)
        {
            ResumeEnergy single = resumeSkillResources[i];
            single.Init(this);
            listResumeSkillEnergy.Add(single);
        }
        SkillBase[] skills = GetComponentsInChildren<SkillBase>(true);
        for (int i = 0, length = skills.Length; i < length; ++i)
        {
            SkillBase single = skills[i];
            single.Init(this);
            listSkills.Add(single);
        }
        listSkills.Sort((a, b) => { return b.CastOrder - a.CastOrder; });

        fAttackMinRange = AttackMinRange;
        OutBattleRange = Mathf.Max(fAttackMaxRange = AttackMaxRange, OutBattleRange);
        Init();
        InitBloodUI();
        BattleManager.Instance.RegisterInteractiveObj(this);

        InteractiveObjExt[] exts = GetComponentsInChildren<InteractiveObjExt>(true);
        for (int i = 0, length = exts.Length; i < length; ++i)
        {
            InteractiveObjExt single = exts[i];
            single.Init(this);
            listExtModules.Add(single);
        }

        OnInitOver.Invoke();
    }
    protected virtual void Init() { }

    private void InitBloodUI()
    {
        if (!tsfBloodUIPoint)
        {
            tsfBloodUIPoint = new GameObject("BloodPoint").transform;
            tsfBloodUIPoint.SetParent(transform, false);
            Vector3 vecCenter = ColliderSelf.bounds.center;
            vecCenter.y = ColliderSelf.bounds.max.y + 0.3f;
            tsfBloodUIPoint.position = vecCenter;
        }
        if (!prefabBloodUI)
            prefabBloodUI = Resources.Load<GameObject>("GUI/SceneUI/SceneObjHeadInfo");
        if (!prefabBloodUI)
        {
            Debug.LogError("加载血条资源GUI/SceneUI/SceneObjHeadInfo失败！");
            return;
        }
        UIHeadInfo = Instantiate(prefabBloodUI).GetComponent<SceneObjHeadInfo>();
        UIHeadInfo.Init(this);
    }

    private void OnDestroy()
    {
        if (UIHeadInfo) UIHeadInfo.Destroy();
        BattleManager.Instance.UnregisterInteractiveObj(this);
    }

    public void DoUpdate(float deltaTime)
    {
        if (!IsAlive || !gameObject.activeInHierarchy) return;
        for (int i = 0, length = listBuff.Count; i < length; ++i)
        {
            if (listBuff[i].IsEnd)
                listRemoveBuff.Add(listBuff[i]);
            else
                listBuff[i].DoUpdate(deltaTime);
        }
        for (int i = 0, length = listRemoveBuff.Count; i < length; ++i)
        {
            BuffBase removeBuff = listRemoveBuff[i];
            listBuff.Remove(removeBuff);
            removeBuff.Remove(false);
        }
        listRemoveBuff.Clear();
        for (int i = 0, length = listResumeSkillEnergy.Count; i < length; ++i)
        {
            ResumeEnergy single = listResumeSkillEnergy[i];
            if (single) single.DoUpdate(deltaTime);
        }
        for (int i = 0, length = listSearchEnemy.Count; i < length; ++i)
        {
            SearchEnemyBase single = listSearchEnemy[i];
            if (single) single.DoUpdate(deltaTime);
        }
        for (int i = 0, length = listSkills.Count; i < length; ++i)
        {
            SkillBase single = listSkills[i];
            if (single) single.DoUpdate(deltaTime);
        }

        OnUpdate(deltaTime);
        UIHeadInfo.DoUpdate(deltaTime);
        for (int i = 0, length = listExtModules.Count; i < length; ++i)
        {
            InteractiveObjExt single = listExtModules[i];
            if (single) single.DoUpdate(deltaTime);
        }
    }
    protected virtual void OnUpdate(float deltaTime)
    {
        bool haveTarget = CurrentTarget;
        if (ForceTarget && ForceTarget.IsAlive && ForceTarget.transform.position.SqrDistanceWith(transform.position) <= fAttackMaxRange * fAttackMaxRange)
            CurrentTarget = ForceTarget;
        else if (IsFindNextEnemy())
            CurrentTarget = FindNextEnemy();
        if (CurrentSkill && CurrentSkill.IsWorking && CurrentSkill.IsForbidOtherSkills) return;
        for (int i = 0, length = listSkills.Count; i < length; ++i)
        {
            if (CanCastSkill(listSkills[i]) && listSkills[i].Execute(CurrentTarget) == SkillBase.CastResult.Success)
            {
                CurrentSkill = listSkills[i];
                OnAttackTarget();
                if (CurrentSkill.IsForbidOtherSkills)
                    break;
            }
        }
        if (!haveTarget && CurrentTarget)
            OnFightBegin.Invoke();
        else if (haveTarget && !CurrentTarget)
            OnFightOver.Invoke();
    }

    public void DoFixedUpdate(float fixedDeltaTime)
    {
        if (!gameObject.activeInHierarchy) return;
        for (int i = 0, length = listResumeSkillEnergy.Count; i < length; ++i)
        {
            ResumeEnergy single = listResumeSkillEnergy[i];
            if (single) single.DoFixedUpdate(fixedDeltaTime);
        }

        OnFixedUpdate(fixedDeltaTime);
    }
    protected virtual void OnFixedUpdate(float fixedDeltaTime) { }

    protected virtual void OnAttackTarget() { }

    public void ChangeToFog(bool isFog)
    {
        if (!ColliderSelf) return;
        IsFog = isFog;
        ColliderSelf.isTrigger = isFog || IsTrigger;
        Renderer renderer = GetComponent<Renderer>();
        if (isFog)
        {
            if (renderer)
                Common.SetMaterialRenderingMode(renderer.material, Common.RenderingMode.Transparent);
            renderer.material.DOFade(0.3f, 0.3f);
        }
        else
        {
            GetComponent<Renderer>().material.DOFade(1, 0.3f).onComplete = () =>
            {
                if (renderer)
                    Common.SetMaterialRenderingMode(renderer.material, Common.RenderingMode.Opaque);
            };
        }
    }

    public bool IsEnemy(int camp)
    {
        return Camp != camp;
    }

    public void ClickObj() { if (bCanClick) OnClickObj(); }
    protected virtual void OnClickObj()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (!renderer) return;
        bCanClick = false;
        Color c = renderer.material.color;
        renderer.material.DOColor(Color.yellow, 0.1f).onComplete = () =>
        {
            renderer.material.DOColor(c, 0.1f).onComplete = () => bCanClick = true;
        };
    }

    public void DoDamage(Damage damage, InteractiveObj source)
    {
        if (!IsAlive || damage.value == 0) return;
        OnDamage(damage, source);
        AddFightingEnemey(source);
        if (HP < 1) Death();
    }
    protected virtual void OnDamage(Damage damage, InteractiveObj source) { HP -= CalDefDamage(damage.value); }

    public void DoHeal(int heal)
    {
        if (!IsAlive) return;
        OnHeal(heal);
    }
    protected virtual void OnHeal(int heal) { HP += CalDefHeal(heal); }

    public SkillEnergy GetSkillEnergy(WorldSetting.Energy type)
    {
        return dicSkillEnergy.ContainsKey(type) ? dicSkillEnergy[type] : null;
    }
    public virtual bool HasSkillEnergy(WorldSetting.Energy type, float value)
    {
        SkillEnergy skillResource = GetSkillEnergy(type);
        return skillResource && skillResource.Current >= value;
    }
    /// <summary>
    /// 尝试增/减技能资源
    /// </summary>
    /// <param name="type">技能资源类型</param>
    /// <param name="value">改变数量</param>
    /// <param name="allowFree">当目标资源不存在时也判断为成功</param>
    /// <returns></returns>
    public virtual bool TryChangeSkillEnergy(WorldSetting.Energy type, float value)
    {
        SkillEnergy skillEnergy = GetSkillEnergy(type);
        return skillEnergy && skillEnergy.ChangeValue(value);
    }

    protected void Death()
    {
        HP = 0;
        for (int i = 0, length = listBuff.Count; i < length; ++i)
            listBuff[i].Remove(true);
        listBuff.Clear();
        ForceTarget = CurrentTarget = null;
        dicFightingObjs.Clear();
        AudioSystem.Instance.PlayAtPos(clipDead, transform.position);
        EffectPlayer.PlayAtPos(effectDead, transform.position);
        OnSelfDeath();
        IsAlive = false;
        UIHeadInfo.DoUpdate(0);
        OnDeath.Invoke();
    }
    protected virtual void OnSelfDeath()
    {
        Debug.Log(gameObject.name + " Default Death");
        Destroy(gameObject);
    }

    public void AddFightingEnemey(InteractiveObj obj)
    {
        if (obj && obj.IsEnemy(Camp) && !obj.WillNotBeFind) dicFightingObjs[obj.ID] = obj;
    }

    public void RemoveFightingEnemey(int id)
    {
        if (CurrentTarget.ID == id) CurrentTarget = null;
        dicFightingObjs.Remove(id);
    }

    public virtual InteractiveObj FindNextEnemy()
    {
        InteractiveObj nearestTarget = null;
        float nearestDistance = 100000000;
        listRemoveEnemies.Clear();
        foreach (var enemy in dicFightingObjs.Values)
        {
            if (!enemy || !enemy.IsAlive || !enemy.IsEnemy(Camp) || enemy.WillNotBeFind ||
                enemy.transform.position.SqrDistanceWith(transform.position) > OutBattleRange * OutBattleRange)
            {
                listRemoveEnemies.Add(enemy.ID);
                continue;
            }
            float distance = enemy.transform.position.SqrDistanceWith(transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTarget = enemy;
            }
        }
        for (int i = 0, length = listRemoveEnemies.Count; i < length; ++i)
            dicFightingObjs.Remove(listRemoveEnemies[i]);
        return nearestTarget;
    }

    public void AdjustProjector(Projector projector, float projectorHeight = 1)
    {
        if (ColliderSelf)
        {
            var size = ColliderSelf.bounds.size.XZ().magnitude;
            projector.fieldOfView = (Mathf.Atan2(size / 2.0f, projectorHeight) * 180f / Mathf.PI) * 2;
        }
        else
        {
            projector.fieldOfView = 30;
        }
    }

    public virtual bool IsFindNextEnemy()
    {
        if (!CurrentTarget || !CurrentTarget.IsAlive || !CurrentTarget.IsEnemy(Camp)) return true;
        return CurrentTarget.transform.position.SqrDistanceWith(transform.position) > fAttackMaxRange * fAttackMaxRange;
    }
    public virtual bool CanCastSkill(SkillBase skill) { return skill; }
    public virtual void OnSkillStart(SkillBase skill) { }
    public virtual void OnSkillOver(SkillBase skill)
    {
        if (Role && (!CurrentSkill || !CurrentSkill.IsWorking))
            Role.PlayAction(Role.strNormalAnimName);
    }
    public virtual void OnAnimLogicOver(int nameHash) { }

    public virtual void OnAnimStateExit(int nameHash) { if (Role) Role.PlayAction(Role.strNormalAnimName); }

    public void AddBuff(BuffBase buff)
    {
        listBuff.Add(buff);
        listBuff.Sort((a, b) => { return a.nOrder - b.nOrder; });
    }

    public int CalAtkDamage(int damage)
    {
        float fDamage = damage + ATK;
        for (int i = 0, length = listBuff.Count; i < length; ++i)
            fDamage = listBuff[i].CalAtkDamage(fDamage);
        return Mathf.CeilToInt(fDamage);
    }

    public int CalDefDamage(int damage)
    {
        float fDamage = damage;
        for (int i = 0, length = listBuff.Count; i < length; ++i)
            fDamage = listBuff[i].CalDefDamage(fDamage);
        return Mathf.Max(Mathf.CeilToInt(fDamage) - DEF, 0);
    }

    public int CalAtkHeal(int heal)
    {
        float fHeal = heal + ATK;
        for (int i = 0, length = listBuff.Count; i < length; ++i)
            fHeal = listBuff[i].CalAtkHeal(fHeal);
        return Mathf.CeilToInt(fHeal);
    }

    public int CalDefHeal(int heal)
    {
        float fHeal = heal;
        for (int i = 0, length = listBuff.Count; i < length; ++i)
            fHeal = listBuff[i].CalDefHeal(fHeal);
        return Mathf.CeilToInt(fHeal);
    }
}
