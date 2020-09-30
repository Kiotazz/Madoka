using UnityEngine;
using DG.Tweening;

public class Missle : MonoBehaviour
{
    public bool Alive = true;
    const float lockTargetInterval = 0.2f;

    float fSpeed = 1;
    float fDestroyTime = 0;
    Transform tsfTarget;
    Vector3 vecTargetPos;
    System.Action<Transform> onMissleHitTarget;
    System.Action<Vector3> onMissleHitPos;

    //追踪目标---------------------------------------------------------------------------
    public void Init(Transform target, float speed, System.Action<Transform> onHit)
    {
        tsfTarget = target;
        vecTargetPos = tsfTarget.position;
        fSpeed = speed / (1 / lockTargetInterval);
        onMissleHitTarget = onHit;
        BallFly();
    }

    void BallFly()
    {
        if (!tsfTarget)
        {
            Init(vecTargetPos, fSpeed * (1 / lockTargetInterval), (pos) =>
            {
                if (onMissleHitTarget != null) onMissleHitTarget(null);
            });
            return;
        }

        float sqrDistance = tsfTarget.position.SqrDistanceWith(transform.position);
        if (sqrDistance < fSpeed)
        {
            Destroy(gameObject);
            if (onMissleHitTarget != null) onMissleHitTarget(tsfTarget);
            return;
        }

        vecTargetPos = tsfTarget.position;
        Vector3 targetPos = sqrDistance <= fSpeed * fSpeed ? vecTargetPos : (transform.position + (vecTargetPos - transform.position).normalized * fSpeed);
        Tweener tween = transform.DOMove(targetPos, lockTargetInterval);
        tween.SetEase(Ease.Linear);
        tween.onComplete = BallFly;
    }
    //------------------------------------------------------------------------------------

    //直线--------------------------------------------------------------------------------
    public void Init(Vector3 position, float speed, System.Action<Vector3> onHit)
    {
        float time = Vector3.Distance(position, transform.position) / speed;
        if (time < 0.05f)
        {
            Destroy(gameObject);
            if (onHit != null) onHit(transform.position);
            return;
        }
        Tweener tween = transform.DOMove(position, time);
        tween.SetEase(Ease.Linear);
        tween.onComplete = () =>
        {
            Destroy(gameObject);
            if (onHit != null) onHit(transform.position);
        };
    }
    //------------------------------------------------------------------------------------

    //抛物线//----------------------------------------------------------------------------
    private Vector3 ShotSpeed;       // 初速度向量
    private Vector3 Gravity;     // 重力向量
    private Vector3 currentAngle;// 当前角度
    private float dTime = 0;
    private bool bBulletWorking = false;

    public void InitBullet(Vector3 position, float speed, System.Action<Vector3> onHit)
    {
        vecTargetPos = position;
        fSpeed = speed;
        onMissleHitPos = onHit;
        float time = Vector3.Distance(transform.position, position) / speed;
        // 计算初速度
        ShotSpeed = new Vector3((position.x - transform.position.x) / time,
            (position.y - transform.position.y) / time - 0.5f * Physics.gravity.y * time, (position.z - transform.position.z) / time);
        Gravity = Vector3.zero;
        dTime = 0;
        bBulletWorking = true;
    }
    //------------------------------------------------------------------------------------

    private Vector3 vecDirection;
    private Vector3 startPos;
    private bool bIsFlyFront = false;
    private float fMaxRange = 0;
    private System.Action<GameObject, Missle> onHitObj;
    public void InitFlyFront(Vector3 direction, float speed, float maxRange, System.Action<GameObject, Missle> onHit)
    {
        startPos = transform.position;
        vecDirection = direction;
        fSpeed = speed / Time.fixedDeltaTime;
        fMaxRange = maxRange;
        onHitObj = onHit;
        bIsFlyFront = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!Alive) return;
        if (fDestroyTime > 0 && Time.timeSinceLevelLoad > fDestroyTime)
        {
            Alive = false;
            Destroy(gameObject);
            return;
        }
        if (bIsFlyFront)
        {
            transform.Translate(vecDirection * fSpeed, Space.World);
            if (fMaxRange > 0 && transform.position.SqrDistanceWith(startPos) > fMaxRange * fMaxRange)
            {
                Alive = false;
                Destroy(gameObject);
                if (onHitObj != null) onHitObj(null, this);
                return;
            }
        }
        if (bBulletWorking)
        {
            // v=gt
            Gravity.y = Physics.gravity.y * (dTime += Time.fixedDeltaTime);

            //模拟位移
            transform.position += (ShotSpeed + Gravity) * Time.fixedDeltaTime;

            // 弧度转度：Mathf.Rad2Deg
            currentAngle.x = -Mathf.Atan((ShotSpeed.y + Gravity.y) / ShotSpeed.z) * Mathf.Rad2Deg;

            // 设置当前角度
            transform.eulerAngles = currentAngle;
            if (transform.position.y < vecTargetPos.y)
            {
                bBulletWorking = false;
                Destroy(gameObject);
                if (onMissleHitPos != null) onMissleHitPos(vecTargetPos);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!bIsFlyFront || !Alive) return;
        if (onHitObj != null) onHitObj(other.gameObject, this);
    }

    public void SetLifeTime(int time)
    {
        if (time > 0) fDestroyTime = Time.timeSinceLevelLoad + time / 1000f;
    }
}
