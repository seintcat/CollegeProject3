using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class Player_Hero : Entity
{
    [SerializeField]
    GameObject fxHit;

    // Start is called before the first frame update
    void Start()
    {
        CommitPosition();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void EntityBroadcastReceive(List<string> command)
    {
        switch (command[0])
        {
            case "Attack":
                if (command[3] == playerTeam + "")
                    return;

                if (command[1] == no.ToString())
                    PlayerController.PlayerDamaged(command[2]);
                break;
        }
    }

    public void Attack(float value)
    {
        animator.CrossFade("Attack1", 0, 0);

        SqlAccess access = SqlAccess.GetAccess(SqlAccess.battleDB);
        access.Open();
        access.SqlRead("SELECT entity_no, x, y FROM MAP WHERE entity_no <> " + no + " AND " + "y = " + (int)position.y + " AND " + "x >= " + (int)position.x + " AND " + "team = 0" + " ORDER BY x ASC" + " ;");
        if (access.read && access.dataReader.Read() && access.dataReader.FieldCount > 0)
        {
            Instantiate(fxHit).transform.position = new Vector3(access.dataReader.GetInt32(1), access.dataReader.GetInt32(2), 0);

            List<string> command = new List<string>();
            command.Add("Attack");
            command.Add(access.dataReader.GetInt32(0) + "");
            command.Add(Math.Ceiling(value * 5) + "");
            command.Add(playerTeam + "");
            EntityBroadcast(command);
        }

        access.ShutDown();
    }

    public override void EntityDead()
    {
        DeletePosition();
    }
}

// pierce attack (edit need for x check)
//public void Attack(float value)
//{
//    animator.CrossFade("Attack1", 0, 0);

//    SqlAccess access = SqlAccess.GetAccess(SqlAccess.battleDB);
//    access.Open();
//    access.SqlRead("SELECT entity_no, y FROM MAP WHERE entity_no <> " + no + " AND " + "y = " + (int)position.y + " ;");
//    if (access.read && access.dataReader.FieldCount > 0)
//        while (access.read && access.dataReader.Read())
//        {
//            List<string> command = new List<string>();
//            command.Add("Attack");
//            command.Add(access.dataReader.GetInt32(0) + "");
//            command.Add(Math.Ceiling(value * 5) + "");
//            EntityBroadcast(command);
//        }

//    access.ShutDown();
//}