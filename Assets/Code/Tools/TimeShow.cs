using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimeShow : MonoBehaviour
{
    public Text m_TShow;

    float m_fSTime = 0f;
    int m_n32Total = 0;
    int m_n32Time = 0;
    string m_Format;
    bool flag = false;
    public delegate void TimeCallback();
    protected TimeCallback m_pfunCallback;
    public void Init(int total, string format, TimeCallback callback)
    {
        if (!m_TShow) m_TShow = GetComponent<Text>();
        flag = true;
        m_fSTime = Time.realtimeSinceStartup;
        m_n32Total = total;
        m_n32Time = m_n32Total;
        m_Format = format;
        m_pfunCallback = callback;
    }
    void SetTime()
    {
        m_TShow.text = Common.SecondToStrTime(m_n32Time, m_Format);
    }
    void Update()
    {
        if (flag)
            UpdateTime();
    }
    void UpdateTime()
    {

        m_n32Time = m_n32Total - (int)(Time.realtimeSinceStartup - m_fSTime);

        if (m_n32Time < 0)
        {
            flag = false;
            m_n32Time = 0;
            SetTime();
            if (m_pfunCallback != null)
                m_pfunCallback();
            return;
        }
        SetTime();
    }
    public bool isOver()
    {
        return m_n32Time <= 0 ? true : false;
    }
    public int GetNTime()
    {
        return m_n32Time;
    }
}
