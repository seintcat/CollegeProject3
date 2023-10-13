using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Totem_Wall : Entity
{
    int hp;
    [SerializeField]
    TextMeshProUGUI hpView;
    float wait;
    static readonly float moveWait = 1f, moveTime = 0.8f;
    bool moving, reach, dead;

    [SerializeField]
    GameObject inside;

    // Start is called before the first frame update
    void Start()
    {
        CommitPosition();
        hp = 10;
        moving = false;
        dead = false;
        
        wait = 0;

        hpView.text = hp + "";
    }

    // Update is called once per frame
    void Update()
    {
        wait += Time.deltaTime;
        if (moving && wait > moveTime)
        {
            moving = false;
            wait = 0;
            reach = (int)position.x == 0;
        }
        else if(!moving && wait > moveWait)
        {
            if (reach)
            {
                hp = 0;
                EntityDead();
                return;
            }

            animator.CrossFade("Move", 0, 0);

            SqlAccess access = SqlAccess.GetAccess(SqlAccess.battleDB);
            access.Open();
            access.SqlRead("SELECT entity_name FROM MAP WHERE x = " + ((int)position.x - 1) + " AND y = " + (int)position.y + " ;");
            if (!access.read || !access.dataReader.Read())
            {
                position = new Vector3(((int)position.x - 1), (int)position.y, (int)position.z);
                CommitPosition();
            }
            else if(access.dataReader.GetString(0) != gameObject.name)
            {
                hp = 0;
                EntityDead();
                return;
            }
            access.ShutDown();

            moving = true;
            wait = 0;
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
                    hp -= int.Parse(command[2]);
                    if (hp <= 0)
                    {
                        hp = 0;
                        EntityDead();
                        return;
                    }
                    hpView.text = hp + "";
                }
                break;
        }
    }

    public override void EntityDead()
    {
        if (dead)
            return;
        dead = true;
        DeletePosition();

        hpView.gameObject.SetActive(false);
        animator.CrossFade("Dead", 0, 0);

        if(inside != null)
        {
            GameObject obj = Instantiate(inside);
            obj.transform.position = position;
        }

        moving = false;
        wait = -1;
        Destroy(gameObject, 0.5f);
    }

    private void OnDisable()
    {
        DeletePosition();
    }
    private void OnEnable()
    {
        CommitPosition();
    }
}
