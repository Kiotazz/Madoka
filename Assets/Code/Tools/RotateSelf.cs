using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSelf : MonoBehaviour
{
    public Vector3 vecAxis;
    public float fRotateSpeed;

    void Update()
    {
        transform.Rotate(vecAxis, fRotateSpeed * Time.deltaTime);
    }
}
