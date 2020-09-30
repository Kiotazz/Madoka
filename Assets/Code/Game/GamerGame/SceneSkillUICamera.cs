using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSkillUICamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.parent = null;
        transform.position = new Vector3(100000, 100000, 100000);
    }
}
