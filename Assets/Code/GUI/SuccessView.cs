using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class SuccessView : UIWindow
{
    public Image img;

    protected override void OnLoad(LoadOperation op)
    {
        img.color = new Color(1, 1, 1, 0);
        img.DOFade(1, 1);
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            int index = SceneManager.GetActiveScene().buildIndex + 1;
            if (index < SceneManager.sceneCountInBuildSettings)
            {
                GameClient.Instance.LoadScene(index);
                UISystem.Instance.Backward();
            }
            else
            {
                UISystem.Instance.ShowMessageBox("恭喜您已通关！");
            }
        }
    }
}
