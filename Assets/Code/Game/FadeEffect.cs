using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeEffect : MonoBehaviour
{
    bool fadeIn = true;
    float duration = 1;
    public bool AutoDestroy = true;
    // Start is called before the first frame update
    public void Init(Color start, Color end, float time, System.Action callback)
    {
        Image img = GetComponent<Image>();
        img.color = start;
        img.DOColor(end, time).onComplete = () =>
        {
            if (callback != null) callback();
            if (AutoDestroy)
                GameClient.Instance.NextTick(() => Destroy(gameObject), 0.3f);
        };
    }

    public static FadeEffect Play(Color start, Color end, float time, System.Action onFinish)
    {
        GameObject objEffect = Instantiate(Resources.Load<GameObject>("Prefabs/FadeEffect"));
        objEffect.transform.SetParent(UISystem.Instance.effectRoot.transform, false);
        objEffect.transform.SetSiblingIndex(UISystem.Instance.effectRoot.transform.childCount - 1);
        FadeEffect effect = objEffect.GetComponent<FadeEffect>();
        effect.Init(start, end, time, onFinish);
        return effect;
    }
}
