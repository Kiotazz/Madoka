using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneRotateWorldButton : MonoBehaviour
{
    const string explosionPath = "Effects/Epic Toon FX/Prefabs/Combat/Explosions/FireballExplosion/ExplosionFireballFire";

    public SceneRotateButton Control { get; protected set; }
    public Rigidbody RigidSelf { get; private set; }

    int _count = 0;

    public void Init(SceneRotateButton control)
    {
        Control = control;
        RigidSelf = GetComponent<Rigidbody>();
        _count = 0;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explosion();
    }

    private void OnTriggerEnter(Collider other)
    {
        Q_SceneObj obj = other.GetComponent<Q_SceneObj>();
        if (obj && obj.objType == Q_SceneObj.Type.Skill) Explosion();
    }

    void Explosion()
    {
        if (RigidSelf.velocity.sqrMagnitude < 8) return;
        EffectPlayer.PlayAtPos(explosionPath, transform.position);
        if (++_count > 2) Destroy(gameObject);
    }
}
