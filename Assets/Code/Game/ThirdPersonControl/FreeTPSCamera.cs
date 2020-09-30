using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FreeTPSCamera : MonoBehaviour
{
    readonly string[] IgnoreLayers = new string[] { "Player", "Ignore Raycast", "Weapon", "UI", "SceneUI", "Monster" };

    [CustomLabel("相机距离")]
    public float freeDistance = 2;
    [CustomLabel("相机最近距离")]
    public float minDistance = 0.5f;
    [CustomLabel("相机最远距离")]
    public float maxDistance = 20;
    [CustomLabel("是否可控制相机距离(鼠标中键)")]
    public bool canControlDistance = true;
    [CustomLabel("更改相机距离的速度")]
    public float distanceSpeed = 1;
    [CustomLabel("视角灵敏度")]
    public float rotateSpeed = 1;
    [CustomLabel("物体转向插值(灵敏度,取值为0到1)")]
    public float TargetBodyRotateLerp = 0.3f;
    [CustomLabel("需要转向的物体")]
    public GameObject TargetBody;//此脚本能操作转向的物体    
    [CustomLabel("相机焦点物体")]
    public GameObject CameraPivot;
    //相机焦点物体    
    [CustomLabel("===锁敌===")]
    public GameObject lockTarget = null;
    public float lockSlerp = 1;
    [CustomLabel("索敌标记")]
    public GameObject lockMark;
    private bool marked;
    [CustomLabel("是否可控制物体转向")]
    public bool CanControlDirection = true;
    [CustomLabel("俯角(0-89)")]
    public float maxDepression = 80;
    [CustomLabel("仰角(0-89)")]
    public float maxEvelation = 80;
    [CustomLabel("景深偏移")]
    public float depthOfSceneOffset = 0;
    private Vector3 PredictCameraPosition;
    private Vector3 offset;
    private Vector3 wallHit;
    private GameObject tmpMark;
    private LayerMask layerMask = new LayerMask();
    private bool bInited = false;

    private UnityEngine.PostProcessing.PostProcessingBehaviour ppBehavior;

    public Camera MainCamera { get; private set; }

    // Use this for initialization    
    void Start()
    {
        bInited = false;
        MainCamera = transform.Find("WorldCamera").GetComponent<Camera>();
        ppBehavior = MainCamera.GetComponent<UnityEngine.PostProcessing.PostProcessingBehaviour>();
        offset = transform.position - CameraPivot.transform.position;
        if (TargetBody == null)
        {
            TargetBody = GameObject.FindGameObjectWithTag("Player");
            Debug.Log("未绑定目标物体，默认替换为Player标签的物体");
        }
        if (CameraPivot) transform.LookAt(CameraPivot.transform);
        else Debug.LogError("未绑定相机焦点物体");

        for (int i = 0, length = IgnoreLayers.Length; i < length; ++i)
            layerMask |= 1 << LayerMask.NameToLayer(IgnoreLayers[i]);
        layerMask = ~layerMask;

        GameClient.Instance.NextTick(() => bInited = true, 0.1f);
    }
    void LockTarget()
    {
        if (lockTarget)
        {
            lockTarget = null;
            marked = false;
            Destroy(tmpMark);
            return;
        }
        Vector3 top = transform.position + new Vector3(0, 1, 0) + transform.forward * 5;
        Collider[] cols = Physics.OverlapBox(top, new Vector3(0.5f, 0.5f, 5), transform.rotation, layerMask);
        foreach (var col in cols)
        {
            lockTarget = col.gameObject;
        }
    }
    bool Inwall()
    {
        RaycastHit hit;
        PredictCameraPosition = CameraPivot.transform.position + offset.normalized * freeDistance;//预测的相机位置
        if (Physics.Linecast(CameraPivot.transform.position, PredictCameraPosition, out hit, layerMask))
        {
            wallHit = hit.point;//碰撞点位置
            return true;
        }
        else
        {
            return false;//没有障碍物
        }
    }
    void FreeCamera(float deltaTime)
    {
        offset = offset.normalized * freeDistance;
        transform.position = CameraPivot.transform.position + offset;//更新位置
        if (CanControlDirection)//控制角色方向开关
        {
            Quaternion TargetBodyCurrentRotation = TargetBody.transform.rotation;
            if (Input.GetKey(KeyCode.A))
            {
                if (Input.GetKey(KeyCode.W))
                {
                    TargetBody.transform.rotation = Quaternion.Lerp(TargetBodyCurrentRotation, Quaternion.Euler(new Vector3(TargetBody.transform.localEulerAngles.x, transform.localEulerAngles.y - 45, TargetBody.transform.localEulerAngles.z)), TargetBodyRotateLerp);
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    TargetBody.transform.rotation = Quaternion.Lerp(TargetBodyCurrentRotation, Quaternion.Euler(new Vector3(TargetBody.transform.localEulerAngles.x, transform.localEulerAngles.y - 135, TargetBody.transform.localEulerAngles.z)), TargetBodyRotateLerp);
                }
                else if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
                {
                    TargetBody.transform.rotation = Quaternion.Lerp(TargetBodyCurrentRotation, Quaternion.Euler(new Vector3(TargetBody.transform.localEulerAngles.x, transform.localEulerAngles.y - 90, TargetBody.transform.localEulerAngles.z)), TargetBodyRotateLerp);
                }
            }
            else if (Input.GetKey(KeyCode.D))
            {
                if (Input.GetKey(KeyCode.W))
                {
                    TargetBody.transform.rotation = Quaternion.Lerp(TargetBodyCurrentRotation, Quaternion.Euler(new Vector3(TargetBody.transform.localEulerAngles.x, transform.localEulerAngles.y + 45, TargetBody.transform.localEulerAngles.z)), TargetBodyRotateLerp);
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    TargetBody.transform.rotation = Quaternion.Lerp(TargetBodyCurrentRotation, Quaternion.Euler(new Vector3(TargetBody.transform.localEulerAngles.x, transform.localEulerAngles.y + 135, TargetBody.transform.localEulerAngles.z)), TargetBodyRotateLerp);
                }
                else if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S)) { TargetBody.transform.rotation = Quaternion.Lerp(TargetBodyCurrentRotation, Quaternion.Euler(new Vector3(TargetBody.transform.localEulerAngles.x, transform.localEulerAngles.y + 90, TargetBody.transform.localEulerAngles.z)), TargetBodyRotateLerp); }
            }
            else if (Input.GetKey(KeyCode.W))
            {
                TargetBody.transform.rotation = Quaternion.Lerp(TargetBodyCurrentRotation, Quaternion.Euler(new Vector3(TargetBody.transform.localEulerAngles.x, transform.localEulerAngles.y, TargetBody.transform.localEulerAngles.z)), TargetBodyRotateLerp);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                TargetBody.transform.rotation = Quaternion.Lerp(TargetBodyCurrentRotation, Quaternion.Euler(new Vector3(TargetBody.transform.localEulerAngles.x, transform.localEulerAngles.y - 180, TargetBody.transform.localEulerAngles.z)), TargetBodyRotateLerp);
            }
        }
        if (canControlDistance)//控制距离开关
        {
            freeDistance -= Input.GetAxis("Mouse ScrollWheel") * distanceSpeed;
        }
        freeDistance = Mathf.Clamp(freeDistance, minDistance, maxDistance);
        if (!lockTarget)
        {
            transform.LookAt(CameraPivot.transform);
        }
        else
        {
            Quaternion tmp = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lockTarget.transform.position - transform.position), lockSlerp * deltaTime);
            transform.rotation = tmp;
        }
        float eulerX = transform.localEulerAngles.x;//相机的x欧拉角,也就是垂直方向
        float inputY = Input.GetAxis("Mouse Y");
        if (!lockTarget && Input.GetMouseButton(1))
        {
            if (!lockTarget)
            {
                transform.RotateAround(CameraPivot.transform.position, Vector3.up, rotateSpeed * Input.GetAxis("Mouse X"));
            }

            Vector3 cameraToPlayer = transform.position - CameraPivot.transform.position;
            float curAngle = -Vector3.SignedAngle(cameraToPlayer.normalized, new Vector3(cameraToPlayer.x, 0, cameraToPlayer.z).normalized, transform.right);
            if (inputY > 0)
            {
                float angle = Mathf.Clamp(-rotateSpeed * inputY, Mathf.Min(-maxDepression - curAngle, 0), 0);
                transform.RotateAround(ThirdPersonPlayer.Instance.BeAtkPoint.position, transform.right, angle);
            }
            else if (inputY < 0)
            {
                float angle = Mathf.Clamp(-rotateSpeed * inputY, 0, Mathf.Max(maxEvelation - curAngle, 0));
                transform.RotateAround(ThirdPersonPlayer.Instance.BeAtkPoint.position, transform.right, angle);
            }
        }
        if (lockTarget)
        {
            offset = CameraPivot.transform.position - (lockTarget.transform.position);
        }
        else
        {
            offset = transform.position - CameraPivot.transform.position;//以上方向发生了变化,记录新的方向向量
        }
        offset = offset.normalized * freeDistance;
        ///在一次Update中,随时记录新的旋转后的位置,然后得到方向,然后判断是否即将被遮挡,如果要被遮挡,将相机移动到计算后的不会被遮挡的位置
        ///如果不会被遮挡,则更新位置为相机焦点位置+方向的单位向量*距离
        float distance = freeDistance;
        if (Inwall())//预测会被遮挡   
        {
            transform.position = CameraPivot.transform.position + (wallHit - CameraPivot.transform.position) * 0.8f;
            distance = Vector3.Distance(wallHit, CameraPivot.transform.position) * 0.8f;
        }
        else
        {
            transform.position = CameraPivot.transform.position + offset;
        }
        distance += depthOfSceneOffset;
        if (ppBehavior)
        {
            UnityEngine.PostProcessing.DepthOfFieldModel.Settings set = ppBehavior.profile.depthOfField.settings;
            if (set.focusDistance != distance)
            {
                set.focusDistance = distance;
                ppBehavior.profile.depthOfField.settings = set;
            }
        }
    }
    // Update is called once per frame  
    public void DoUpdate(float deltaTime)
    {
        if (!bInited) return;
        FreeCamera(deltaTime);
        if (lockTarget)
        {
            if (!marked)
            {
                tmpMark = Instantiate(lockMark, lockTarget.transform.position + new Vector3(0, 2.5f, 0), transform.rotation);
                tmpMark.transform.forward = -Vector3.up; marked = true;
            }
            else
            {
                tmpMark.transform.position = lockTarget.transform.position + new Vector3(0, 2.5f, 0);
                //tmpMark.transform.forward = -transform.up;
                tmpMark.transform.Rotate(Vector3.up * 30 * deltaTime, Space.World);
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            //LockTarget();
        }
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape))
        {
            lockTarget = null;
        }
    }
}
