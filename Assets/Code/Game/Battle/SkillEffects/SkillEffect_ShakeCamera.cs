using UnityEngine;
using DG.Tweening;

public class SkillEffect_ShakeCamera : SkillEffectBase
{
    [CustomLabel("振动时间")]
    public int nDuration = 1000;
    [CustomLabel("震动强度")]
    public Vector3 vecStrength = new Vector3(1, 1, 0);
    [CustomLabel("离摄像头距离衰减")]
    public float fDistanceWeak = 0;

    protected override void OnExecute(InteractiveObj self, InteractiveObj target)
    {
        DoShake(target.BeAtkPoint.position);
    }

    void DoShake(Vector3 position)
    {
        if (!Camera.main) return;
        if (fDistanceWeak > 0)
        {
            float distance = Vector3.Distance(Camera.main.transform.position, position);
            if (fDistanceWeak < distance)
            {
                vecStrength /= distance / fDistanceWeak;
                if (vecStrength.sqrMagnitude < 0.5f) return;
            }
        }
        Camera.main.transform.DOShakePosition(nDuration / 1000f, vecStrength);
    }
}
