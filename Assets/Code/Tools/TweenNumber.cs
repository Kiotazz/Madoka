using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TweenNumber : MonoBehaviour
{
    public bool m_bCutInConsole = true;
    public string m_strPrefix;
    public string m_strSufix;
    public Image m_pobjProgressBar;

    public Color m_colorOver = new Color(0.1f, 1, 0.1f);
    public Color m_colorLow = new Color(1, 1, 0);
    public Color m_colorDanger = new Color(1, 0, 0);

    public int MaxValue { get; protected set; }
    public int CurrentValue { get; protected set; }
    public float Rate { get { return (float)CurrentValue / MaxValue; } }

    protected int StepValue { get; set; }

    Color m_colorOrigin;
    Text m_pobjLabel;
    int m_n32Amply;

    bool m_bStepping = false;
    float m_fStartTime;
    float m_fTotalTime;
    System.Action m_pfunCallback;

    bool m_bInited = false;
    bool m_bUnknown = false;
    // Use this for initialization
    void Init()
    {
        if (m_bInited)
            return;
        m_bInited = true;
        if (m_pobjLabel == null)
            m_pobjLabel = GetComponent<Text>();
        m_colorOrigin = m_pobjLabel.color;
        if (m_pobjLabel == null)
            Destroy(this);
    }

    public void SetCurrent(int curr)
    {
        Init();
        if (m_bUnknown)
            return;
        if (m_bStepping)
            m_bStepping = false;
        CurrentValue = curr;
        StepValue = CurrentValue;
        ShowStep();
    }

    public void SetMax(int max)
    {
        MaxValue = max;
    }

    public void StepTo(int to, float time, System.Action cb)
    {
        Init();
        if (m_bUnknown || to == CurrentValue)
        {
            if (cb != null)
                GameClient.Instance.NextTick(cb);
            return;
        }
        m_pfunCallback += cb;
        m_bStepping = true;
        CurrentValue = to;
        m_n32Amply = CurrentValue - StepValue;
        m_fTotalTime = time;
        m_fStartTime = Time.time;
    }

    public void SetUnknown()
    {
        Init();
        m_bUnknown = true;
        MaxValue = 1;
        CurrentValue = 1;
        m_pobjLabel.text = m_strPrefix + "?????";
        if (m_pobjProgressBar != null)
        {
            m_pobjProgressBar.fillAmount = 1f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_bStepping)
        {
            float elapseTime = Time.time - m_fStartTime;
            if (elapseTime >= m_fTotalTime)
            {
                StepValue = CurrentValue;
                m_bStepping = false;
                ShowStep();
                if (m_pfunCallback != null)
                    GameClient.Instance.NextTick(m_pfunCallback);
                m_pfunCallback = null;
                return;
            }
            int curr = (int)(m_n32Amply * elapseTime / m_fTotalTime) + CurrentValue - m_n32Amply;
            StepValue = curr;
            ShowStep();
        }
    }

    protected void ShowStep()
    {
        float rate = (float)StepValue / MaxValue;
        if (rate <= 0.3f)
        {
            if (rate <= 0.15f) m_pobjLabel.color = m_colorDanger;
            else m_pobjLabel.color = m_colorLow;
        }
        else if (rate > 1f)
        {
            m_pobjLabel.color = m_colorOver;
        }
        else
        {
            m_pobjLabel.color = m_colorOrigin;
        }
        m_pobjLabel.text = "";
        if (!string.IsNullOrEmpty(m_strPrefix)) m_pobjLabel.text += m_strPrefix;
        if (StepValue < 10000 || !m_bCutInConsole)
            m_pobjLabel.text += StepValue.ToString();
        else
            m_pobjLabel.text += StepValue > 999999 ? (StepValue / 1000000f).ToString("f1") + "M" : (Mathf.RoundToInt(StepValue / 1000f)) + "K";
        if (!string.IsNullOrEmpty(m_strSufix)) m_pobjLabel.text += m_strSufix;
        if (m_pobjProgressBar != null && MaxValue != 0)
        {
            m_pobjProgressBar.fillAmount = (float)StepValue / MaxValue;
        }
    }
}
