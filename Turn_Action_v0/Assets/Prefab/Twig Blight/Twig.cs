using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Twig : Entity
{
    float wait;
    static readonly float moveWaitTime = 0.9f, melee1AttackWaitTime = 0.3f, melee1Delay =0.25f;
    Vector2Int target;
    bool attacking, dead;

    // Start is called before the first frame update
    void Start()
    {
        CommitPosition();
        wait = moveWaitTime;
        attacking = false;
        dead = false;
    }

    // Update is called once per frame
    void Update()
    {
        wait -= Time.deltaTime;
        if(wait < 0)
        {
            SqlAccess access = SqlAccess.GetAccess(SqlAccess.battleDB);
            access.Open();
            access.SqlRead("SELECT entity_no, x, y FROM MAP WHERE team = 1" + " ORDER BY x ASC" + " ;");
            if (access.read && access.dataReader.Read() && access.dataReader.FieldCount > 0)
                target = new Vector2Int(access.dataReader.GetInt32(1) + 1, access.dataReader.GetInt32(2));
            else
            {
                animator.CrossFade("LookUp", 0, 0);
                wait = moveWaitTime;
            }
            access.ShutDown();

            // melee1 attack
            if (attacking)
            {
                access.Open();
                access.SqlRead("SELECT entity_no FROM MAP WHERE team = 1 AND x = " + ((int)position.x - 1) + " AND y = " + (int)position.y + " ;");
                List<string> command = new List<string>();
                command.Add("Attack");
                if (access.read && access.dataReader.Read() && access.dataReader.FieldCount > 0)
                    command.Add(access.dataReader.GetInt32(0) + "");
                else
                    command.Add(-1 + "");
                command.Add(2 + "");
                command.Add(playerTeam + "");
                EntityBroadcast(command);
                access.ShutDown();
                wait = melee1Delay;
                attacking = false;
            }
            // melee1 start
            else if(target != null && target.x == (int)position.x && target.y == (int)position.y)
            {
                animator.CrossFade("Melee1", 0, 0);
                wait = melee1AttackWaitTime;
                attacking = true;
            }
            // move to attack
            else if(target != null)
            {
                // list moveable position
                SortedDictionary<float, Vector2Int> dict = new SortedDictionary<float, Vector2Int>();
                List<Vector2Int> list = new List<Vector2Int>();
                list.Add(new Vector2Int((int)position.x, (int)position.y + 1));
                list.Add(new Vector2Int((int)position.x + 1, (int)position.y + 1));
                list.Add(new Vector2Int((int)position.x + 1, (int)position.y));
                list.Add(new Vector2Int((int)position.x + 1, (int)position.y - 1));
                list.Add(new Vector2Int((int)position.x, (int)position.y - 1));
                list.Add(new Vector2Int((int)position.x - 1, (int)position.y - 1));
                list.Add(new Vector2Int((int)position.x - 1, (int)position.y));
                list.Add(new Vector2Int((int)position.x - 1, (int)position.y + 1));
                foreach(Vector2Int dir in list)
                {
                    float val = Vector2Int.Distance(dir, target);
                    if (!dict.ContainsKey(val))
                        dict.Add(val, dir);
                }

                // check moveable position
                foreach (float k in dict.Keys)
                {
                    if (dict[k].x < 0 || dict[k].x > 9 || dict[k].y < 0 || dict[k].y > 4)
                    {
                        continue;
                    }
                    access.Open();
                    access.SqlRead("SELECT entity_no FROM MAP WHERE x = " + dict[k].x + " AND y = " + dict[k].y);
                    if (access.read && access.dataReader.Read() && access.dataReader.FieldCount > 0)
                    {
                        access.ShutDown();
                        continue;
                    }
                    else
                    {
                        animator.CrossFade("Move", 0, 0);
                        position = new Vector3(dict[k].x, dict[k].y, 0);
                        CommitPosition();
                        access.ShutDown(); 
                        wait = moveWaitTime;
                        return;
                    }
                }

                animator.CrossFade("Jump", 0, 0);
                wait = moveWaitTime;
            }
        }
    }

    public override void EntityBroadcastReceive(List<string> command)
    {
        switch (command[0])
        {
            case "Attack":
                if (command[3] == playerTeam + "")
                    return;

                if (command[1] == no.ToString())
                    EntityDead();
                break;
        }
    }

    public override void EntityDead()
    {
        if (dead)
            return;
        dead = true;

        animator.CrossFade("Dead", 0, 0);
        DeletePosition();
        wait = 6;
        Destroy(gameObject, 0.45f);
    }
}
