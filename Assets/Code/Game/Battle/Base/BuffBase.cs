using System.Collections.Generic;
using UnityEngine;

public class BuffBase
{
    public static implicit operator bool(BuffBase buff) { return buff != null; }

    public enum Type
    {
        Buff,
        Debuff,
    }

    public int nOrder = 0;
    public int nDuration = 0;
    public Type eBuffType = Type.Buff;
    public InteractiveObj Master;
    public InteractiveObj Target;

    float startTime = 0;
    bool alive = false;

    public float RemainingTime { get { return Mathf.Max(nDuration / 1000f - Time.timeSinceLevelLoad + startTime, 0); } }
    public bool IsEnd { get { return (startTime + nDuration / 1000f) > Time.timeSinceLevelLoad; } }

    public void Init(int order, int duration, Type type, InteractiveObj master, InteractiveObj target)
    {
        nOrder = order;
        nDuration = duration;
        eBuffType = type;
        Master = master;
        Target = target;
        RefreshTime();
    }

    public void RefreshTime() { startTime = Time.timeSinceLevelLoad; }

    public virtual float CalAtkDamage(float damage)
    {
        return damage;
    }

    public virtual float CalDefDamage(float damage)
    {
        return damage;
    }

    public virtual float CalAtkHeal(float heal)
    {
        return heal;
    }

    public virtual float CalDefHeal(float heal)
    {
        return heal;
    }

    public void Remove(bool isDead)
    {
        alive = false;
        OnRemove(isDead);
    }

    protected virtual void OnRemove(bool isDead)
    {

    }

    public virtual void DoUpdate(float deltaTime)
    {

    }
}
