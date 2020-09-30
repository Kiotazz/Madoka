using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameClient : MonoBehaviour
{
    static GameClient _instance = null;
    public static GameClient Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameClient>();
                if (_instance == null)
                {
                    GameObject go = new GameObject();
                    _instance = go.AddComponent<GameClient>();
                }
                _instance.gameObject.name = "GameClient";
            }
            return _instance;
        }
    }

    public enum GameMode
    {
        FirstPerson,
        ThirdPerson,
        GodView,
    }

    public GameMode Mode;


    void Awake()
    {
        if (Instance != this)
        {
            DestroyImmediate(gameObject);
            return;
        }
        if (Application.isPlaying)
            DontDestroyOnLoad(gameObject);
    }
    // Use this for initialization
    void Start()
    {
        if (!Application.isPlaying) return;
        Event_Game.OnGameStart.Invoke();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        BattleManager.Instance.DoUpdate(deltaTime);
        UISystem.Instance.DoUpdate(deltaTime);
    }

    private void FixedUpdate()
    {
        float fixedDeltaTime = Time.fixedDeltaTime;
        BattleManager.Instance.DoFixedUpdate(fixedDeltaTime);
    }

    private void LateUpdate()
    {
        float deltaTime = Time.deltaTime;
    }

    public void LoadScene() { LoadScene(SceneManager.GetActiveScene().name); }
    public void LoadScene(string sceneName)
    {
        string loadSceneName = sceneName;
        FadeEffect effect = null;
        effect = FadeEffect.Play(new Color(0, 0, 0, 0), Color.black, 1, () =>
         {
             for (int i = UISystem.Instance.sceneuiRoot.transform.childCount - 1; i > -1; --i)
             {
                 Destroy(UISystem.Instance.sceneuiRoot.transform.GetChild(i).gameObject);
             }
             AsyncOperation operation = SceneManager.LoadSceneAsync(loadSceneName);
             operation.completed += (option) =>
             {
                 FadeEffect.Play(Color.black, new Color(0, 0, 0, 0), 1, null);
                 if (effect) Destroy(effect.gameObject);
             };
         });
        effect.AutoDestroy = false;
    }

    public void LoadScene(int index)
    {
        int sceneIndex = index;
        FadeEffect effect = null;
        effect = FadeEffect.Play(new Color(0, 0, 0, 0), Color.black, 1, () =>
        {
            for (int i = UISystem.Instance.sceneuiRoot.transform.childCount - 1; i > -1; --i)
            {
                Destroy(UISystem.Instance.sceneuiRoot.transform.GetChild(i).gameObject);
            }
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
            operation.completed += (option) =>
            {
                FadeEffect.Play(Color.black, new Color(0, 0, 0, 0), 1, null);
                if (effect) Destroy(effect.gameObject);
            };
        });
        effect.AutoDestroy = false;
    }

    public System.Action NextTick(System.Action func)
    {
        return NextTick(func, 0);
    }
    public System.Action NextTick(System.Action func, float delay)
    {
        if (func != null)
            StartCoroutine(DelayCall(func, delay));
        return func;
    }
    public System.Action<int> NextTick(System.Action<int> func, int ret, float delay)
    {
        if (func != null)
            StartCoroutine(DelayCall(func, ret, delay));
        return func;
    }
    protected static IEnumerator DelayCall(System.Action cb, float delay)
    {
        if (delay == 0)
            yield return null;
        else
            yield return new WaitForSeconds(delay);
        cb();
    }
    protected static IEnumerator DelayCall(System.Action<int> cb, int ret, float delay)
    {
        if (delay == 0)
            yield return null;
        else
            yield return new WaitForSeconds(delay);
        cb(ret);
    }
}
