using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GuideDialog : MonoBehaviour
{
    CanvasGroup group;

    public void Init()
    {
        (group = GetComponent<CanvasGroup>()).alpha = 0;
        transform.localScale = Vector3.zero;
        group.DOFade(1, 0.5f);
    }

    public void OnBtnClose()
    {
        group.DOFade(0, 0.5f).onComplete = () => Destroy(gameObject);
    }
}
