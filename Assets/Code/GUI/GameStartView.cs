using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
public class GameStartView : MonoBehaviour
{
    bool end = false;

    public void Init()
    {
        end = false;
    }

    private void Update()
    {
        if (!end && Input.anyKeyDown)
        {
            GameStart();
        }
    }

    void GameStart()
    {

    }
}
