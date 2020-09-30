using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Q_MoveObj : QuickToolBase
{
    [System.Serializable]
    public struct PathInfo
    {
        [CustomLabel("路径点")]
        public Transform tsfPathPoint;
        [CustomLabel("移动到此的速度")]
        public float fMoveSpeed;
        [CustomLabel("在此停留时间")]
        public int nDelay;
    }

    [Header("行进路线")]
    public PathInfo[] walkPaths;
    [CustomLabel("行进音效")]
    public AudioClip clipPlayOnEffect;
    [CustomLabel("首次等待时间")]
    public int nFirstDelay = 0;
    [CustomLabel("默认移动速度")]
    public float fDefaultMoveSpeed = 1;
    [CustomLabel("首尾循环")]
    public bool bCircleMove = false;
    [CustomLabel("只移动一次")]
    public bool bPlayOnce = false;
    [CustomLabel("移动曲线")]
    public Ease easeType = Ease.Linear;
    [CustomLabel("停止时触发")]
    public EventWorker onModeStoped;

    int nCurIndex = 0;
    bool bForward = true;
    AudioPlayer playerOnEffect;

    bool bIsMoving = false;
    float fTimeCounter = 0;
    float fNextDelay = 0;
    Tweener curTween;

    protected override void OnStartWork()
    {
        if (walkPaths.Length < 1)
        {
            Debug.LogError(name + "未设置行进路径！");
            return;
        }
        transform.position = walkPaths[0].tsfPathPoint.position;
        fNextDelay = nFirstDelay;
        GameClient.Instance.NextTick(() => { playerOnEffect = AudioSystem.Instance.PlayOnTransform(clipPlayOnEffect, transform); }, nFirstDelay / 1000f);
    }

    protected override void OnStopWork()
    {
        if (curTween == null) return;
        if (curTween.IsPlaying())
            curTween.Kill();
        curTween = null;
        bIsMoving = false;
    }

    private void Update()
    {
        if (!IsWorking) return;
        if (!bIsMoving && (fTimeCounter += Time.deltaTime * 1000) > fNextDelay)
        {
            MoveToNext();
            fTimeCounter = 0;
        }
    }

    void MoveToNext()
    {
        if (walkPaths.Length < 1) return;
        if (bForward)
        {
            if ((++nCurIndex) >= walkPaths.Length)
            {
                OnEnd();
                if (bPlayOnce)
                {
                    return;
                }
                if (bCircleMove)
                {
                    nCurIndex = 0;
                }
                else
                {
                    bForward = false;
                    nCurIndex = walkPaths.Length - 2;
                }
            }
        }
        else if ((--nCurIndex) < 0)
        {
            bForward = true;
            nCurIndex = 1;
            OnEnd();
        }
        nCurIndex = Mathf.Clamp(nCurIndex, 0, walkPaths.Length - 1);
        PathInfo path = walkPaths[nCurIndex];
        float length = Vector3.Distance(transform.position, path.tsfPathPoint.position);
        float speed = path.fMoveSpeed > 0 ? path.fMoveSpeed : fDefaultMoveSpeed;
        fNextDelay = path.nDelay;
        bIsMoving = true;
        curTween = transform.DOMove(path.tsfPathPoint.position, length / speed);
        curTween.SetEase(easeType);
        curTween.onComplete = () => bIsMoving = false;
    }

    void OnEnd()
    {
        if (playerOnEffect)
            playerOnEffect.Recycle();
        onModeStoped?.DoTriggerEvents();
    }
}
