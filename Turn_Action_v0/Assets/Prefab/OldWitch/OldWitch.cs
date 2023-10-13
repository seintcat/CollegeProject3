using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OldWitch : Entity
{
    int hp, aiMove;
    [SerializeField]
    TextMeshProUGUI hpView;
    [SerializeField]
    GameObject frog;

    float wait;
    static readonly float cooldown = 0.7f, moveWaitTime = 1, melee1Speed = 6, melee1RecoverSpeed = 12, magicWaitTime = 1, magicDelay = 0.5f, transformTime = 3;
    //Vector3 target, posNow;
    Vector2Int posNow;

    bool dead;

    // Start is called before the first frame update
    void Start()
    {
        CommitPosition();
        hp = 5;
        hpView.text = hp + "";

        wait = 0f;
        aiMove = 0;
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
            // move(avoid) wait end
            case 1:
                if (wait > moveWaitTime)
                    chooseAI = true;
                break;
            // melee1 move check
            case 2:
                position = new Vector3(position.x - (Time.deltaTime * melee1Speed), position.y, position.z);
                if (posNow.x != (int)position.x)
                {
                    SqlAccess access = SqlAccess.GetAccess(SqlAccess.battleDB);
                    access.Open();
                    access.SqlRead("SELECT entity_no FROM MAP WHERE team = 1 AND x = " + ((int)transform.position.x - 1) + " AND y = " + (int)transform.position.y + " ;");
                    List<string> command = new List<string>();
                    command.Add("Attack");
                    if (access.read && access.dataReader.Read() && access.dataReader.FieldCount > 0)
                        command.Add(access.dataReader.GetInt32(0) + "");
                    else
                        command.Add(-1 + "");
                    command.Add(3 + "");
                    command.Add(playerTeam + "");
                    EntityBroadcast(command);
                    access.ShutDown();
                    CommitPosition();
                    posNow.x = (int)position.x;
                }

                // melee1 end
                if (posNow.x < 0)
                    chooseAI = true;
                break;
            // melee1 recover 1 check
            case 3:
                position = new Vector3(position.x, position.y + (Time.deltaTime * melee1RecoverSpeed), position.z);

                // melee1 recover 1 end
                if ((int)position.y > 8)
                    chooseAI = true;
                break;
            // melee1 recover 2 check
            case 4:
                position = new Vector3(position.x + (Time.deltaTime * melee1RecoverSpeed), position.y, position.z);

                // melee1 recover 2 end
                if ((int)position.x > 10)
                    chooseAI = true;
                break;
            // melee1 recover 3 check
            case 5:
                position = new Vector3(position.x, position.y - (Time.deltaTime * melee1RecoverSpeed), position.z);

                // melee1 recover 3 end
                if (position.y < posNow.y)
                    chooseAI = true;
                break;
            // melee1 recover 4 check
            case 6:
                position = new Vector3(position.x - (Time.deltaTime * melee1Speed), position.y, position.z);

                // melee1 recover 4 end
                if (position.x < 9)
                    chooseAI = true;
                break;
            // magic  AttackWait end
            case 7:
                if (wait > magicWaitTime)
                    chooseAI = true;
                break;
            // magic delay end
            case 8:
                if (wait > magicDelay)
                    chooseAI = true;
                break;
        }

        if (chooseAI)
        {
            // melee1 recover 1
            if (aiMove == 2)
            {
                SqlAccess access = SqlAccess.GetAccess(SqlAccess.battleDB);
                access.Open();
                access.SqlRead("SELECT entity_no FROM MAP WHERE entity_no = " + no + ";");
                if (access.read && access.dataReader.FieldCount > 0)
                    access.SqlExecute("DELETE FROM MAP WHERE entity_no = " + no + ";");

                access.ShutDown();
                aiMove = 3;
            }
            // melee1 recover 2
            else if (aiMove == 3)
                aiMove = 4;
            // melee1 recover 3
            else if (aiMove == 4)
                aiMove = 5;
            // melee1 recover 4
            else if (aiMove == 5)
            {
                position = new Vector3(position.x, posNow.y, position.z);
                CommitPosition();
                aiMove = 6;
            }
            // melee1 recover 5
            else if (aiMove == 6)
            {
                position = new Vector3(9, position.y, position.z);
                CommitPosition();
                animator.CrossFade("Idle", 0, 0);
                aiMove = 0;
                wait = 0;
            }
            // magic
            else if (aiMove == 7)
            {
                PlayerController.PlayerTransform(frog, transformTime);
                aiMove = 8;
                wait = 0;
            }
            // choose next behavior
            else
            {
                // 0 = idle, 1 = move(avoid), 2 ~ 4 = melee1, 5 = magic
                int nextai = Random.Range(0, 6);

                // idle
                if (nextai == 0)
                {
                    aiMove = 0;
                    wait = 0;
                }
                // move when avoid
                else if (nextai == 1)
                {
                    animator.CrossFade("Move", 0, 0);
                    aiMove = 1;
                    wait = 0;
                }
                // melee1 start
                else if (nextai > 1 && nextai < 5)
                {
                    animator.CrossFade("Melee1", 0, 0);
                    posNow = new Vector2Int((int)position.x, Random.Range(0, 5));
                    aiMove = 2;
                }
                // magic
                else if (nextai == 5)
                {
                    animator.CrossFade("Magic", 0, 0);
                    aiMove = 7;
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

                if (command[1] == no.ToString())
                {
                    // avoid attack
                    if (aiMove == 1)
                    {
                        // list moveable position
                        List<Vector2> list = new List<Vector2>();
                        if ((int)(transform.position.y + 1) < 5)
                            list.Add(new Vector2((int)transform.position.x, (int)(transform.position.y + 1)));
                        if ((int)(transform.position.x + 1) < 10 && (int)(transform.position.y + 1) < 5)
                            list.Add(new Vector2((int)(transform.position.x + 1), (int)(transform.position.y + 1)));
                        if ((int)(transform.position.x + 1) < 10)
                            list.Add(new Vector2((int)(transform.position.x + 1), (int)transform.position.y));
                        if ((int)(transform.position.x + 1) < 10 && (int)(transform.position.y - 1) > -1)
                            list.Add(new Vector2((int)(transform.position.x + 1), (int)(transform.position.y - 1)));
                        if ((int)(transform.position.y - 1) > -1)
                            list.Add(new Vector2((int)transform.position.x, (int)(transform.position.y - 1)));
                        if ((int)(transform.position.x - 1) > 4 && (int)(transform.position.y - 1) > -1)
                            list.Add(new Vector2((int)(transform.position.x - 1), (int)(transform.position.y - 1)));
                        if ((int)(transform.position.x - 1) > 4)
                            list.Add(new Vector2((int)(transform.position.x - 1), (int)transform.position.y));
                        if ((int)(transform.position.x - 1) > 4 && (int)(transform.position.y + 1) < 5)
                            list.Add(new Vector2((int)(transform.position.x - 1), (int)(transform.position.y + 1)));

                        // find entity can't move
                        SqlAccess access = SqlAccess.GetAccess(SqlAccess.battleDB);
                        access.Open();
                        access.SqlRead("SELECT x, y FROM MAP WHERE" +
                            " x <= " + (int)(transform.position.x + 1) + " AND x >= " + (int)(transform.position.x - 1) +
                            " AND y <= " + (int)(transform.position.y + 1) + " AND y >= " + (int)(transform.position.y - 1) + " ;");
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
                        if (list.Count > 0)
                        {
                            int index = Random.Range(0, list.Count);
                            transform.position = new Vector3(list[index].x, list[index].y, 0);
                        }

                        CommitPosition();
                        animator.Rebind();
                        animator.CrossFade("Move", 0, 0);
                        aiMove = 0;
                        wait = 0;
                        return;
                    }
                    
                    hp -= int.Parse(command[2]);
                    if (hp <= 0)
                    {
                        hp = 0;
                        EntityDead();
                        return;
                    }
                    hpView.text = hp + "";

                    // no nockback
                    if (aiMove != 2)
                        animator.CrossFade("Damage", 0, 0);

                    // magic cancel
                    if(aiMove == 7)
                    {
                        aiMove = 0;
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
