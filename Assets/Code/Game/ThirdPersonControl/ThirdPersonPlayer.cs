using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;

public class ThirdPersonPlayer : InteractiveObj
{
    readonly string[] IgnoreLayers = new string[] { "Player", "Ignore Raycast", "Weapon", "UI", "SceneUI" };

    public static ThirdPersonPlayer Instance;

    public enum JumpStatus
    {
        Idle,
        Jump,
        Falling,
        LogicOver,
    }

    [CustomLabel("刚体预制体")]
    public GameObject objBodyPrefab;
    [CustomLabel("移动速度")]
    public float fMoveSpeed = 1.5f;
    [CustomLabel("可攀登高度")]
    public float fClimbHeight = 0.3f;
    [CustomLabel("动画控制跳跃")]
    public bool bJumpByAnim = false;
    [CustomLabel("跳跃力")]
    public float fJumpStrength = 350;
    [CustomLabel("摔死高度")]
    public float fDeadHeight = -1500;
    [CustomLabel("音效"), Header("行走脚步音效")]
    public AudioClip[] clipWalks;
    [CustomLabel("行走音效播放间隔")]
    public float fWalkSoundInterval = 0.2f;
    [CustomLabel("起跳音效")]
    public AudioClip clipJumpStart;
    [CustomLabel("摄像机预设体")]
    public GameObject prefabCamera;
    [CustomLabel("摄像机起始位置")]
    public Transform tsfCameraPos;
    [CustomLabel("摄像机焦点")]
    public Transform tsfCameraLookPoint;

    [CustomLabel("关卡开始弹窗路径")]
    public string LevelStartWindowPath;

    [CustomLabel("解锁跳跃技能")]
    public bool bCanUseJumpSkill = false;
    [CustomLabel("解锁射击技能")]
    public bool bCanUseShootSkill = false;
    [CustomLabel("解锁拾取技能")]
    public bool bCanUseItemSkill = false;

    [CustomLabel("马里奥帽子")]
    public GameObject objProtect;

    public FreeTPSCamera CameraController { get; protected set; }
    public ThirdPersonBody Body { get; protected set; }
    public bool IsMoving { get; protected set; } = false;
    public bool IsGrounded { get { return Body.IsGrounded; } }
    public bool CanJump { get { return IsAlive && IsGrounded && MyJumpStatus != JumpStatus.Jump; } }

    bool _canOperate = true;
    public bool CanOperate
    {
        get { return _canOperate; }
        set
        {
            value = value && IsAlive;
            if (_canOperate == value) return;
            _canOperate = CameraController.CanControlDirection = value;
        }
    }

    bool _isProtected = false;
    public bool IsProtected
    {
        get { return _isProtected; }
        set
        {
            value = value && IsAlive;
            if (_isProtected == value) return;
            objProtect.SetActive(_isProtected = value);
        }
    }

    bool isTriggered = false;

    float fWalkSoundCouter;
    Dictionary<string, int> dicPickedItems = new Dictionary<string, int>();
    JumpStatus _jumpStatus = JumpStatus.Idle;
    public JumpStatus MyJumpStatus
    {
        get { return _jumpStatus; }
        protected set
        {
            if (_jumpStatus == value) return;
            _jumpStatus = value;
            if (!bJumpByAnim) return;
            Body.RigidSelf.useGravity = _jumpStatus != JumpStatus.Jump && _jumpStatus != JumpStatus.Falling;
            Vector3 vel = Body.RigidSelf.velocity;
            vel.y = Body.RigidSelf.useGravity ? -3 : 0;
            Body.RigidSelf.velocity = vel;
        }
    }
    LayerMask layerMask = new LayerMask();
    int jumpHash = Animator.StringToHash("Jump");
    int jumpEndHash = Animator.StringToHash("JumpEnd");

    protected override void Init()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        for (int i = 0, length = IgnoreLayers.Length; i < length; ++i)
            layerMask |= 1 << LayerMask.NameToLayer(IgnoreLayers[i]);
        layerMask = ~layerMask;

        if (!objBodyPrefab) objBodyPrefab = Resources.Load<GameObject>("Prefabs/ThirdPersonPlayer/ThirdPersonBody");
        GameObject objRigid = Instantiate(objBodyPrefab);
        Body = objRigid.GetComponent<ThirdPersonBody>();
        Body.Init(this);
        Destroy(ColliderSelf);
        ColliderSelf = Body.ColliderSelf;

        if (!tsfCameraLookPoint) tsfCameraLookPoint = BeAtkPoint;

        if (!prefabCamera) prefabCamera = Resources.Load<GameObject>("Prefabs/ThirdPersonPlayer/FreeTPSCamera");
        GameObject objCamera = Instantiate(prefabCamera, tsfCameraPos.position, Quaternion.identity);
        CameraController = objCamera.GetComponent<FreeTPSCamera>();
        CameraController.TargetBody = objRigid;
        CameraController.CameraPivot = tsfCameraLookPoint.gameObject;

        dicPickedItems.Clear();

        UISystem.Instance.CreateRootWindow("GUI/GamerGame/MainView");
        UISystem.Instance.CreateMessageBox(LevelStartWindowPath);

        BattleManager.Instance.BattleBegin();
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (!BattleManager.Instance.IsBattleBegin || !IsAlive) return;
        if (transform.position.y < fDeadHeight)
            Death();
        if (Input.GetKeyDown(KeyCode.F))
            PickUpItem();

        if (Input.GetKeyDown(KeyCode.P))
            GameClient.Instance.LoadScene();
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            DEF = 999999999;
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            bCanUseJumpSkill = true;
            bCanUseShootSkill = true;
            bCanUseItemSkill = true;
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            fMoveSpeed = 7;
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            fJumpStrength = 2000;
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            Reborn();
        }
        transform.position = Body.Position;
        transform.rotation = Body.Rotation;

        if (Input.GetKeyDown(KeyCode.Space)) Jump();

        CameraController.DoUpdate(Time.deltaTime);
    }

    private void OnAnimatorMove()
    {
        if (Body.Velocity.y < -1f)
        {
            if (!bJumpByAnim && MyJumpStatus != JumpStatus.Falling)
            {
                MyJumpStatus = JumpStatus.Falling;
                Role.PlayAction("Fall");
            }
        }
        else if (!bJumpByAnim && MyJumpStatus == JumpStatus.Falling && IsGrounded)
        {
            MyJumpStatus = JumpStatus.LogicOver;
            Role.PlayAction("JumpEnd", 0.2f);
        }

        float moveAxis = Mathf.Max(Mathf.Abs(Input.GetAxis("Horizontal")), Mathf.Abs(Input.GetAxis("Vertical")));
        if (!BattleManager.Instance.IsBattleBegin || !IsAlive || UISystem.Instance.InAnyGuide || !CanOperate)
            moveAxis = 0;
        Role.SetMoveSpeed(moveAxis);
        //Role.AnimatorSelf.SetFloat("MoveDirection", vert < 0 ? 2 : hori);
        transform.rotation *= Role.AnimatorSelf.deltaRotation;
        Vector3 deltaPosition = Role.AnimatorSelf.deltaPosition;
        Body.RotateBy(Role.AnimatorSelf.deltaRotation);

        RaycastHit hit;
        Collider hitCol = null;
        if (Physics.Raycast(transform.position + new Vector3(0, 0.1f), transform.forward,
            out hit, 0.3f, layerMask, QueryTriggerInteraction.Ignore))
        {
            hitCol = hit.collider;
            if (!isTriggered) Body.OnTrigger(hitCol);
            if (Vector3.SignedAngle(transform.forward, hit.normal, transform.right) > -95f && CheckColliderIsStatic(hitCol))
            {
                float deltaHeight = hitCol.bounds.max.y - transform.position.y;
                if (IsGrounded && deltaHeight < fClimbHeight)
                    Body.MoveBy(new Vector3(0, deltaHeight, 0));
                else
                    moveAxis = 0;
            }
        }

        Collider[] cols = Physics.OverlapBox(ColliderSelf.bounds.center + transform.forward * 0.3f, ColliderSelf.bounds.extents / 2,
            transform.rotation, layerMask, QueryTriggerInteraction.Ignore);
        for (int i = 0, length = cols.Length; i < length; ++i)
        {
            if (cols[i] == hitCol) continue;
            if (!isTriggered) Body.OnTrigger(cols[i]);
            if (CheckColliderIsStatic(cols[i]))
            {
                moveAxis = 0;
                break;
            }
        }
        isTriggered = true;
        if (moveAxis == 0 /*|| (MyJumpStatus == JumpStatus.Jump || MyJumpStatus == JumpStatus.Falling) && !IsMoving*/)
        {
            transform.position += deltaPosition;
            Body.MoveBy(deltaPosition);
            IsMoving = false;
            return;
        }

        IsMoving = true;
        if (MyJumpStatus == JumpStatus.LogicOver)
        {
            Role.PlayAction(Role.strNormalAnimName, 0.2f);
            MyJumpStatus = JumpStatus.Idle;
        }
        Vector3 move = transform.forward * moveAxis * fMoveSpeed * Time.fixedDeltaTime + deltaPosition;
        transform.position += move;
        if ((fWalkSoundCouter += Time.deltaTime) > fWalkSoundInterval && moveAxis > 0)
        {
            fWalkSoundCouter = 0;
            if (clipWalks.Length > 0)
                AudioSystem.Instance.PlayOnTransform(clipWalks[Random.Range(0, clipWalks.Length)], transform);
        }

        isTriggered = false;
        Body.MoveBy(move);
    }

    bool CheckColliderIsStatic(Collider col)
    {
        Rigidbody rigid = col.GetComponent<Rigidbody>();
        return !rigid || rigid.isKinematic || (rigid.constraints & RigidbodyConstraints.FreezePositionX) > 0 ||
            (rigid.constraints & RigidbodyConstraints.FreezePositionZ) > 0;
    }

    public void Jump()
    {
        if (!CanJump) return;
        Role.PlayAction("Jump");
        MyJumpStatus = JumpStatus.Jump;
    }

    void PickUpItem()
    {
        if (!IsAlive || UISystem.Instance.InAnyGuide) return;
        Collider[] cols = Physics.OverlapBox(transform.position, ColliderSelf.bounds.size / 2, transform.rotation);
        for (int i = 0; i < cols.Length; i++)
        {
            Q_SceneObj sceneObj = cols[i].GetComponent<Q_SceneObj>();
            if (!sceneObj) continue;
            switch (sceneObj.objType)
            {
                case Q_SceneObj.Type.Item:
                    if (!dicPickedItems.ContainsKey(sceneObj.name))
                        dicPickedItems[sceneObj.name] = 1;
                    dicPickedItems[sceneObj.name] = dicPickedItems[sceneObj.name] + 1;
                    sceneObj.UseObj();
                    return;
                case Q_SceneObj.Type.NeedItem:
                    if (!dicPickedItems.ContainsKey(sceneObj.strNeedItemName))
                    {
                        UISystem.Instance.ShowMessageBox("需要" + sceneObj.strNeedItemName + "!");
                        return;
                    }
                    int num = dicPickedItems[sceneObj.strNeedItemName];
                    if (--num < 0)
                    {
                        dicPickedItems.Remove(sceneObj.strNeedItemName);
                        UISystem.Instance.ShowMessageBox("需要" + sceneObj.strNeedItemName + "!");
                        return;
                    }
                    sceneObj.UseObj();
                    if (num == 0) dicPickedItems.Remove(sceneObj.strNeedItemName);
                    else dicPickedItems[sceneObj.strNeedItemName] = num;
                    return;
            }
        }
    }

    public override InteractiveObj FindNextEnemy()
    {
        return null;
    }

    protected override void OnDamage(Damage damage, InteractiveObj source)
    {
        if (IsProtected)
        {
            IsProtected = false;
            return;
        }
        base.OnDamage(damage, source);
        Role.PlayAction(Role.strDamageAnimName, 0.1f);
        if (CameraController.MainCamera)
            CameraController.MainCamera.transform.DOShakePosition(0.3f, new Vector3(0.12f, 0.12f), 20);
    }

    protected override void OnSelfDeath()
    {
        Role.PlayAction(Role.strDeathAnimName, 0.1f);
        BattleManager.Instance.BattleOver(false);
        CanOperate = false;
        IsProtected = false;
    }

    protected override void OnAttackTarget()
    {
        if (CurrentTarget)
            transform.LookAt(CurrentTarget.BeAtkPoint);
    }

    public override void OnAnimLogicOver(int nameHash)
    {
        if (!IsAlive) return;
        if (IsMoving && IsGrounded)
        {
            Role.PlayAction(Role.strNormalAnimName);
            MyJumpStatus = JumpStatus.Idle;
        }
        else if (nameHash == jumpHash && bJumpByAnim)
        {
            MyJumpStatus = JumpStatus.LogicOver;
        }
        ClearActions();
    }

    public override void OnAnimStateExit(int nameHash)
    {
        if (nameHash == jumpHash) MyJumpStatus = bJumpByAnim ? JumpStatus.Idle : JumpStatus.Falling;
        if (nameHash == jumpEndHash && MyJumpStatus == JumpStatus.LogicOver) MyJumpStatus = JumpStatus.Idle;
        ClearActions();
    }

    //Anim Event
    void JumpStart()
    {
        Body.RigidSelf.AddForce(0, fJumpStrength, 0, ForceMode.Impulse);
        AudioPlayer p = AudioSystem.Instance.PlayAtPos(clipJumpStart, Body.Position);
        if (p)
        {
            p.audioSource.spatialBlend = 1;
            p.audioSource.volume = 1f;
        }
    }

    void JumpToTop()
    {
        MyJumpStatus = JumpStatus.Falling;
    }
    //Anim Event Over

    public override void OnSkillStart(SkillBase skill)
    {

    }

    public override void OnSkillOver(SkillBase skill)
    {
        ClearActions();
    }

    void ClearActions()
    {

    }

    public override bool CanCastSkill(SkillBase skill)
    {
        return true;
    }

    public override bool IsFindNextEnemy()
    {
        return false;
    }

    public void Reborn()
    {
        if (IsAlive) return;
        HP = MaxHP;
        IsAlive = true;
        BattleManager.Instance.BattleBegin();
        ColliderSelf.enabled = true;
        CanOperate = true;
        if (Body.Position.y < -1000)
        {
            Body.RigidSelf.velocity = Vector3.zero;
            Vector3 pos = Body.Position;
            pos.y = 200;
            Body.MoveTo(pos);
            transform.position = pos;
            NavMeshHit meshHit;
            if (NavMesh.SamplePosition(pos, out meshHit, 10000, 1))
            {
                Body.MoveTo(meshHit.position);
                transform.position = meshHit.position;
            }
        }
    }
}
