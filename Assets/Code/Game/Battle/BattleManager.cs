using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class BattleManager
{
    public static implicit operator bool(BattleManager manager) { return manager != null; }
    private static BattleManager _instance;
    public static BattleManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = new BattleManager();
                _instance.Init();
            }
            return _instance;
        }
    }
    private BattleManager() { }


    public Commander MainCommander { get; protected set; }
    public bool IsBattleBegin { get; protected set; } = false;

    public Dictionary<int, InteractiveObj> dicInteractiveObjs { get; private set; } = new Dictionary<int, InteractiveObj>();

    private void Init()
    {
        if (GameClient.Instance.Mode == GameClient.GameMode.GodView)
        {
            MainCommander = new Commander();
            MainCommander.Init();
        }
    }

    public void BattleBegin()
    {
        IsBattleBegin = true;
        Event_Battle.OnBattleBegin.Invoke();
    }

    public void BattleOver(bool isWin)
    {
        IsBattleBegin = false;
        Event_Battle.OnBattleOver.Invoke(isWin);
        if (isWin)
        {
            UISystem.Instance.CreateWindow("GUI/GamerGame/SuccessView");
        }
        else
        {
            UISystem.Instance.CreateWindow("GUI/GamerGame/FailView");
        }
    }

    public void DoUpdate(float deltaTime)
    {
        foreach (var interactiveObj in dicInteractiveObjs.Values)
            interactiveObj.DoUpdate(deltaTime);
        if (IsBattleBegin)
        {
            if (GameClient.Instance.Mode == GameClient.GameMode.GodView)
                MainCommander.DoUpdate(deltaTime);
        }
    }

    public void DoFixedUpdate(float fixedDeltaTime)
    {
        foreach (var interactiveObj in dicInteractiveObjs.Values)
            interactiveObj.DoFixedUpdate(fixedDeltaTime);
    }

    public void RegisterInteractiveObj(InteractiveObj newObj)
    {
        dicInteractiveObjs[newObj.ID] = newObj;
    }

    public void UnregisterInteractiveObj(InteractiveObj delObj)
    {
        dicInteractiveObjs.Remove(delObj.ID);
    }
}
