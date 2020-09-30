using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Q_ObjGenerator : QuickToolBase
{
    [CustomLabel("要创建的预设体")]
    public GameObject objPrefab;
    [CustomLabel("目标父节点")]
    public Transform tsfGenerateRoot;
    [CustomLabel("创建间隔")]
    public int nInterval = 1000;

    float fTimeCounter = 0;

    void Generate()
    {
        if (!tsfGenerateRoot) tsfGenerateRoot = transform;
        GameObject obj = Instantiate(objPrefab, tsfGenerateRoot);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        InteractiveObj[] sceneObjs = obj.GetComponentsInChildren<InteractiveObj>(true);
    }

    private void Update()
    {
        if (!IsWorking) return;
        if ((fTimeCounter += Time.deltaTime * 1000) > nInterval)
        {
            Generate();
            fTimeCounter = 0;
        }
    }
}
