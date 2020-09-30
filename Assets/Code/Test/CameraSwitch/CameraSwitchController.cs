using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CameraSwitchController : MonoBehaviour
{
    BackupCameraProjectionChange viewSwitcher;
    Camera cameraSelf;
    Character Character;

    Transform tsfPosPoint;
    Transform tsf2DPos;
    Transform tsf3DPos;

    bool bIsChanging = false;
    bool bIsTweenRotation = false;
    bool bInited = false;

    bool _currentIs2D;
    public bool CurrentIs2D { get { return _currentIs2D; } }
    public bool CanSwitch { get { return !bIsChanging; } }

    public void Init(Character chara, Transform tsfPointsRoot, Transform tsf2D, Transform tsf3D)
    {
        viewSwitcher = GetComponent<BackupCameraProjectionChange>();
        cameraSelf = GetComponent<Camera>();
        Character = chara;
        tsfPosPoint = tsfPointsRoot;
        tsf2DPos = tsf2D;
        tsf3DPos = tsf3D;
        _currentIs2D = cameraSelf.orthographic;
        bInited = true;
    }

    public void Switch(System.Action callback) { Switch(!CurrentIs2D, callback); }
    public void Switch(bool to2D, System.Action callback)
    {
        if (bIsChanging || !viewSwitcher.CanChange || to2D == CurrentIs2D) return;
        bIsChanging = true;
        viewSwitcher.ChangeProjection = true;

        tsfPosPoint.rotation = Quaternion.identity;

        Vector3 vec = to2D ? tsf2DPos.position : tsf3DPos.position;
        if (to2D) vec.y = Character.transform.position.y;
        Tweener tween = gameObject.transform.DOMove(vec, viewSwitcher.ProjectionChangeTime - 0.25f);
        tween.SetEase(Ease.InOutQuad);
        tween.onComplete = () =>
         {
             bIsTweenRotation = true;
             tsfPosPoint.rotation = Quaternion.identity;

             if (to2D) gameObject.transform.DOMove(tsf2DPos.position, 0.3f);
             Tweener tween2 = gameObject.transform.DORotateQuaternion(to2D ? tsf2DPos.rotation : tsf3DPos.rotation, 0.3f);
             tween2.SetEase(Ease.InOutQuad);
             tween2.onComplete = () =>
             {
                 _currentIs2D = cameraSelf.orthographic;
                 bIsTweenRotation = bIsChanging = false;
                 if (callback != null)
                     callback();
             };
         };
    }

    private void LateUpdate()
    {
        if (!bInited || !BattleManager.Instance.IsBattleBegin) return;
        if (bIsChanging)
        {
            if (!bIsTweenRotation)
                cameraSelf.transform.LookAt(Character.transform);
            return;
        }
        tsfPosPoint.rotation = Quaternion.identity;

        transform.position = CurrentIs2D ? tsf2DPos.position : tsf3DPos.position;
    }
}
