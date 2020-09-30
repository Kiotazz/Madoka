using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneRotateFakeButton : MonoBehaviour
{
    public SceneRotateButton Control { get; protected set; }
    public Rigidbody2D RigidSelf { get; private set; }

    public void Init(SceneRotateButton control)
    {
        Control = control;
        RigidSelf = GetComponent<Rigidbody2D>();
    }

    private void OnMouseUpAsButton()
    {
        if (!Control.Master.IsAlive) return;
        RigidSelf.AddTorque(Control.Speed, ForceMode2D.Impulse);
        Control.ExecuteSkill();
    }
}
