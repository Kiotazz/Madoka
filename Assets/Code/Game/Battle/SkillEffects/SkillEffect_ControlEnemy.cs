using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_ControlEnemy : SkillEffectBase
{
    protected override void OnExecute(InteractiveObj self, InteractiveObj target)
    {
        if (target.IsEnemy(self.Camp))
        {
            target.Camp = self.Camp;
            SceneObjHeadInfo info = target.UIHeadInfo;
            if (info)
            {
                if (self.Camp == Common.MyselfCamp)
                {
                    info.txtBlood.color = Color.green;
                    info.sldBlood.fillRect.GetComponent<UnityEngine.UI.Graphic>().color = Color.green;
                    DrawAttackRange draw = target.GetComponent<DrawAttackRange>();
                    if (draw) draw.bNeedUpdate = false;
                    LineRenderer line = target.GetComponent<LineRenderer>();
                    if (line) line.enabled = false;
                }
                else
                {
                    info.txtBlood.color = Color.red;
                    info.sldBlood.fillRect.GetComponent<UnityEngine.UI.Graphic>().color = Color.red;
                }
            }
            self.DoDamage(new Damage(WorldSetting.Effect.Dark, 5), WorldInteractObj.Instance);
        }
    }
}
