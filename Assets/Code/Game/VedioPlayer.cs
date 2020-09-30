using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RenderHeads.Media.AVProVideo;

public class VedioPlayer : MonoBehaviour
{
    public static NormalEvent OnVedioFinished { get; private set; } = new NormalEvent();

    System.Action cbOnFinished;
    MediaPlayer _mediaPlayer;
    DisplayIMGUI _display;
    string _filePath;
    bool _usingFading = false;

    bool bFadeOK = false;
    bool bLoadOK = false;
    bool ReadyToPlay { get { return bFadeOK && bLoadOK; } }

    FadeEffect fadeLoad;

    public void Init(string filePath, System.Action callback)
    {
        bFadeOK = bLoadOK = false;
        _filePath = filePath;
        cbOnFinished = callback;
        _mediaPlayer = GetComponentInChildren<MediaPlayer>();
        _display = GetComponentInChildren<DisplayIMGUI>();
        _mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
        LoadVideo(filePath);
        _mediaPlayer.m_Volume = 0.3f;

        ThirdPersonPlayer.Instance.CanOperate = false;
        if (BGM.Instance) BGM.Instance.source.mute = true;
        _display.gameObject.SetActive(false);
        fadeLoad = FadeEffect.Play(new Color(1, 1, 1, 0), Color.white, 1, () =>
        {
            bFadeOK = true;
            TryPlayVedio();
        });
        fadeLoad.AutoDestroy = false;
    }

    // Callback function to handle events
    public void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    {
        switch (et)
        {
            case MediaPlayerEvent.EventType.ReadyToPlay:
                bLoadOK = true;
                TryPlayVedio();
                break;
            case MediaPlayerEvent.EventType.Started:
                break;
            case MediaPlayerEvent.EventType.FirstFrameReady:
                break;
            case MediaPlayerEvent.EventType.MetaDataReady:
                break;
            case MediaPlayerEvent.EventType.FinishedPlaying:
                FadeEffect.Play(Color.white, new Color(1, 1, 1, 0), 1, Close);
                _display.gameObject.SetActive(false);
                break;
            case MediaPlayerEvent.EventType.Error:
                Close();
                break;
        }
    }

    private void Close()
    {
        if (fadeLoad)
        {
            DestroyImmediate(fadeLoad.gameObject);
            fadeLoad = null;
        }
        ThirdPersonPlayer.Instance.CanOperate = true;
        if (BGM.Instance) BGM.Instance.source.mute = false;
        OnVedioFinished.Invoke();
        if (cbOnFinished != null) cbOnFinished();
        Destroy(gameObject);
    }

    private void TryPlayVedio()
    {
        if (!ReadyToPlay) return;
        _display.gameObject.SetActive(true);
        if (fadeLoad)
        {
            DestroyImmediate(fadeLoad.gameObject);
            fadeLoad = null;
        }
        _mediaPlayer.Play();
    }

    private void LoadVideo(string filePath)
    {
        // IF we're not using fading then load the video immediately
        if (!_usingFading)
        {
            // Load the video
            if (!_mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, filePath, _mediaPlayer.m_AutoStart))
            {
                Debug.LogError("Failed to open video!");
            }
        }
        else
        {
            StartCoroutine("LoadVideoWithFading");
        }
    }

    private IEnumerator LoadVideoWithFading()
    {
        const float FadeDuration = 0.25f;
        float fade = FadeDuration;

        // Fade down
        while (fade > 0f && Application.isPlaying)
        {
            fade -= Time.deltaTime;
            fade = Mathf.Clamp(fade, 0f, FadeDuration);

            _display._color = new Color(1f, 1f, 1f, fade / FadeDuration);
            _display._mediaPlayer.Control.SetVolume(fade / FadeDuration);

            yield return null;
        }

        // Wait 3 frames for display object to update
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // Load the video
        if (Application.isPlaying)
        {
            if (!_mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, _filePath, _mediaPlayer.m_AutoStart))
            {
                Debug.LogError("Failed to open video!");
            }
            else
            {
                // Wait for the first frame to come through (could also use events for this)
                while (Application.isPlaying && (VideoIsReady(_mediaPlayer) || AudioIsReady(_mediaPlayer)))
                {
                    yield return null;
                }

                // Wait 3 frames for display object to update
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
            }
        }

        // Fade up
        while (fade < FadeDuration && Application.isPlaying)
        {
            fade += Time.deltaTime;
            fade = Mathf.Clamp(fade, 0f, FadeDuration);

            _display._color = new Color(1f, 1f, 1f, fade / FadeDuration);
            _display._mediaPlayer.Control.SetVolume(fade / FadeDuration);

            yield return null;
        }
    }

    private static bool VideoIsReady(MediaPlayer mp)
    {
        return (mp != null && mp.TextureProducer != null && mp.TextureProducer.GetTextureFrameCount() <= 0);

    }
    private static bool AudioIsReady(MediaPlayer mp)
    {
        return (mp != null && mp.Control != null && mp.Control.CanPlay() && mp.Info.HasAudio() && !mp.Info.HasVideo());
    }

    public static void Play(string path, System.Action onFinished)
    {
        GameObject player = Instantiate(Resources.Load<GameObject>("Prefabs/VedioPlayer"));
        player.AddComponent<VedioPlayer>().Init(path, onFinished);
    }
}
