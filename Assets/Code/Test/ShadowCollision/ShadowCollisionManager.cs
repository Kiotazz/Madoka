using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowCollisionManager : MonoBehaviour
{
    public ShadowCollision[] shadowObjs;
    public Light[] lights;

    List<Light> listLights = new List<Light>();

    void Start()
    {
        for (int i = 0, length = lights.Length; i < length; ++i)
            listLights.Add(lights[i]);
        for (int i = 0, length = shadowObjs.Length; i < length; ++i)
            shadowObjs[i].Init(this);
    }

    void Update()
    {

    }

    public List<Light> GetLights()
    {
        return listLights;
    }

    public void AddLight(Light l)
    {
        listLights.Add(l);
    }
}
