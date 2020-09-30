using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_PickUp : SkillEffectBase
{
    [CustomLabel("拾取范围")]
    public float fDistance = 3;
    [CustomLabel("投掷力")]
    public float fThrowPower = 10;

    GameObject objPickedItem;
    //UnityChan.IKCtrlRightHand ikCtrl;

    protected override void OnExecute(InteractiveObj self, InteractiveObj target)
    {
        PickUpItem(self);
    }

    protected override void OnExecute(InteractiveObj self, Vector3 pos)
    {
        PickUpItem(self);
    }

    void PickUpItem(InteractiveObj self)
    {
        //if (!ikCtrl) ikCtrl = self.GetComponent<UnityChan.IKCtrlRightHand>();
        if (objPickedItem)
        {
            ThrowItem(self);
            return;
        }
        Collider[] items = Physics.OverlapSphere(self.transform.position, fDistance, 1 << LayerMask.NameToLayer("Accessable"));
        for (int i = 0, length = items.Length; i < length; ++i)
        {
            Q_SceneObj sceneObj = items[i].GetComponent<Q_SceneObj>();
            if (!sceneObj) continue;
            switch (sceneObj.objType)
            {
                case Q_SceneObj.Type.PickUpItem:
                    Rigidbody rigid = sceneObj.GetComponent<Rigidbody>();
                    Vector3 localPos = transform.localPosition;
                    Transform tsfSceneCamera = ThirdPersonPlayer.Instance.CameraController.transform;
                    rigid.constraints = RigidbodyConstraints.FreezeAll;
                    rigid.transform.SetParent(ThirdPersonPlayer.Instance.transform.Find("HandItemPos"), false);
                    rigid.transform.localPosition = Vector3.zero;
                    rigid.transform.localRotation = Quaternion.identity;
                    rigid.gameObject.SetLayer(self.gameObject.layer);
                    objPickedItem = sceneObj.gameObject;
                    //ikCtrl.isIkActive = true;
                    //ikCtrl.targetObj = sceneObj.transform;
                    return;
            }
        }
    }

    void ThrowItem(InteractiveObj self)
    {
        if (!objPickedItem)
        {
            Debug.LogError("没有可投掷的物体！");
            return;
        }
        Rigidbody rigid = objPickedItem.GetComponent<Rigidbody>();
        Vector3 localPos = transform.localPosition;
        Transform tsfSceneCamera = ThirdPersonPlayer.Instance.CameraController.transform;
        rigid.transform.parent = null;
        rigid.constraints = RigidbodyConstraints.None;
        rigid.gameObject.SetLayer(LayerMask.NameToLayer("Accessable"));
        Vector3 forward = tsfSceneCamera.forward;
        forward.y = 0;
        rigid.AddForce(forward * fThrowPower, ForceMode.VelocityChange);
        objPickedItem = null;
        //ikCtrl.isIkActive = false;
        //ikCtrl.targetObj = null;
    }
}
