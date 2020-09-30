using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResumeEnergy : MonoBehaviour
{
    public bool IgnoreTimeScale = true;
    public WorldSetting.Energy E_Type = WorldSetting.Energy.Mana;
    public float Interval = 1f;
    public float Value = 3f;

    public InteractiveObj Master { get; protected set; }

    SkillEnergy tarSkillEnergy;
    float counter = 0;

    public void Init(InteractiveObj master)
    {
        Master = master;
        counter = 0;
        tarSkillEnergy = master.GetSkillEnergy(E_Type);
    }

    public void DoUpdate(float deltaTime)
    {
        if (IgnoreTimeScale && (counter += deltaTime) > Interval && tarSkillEnergy)
        {
            counter = 0;
            tarSkillEnergy.ChangeValue(Value);
        }
    }

    public void DoFixedUpdate(float fixedDeltaTime)
    {
        if (!IgnoreTimeScale && (counter += fixedDeltaTime) > Interval && tarSkillEnergy)
        {
            counter = 0;
            tarSkillEnergy.ChangeValue(Value);
        }
    }
}
