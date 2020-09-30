using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MessageBox : MonoBehaviour
{

    public void Init(string text, float time)
    {
        GetComponentInChildren<Text>().text = text;
        gameObject.transform.DOMove(new Vector3(0, 200, 0), time).onComplete=() => Destroy(gameObject);
        gameObject.GetComponent<CanvasGroup>().DOFade(0, time).SetEase(Ease.InOutQuad);
    }
}
