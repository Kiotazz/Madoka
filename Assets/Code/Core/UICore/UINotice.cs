using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class UINotice : MonoBehaviour 
{
	public Text m_pobjNoticeLabel;
	public float fSpeed = 100f;

    Mask labelClipPanel;

    public bool inited { get; protected set; }

    public void OnLoad()
    {
        gameObject.SetActive(false);
        labelClipPanel = m_pobjNoticeLabel.transform.GetComponentInParent<Mask>();
        m_pobjNoticeLabel.transform.localPosition = new Vector3(labelClipPanel.rectTransform.localScale.z / 2, 0, 0);
    }

	public void Init(string text, string[] paramList)
	{
        if (inited)
            return;
        inited = true;
		m_pobjNoticeLabel.text = text;
	}

    public void Init(string text)
    {
        if (inited)
            return;
        inited = true;
        m_pobjNoticeLabel.text = text;
    }

    void Begin()
    {
        float length = UGUIMathf.CalculateRelativeBounds(m_pobjNoticeLabel.rectTransform).extents.x * 2;
        Debug.Log("label length " + length);
        float x = labelClipPanel.rectTransform.localScale.z + length;
        m_pobjNoticeLabel.transform.DOMoveX(labelClipPanel.rectTransform.localScale.z / 2 - x, x / fSpeed).onComplete = (() => 
        {
            gameObject.SetActive(false);
            GameClient.Instance.NextTick(() => 
            {
                UISystem.Instance.DestroyNotice(this);
            }, 0.5f);
        });
    }

    public void Show()
    {
        gameObject.SetActive(true);
        Invoke("Begin", 0.05f);
    }
}
