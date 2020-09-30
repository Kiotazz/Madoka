using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Q_RotateChildOnMove : MonoBehaviour
{
    [CustomLabel("旋转速度")]
    public float fRotateSpeed = 1000;

    Vector3 vecLastPos;
    Transform target;

    void Awake()
    {
        target = transform.GetChild(0);
        vecLastPos = transform.position;
    }

    void Update()
    {
        if (vecLastPos == transform.position) return;
        Vector3 moveDir = transform.position - vecLastPos;
        transform.LookAt(transform.position + moveDir * 100);
        target.Rotate(new Vector3(fRotateSpeed * Mathf.Abs(moveDir.magnitude) * Time.deltaTime, 0, 0));
        vecLastPos = transform.position;
    }
}
