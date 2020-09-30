using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNavAgent : MonoBehaviour
{
    public GameObject prefab;

    private void Start()
    {
        BattleTeam team = BattleManager.Instance.MainCommander.GetTeam(0);
        for (int i = 0; i < 5; ++i)
        {
            GameObject objChara = Instantiate(prefab, transform.position, Quaternion.identity, transform);
            objChara.GetComponentInChildren<Renderer>().material.color = Color.red;
            team.AddMember(objChara.GetComponent<Character>());
        }

        team = BattleManager.Instance.MainCommander.GetTeam(1);
        for (int i = 0; i < 5; ++i)
        {
            GameObject objChara = Instantiate(prefab, transform.position + Vector3.one, Quaternion.identity, transform);
            objChara.GetComponentInChildren<Renderer>().material.color = Color.green;
            team.AddMember(objChara.GetComponent<Character>());
        }

        team = BattleManager.Instance.MainCommander.GetTeam(2);
        for (int i = 0; i < 5; ++i)
        {
            GameObject objChara = Instantiate(prefab, transform.position + Vector3.one * 2, Quaternion.identity, transform);
            objChara.GetComponentInChildren<Renderer>().material.color = Color.yellow;
            team.AddMember(objChara.GetComponent<Character>());
        }

        BattleManager.Instance.BattleBegin();
        Vector3 pos = transform.position;
        pos.y = 10;
        BattleManager.Instance.MainCommander.Input.MainCamera.transform.position = pos;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            BattleManager.Instance.MainCommander.SetControlTeam(0);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            BattleManager.Instance.MainCommander.SetControlTeam(1);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            BattleManager.Instance.MainCommander.SetControlTeam(2);
    }
}
