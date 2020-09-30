using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameEntrance : MonoBehaviour
{
    public Image imgStaffs;

    bool bStaffShow = false;
    bool bFading = false;

    private void Start()
    {
        Screen.SetResolution(1920, 1080, true, 80);

        bStaffShow = bFading = false;
        imgStaffs.color = new Color(1, 1, 1, 0);
        imgStaffs.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (bStaffShow && !bFading && Input.anyKeyDown)
        {
            bStaffShow = false;
            bFading = true;
            imgStaffs.DOFade(0, 1f).SetEase(Ease.Linear);
            GameClient.Instance.NextTick(() =>
            {
                bFading = false;
                imgStaffs.gameObject.SetActive(false);
            }, 1f);
        }
    }

    public void GameStart()
    {
        GameClient.Instance.LoadScene(1);
    }

    public void ShowStaff()
    {
        if (bStaffShow || bFading) return;
        bStaffShow = bFading = true;
        imgStaffs.gameObject.SetActive(true);
        imgStaffs.DOFade(1, 1f).SetEase(Ease.Linear);
        GameClient.Instance.NextTick(() => bFading = false, 1f);
    }

    public void GameExit()
    {
        Application.Quit();
    }
}
