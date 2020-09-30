using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UISystem
{
    static UISystem s_Instance = null;
    public static UISystem Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = new UISystem();
                if (Application.isPlaying)
                    s_Instance.Init();
            }
            return s_Instance;
        }
    }
    private UISystem() { }



    public Canvas uiRoot { get; protected set; }
    public Canvas sceneuiRoot { get; protected set; }
    public Canvas effectRoot { get; protected set; }
    public Canvas debugRoot { get; protected set; }

    public float Height { get { return uiRoot.pixelRect.height; } }
    public float Width { get { return Screen.width * Height / Screen.height; } }
    public Vector3 uniformScale
    {
        get { return new Vector3((1 / uiRoot.transform.localScale.x), (1 / uiRoot.transform.localScale.y), (1 / uiRoot.transform.localScale.z)); }
    }

    Dictionary<string, UIWindow> m_dictWindows = new Dictionary<string, UIWindow>();
    List<UIWindow> m_listWindows = new List<UIWindow>();
    Queue<UIMessageBox> m_pqueMessageBox = new Queue<UIMessageBox>();
    Queue<UINotice> m_queNotices = new Queue<UINotice>();
    Queue<UIDialogue> m_queDialogue = new Queue<UIDialogue>();

    public bool inLoading { get { return m_pobjLoadingBox != null; } }
    GameObject m_pobjLoadingBox;
    public UIWindow RootWindow
    {
        get
        {
            if (m_listWindows != null && m_listWindows.Count > 0)
                return m_listWindows[0];
            return null;
        }
    }
    public UIWindow TopWindow
    {
        get
        {
            if (m_listWindows != null && m_listWindows.Count > 0)
                return m_listWindows[m_listWindows.Count - 1];
            return null;
        }
    }
    public UIWindow SecondWindow
    {
        get
        {
            if (m_listWindows != null && m_listWindows.Count > 1)
                return m_listWindows[m_listWindows.Count - 2];
            return null;
        }
    }
    public UIMessageBox TopMessageBox
    {
        get
        {
            if (m_pqueMessageBox.Count > 0)
                return m_pqueMessageBox.Peek();
            return null;
        }
    }
    public UILayer TopLayer
    {
        get
        {
            if (TopMessageBox != null)
                return TopMessageBox;
            return TopWindow;
        }
    }

    public bool InAnyGuide { get { return false; } }

    public bool inited { get; protected set; } = false;
    public void Init()
    {
        if (inited)
            return;
        inited = true;
        //Canvas[] roots = GameObject.FindObjectsOfType<Canvas>();
        //foreach (Canvas root in roots)
        //{
        //    if (root.tag == "Debug")
        //    {
        //        debugRoot = root;
        //    }
        //    else
        //    {
        //        if (root.gameObject.layer == LayerMask.NameToLayer("UI"))
        //        {
        //            if (uiRoot != null) Debug.LogWarning("UIRoot has more than 1!");
        //            uiRoot = root;
        //        }
        //        else if (root.gameObject.layer == LayerMask.NameToLayer("SceneUI"))
        //        {
        //            if (sceneuiRoot != null) Debug.LogWarning("UIRoot has more than 1!");
        //            sceneuiRoot = root;
        //        }
        //    }
        //}

        if (uiRoot == null)
        {
            GameObject prefabUIRoot = Resources.Load<GameObject>("Prefabs/UIRoot");
            if (prefabUIRoot)
            {
                GameObject objRoot = GameObject.Instantiate(prefabUIRoot);
                if (Application.isPlaying) GameObject.DontDestroyOnLoad(objRoot);
                Canvas[] roots = objRoot.GetComponentsInChildren<Canvas>(true);
                foreach (Canvas root in roots)
                {
                    if (root.tag == "Debug")
                    {
                        debugRoot = root;
                    }
                    else
                    {
                        if (root.name.Contains("UI"))
                        {
                            if (uiRoot != null) Debug.LogWarning("UIRoot has more than 1!");
                            uiRoot = root;
                        }
                        else if (root.name.Contains("Scene"))
                        {
                            if (sceneuiRoot != null) Debug.LogWarning("SceneUIRoot has more than 1!");
                            sceneuiRoot = root;
                        }
                        else if (root.name.Contains("Effect"))
                        {
                            if (effectRoot != null) Debug.LogWarning("EffectRoot has more than 1!");
                            effectRoot = root;
                        }
                    }
                }
            }
        }
        if (uiRoot == null)
        {
            Debug.LogWarning("No UIRoot found!");
            return;
        }

        m_dictWindows.Clear();
        m_listWindows.Clear();

        if (TopWindow)
            TopWindow.Create();
    }


    /// /////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///                              Window  管理
    /// /////////////////////////////////////////////////////////////////////////////////////////////////////////
    public UIWindow CreateRootWindow(string path)
    {
        while (TopWindow != null)
            DestroyWindow(TopWindow);
        return CreateWindow(path);
    }
    public UIWindow CreateWindow(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        char[] seprator = { '/', '\\' };
        string[] paths = path.Split(seprator, System.StringSplitOptions.RemoveEmptyEntries);
        string layerName = paths[paths.Length - 1];
        UIWindow layer = null;
        if (m_dictWindows.TryGetValue(layerName, out layer))
        {
            m_listWindows.Remove(layer);
            m_listWindows.Add(layer);
            return layer;
        }
        Object prefab = Resources.Load(path);
        if (prefab == null)
        {
            Debug.LogWarning("GUI prefab " + path + " could not found!");
            return null;
        }
        GameObject obj = Object.Instantiate(prefab) as GameObject;
        layer = obj.GetComponent<UIWindow>();
        RectTransform rtsf = obj.GetComponent<RectTransform>();
        rtsf.SetParent(uiRoot.transform, false);
        rtsf.offsetMin = Vector2.zero;
        rtsf.offsetMax = Vector2.zero;
        //layer.transform.localScale = Vector3.one;
        //layer.transform.localEulerAngles = Vector3.zero;
        layer.name = layerName;

        m_dictWindows.Add(layerName, layer);
        m_listWindows.Add(layer);
        layer.Create();

        return layer;
    }
    public T FindWindow<T>() where T : UIWindow
    {
        if (m_listWindows == null || m_listWindows.Count == 0)
            return null;
        foreach (UILayer window in m_listWindows)
        {
            if (window is T)
                return window as T;
        }
        return null;
    }
    void DestroyWindow(UIWindow layer)
    {
        if (!m_dictWindows.ContainsKey(layer.name))
        {
            Debug.LogWarning("Try to destroy an unexist layer!");
        }
        m_dictWindows.Remove(layer.name);
        m_listWindows.Remove(layer);
        layer.Destroy();
        Object.DestroyImmediate(layer.gameObject);
        if (TopWindow != null)
            TopWindow.Reshow();
    }
    public UIWindow Backward()
    {
        if (TopWindow != null && TopWindow != RootWindow)
        {
            DestroyWindow(TopWindow);
            TopWindow.Reshow();
            return TopWindow;
        }
        return null;
    }


    /// /////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///                              Message Box 管理
    /// /////////////////////////////////////////////////////////////////////////////////////////////////////////
    public UIMessageBox CreateMessageBox(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        char[] seprator = { '/', '\\' };
        string[] paths = path.Split(seprator, System.StringSplitOptions.RemoveEmptyEntries);
        string boxName = paths[paths.Length - 1];

        Object prefab = Resources.Load(path);
        if (prefab == null)
        {
            Debug.LogWarning("GUI prefab " + path + " could not found!");
            return null;
        }

        GameObject pobjMessageBox = Object.Instantiate(prefab) as GameObject;
        pobjMessageBox.transform.SetParent(uiRoot.transform, false);
        //pobjMessageBox.transform.localScale = Vector3.one;
        //pobjMessageBox.transform.localPosition = Vector3.zero;
        //pobjMessageBox.transform.localEulerAngles = Vector3.zero;
        pobjMessageBox.name = boxName;

        GameObject obj = new GameObject();
        obj.transform.SetParent(pobjMessageBox.transform, false);
        //obj.transform.localPosition = Vector3.zero;
        //obj.transform.localScale = Vector3.one;
        obj.layer = pobjMessageBox.layer;
        obj.AddComponent<BoxCollider>().size = new Vector3(10000f, 10000f, 1f);

        UIMessageBox box = pobjMessageBox.GetComponent<UIMessageBox>();
        m_pqueMessageBox.Enqueue(box);
        box.Create();
        //WorldManager.Instance.EnableEvent(false);
        return box;
    }
    public void DestroyMessageBox(UIMessageBox box)
    {
        if (TopMessageBox != box)
        {
            Debug.LogError("Try to destroy a non-top MessageBox");
            return;
        }
        Object.DestroyImmediate(m_pqueMessageBox.Dequeue().gameObject);
        //WorldManager.Instance.EnableEvent(true);
        if (TopMessageBox != null)
            TopMessageBox.Show();
    }
    public void PopGeneralMessageBox(string id, string text, System.Action<int> cb)
    {
        //GeneralMessageBox box = CreateMessageBox("GUI/General/GeneralMessageBox") as GeneralMessageBox;
        //box.Init(id, "");
        //box.setInfoText(text);
        //box.SetCallback(cb);
    }

    /// /////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///                              Dialogue  管理
    /// /////////////////////////////////////////////////////////////////////////////////////////////////////////
    public UIDialogue CreateDialogue(string path)
    {
        char[] seprator = { '/', '\\' };
        string[] paths = path.Split(seprator, System.StringSplitOptions.RemoveEmptyEntries);
        string boxName = paths[paths.Length - 1];

        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogWarning("GUI prefab GUI/" + path + " could not found!");
            return null;
        }

        GameObject obj = Object.Instantiate(prefab);
        obj.transform.parent = uiRoot.transform;
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localEulerAngles = Vector3.zero;
        UIDialogue dialog = obj.GetComponent<UIDialogue>();
        m_queDialogue.Enqueue(dialog);
        dialog.Create();
        //WorldManager.Instance.EnableEvent(false);
        return dialog;
    }
    public void DestroyDialogue(UIDialogue dialog)
    {
        //DebugLog.LogWarning("Try to destroy an unmatched DialogGuide");
        if (m_queDialogue.Peek() != dialog)
            return;
        Object.DestroyImmediate(m_queDialogue.Dequeue().gameObject);
        //WorldManager.Instance.EnableEvent(true);
    }


    /// /////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///                              Notice  管理
    /// /////////////////////////////////////////////////////////////////////////////////////////////////////////
    public UINotice CreateNotice()
    {
        Object prefab = Resources.Load("GUI/General/NoticeUI");
        GameObject pobjNoticeUI = Object.Instantiate(prefab) as GameObject;
        pobjNoticeUI.transform.parent = uiRoot.transform;
        pobjNoticeUI.transform.localScale = Vector3.one;
        pobjNoticeUI.transform.localPosition = Vector3.zero;
        pobjNoticeUI.transform.localEulerAngles = Vector3.zero;
        UINotice ui = pobjNoticeUI.GetComponent<UINotice>();
        m_queNotices.Enqueue(ui);
        ui.OnLoad();
        if (m_queNotices.Count == 1)
            ui.Show();
        return ui;
    }
    public void DestroyNotice(UINotice notice)
    {
        if (m_queNotices.Count > 0 && m_queNotices.Peek() == notice)
        {
            m_queNotices.Dequeue();
            Object.DestroyImmediate(notice.gameObject);
            if (m_queNotices.Count > 0)
                m_queNotices.Peek().Show();
        }
    }


    /// //////////////////////////////////////////////////////////////////////////////////////////////////////
    ///                               Loading Box 管理
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////
    public UILoading CreateLoadingBox()
    {
        return CreateLoadingBox("GUI/General/LoadingBox");
    }
    public UILoading CreateLoadingBox(string loadbox)
    {
        if (inLoading)
            return null;
        Object prefab = Resources.Load(loadbox);
        m_pobjLoadingBox = Object.Instantiate(prefab) as GameObject;
        m_pobjLoadingBox.transform.parent = uiRoot.transform;
        m_pobjLoadingBox.transform.localScale = Vector3.one;
        m_pobjLoadingBox.transform.localPosition = Vector3.zero;
        m_pobjLoadingBox.transform.localEulerAngles = Vector3.zero;
        UILoading loadingBox = m_pobjLoadingBox.GetComponent<UILoading>();
        //WorldManager.Instance.EnableEvent(false);
        return loadingBox;
    }
    public void DestroyLoadingBox()
    {
        if (inLoading)
        {
            Object.DestroyImmediate(m_pobjLoadingBox);
            m_pobjLoadingBox = null;
            //WorldManager.Instance.EnableEvent(true);
        }
    }

    public bool IsMouseOnUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    public void DoUpdate(float deltaTime)
    {
        for (int i = 0, length = m_listWindows.Count; i < length; ++i)
            m_listWindows[i].DoUpdate(deltaTime);

        if (fMsgBoxTimer > 0) fMsgBoxTimer -= Time.deltaTime;
    }


    const string MessageBoxPath = "GUI/MessageBox/MessageBox";
    const string GuideDialogFolder = "GUI/GuideDialog/";

    GameObject objMsgBoxPrefab;
    GuideDialog curGuide;
    float fMsgBoxTimer = 0;
    //QuickTools
    public void ShowMessageBox(string text) { ShowMessageBox(text, 1.5f); }
    public void ShowMessageBox(string text, float time)
    {
        if (string.IsNullOrEmpty(text) || fMsgBoxTimer > 0) return;
        if (!objMsgBoxPrefab) objMsgBoxPrefab = Resources.Load<GameObject>(MessageBoxPath);
        GameObject obj = GameObject.Instantiate(objMsgBoxPrefab, uiRoot.transform);
        obj.GetComponent<MessageBox>().Init(text, time);
        fMsgBoxTimer = 0.5f;
    }

    public void ShowGuideDialog(string name)
    {
        GameObject objGuidePrefab = Resources.Load<GameObject>(GuideDialogFolder + name);
        if (!objGuidePrefab)
        {
            Debug.LogError("加载GuideDialog失败！请检查路径：" + GuideDialogFolder + name);
            return;
        }
        if (curGuide) GameObject.Destroy(curGuide.gameObject);
        GameObject obj = GameObject.Instantiate(objGuidePrefab, uiRoot.transform);
        curGuide = obj.GetComponent<GuideDialog>();
        curGuide.Init();
    }

    public void CloseGuide()
    {
        if (curGuide) curGuide.OnBtnClose();
    }
}
