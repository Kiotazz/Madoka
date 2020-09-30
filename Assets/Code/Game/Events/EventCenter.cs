//用来定义全局事件
using UnityEngine.Events;

public static class Event_Game
{
    public static UnityEvent OnGameStart { get; private set; } = new UnityEvent();
    public static UnityEvent OnLoadSceneOver { get; private set; } = new UnityEvent();
}

public static class Event_Battle
{
    public static UnityEvent OnBattleBegin { get; private set; } = new UnityEvent();
    public static BoolEvent OnBattleOver { get; private set; } = new BoolEvent();
    public static CharacterEvent OnCharacterDead { get; private set; } = new CharacterEvent();
}