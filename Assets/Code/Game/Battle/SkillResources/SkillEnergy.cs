using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEnergy : MonoBehaviour
{
    [CustomLabel("资源类型")]
    public WorldSetting.Energy E_type = WorldSetting.Energy.Mana;
    [CustomLabel("初始值")]
    public float StartValue = 0;
    [CustomLabel("最大值")]
    public float MaxValue = 0;

    public float Current { get; protected set; }
    public InteractiveObj Master { get; protected set; }

    public void Init(InteractiveObj master)
    {
        Master = master;
        Current = MaxValue > 0 ? Mathf.Min(StartValue, MaxValue) : StartValue;
    }

    public bool ChangeValue(float value)
    {
        if (Current + value < 0) return false;
        if ((Current += value) > MaxValue && MaxValue > 0) Current = MaxValue;
        return true;
    }
}
