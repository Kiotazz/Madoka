using System.Collections.Generic;
using UnityEngine;

public class InputManager
{
    public static implicit operator bool(InputManager manager) { return manager != null; }

    const string CameraPrefabPath = "Prefabs/CameraController";

    public InteractiveObjEvent OnClickInteractiveObj { get; private set; } = new InteractiveObjEvent();
    public Vector3Event OnClickGround { get; private set; } = new Vector3Event();

    public Commander Master { get; protected set; }
    public CameraController MainCamera { get; protected set; }

    bool bIsMouseLeftDown;
    float fMouseLeftPressTime;
    Vector2 vecMouseLeftPressPos;

    bool bIsMouseRightDown;
    float fMouseRightPressTime;
    Vector2 vecMouseRightPressPos;

    Vector2 vecLastMousePos;
    Vector2 vecMousePos;

    public void Init(Commander commander)
    {
        Master = commander;
        MainCamera = GameObject.Instantiate(Resources.Load<GameObject>(CameraPrefabPath), Vector3.zero, Quaternion.identity).GetOrAddComponent<CameraController>();
        MainCamera.name = "CameraController";
        MainCamera.Init();
        Reset();
    }

    public void Reset()
    {
        vecMousePos = Input.mousePosition;
        vecLastMousePos = vecMousePos;

        bIsMouseLeftDown = false;
        fMouseLeftPressTime = 0;
        vecMouseLeftPressPos = Vector2.zero;

        bIsMouseRightDown = false;
        fMouseRightPressTime = 0;
        vecMouseRightPressPos = Vector2.zero;
    }

    public void DoUpdate(float deltaTime)
    {
        vecLastMousePos = vecMousePos;
        vecMousePos = Input.mousePosition;
        if (Input.GetMouseButtonUp(0))
        {
            bIsMouseLeftDown = false;
            if (vecMouseLeftPressPos.SqrDistanceWith(vecMousePos) < 1 && fMouseLeftPressTime < 1.5f)
            {
                RaycastHit hit;
                if (Physics.Raycast(MainCamera.CameraWorld.ScreenPointToRay(vecMousePos), out hit, 10000, 1 << LayerMask.NameToLayer("Accessable")))
                {
                    InteractiveObj interactiveObj = hit.collider.GetComponent<InteractiveObj>();
                    if (interactiveObj)
                    {
                        interactiveObj.ClickObj();
                        OnClickInteractiveObj.Invoke(interactiveObj);
                    }
                }
                else if (Physics.Raycast(MainCamera.CameraWorld.ScreenPointToRay(vecMousePos), out hit, 1 << LayerMask.NameToLayer("Ground")))
                    OnClickGround.Invoke(hit.point);
            }
            MainCamera.OnPress(false);
        }
        if (Input.GetMouseButtonUp(1))
        {
            bIsMouseRightDown = false;
        }

        if (bIsMouseLeftDown || bIsMouseRightDown)
        {
            if (bIsMouseLeftDown) fMouseLeftPressTime += deltaTime;
            if (bIsMouseRightDown) fMouseRightPressTime += deltaTime;
            Vector2 delta = vecMousePos - vecLastMousePos;
            if (delta.x != 0 || delta.y != 0)
                MainCamera.OnDrag(delta);
        }

        if (Input.GetMouseButtonDown(0) && !bIsMouseRightDown && !UISystem.Instance.IsMouseOnUI())
        {
            bIsMouseLeftDown = true;
            fMouseLeftPressTime = 0;
            vecMouseLeftPressPos = vecMousePos;
            MainCamera.OnPress(true);
        }
        if (Input.GetMouseButtonDown(1) && !bIsMouseLeftDown && !UISystem.Instance.IsMouseOnUI())
        {
            bIsMouseRightDown = true;
            fMouseRightPressTime = 0;
            vecMouseRightPressPos = vecMousePos;
            MainCamera.OnPress(true);
        }
        MainCamera.DoUpdate(deltaTime);
    }
}
