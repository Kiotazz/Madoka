using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawAttackRange : MonoBehaviour
{
    [CustomLabel("需要刷新")]
    public bool bNeedUpdate = false;
    InteractiveObj master;
    Search_Around targetRound;
    Search_Cone targetCone;

    LineRenderer lineSelf;


    // Start is called before the first frame update
    void Start()
    {
        master = GetComponentInParent<InteractiveObj>();
        targetRound = GetComponentInChildren<Search_Around>();
        targetCone = GetComponentInChildren<Search_Cone>();
        lineSelf = transform.GetOrAddComponent<LineRenderer>();
        lineSelf.enabled = false;
        if (master) master.OnDeath.AddListener(() =>
        {
            if (!lineSelf) lineSelf = GetComponent<LineRenderer>();
            if (lineSelf) lineSelf.enabled = false;
        });
        if (!bNeedUpdate) Execute();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (bNeedUpdate)
        {
            if (master && ThirdPersonPlayer.Instance)
            {
                float sqrDistance = master.transform.position.SqrDistanceWith(ThirdPersonPlayer.Instance.transform.position);
                if (sqrDistance > 300)
                {
                    if (!lineSelf) lineSelf = GetComponent<LineRenderer>();
                    if (lineSelf && lineSelf.enabled) lineSelf.enabled = false;
                    return;
                }
            }
            Execute();
        }
    }

    public void Execute()
    {
        if (master && !master.IsAlive) return;
        if (targetRound) DrawTool.DrawCircle(transform, transform.position, targetRound.fRange, Color.red);
        if (targetCone) DrawTool.DrawSector(transform, transform.position, targetCone.fAngle, targetCone.fRange / 2, Color.red);
    }
}
