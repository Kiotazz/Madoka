using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour
{
    [CustomLabel("目标物体")]
    public Transform tsfTarget;
    [CustomLabel("旋转轴")]
    public Vector3 vecAxis;
    [CustomLabel("每秒角度")]
    public float fAngle = 30;

    void Update()
    {
        if (tsfTarget)
            transform.RotateAround(tsfTarget.position, vecAxis, fAngle * Time.deltaTime);
    }
}
