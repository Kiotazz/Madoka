using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    public Transform tsfCameraMoveRoot;
    public Transform tsfCameraRotateRoot;
    public Camera cameraMain;
    public Camera cameraCopy;
    public float fDragCameraSpeed = 22;
    public int nDragCameraWaitNum = 1;
    public float fRotateCameraHoriSpeed = 1;
    public float fRotateCameraVertSpeed = 0.07f;
    public float fRotateCameraMouseSpeed = 0.2f;
    public float fRotateCameraAutoHoriSpeed = 100;
    public float fRotateCameraAutoVertSpeed = 100;
    public int nCameraMinVertAngle = 30;
    public int nCameraMaxVertAngle = 85;
    public int nCameraZoomOnePixels = 20;
    public int nCameraMinFov = 18;
    public int nCameraMaxFov = 50;
    public Vector2 vecCameraMinPos = new Vector2(-25, -20);
    public Vector2 vecCameraMaxPos = new Vector2(20, 20);

    public bool bIsAutoRotateLeft = false;
    public bool bIsAutoRotateRight = false;
    public bool bIsAutoRotateUp = false;
    public bool bIsAutoRotateDown = false;

    public Camera CameraWorld { get { return cameraMain; } }
    public bool IsRotateMove { get; protected set; } = false;
    public bool IsCameraMoving { get; protected set; } = false;
    public bool IsCameraRotating { get; protected set; } = false;

    Vector2 vecCameraRotateDelta = Vector2.zero;
    Vector3 vecScreenCenter = new Vector3(Screen.width / 2, Screen.height, 0);
    int nDragWaitCounter = 0;
    bool bIsRunOnWindows = false;

    public void Init()
    {
        bIsRunOnWindows = Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer;
        vecScreenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        cameraCopy.CopyFrom(CameraWorld);
        ResetCameraToStartPos();
    }

    public void OnPress(bool isPress)
    {
        nDragWaitCounter = 0;
    }

    public void OnDrag(Vector2 delta)
    {
        if (IsRotateMove)
            CameraRotate(vecCameraRotateDelta);
        else if (++nDragWaitCounter > nDragCameraWaitNum)
        {
            delta.x = delta.x / Screen.width;
            delta.y = delta.y / Screen.height;
            CameraMove(delta);
        }
    }

    // Camera Operate
    void CameraRotate(Vector2 delta)
    {
        //delta calculate in Update function
        if (delta.x == 0 && delta.y == 0) return;
        StopCameraMove();
        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
            tsfCameraRotateRoot.RotateAround(tsfCameraMoveRoot.position, Vector3.up, delta.x);
        else
        {
            Vector3 vecHitPosToMe = tsfCameraRotateRoot.position - tsfCameraMoveRoot.position;
            Vector3 vecCameraForward = new Vector3(vecHitPosToMe.x, 0, vecHitPosToMe.z);
            float curAngle = Vector3.Angle(vecHitPosToMe.normalized, vecCameraForward.normalized);
            //local angle = -delta.y * Time.deltaTime * 3;
            float angle = delta.y > 0 ? Mathf.Clamp(-delta.y, Mathf.Min(nCameraMinVertAngle - curAngle, 0), 0) :
                Mathf.Clamp(-delta.y, 0, Mathf.Max(nCameraMaxVertAngle - curAngle, 0));
            if (Mathf.Abs(angle) > 0.1)
                tsfCameraRotateRoot.RotateAround(tsfCameraMoveRoot.position, tsfCameraRotateRoot.right, angle);
        }

        tsfCameraRotateRoot.parent = transform;
        tsfCameraRotateRoot.LookAt(tsfCameraMoveRoot);
        tsfCameraMoveRoot.eulerAngles = new Vector3(0, tsfCameraRotateRoot.eulerAngles.y, 0);
        tsfCameraRotateRoot.parent = tsfCameraMoveRoot;
        //DoTween会抖动，原因不明
        CameraWorld.transform.position = tsfCameraRotateRoot.position;
        CameraWorld.transform.LookAt(tsfCameraMoveRoot);
        //IsCameraRotating = true;
        //CameraWorld.transform.DOMove(tsfCameraRotateRoot.position, 0.05f).onComplete = () =>
        //{
        //    CameraWorld.transform.LookAt(tsfCameraMoveRoot);
        //    IsCameraRotating = false;
        //};
    }

    void CameraMove(Vector2 delta)
    {
        if (IsCameraRotating) return;
        CameraMoveTo(tsfCameraMoveRoot.position + tsfCameraMoveRoot.right * -delta.x * fDragCameraSpeed + tsfCameraMoveRoot.forward * -delta.y * fDragCameraSpeed);
    }

    void CameraMoveTo(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, vecCameraMinPos.x, vecCameraMaxPos.x);
        pos.z = Mathf.Clamp(pos.z, vecCameraMinPos.y, vecCameraMaxPos.y);
        tsfCameraMoveRoot.position = pos;
        IsCameraMoving = true;
        CameraWorld.transform.DOMove(tsfCameraRotateRoot.position, 0.05f).onComplete = () => { IsCameraMoving = false; };
    }

    void StopCameraMove()
    {
        if (!IsCameraMoving || !CameraWorld) return;
        IsCameraMoving = false;
        if (CameraWorld.transform.DOKill() < 1) return;
        tsfCameraRotateRoot.parent = transform;
        tsfCameraRotateRoot.position = CameraWorld.transform.position;
        RaycastHit hit;
        if (!Physics.Raycast(cameraCopy.ScreenPointToRay(vecScreenCenter), out hit, 10000, 1 << LayerMask.NameToLayer("Ground")))
        {
            Debug.LogError("重置摄像机位置失败！");
            return;
        }
        tsfCameraMoveRoot.position = hit.point;
        tsfCameraRotateRoot.parent = tsfCameraMoveRoot;
    }

    void ResetCameraToStartPos()
    {
        tsfCameraRotateRoot.parent = transform;
        tsfCameraRotateRoot.position = new Vector3(0, 20, -20);
        tsfCameraMoveRoot.eulerAngles = Vector3.zero;
        tsfCameraRotateRoot.eulerAngles = new Vector3(45, 0, 0);

        RaycastHit hit;
        if (!Physics.Raycast(cameraCopy.ScreenPointToRay(vecScreenCenter), out hit, 10000, 1 << LayerMask.NameToLayer("Ground")))
        {
            Debug.LogError("重置摄像机位置失败！");
            return;
        }
        tsfCameraMoveRoot.position = hit.point;
        tsfCameraRotateRoot.parent = tsfCameraMoveRoot;
        cameraCopy.fieldOfView = 30;
        CameraWorld.transform.position = tsfCameraRotateRoot.position;
        CameraWorld.transform.eulerAngles = tsfCameraRotateRoot.eulerAngles;
        CameraWorld.fieldOfView = 30;
    }
    // Camera Operate end

    Vector3 GetGroundPosAtScreenPos(Vector3 screenPosition) // return worldPos, floorIndex
    {
        RaycastHit hit;
        Ray ray = cameraCopy.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out hit, 10000, 1 << LayerMask.NameToLayer("Ground")))
            return hit.point;
        if (Physics.Raycast(ray, out hit, 10000, 1 << LayerMask.NameToLayer("Default")))
            return hit.point;
        return Vector3.zero;
    }

    GameObject TryGetClickObjByLayers(string[] layers, int rayLength = 10000)
    {
        RaycastHit hit;
        Ray ray = CameraWorld.ScreenPointToRay(Input.mousePosition);
        for (int i = 0, length = layers.Length; i < length; ++i)
            if (Physics.Raycast(ray, out hit, rayLength, 1 << LayerMask.NameToLayer(layers[i])))
                return hit.collider.gameObject;
        return null;
    }

    Vector3 lastTouchPos1 = Vector3.zero;
    Vector3 lastTouchPos2 = Vector3.zero;
    Vector3 lastTouch2ToTouch1 = Vector3.zero;
    float lastTouchDistance = 0;
    Vector3 curTouchPos1 = Vector3.zero;
    Vector3 curTouchPos2 = Vector3.zero;
    Vector3 curTouch2ToTouch1 = Vector3.zero;
    float curTouchDistance = 0;
    Vector2 lastMousePos = new Vector2(0, 0);
    public void DoUpdate(float deltaTime)
    {
        if (!CameraWorld) return;
        if (IsCameraRotating)
            CameraWorld.transform.LookAt(tsfCameraMoveRoot);

        // Camera Rotate && Zoom
        float tarFov = CameraWorld.fieldOfView;
        if (Input.touchCount > 1)
        {
            Touch touch1 = Input.GetTouch(0), touch2 = Input.GetTouch(1);
            curTouchPos1 = touch1.position;
            curTouchPos2 = touch2.position;
            curTouchDistance = Vector3.Distance(curTouchPos1, curTouchPos2);
            curTouch2ToTouch1 = curTouchPos2 - curTouchPos1;
            if (touch2.phase == TouchPhase.Began) // Rotate Begin
            {
                lastTouchPos1 = curTouchPos1;
                lastTouchPos2 = curTouchPos2;
                lastTouch2ToTouch1 = curTouch2ToTouch1;
                lastTouchDistance = curTouchDistance;
                vecCameraRotateDelta = Vector2.zero;
                IsRotateMove = true;
                StopCameraMove();
            }
            else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                if (IsRotateMove)
                {
                    float xOffset1 = curTouchPos1.x - lastTouchPos1.x, xOffset2 = curTouchPos2.x - lastTouchPos2.x;
                    float yOffset1 = curTouchPos1.y - lastTouchPos1.y, yOffset2 = curTouchPos2.y - lastTouchPos2.y;
                    if (yOffset1 * yOffset2 > 0 && Mathf.Abs(yOffset1) > Mathf.Abs(xOffset1) && Mathf.Abs(yOffset2) > Mathf.Abs(xOffset2))
                        vecCameraRotateDelta = new Vector2(0, (yOffset1 + yOffset2) / 2 * fRotateCameraVertSpeed);
                    else
                    {
                        float angle = Vector3.Angle(lastTouch2ToTouch1, curTouch2ToTouch1) * Mathf.Sign(Vector3.Cross(lastTouch2ToTouch1, curTouch2ToTouch1).z);
                        vecCameraRotateDelta = new Vector2(angle * fRotateCameraHoriSpeed, 0);
                    }
                }
                tarFov = tarFov - (curTouchDistance - lastTouchDistance) / nCameraZoomOnePixels;
                lastTouchPos1 = new Vector3(curTouchPos1.x, curTouchPos1.y, 0);
                lastTouchPos2 = new Vector3(curTouchPos2.x, curTouchPos2.y, 0);
                lastTouch2ToTouch1 = new Vector3(curTouch2ToTouch1.x, curTouch2ToTouch1.y, 0);
                lastTouchDistance = curTouchDistance;
            }
            if (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended)
                IsRotateMove = false;
        }


        if (bIsRunOnWindows || Application.isEditor)
        {
            float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
            if (scrollDelta != 0)
                tarFov = tarFov + Mathf.Sign(scrollDelta) * -1;

            if (Input.GetMouseButtonDown(1))
            {
                lastMousePos = Input.mousePosition;
                vecCameraRotateDelta = Vector2.zero;
                IsRotateMove = true;
                StopCameraMove();
            }
            else if (IsRotateMove && Input.GetMouseButton(1))
            {
                Vector3 mousePos = Input.mousePosition;
                vecCameraRotateDelta = (new Vector2(mousePos.x, mousePos.y) - lastMousePos) * fRotateCameraMouseSpeed;
                lastMousePos = new Vector2(mousePos.x, mousePos.y);
            }

            if (Input.GetMouseButtonUp(1))
                IsRotateMove = false;
        }

        tarFov = Mathf.Clamp(tarFov, nCameraMinFov, nCameraMaxFov);
        if (tarFov != CameraWorld.fieldOfView)
        {
            cameraCopy.fieldOfView = tarFov;
            CameraWorld.fieldOfView = tarFov;
        }
        // 镜头移动处理结束

        Vector2 vecAutoRotateDelta = Vector2.zero;
        if (bIsAutoRotateLeft)
            vecAutoRotateDelta.x = vecAutoRotateDelta.x + fRotateCameraAutoHoriSpeed * deltaTime;
        if (bIsAutoRotateRight)
            vecAutoRotateDelta.x = vecAutoRotateDelta.x - fRotateCameraAutoHoriSpeed * deltaTime;
        if (bIsAutoRotateUp)
            vecAutoRotateDelta.y = vecAutoRotateDelta.y - fRotateCameraAutoVertSpeed * deltaTime;
        if (bIsAutoRotateDown)
            vecAutoRotateDelta.y = vecAutoRotateDelta.y + fRotateCameraAutoVertSpeed * deltaTime;
        CameraRotate(vecAutoRotateDelta);
    }
}
