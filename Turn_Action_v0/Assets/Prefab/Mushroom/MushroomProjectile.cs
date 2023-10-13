using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomProjectile : MonoBehaviour
{
    Vector3 target, position;
    float moveTime;
    static readonly float speed = 0.7f, waitTime = 0.5f, deleteDelay = 0.4f;
    [SerializeField]
    Animator animator;
    [SerializeField]
    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        moveTime = -1;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(spriteRenderer.sortingOrder != ((4 - (int)transform.position.y) * 2) + 1)
            spriteRenderer.sortingOrder = ((4 - (int)transform.position.y) * 2) + 1;

        if (position == target)
        {
            moveTime += Time.deltaTime;

            if(moveTime > waitTime)
            {
                SqlAccess access = SqlAccess.GetAccess(SqlAccess.battleDB);
                access.Open();
                access.SqlRead("SELECT entity_no FROM MAP WHERE team = 1 AND x = " + (int)transform.position.x + " AND y = " + (int)transform.position.y + " ;");
                List<string> command = new List<string>();
                command.Add("Attack");
                if (access.read && access.dataReader.Read() && access.dataReader.FieldCount > 0)
                    command.Add(access.dataReader.GetInt32(0) + "");
                else
                    command.Add(-1 + "");
                command.Add(3 + "");
                command.Add(false + "");
                Entity.EntityBroadcast(command);
                access.ShutDown();
                animator.CrossFade("ProjectileEnd", 0, 0);
                Destroy(gameObject, deleteDelay);
                enabled = false;
            }
            return;
        }
        if (moveTime > 1)
        {
            moveTime = 0;
            transform.position = Vector3.Lerp(position, target, 1);
            position = target;
            return;
        }
        else if (moveTime < 0)
            return;

        moveTime += (Time.deltaTime * speed);
        transform.position = Vector3.Lerp(position, target, moveTime);
    }

    public void Attack(Vector2 _target)
    {
        target = _target;
        position = transform.position;
        moveTime = 0;
    }
}
