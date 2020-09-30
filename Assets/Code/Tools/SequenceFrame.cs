using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SequenceFrame : MonoBehaviour
{
    public bool m_bPlayOnStart = true;
    public bool m_bIsLoop = true;
    public bool m_bRandomBegin = false;

    public bool m_bInvertY = false;
    public int m_nTotalInX = 7;
    public int m_nTotalInY = 7;
    int m_nXCounter = 0;
    int m_nYCounter = 0;

    public int m_nFrameInterval = 3;
    int m_nCounter = 0;

    public bool m_bUseFixedUpdate = false;
    public bool m_bAlwaysTop = true;
    public int m_nRendeQueue = 5000;

    Material m_mMat;
    RawImage m_img;
    bool m_bEffect = false;
    bool m_bPlayOver = true;
    System.Action<string> m_cbPlayOver;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend)
            m_mMat = rend.material;
        if (!m_mMat)
            m_img = GetComponent<RawImage>();
        m_bEffect = m_mMat || m_img;

        if (m_nFrameInterval < 1) m_nFrameInterval = 1;
        if (m_nTotalInX < 1) m_nTotalInX = 1;
        if (m_nTotalInY < 1) m_nTotalInY = 1;

        if (m_mMat && m_bAlwaysTop)
        {
            m_mMat.renderQueue = m_nRendeQueue;
            m_mMat.DisableKeyword("ZTest");
        }
        if (m_img)
        {
            float w = (float)1 / m_nTotalInX, h = (float)1 / m_nTotalInY;
            m_img.uvRect = new Rect(0, 0, w, h);
            if (m_bAlwaysTop)
            {
                m_img.material.renderQueue = m_nRendeQueue;
                m_img.material.DisableKeyword("ZTest");
            }
        }

        if (m_bPlayOnStart) Play();
    }

    void Update()
    {
        if (!m_bUseFixedUpdate && m_bEffect)
            UpdateFrame();
    }

    void FixedUpdate()
    {
        if (m_bUseFixedUpdate && m_bEffect)
            UpdateFrame();
    }

    void UpdateFrame()
    {
        if (m_bPlayOver) return;
        if ((m_nCounter = ++m_nCounter % m_nFrameInterval) == 0)
        {
            if ((m_nXCounter = (m_nXCounter + 1) % m_nTotalInX) == 0)
            {
                if ((m_nYCounter = m_nYCounter + 1) >= m_nTotalInY)
                {
                    if (m_cbPlayOver != null)
                    {
                        m_cbPlayOver(name);
                        m_cbPlayOver = null;
                    }
                    if (!m_bIsLoop)
                    {
                        m_bPlayOver = true;
                        return;
                    }
                    m_nYCounter = 0;
                }
            }
            if (m_mMat)
            {
                Vector2 offset = m_mMat.mainTextureOffset;
                offset.x = (float)m_nXCounter / m_nTotalInX;
                offset.y = (float)(m_bInvertY ? m_nTotalInY - 1 - m_nYCounter : m_nYCounter) / m_nTotalInY;
                m_mMat.mainTextureOffset = offset;
            }
            if (m_img)
            {
                Rect r = m_img.uvRect;
                r.x = (float)m_nXCounter / m_nTotalInX;
                r.y = (float)(m_bInvertY ? m_nTotalInY - 1 - m_nYCounter : m_nYCounter) / m_nTotalInY;
                m_img.uvRect = r;
            }
        }
    }

    public void Play() { Play(m_bIsLoop); }
    public void Play(bool isLoop)
    {
        m_bIsLoop = isLoop;
        if (m_bRandomBegin)
        {
            m_nXCounter = Random.Range(0, m_nTotalInX - 1);
            m_nYCounter = Random.Range(0, m_nTotalInY - 1);
        }
        else
            m_nXCounter = m_nYCounter = 0;

        gameObject.SetActive(true);
        m_bPlayOver = false;
    }

    public void SetPause(bool startPause)
    {
        m_bPlayOver = startPause;
    }

    public void Stop() { Stop(false); }
    public void Stop(bool keepObjActive)
    {
        m_bPlayOver = true;
        if (!keepObjActive) gameObject.SetActive(false);
    }
}
