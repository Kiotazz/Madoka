using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGM : MonoBehaviour
{
    public static BGM Instance;

    public AudioSource source { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        source = GetComponent<AudioSource>();
    }
}
