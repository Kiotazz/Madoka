using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleTeam
{
    public static implicit operator bool(BattleTeam team) { return team != null; }

    public CharacterEvent OnLeaderChange { get; private set; } = new CharacterEvent();
    public CharacterEvent OnAddMember { get; private set; } = new CharacterEvent();
    public CharacterEvent OnRemoveMember { get; private set; } = new CharacterEvent();
    public InteractiveObjEvent OnAttackTarget { get; private set; } = new InteractiveObjEvent();

    const int MaxTeamMember = 5;

    public Commander MyCommander { get; protected set; }
    public Vector3 Position { get { return MembersCount > 0 ? listMembers[0].transform.position : Vector3.zero; } }
    public Character Leader { get { return MembersCount > 0 ? listMembers[0] : null; } }
    public int MembersCount { get { return listMembers.Count; } }
    public bool IsEmpty { get { return listMembers.Count < 1; } }

    List<Character> listMembers = new List<Character>();

    public void Init(Commander commander)
    {
        MyCommander = commander;
    }

    public bool AddMember(Character c)
    {
        if (listMembers.Count < MaxTeamMember)
        {
            listMembers.Add(c);
            c.SetTeam(this);
            OnAddMember.Invoke(c);
            return true;
        }
        return false;
    }

    public bool RemoveMember(Character c)
    {
        if (listMembers.Remove(c))
        {
            if (c.myTeam == this)
                c.SetTeam(null);
            OnRemoveMember.Invoke(c);
            return true;
        }
        return false;
    }

    public void Clear()
    {
        for (int i = 0, length = MembersCount; i < length; ++i)
        {
            Character c = listMembers[i];
            if (c.myTeam == this)
                c.SetTeam(null);
            OnRemoveMember.Invoke(c);
        }
        listMembers.Clear();
    }

    public Character GetMember(int index)
    {
        return index > -1 && index < MembersCount ? listMembers[index] : null;
    }

    public bool SetLeader(Character c)
    {
        if (!listMembers.Contains(c)) return false;
        listMembers.Remove(c);
        listMembers.Insert(0, c);
        OnLeaderChange.Invoke(c);
        return true;
    }

    public void MoveTo(Vector3 pos)
    {
        for (int i = 0, length = MembersCount; i < length; ++i)
        {
            Character c = listMembers[i];
            if (!c.IsAlive) continue;
            Vector3 offset = Vector3.zero;
            switch (i)
            {
                case 1:
                    offset = new Vector3(-1, 0, -1);
                    break;
                case 2:
                    offset = new Vector3(1, 0, -1);
                    break;
                case 3:
                    offset = new Vector3(-1, 0, 1);
                    break;
                case 4:
                    offset = new Vector3(1, 0, 1);
                    break;
            }
            c.MoveTo(pos + offset);
        }
    }

    public void AttackTarget(InteractiveObj tar)
    {
        float lastDistance = 0;
        float currentOffset = 0;
        for (int i = 0, length = MembersCount; i < length; ++i)
        {
            Character c = listMembers[i];
            if (!c.IsAlive) continue;
            float minRange = c.AttackMinRange;
            if (lastDistance == minRange)
                ++currentOffset;
            lastDistance = minRange;
            c.AttackTarget(tar, currentOffset * 3);
        }
        OnAttackTarget.Invoke(tar);
    }
}
