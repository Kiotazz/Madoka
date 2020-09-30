using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowCollision : MonoBehaviour
{
    [System.Serializable]
    public struct Line
    {
        public Transform tsfStart;
        public Transform tsfEnd;
    }
    public class MoveInfo
    {
        public class Record
        {
            public Vector3 vecLasePos;
            public Vector3 vecSpeed;
        }
        public Transform transform;
        public Dictionary<string, Record> dicRecords;

        public MoveInfo(Transform tsf)
        {
            transform = tsf;
            dicRecords = new Dictionary<string, Record>();
        }
        public MoveInfo()
        {
            dicRecords = new Dictionary<string, Record>();
        }
        public Record GetRecord(string key)
        {
            if (dicRecords.ContainsKey(key))
                return dicRecords[key];
            return dicRecords[key] = new Record() { vecLasePos = Vector3.zero, vecSpeed = Vector3.zero };
        }
        public void Reset(string key)
        {
            dicRecords[key].vecLasePos = dicRecords[key].vecSpeed = Vector3.zero;
        }
    }

    public float fSpeedRatio = 1;

    public Line[] lines;
    Dictionary<string, MoveInfo> dicMoveInfos = new Dictionary<string, MoveInfo>();

    Vector3 vecLastPos;

    ShadowCollisionManager manager;
    Rigidbody rigidMySelf;
    float fHitProtectTimer = 0;
    int nHitCount = 0;
    bool bInited = false;
    public void Init(ShadowCollisionManager manager)
    {
        bInited = true;
        this.manager = manager;
        vecLastPos = transform.position;
        rigidMySelf = GetComponent<Rigidbody>();

        if (lines.Length < 1)
            return;
        Transform parent = lines[0].tsfStart.parent;
        for (int i = 0, length = lines.Length; i < length; ++i)
        {
            Vector3 line = lines[i].tsfEnd.position - lines[i].tsfStart.position;
            Vector3 startPoint = lines[i].tsfStart.position;
            int count = (int)(Vector3.Distance(lines[i].tsfEnd.position, lines[i].tsfStart.position) / 0.1);
            Vector3 add = line / count;
            for (int j = 0, length2 = count + 1; j < length2; ++j)
            {
                Transform tsf = new GameObject().transform;
                tsf.name = i + "-" + j;
                tsf.transform.SetParent(parent, false);
                tsf.transform.position = startPoint + add;
                dicMoveInfos[tsf.name] = new MoveInfo(tsf);
            }
            dicMoveInfos[lines[i].tsfStart.name] = new MoveInfo(lines[i].tsfStart);
            dicMoveInfos[lines[i].tsfEnd.name] = new MoveInfo(lines[i].tsfEnd);
        }
    }

    void Update()
    {
        if ((fHitProtectTimer -= Time.deltaTime) < 0)
            nHitCount = 0;
        if (!bInited || vecLastPos == transform.position) return;
        vecLastPos = transform.position;

        List<Light> lights = manager.GetLights();
        for (int i = 0, length = lights.Count; i < length; ++i)
        {
            Light light = lights[i];
            RaycastHit hit;
            foreach (var point in dicMoveInfos.Values)
            {
                MoveInfo.Record record = point.GetRecord(light.name);
                if (!Physics.Raycast(light.transform.position, point.transform.position - light.transform.position, out hit, 10000, 1 << LayerMask.NameToLayer("Wall")))
                {
                    point.Reset(light.name);
                    continue;
                }
                GameObject objWall = hit.collider.gameObject;
                Vector3 speed = record.vecLasePos == Vector3.zero ? Vector3.zero : hit.point - record.vecLasePos;
                record.vecSpeed = speed;
                record.vecLasePos = hit.point;
                Renderer renderer = objWall.GetComponent<Renderer>();
                if (!renderer || !renderer.receiveShadows)
                {
                    point.Reset(light.name);
                    continue;
                }
                float totalDistance = Vector3.Distance(light.transform.position, hit.point);
                switch (light.type)
                {
                    case LightType.Spot:
                        if (Vector3.Angle((hit.point - light.transform.position).normalized, light.transform.forward) > light.spotAngle / 2)
                        {
                            point.Reset(light.name);
                            continue;
                        }
                        if (totalDistance > light.range)
                        {
                            point.Reset(light.name);
                            continue;
                        }
                        break;
                    case LightType.Point:
                        if (totalDistance > light.range)
                        {
                            point.Reset(light.name);
                            continue;
                        }
                        break;
                }
                RaycastHit[] hits = Physics.RaycastAll(light.transform.position, point.transform.position - light.transform.position, 10000,
                    1 << LayerMask.NameToLayer("Obj"));
                if (hits == null || hits.Length < 1) continue;
                for (int j = 0, lenObj = hits.Length; j < lenObj; ++j)
                {
                    if (hits[j].collider.gameObject == gameObject)
                        continue;

                    ShadowCollision target = hits[j].collider.GetComponent<ShadowCollision>();
                    target.Hit(speed * (totalDistance - Vector3.Distance(light.transform.position, point.transform.position)) * rigidMySelf.mass * fSpeedRatio);
                    break;
                }
            }
        }
    }

    public void Hit(Vector3 velocity)
    {
        if (fHitProtectTimer >= 0) return;
        if (++nHitCount > 2)
        {
            fHitProtectTimer = 0.5f;
            return;
        }
        rigidMySelf.velocity += velocity;
    }
}
