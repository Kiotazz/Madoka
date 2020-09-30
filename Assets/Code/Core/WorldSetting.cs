using System.Collections.Generic;

public class WorldSetting
{
    /// <summary>
    /// 技能/伤害属性
    /// </summary>
    public enum Effect
    {
        Physical,
        Fire,
        Cold,
        Light,
        Dark,
        Element,
        Nature,
    }

    /// <summary>
    /// 技能资源
    /// </summary>
    public enum Energy
    {
        Mana,
        Energy,
        HP,
    }
}
