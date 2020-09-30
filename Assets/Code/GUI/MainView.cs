using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainView : UIWindow
{
    public GameObject prefabBlood;
    public GridLayoutGroup bloodLayout;

    public GameObject objPickUp;
    public GameObject objShoot;
    public GameObject objJump;

    List<GameObject> objBlood = new List<GameObject>();

    protected override void OnLoad(LoadOperation op)
    {
        ThirdPersonPlayer.Instance.OnHpChange.AddListener(RefreshHP);
        RefreshHP(ThirdPersonPlayer.Instance.HP);
    }

    void RefreshHP(long value)
    {
        int curHp = Mathf.Max((int)value, 0);
        for (int i = objBlood.Count - 1; i >= curHp; --i)
        {
            Destroy(objBlood[i].gameObject);
            objBlood.RemoveAt(i);
        }
        for (int i = objBlood.Count; i < curHp; ++i)
        {
            objBlood.Add(Instantiate(prefabBlood, bloodLayout.transform));
        }
    }

    private void Update()
    {
        if (!ThirdPersonPlayer.Instance) return;
        if (objPickUp.activeSelf != ThirdPersonPlayer.Instance.bCanUseItemSkill)
            objPickUp.SetActive(ThirdPersonPlayer.Instance.bCanUseItemSkill);
        if (objShoot.activeSelf != ThirdPersonPlayer.Instance.bCanUseShootSkill)
            objShoot.SetActive(ThirdPersonPlayer.Instance.bCanUseShootSkill);
        if (objJump.activeSelf != ThirdPersonPlayer.Instance.bCanUseJumpSkill)
            objJump.SetActive(ThirdPersonPlayer.Instance.bCanUseJumpSkill);
    }

    public void Jump()
    {
        if (!ThirdPersonPlayer.Instance) return;
        ThirdPersonPlayer.Instance.Jump();
    }
}
