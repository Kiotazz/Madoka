using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asset_Role : MonoBehaviour
{
    [CustomLabel("受击特效挂载点")]
    public Transform tsfAtkPoint;

    [Header("速度等级"), CustomLabel("速度")]
    public float[] fSpeedArray = new float[] { 1, 3.5f };

    [CustomLabel("普通状态名")]
    public string strNormalAnimName = "Move";
    [CustomLabel("受击状态名")]
    public string strDamageAnimName = "Damage";
    [CustomLabel("死亡状态名")]
    public string strDeathAnimName = "Die";

    public InteractiveObj Master { get; private set; }
    public Animator AnimatorSelf { get; protected set; }

    float curSpeed = -1;

    public void Init(InteractiveObj master)
    {
        Master = master;
        AnimatorSelf = GetComponent<Animator>();
        if (!tsfAtkPoint) tsfAtkPoint = transform;
        PlayAction(strNormalAnimName, 0.1f);
    }

    public void PlayAction(string name, float crossFadeTime = 0.1f)
    {
        if (!AnimatorSelf || !Master.IsAlive) return;
        int layer = -1;
        int nameHash = Animator.StringToHash(name);
        for (int i = 0, length = AnimatorSelf.layerCount; i < length; ++i)
        {
            if (AnimatorSelf.HasState(i, nameHash))
            {
                layer = i;
                break;
            }
        }
        if (layer < 0)
        {
            layer = 0;
            nameHash = Animator.StringToHash(strNormalAnimName);
        }
        if (crossFadeTime > 0)
            AnimatorSelf.CrossFade(nameHash, crossFadeTime, layer);
        else
            AnimatorSelf.Play(nameHash, layer);
    }

    public bool IsPlaying(string name)
    {
        if (!AnimatorSelf) return false;
        for (int i = 0, length = AnimatorSelf.layerCount; i < length; ++i)
            if (AnimatorSelf.GetCurrentAnimatorStateInfo(i).IsName(name))
                return true;
        return false;
    }

    public void SetMoveSpeed(float speed)
    {
        if (!AnimatorSelf) return;
        float deltaSpeed = 0;
        for (int i = 0, length = fSpeedArray.Length; i < length; ++i)
        {
            if (speed < fSpeedArray[i])
            {
                float lastLevel = i > 0 ? fSpeedArray[i - 1] : 0;
                deltaSpeed += (speed - lastLevel) / (fSpeedArray[i] - lastLevel) * 0.5f;
                break;
            }
            else
            {
                deltaSpeed += 0.5f;
            }
        }
        if (deltaSpeed.Equals(curSpeed)) return;
        AnimatorSelf.SetFloat("MoveSpeed", curSpeed = deltaSpeed);
    }
}
