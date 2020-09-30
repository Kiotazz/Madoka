using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldInteractObj : InteractiveObj
{
    static WorldInteractObj _instance;
    public static WorldInteractObj Instance
    {
        get
        {
            if (!_instance)
            {
                GameObject obj = new GameObject("WorldInteractObj");
                _instance = obj.AddComponent<WorldInteractObj>();
                if (Application.isPlaying) DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    protected override void Init()
    {
        WillNotBeFind = true;
        HP = long.MaxValue;
        ATK = 0;
        DEF = int.MaxValue;
        Camp = RealCamp = -10086;
        fBloodUIShowDistance = -10086;
    }

    protected override void OnUpdate(float deltaTime)
    {

    }
    public override InteractiveObj FindNextEnemy()
    {
        return null;
    }
    public override bool IsFindNextEnemy()
    {
        return false;
    }

    public override bool CanCastSkill(SkillBase skill)
    {
        return false;
    }

    protected override void OnDamage(Damage damage, InteractiveObj source)
    {

    }

    public override bool TryChangeSkillEnergy(WorldSetting.Energy type, float value)
    {
        return true;
    }

    public override bool HasSkillEnergy(WorldSetting.Energy type, float value)
    {
        return true;
    }
}
