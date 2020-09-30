using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LevelStartMsg : UIMessageBox
{
    public static LevelStartMsg Instance;

    public Image imgStart;
    // Start is called before the first frame update
    protected override void OnLoad(LoadOperation op)
    {
        if (Instance) Instance.ExitNow();
        Instance = this;
        imgStart.color = new Color(1, 1, 1, 0);
        Tweener t = imgStart.DOFade(1, 1f);
        t.SetEase(Ease.Linear);
        t.onComplete = () =>
        {
            Tweener t2 = imgStart.DOFade(0, 2f);
            t2.SetEase(Ease.Linear);
            t2.onComplete = ExitNow;
        };
    }

    void ExitNow()
    {
        UISystem.Instance.DestroyMessageBox(this);
        Instance = null;
    }
}
