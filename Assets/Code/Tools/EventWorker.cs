using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventWorker : MonoBehaviour
{
    [CustomLabel("立即触发事件")]
    public bool bAutoEvent = false;
    [CustomLabel("触发延时")]
    public int nEventDelay = 0;
    [CustomLabel("触发后立即隐藏本物体")]
    public bool bDisableAfterEnd = false;
    [CustomLabel("触发后销毁本物体")]
    public bool bDestroyAfterEnd = true;
    [CustomLabel("触发音效")]
    public AudioClip clipPlayOnEnd;
    [CustomLabel("音效跟随物体")]
    public bool bPlayAudioByTransform = false;
    [Header("触发时激活"), CustomLabel("物体")]
    public GameObject[] objSetActiveWhenEnd;
    [Header("触发时隐藏"), CustomLabel("物体")]
    public GameObject[] objSetDisableWhenEnd;
    [Header("触发时销毁"), CustomLabel("物体")]
    public GameObject[] objDestroyWhenEnd;
    [Header("触发时通知开始"), CustomLabel("物体")]
    public GameObject[] objStartWorkWhenEnd;
    [Header("触发时通知暂停"), CustomLabel("物体")]
    public GameObject[] objStopWorkWhenEnd;

    private void Start()
    {
        if (bAutoEvent) DoTriggerEvents();
    }

    public void DoTriggerEvents()
    {
        if (nEventDelay > 0)
            GameClient.Instance.NextTick(RealDoEvents, nEventDelay / 1000f);
        else
            RealDoEvents();
        if (bDisableAfterEnd) gameObject.SetActive(false);
    }

    void RealDoEvents()
    {
        if (clipPlayOnEnd)
        {
            AudioPlayer p = null;
            if (bPlayAudioByTransform)
                p = AudioSystem.Instance.PlayOnTransform(clipPlayOnEnd, transform);
            else
                p = AudioSystem.Instance.PlayAtPos(clipPlayOnEnd, transform.position);
            if (p)
            {
                p.audioSource.spatialBlend = 1f;
                p.audioSource.volume = 1f;
                p.audioSource.maxDistance = 300f;
            }
        }
        if (objSetActiveWhenEnd != null)
        {
            for (int i = 0, length = objSetActiveWhenEnd.Length; i < length; ++i)
            {
                if (objSetActiveWhenEnd[i])
                    objSetActiveWhenEnd[i].SetActive(true);
            }
        }
        if (objSetDisableWhenEnd != null)
        {
            for (int i = 0, length = objSetDisableWhenEnd.Length; i < length; ++i)
            {
                if (objSetDisableWhenEnd[i])
                    objSetDisableWhenEnd[i].SetActive(false);
            }
        }
        if (objDestroyWhenEnd != null)
        {
            for (int i = 0, length = objDestroyWhenEnd.Length; i < length; ++i)
            {
                if (objDestroyWhenEnd[i])
                {
                    Destroy(objDestroyWhenEnd[i]);
                }
            }
        }
        if (objStartWorkWhenEnd != null)
        {
            for (int i = 0, length = objStartWorkWhenEnd.Length; i < length; ++i)
            {
                if (!objStartWorkWhenEnd[i]) continue;
                if (objStartWorkWhenEnd[i] == gameObject)
                {
                    Debug.LogError("通知对象不能是自身！！");
                    continue;
                }
                QuickToolBase[] quickTools = objStartWorkWhenEnd[i].GetComponentsInChildren<QuickToolBase>(true);
                for (int j = 0, lenTools = quickTools.Length; j < length; ++j)
                    quickTools[j].StartWork();
            }
        }
        if (objStopWorkWhenEnd != null)
        {
            for (int i = 0, length = objStopWorkWhenEnd.Length; i < length; ++i)
            {
                if (!objStopWorkWhenEnd[i]) continue;
                if (objStopWorkWhenEnd[i] == gameObject)
                {
                    Debug.LogError("通知对象不能是自身！！");
                    continue;
                }
                QuickToolBase[] quickTools = objStopWorkWhenEnd[i].GetComponentsInChildren<QuickToolBase>(true);
                for (int j = 0, lenTools = quickTools.Length; j < length; ++j)
                    quickTools[j].StopWork();
            }
        }
        if (bDestroyAfterEnd) Destroy(gameObject);
    }
}
