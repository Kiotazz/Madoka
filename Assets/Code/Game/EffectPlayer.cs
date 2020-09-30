using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectPlayer : MonoBehaviour
{
    [CustomLabel("立即播放")]
    public bool bPlayOnAwake = false;
    [CustomLabel("播放结束时触发")]
    public EventWorker onEffectEnd;

    public bool Alive { get; protected set; } = false;

    ParticleSystem particle;
    System.Action callback;

    void Start()
    {
        if (bPlayOnAwake) Play();
    }

    public void Play() { Play(null); }
    public void Play(System.Action onFinished)
    {
        gameObject.SetActive(true);
        if (!particle) particle = GetComponent<ParticleSystem>();
        particle.Play();
        Alive = !particle.main.loop;
        if (onFinished != null) callback = onFinished;
    }

    public void Recycle()
    {
        if (!Alive) return;
        Alive = false;
        if (callback != null) callback();
        onEffectEnd?.DoTriggerEvents();
    }

    private void Update()
    {
        if (Alive && !particle.isPlaying)
            Recycle();
    }

    public static EffectPlayer PlayAtPos(string path, Vector3 worldPos, System.Action onFinish = null)
    {
        return PlayAtPos(Resources.Load<GameObject>(path), worldPos, onFinish);
    }
    public static EffectPlayer PlayAtPos(GameObject prefab, Vector3 worldPos, System.Action onFinish = null)
    {
        if (!prefab)
        {
            if (onFinish != null) onFinish();
            return null;
        }
        if (!prefab.GetComponent<ParticleSystem>())
        {
            Debug.LogError("没有在预设体" + prefab.name + "上找到ParticleSystem");
            if (onFinish != null) onFinish();
            return null;
        }
        GameObject obj = Instantiate(prefab);
        obj.transform.position = worldPos;
        EffectPlayer player = obj.GetOrAddComponent<EffectPlayer>();
        player.callback = onFinish;
        player.bPlayOnAwake = true;
        return player;
    }
    public static EffectPlayer PlayOnTransform(string path, Transform parent, System.Action onFinish = null)
    {
        return PlayOnTransform(Resources.Load<GameObject>(path), parent, onFinish);
    }
    public static EffectPlayer PlayOnTransform(GameObject prefab, Transform parent, System.Action onFinish = null)
    {
        if (!prefab)
        {
            if (onFinish != null) onFinish();
            return null;
        }
        if (!prefab.GetComponent<ParticleSystem>())
        {
            Debug.LogError("没有在预设体" + prefab.name + "上找到ParticleSystem");
            if (onFinish != null) onFinish();
            return null;
        }
        GameObject obj = Instantiate(prefab);
        obj.transform.SetParent(parent, false);
        EffectPlayer player = obj.GetOrAddComponent<EffectPlayer>();
        player.callback = onFinish;
        player.bPlayOnAwake = true;
        return player;
    }
}
