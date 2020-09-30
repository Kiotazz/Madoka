using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioSource audioSource { get; protected set; }

    public bool Alive { get; protected set; } = false;

    public void Play(AudioClip clip) { Play(clip, 1); }
    public void Play(AudioClip clip, float volume)
    {
        if (!audioSource)
            audioSource = gameObject.GetOrAddComponent<AudioSource>();
        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
        Alive = true;
    }

    private void Update()
    {
        if (Alive && !audioSource.isPlaying)
            Recycle();
    }

    public void Recycle()
    {
        if (!Alive) return;
        audioSource.Stop();
        Alive = false;
        AudioSystem.Instance.Recycle(this);
    }
}
