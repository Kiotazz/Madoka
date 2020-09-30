using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Immediate : SkillBase
{
    protected override CastResult OnCast()
    {
        Settlement();
        return CastResult.Success;
    }
}
