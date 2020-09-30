using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSystem : MonoBehaviour
{
    const string AudioPlayerPath = "Prefabs/AudioPlayer";

    static AudioSystem _instance = null;
    public static AudioSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType(typeof(AudioSystem)) as AudioSystem;
                if (_instance == null)
                {
                    GameObject go = new GameObject();
                    _instance = go.AddComponent<AudioSystem>();
                    _instance.Init();
                }
                _instance.gameObject.name = "AudioSystem";
                if (Application.isPlaying)
                    DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }

    GameObject objAudioPlayerPrefab;
    Transform tsfActiveAudios;
    Transform tsfAudioPlayerPool;
    List<AudioPlayer> listPlayerPool = new List<AudioPlayer>();

    void Awake()
    {
        if (Instance != this)
        {
            DestroyImmediate(gameObject);
            return;
        }
    }

    private void Init()
    {
        objAudioPlayerPrefab = Resources.Load<GameObject>(AudioPlayerPath);
        GameObject obj = new GameObject();
        obj.name = "AudioPlayerPool";
        tsfAudioPlayerPool = obj.transform;
        tsfAudioPlayerPool.SetParent(transform, false);
        obj.SetActive(false);

        obj = new GameObject();
        obj.name = "ActiveAudios";
        tsfActiveAudios = obj.transform;
        tsfActiveAudios.SetParent(transform, false);
    }

    public AudioPlayer PlayAtPos(AudioClip clip, Vector3 worldPos)
    {
        AudioPlayer player = GetPlayer();
        player.transform.position = worldPos;
        player.Play(clip);
        return player;
    }

    public AudioPlayer PlayOnTransform(AudioClip clip, Transform parent)
    {
        if (!clip) return null;
        AudioPlayer player = GetPlayer();
        player.transform.SetParent(parent, false);
        player.Play(clip);
        return player;
    }

    AudioPlayer GetPlayer()
    {
        AudioPlayer player = null;
        if (listPlayerPool.Count > 0)
        {
            player = listPlayerPool[listPlayerPool.Count - 1];
            listPlayerPool.RemoveAt(listPlayerPool.Count - 1);
            player.transform.parent = tsfActiveAudios;
        }
        else
        {
            GameObject obj = Instantiate(objAudioPlayerPrefab, tsfActiveAudios);
            player = obj.GetComponent<AudioPlayer>();
        }
        return player;
    }

    public void Recycle(AudioPlayer player)
    {
        player.transform.parent = tsfAudioPlayerPool;
        listPlayerPool.Add(player);
    }
}
