using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Mushroom : Entity
{
    int hp, aiMove;
    [SerializeField]
    TextMeshProUGUI hpView;

    static readonly float melee1MoveWaitTime = 0.3f, melee1AttackWaitTime = 0.75f, melee1Delay =0.25f, cooldown = 1f, melee2AttackWaitTime = 0.7f, melee2Delay = 0.25f, range1AttackWaitTime = 0.75f, range1Delay = 0.35f;
    float wait;
    Vector3Int preMeleePos;

    [SerializeField]
    GameObject projectile;
    List<Vector3> projectilePos;

    bool dead;

    // Start is called before the first frame update
    void Start()
    {
        CommitPosition();
        hp = 10;
        hpView.text = hp + "";

        wait = 0f;
        aiMove = 0;
        projectilePos = new List<Vector3>();
        projectilePos.Add(new Vector3(0, 1.4f, 0));
        projectilePos.Add(new Vector3(1.41f, 1.15f, 0));
        projectilePos.Add(new Vector3(-0.2f, 0.7f, 0));

        dead = false;
    }

    // Update is called once per frame
    void Update()
    {
        wait += Time.deltaTime;
        bool chooseAI = false;

        switch (aiMove)
        {
            // wait end
            case 0:
                if (wait > cooldown)
                    chooseAI = true;
                break;
            // move delay end
            case 1:
                if (wait > cooldown)
                    chooseAI = true;
                break;
            //  melee1 MoveWait end
            case 2:
                if (wait > melee1MoveWaitTime)
                    chooseAI = true;
                break;
            //  melee1 AttackWait end
            case 3:
                if (wait > melee1AttackWaitTime)
                    chooseAI = true;
                break;
            // melee1 delay end
            case 4:
                if (wait > melee1Delay)
                    chooseAI = true;
                break;
            //  melee2 MoveWait end
            case 5:
                if (wait > melee1MoveWaitTime)
                    chooseAI = true;
                break;
            //  melee2 AttackWait end
            case 6:
                if (wait > melee2AttackWaitTime)
                    chooseAI = true;
                break;
            // melee2 delay end
            case 7:
                if (wait > melee2Delay)
                    chooseAI = true;
                break;
            //  range1 AttackWait end
            case 8:
                if (wait > range1AttackWaitTime)
                    chooseAI = true;
                break;
            // range1 delay end
            case 9:
                if (wait > range1Delay)
                    chooseAI = true;
                break;
        }

        if (chooseAI)
        {
            // start melee1
            if (aiMove == 2)
            {
                animator.CrossFade("Melee1", 0, 0);
                aiMove = 3;
                wait = 0;
            }
            // damage melee1
            else if (aiMove == 3)
            {
                SqlAccess access = SqlAccess.GetAccess(SqlAccess.battleDB);
                access.Open();
                access.SqlRead("SELECT entity_no FROM MAP WHERE team = 1 AND x = " + ((int)position.x - 1) + " AND y = " + (int)position.y + " ;");
                List<string> command = new List<string>();
                command.Add("Attack");
                if (access.read && access.dataReader.Read() && access.dataReader.FieldCount > 0)
                    command.Add(access.dataReader.GetInt32(0) + "");
                else
                    command.Add(-1 + "");
                command.Add(5 + "");
                command.Add(playerTeam + "");
                EntityBroadcast(command);
                access.ShutDown();
                aiMove = 4;
                wait = 0;
            }
            // melee1, melee2 recovery 
            else if (aiMove == 4 || aiMove == 7)
            {
                animator.CrossFade("Move", 0, 0);
                position = new Vector3(preMeleePos.x, preMeleePos.y, preMeleePos.z);
                CommitPosition();
                preMeleePos = new Vector3Int(-1, -1, -1);
                aiMove = 0;
                wait = 0;
            }
            // start melee2
            else if (aiMove == 5)
            {
                animator.CrossFade("Melee2", 0, 0);
                aiMove = 6;
                wait = 0;
            }
            // damage melee2
            else if (aiMove == 6)
            {
                SqlAccess access = SqlAccess.GetAccess(SqlAccess.battleDB);
                access.Open();
                access.SqlRead("SELECT entity_no FROM MAP WHERE team = 1 AND x = " + ((int)position.x - 1) + " AND y = " + (int)position.y + " ;");
                List<string> command = new List<string>();
                command.Add("Attack");
                if (access.read && access.dataReader.Read() && access.dataReader.FieldCount > 0)
                {
                    command.Add(access.dataReader.GetInt32(0) + "");
                    hp += 5;
                    hpView.text = hp + "";
                }
                else
                    command.Add(-1 + "");
                command.Add(5 + "");
                command.Add(playerTeam + "");
                EntityBroadcast(command);
                access.ShutDown();
                aiMove = 7;
                wait = 0;
            }
            // damage range1
            else if (aiMove == 8)
            {
                List<Vector2Int> list = new List<Vector2Int>();
                list.Add(new Vector2Int(Random.Range(0, 5), Random.Range(0, 5)) );
                while (list.Count < 3)
                {
                    Vector2Int pos = new Vector2Int(Random.Range(0, 5), Random.Range(0, 5));
                    bool equal = true;
                    foreach (Vector2Int p in list)
                        if (p.x == pos.x || p.y == pos.y)
                            equal = false;

                    if (equal)
                        list.Add(pos);
                }

                for(int i = 0; i < 3; i++)
                {
                    GameObject _projectile = Instantiate(projectile);
                    _projectile.transform.position = position + projectilePos[i];
                    _projectile.GetComponent<MushroomProjectile>().Attack(list[i]);
                }

                aiMove = 9;
                wait = 0;
            }
            // range1 recovery 
            else if (aiMove == 9)
            {
                aiMove = 0;
                wait = 0;
            }
            // choose next behavior
            else
            {
                // 0 = idle, 1 = move, 2 = melee1(1/10 melee2), 3 = range1
                int nextai = Random.Range(0, 4);

                // idle
                if (nextai == 0)
                {
                    aiMove = 0;
                    wait = 0;
                }
                // move
                else if (nextai == 1)
                {
                    // list moveable position
                    List<Vector2> list = new List<Vector2>();
                    if((int)(position.y + 1) < 5)
                        list.Add(new Vector2((int)position.x, (int)(position.y + 1)));
                    if ((int)(position.x + 1) < 10 && (int)(position.y + 1) < 5)
                        list.Add(new Vector2((int)(position.x + 1), (int)(position.y + 1)));
                    if((int)(position.x + 1) < 10)
                        list.Add(new Vector2((int)(position.x + 1), (int)position.y));
                    if ((int)(position.x + 1) < 10 && (int)(position.y - 1) > -1)
                        list.Add(new Vector2((int)(position.x + 1), (int)(position.y - 1)));
                    if ((int)(position.y - 1) > -1)
                        list.Add(new Vector2((int)position.x, (int)(position.y - 1)));
                    if ((int)(position.x - 1) > 4 && (int)(position.y - 1) > -1)
                        list.Add(new Vector2((int)(position.x - 1), (int)(position.y - 1)));
                    if ((int)(position.x - 1) > 4)
                        list.Add(new Vector2((int)(position.x - 1), (int)position.y));
                    if ((int)(position.x - 1) > 4 && (int)(position.y + 1) < 5)
                        list.Add(new Vector2((int)(position.x - 1), (int)(position.y + 1)));

                    // find entity can't move
                    SqlAccess access = SqlAccess.GetAccess(SqlAccess.battleDB);
                    access.Open();
                    access.SqlRead("SELECT x, y FROM MAP WHERE" + 
                        " x <= " + (int)(position.x + 1) + " AND x >=" + (int)(position.x - 1) +
                        " AND y <= " + (int)(position.y + 1) + " AND y >=" + (int)(position.y - 1) + " ;");
                    if (access.read && access.dataReader.FieldCount > 0)
                        while (access.dataReader.Read())
                        {
                            Vector2 target = new Vector2(access.dataReader.GetInt32(0), access.dataReader.GetInt32(1));
                            for (int i = 0; i < list.Count; i++)
                                if (list[i] == target)
                                {
                                    list.RemoveAt(i);
                                    break;
                                }
                        }
                    access.ShutDown();

                    // select position
                    if(list.Count > 0)
                    {
                        int index = Random.Range(0, list.Count);
                        transform.position = new Vector3(list[index].x, list[index].y, 0);
                        CommitPosition();
                        animator.CrossFade("Move", 0, 0);
                        aiMove = 1;
                        wait = 0;
                    }
                }
                // melee1(1/10 melee2)
                else if (nextai == 2)
                {
                    preMeleePos = new Vector3Int((int)position.x, (int)position.y, (int)position.z);
                    SqlAccess access = SqlAccess.GetAccess(SqlAccess.battleDB);
                    access.Open();
                    access.SqlRead("SELECT entity_no, x, y FROM MAP WHERE team = 1" + " ORDER BY x DESC" + " ;");
                    if (access.read && access.dataReader.Read() && access.dataReader.FieldCount > 0)
                    {
                        transform.position = new Vector3(access.dataReader.GetInt32(1) + 1, access.dataReader.GetInt32(2), 0);
                        CommitPosition();
                    }
                    access.ShutDown();
                    animator.CrossFade("Move", 0, 0);

                    // melee2 branch
                    if (Random.Range(0, 10) == 0)
                        aiMove = 5;
                    else
                        aiMove = 2;
                    wait = 0;
                }
                // range1 
                else if (nextai == 3)
                {
                    animator.CrossFade("Range1", 0, 0);
                    aiMove = 8;
                    wait = 0;
                }
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

                if(command[1] == no.ToString())
                {
                    hp -= int.Parse(command[2]);
                    if(hp <= 0)
                    {
                        hp = 0;
                        EntityDead();
                        return;
                    }
                    hpView.text = hp + "";

                    if(aiMove != 5 && aiMove != 6 && aiMove != 7 && aiMove != 8)
                        animator.CrossFade("Damage", 0, 0);

                    if (aiMove == 2 || aiMove == 3)
                    {
                        aiMove = 4;
                        wait = 0;
                    }
                }
                break;
        }
    }

    public override void EntityDead()
    {
        if (dead)
            return;
        dead = true;

        hpView.gameObject.SetActive(false);
        DeletePosition();
        animator.CrossFade("Dead", 0, 0);
        aiMove = 0;
        wait = -1;
        Destroy(gameObject, 1f);
    }
}