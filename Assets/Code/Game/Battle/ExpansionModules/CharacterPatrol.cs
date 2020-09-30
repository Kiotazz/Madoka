using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class CharacterPatrol : InteractiveObjExt
{
    [System.Serializable]
    public struct PatrolInfo
    {
        [CustomLabel("路径点")]
        public Transform tsfPath;
        [CustomLabel("停留时间")]
        public float nStayTime;
    }

    [SerializeField, Header("巡逻路径")]
    public PatrolInfo[] patrolPath;
    [CustomLabel("巡逻移动速度")]
    public float fPatrolSpeed = 1;

    int _currentStep = -1;
    public int CurrentStep
    {
        get { return _currentStep; }
        protected set
        {
            if (value < 0) return;
            _currentStep = value >= patrolPath.Length ? 0 : value;
        }
    }
    public Character MasterChara { get; protected set; }
    bool _pause = false;
    public bool Pause
    {
        get { return _pause; }
        set
        {
            if (_pause == value) return;
            if (!(_pause = value))
                MoveToNextDestination();
        }
    }

    float fStayCounter = 0;
    float fStayTime = 0;

    // Start is called before the first frame update
    protected override void OnInit(InteractiveObj obj)
    {
        MasterChara = obj as Character;
        MasterChara.OnArriveDestination.AddListener(OnCharacterArrive);
        MasterChara.OnFightBegin.AddListener(OnCharacterFightBegin);
        MoveToNextDestination();
    }

    // Update is called once per frame
    public override void DoUpdate(float deltaTime)
    {
        if (fStayTime < 0) return;
        if ((fStayCounter += deltaTime) > fStayTime)
            MoveToNextDestination();
    }

    void OnCharacterArrive()
    {
        if (MasterChara.CurrentTarget || MasterChara.IsChasing || !MasterChara.IsAlive || MasterChara.status != Character.CharaStatus.Idle) return;
        if (patrolPath.Length < 1)
        {
            Debug.LogError("Patrol path cannot be empty!");
            return;
        }
        fStayTime = Mathf.Max(patrolPath[CurrentStep].nStayTime, 0.5f) / 1000f;
        fStayCounter = 0;
    }

    void MoveToNextDestination()
    {
        if (Pause) return;
        if (MasterChara.CurrentTarget || MasterChara.IsChasing || !MasterChara.IsAlive || MasterChara.status != Character.CharaStatus.Idle) return;
        if (patrolPath.Length < 1)
        {
            Debug.LogError("Patrol path cannot be empty!");
            return;
        }
        ++CurrentStep;
        MasterChara.SetMoveSpeed(fPatrolSpeed);
        MasterChara.Weak_MoveTo(patrolPath[CurrentStep].tsfPath.position);
        fStayTime = -10086;
    }

    void OnCharacterFightBegin()
    {
        MasterChara.StopMove();
        MasterChara.SetMoveSpeed(MasterChara.fMoveSpeed);
    }
}
