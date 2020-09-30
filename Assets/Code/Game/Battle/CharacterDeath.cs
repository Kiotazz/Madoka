using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDeath : InteractiveObjExt
{
    protected override void OnInit(InteractiveObj obj)
    {
        Master.OnDeath.AddListener(() => Destroy(Master.gameObject));
    }
}
