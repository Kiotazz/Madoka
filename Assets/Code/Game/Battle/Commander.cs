using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Commander
{
    public static implicit operator bool(Commander commander) { return commander != null; }

    public const int MaxTeamNum = 5;

    public static BattleTeamEvent ControlTeamChange = new BattleTeamEvent();

    public int Camp { get; protected set; } = 1;
    public InputManager Input { get; protected set; }
    public BattleTeam ControlTeam { get; protected set; }
    public int TeamsCount { get { return listTeams.Count; } }

    List<BattleTeam> listTeams = new List<BattleTeam>();
    List<SkillBase> listCommanderSkills = new List<SkillBase>();

    public void Init()
    {
        Input = new InputManager();
        Input.Init(this);
        for (int i = 0, length = MaxTeamNum; i < length; ++i)
        {
            BattleTeam team = new BattleTeam();
            team.Init(this);
            listTeams.Add(team);
        }
        Input.OnClickGround.AddListener(OnClickGround);
        Input.OnClickInteractiveObj.AddListener(OnClickInteractiveObj);

        Event_Battle.OnBattleBegin.AddListener(Input.Reset);
        Event_Battle.OnBattleOver.AddListener((isWin) => { });
    }

    public void DoUpdate(float deltaTime)
    {
        Input.DoUpdate(deltaTime);
        for (int i = 0, length = listCommanderSkills.Count; i < length; ++i)
            listCommanderSkills[i].DoUpdate(deltaTime);
    }

    public void Reset()
    {
        for (int i = 0, length = MaxTeamNum; i < length; ++i)
            listTeams[i].Clear();
        listCommanderSkills.Clear();
    }

    public BattleTeam GetTeam(int index)
    {
        return index > -1 && index < TeamsCount ? listTeams[index] : null;
    }

    public void SetControlTeam(int index)
    {
        BattleTeam team = GetTeam(index);
        if (team)
        {
            ControlTeam = team;
            ControlTeamChange.Invoke(team);
        }
    }

    public void SetCamp(int camp)
    {
        Camp = camp;
    }

    void OnClickGround(Vector3 pos)
    {
        if (ControlTeam) ControlTeam.MoveTo(pos);
    }

    void OnClickInteractiveObj(InteractiveObj obj)
    {
        if (ControlTeam && obj.IsEnemy(Camp)) ControlTeam.AttackTarget(obj);
    }
}
