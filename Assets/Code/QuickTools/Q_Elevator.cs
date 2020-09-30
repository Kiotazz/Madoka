using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Q_Elevator : MonoBehaviour
{
    BoxCollider colMyself;
    Vector3 vecLastPos;

    private void Awake()
    {
        colMyself = GetComponent<BoxCollider>();
        vecLastPos = transform.position;
    }

    void Update()
    {
        ThirdPersonPlayer p = ThirdPersonPlayer.Instance;
        if (!p) return;
        Vector3 pos = p.Body.Position;
        pos.y = transform.position.y;
        if (colMyself.bounds.Contains(pos))
            p.Body.MoveBy(transform.position - vecLastPos);
        vecLastPos = transform.position;
    }
}
