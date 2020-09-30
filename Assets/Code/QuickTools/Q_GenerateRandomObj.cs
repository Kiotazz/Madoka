using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Q_GenerateRandomObj : MonoBehaviour
{
    public GameObject[] objPrefabs;
    public Transform[] tsfGeneratePos;

    public int nDelay = 100;

    float _counter = 0;

    void Update()
    {
        if (objPrefabs.Length < 1 || tsfGeneratePos.Length < 1) return;
        if ((_counter += Time.deltaTime) > nDelay / 1000)
        {
            _counter = 0;
            GameObject obj = Instantiate(objPrefabs[Random.Range(0, objPrefabs.Length - 1)]);
            Transform target = tsfGeneratePos[Random.Range(0, tsfGeneratePos.Length - 1)];
            obj.transform.position = target.position;
            obj.transform.rotation = target.rotation;
        }
    }
}
