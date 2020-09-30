using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SceneRotateButton : MonoBehaviour
{
    [CustomLabel("速度")]
    public int Speed = 0;

    public InteractiveObj Master { get; protected set; }
    public Rigidbody RigidSelf { get; private set; }
    public Rigidbody2D RigidFake { get; private set; }
    public bool IsAlive { get; protected set; } = true;

    SkillBase bindSkill;

    public void Init(InteractiveObj master)
    {
        Master = master;
        RigidSelf = GetComponentInChildren<Rigidbody>();
        RigidSelf.GetComponent<SceneRotateWorldButton>().Init(this);
        RigidSelf.gameObject.SetActive(false);
        RigidFake = GetComponentInChildren<Rigidbody2D>();
        RigidFake.GetComponent<SceneRotateFakeButton>().Init(this);
        RigidFake.gameObject.SetActive(true);
        bindSkill = GetComponentInChildren<SkillBase>();
        if (bindSkill)
        {
            bindSkill.Init(master);
            Transform tsf = master.transform.Find("SkillStartPos");
            bindSkill.transform.SetParent(tsf ? tsf : master.BeAtkPoint, false);
        }
    }

    private void Update()
    {
        if (bindSkill) bindSkill.DoUpdate(Time.deltaTime);
        if (!IsAlive) return;
        if (RigidFake.angularVelocity < -3000) ButtonAbandon();
    }

    public void ExecuteSkill()
    {
        if (!IsAlive) return;
        if (bindSkill) bindSkill.Execute(null);
    }

    private void ButtonAbandon()
    {
        if (!IsAlive) return;
        IsAlive = false;
        RigidSelf.transform.localRotation = RigidFake.transform.localRotation;
        RigidFake.gameObject.SetActive(false);
        RigidSelf.gameObject.SetActive(true);
        RigidSelf.AddRelativeForce(RigidFake.angularVelocity, 0, 0, ForceMode.Acceleration);
        RigidSelf.gameObject.SetLayer(1 << LayerMask.GetMask("Player"), true);
        Vector3 localPos = transform.localPosition;
        Quaternion localRot = RigidSelf.transform.localRotation;
        Transform tsfSceneCamera = ThirdPersonPlayer.Instance.CameraController.transform;
        RigidSelf.transform.parent = tsfSceneCamera;
        RigidSelf.transform.localPosition = localPos;
        RigidSelf.transform.localRotation = localRot;
        RigidSelf.transform.parent = null;
        RigidSelf.constraints = RigidbodyConstraints.None;
        //RigidSelf.transform.DOScale(new Vector3(2, 2, 2), 3);
        RigidSelf.AddForce(tsfSceneCamera.forward * 30 + Vector3.up * 10, ForceMode.VelocityChange);
    }
}
