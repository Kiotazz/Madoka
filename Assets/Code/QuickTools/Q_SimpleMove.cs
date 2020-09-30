using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Q_SimpleMove : MonoBehaviour
{
    public Vector3 vecDirection;
    public float fMinSpeed = 1f;
    public float fMaxSpeed = 3f;
    public float fLifeTime = 999999999;

    float _speed = 1f;
    float _destroyTime = 0;

    void Start()
    {
        _speed = Random.Range(fMinSpeed, fMaxSpeed);
        _destroyTime = Time.timeSinceLevelLoad + fLifeTime;
    }


    void Update()
    {
        transform.Translate(vecDirection * _speed, Space.World);
        if (Time.timeSinceLevelLoad > _destroyTime)
            Destroy(gameObject);
    }
}
