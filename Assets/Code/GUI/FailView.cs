using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FailView : UIWindow
{
    public Text txtDeath;
    public Text txtNext;

    bool _init = false;

    protected override void OnLoad(LoadOperation op)
    {
        txtDeath.color = new Color(1, 1, 1, 0);
        txtNext.color = new Color(1, 1, 1, 0);
        txtDeath.DOColor(Color.red, 5);
        GameClient.Instance.NextTick(() =>
        {
            _init = true;
            txtNext.DOColor(Color.red, 3);

        }, 2f);
    }

    void Update()
    {
        if (!_init) return;
        if (Input.GetKeyDown(KeyCode.U))
        {
            UISystem.Instance.Backward();
            ThirdPersonPlayer.Instance.Reborn();
            return;
        }
        if (Input.anyKeyDown)
        {
            _init = false;
            GameClient.Instance.LoadScene();
            UISystem.Instance.Backward();
        }
    }
}
