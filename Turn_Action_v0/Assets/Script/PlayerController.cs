using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    Entity player, hide;
    public static PlayerController controller;
    bool charge, transforming;
    float chargeTime, attackDelay, moveDelay, transformTime;
    int hp, maxhp;

    [SerializeField]
    TextMeshProUGUI hpText;
    [SerializeField]
    BarImplement hpBar;

    private void Awake()
    {
        if (controller != null)
        {
            Destroy(gameObject);
            return;
        }

        controller = this;
        charge = false;
        transforming = false;
        chargeTime = 0f;
        attackDelay = 0f;
        moveDelay = 0f;
        hp = 100;
        maxhp = 100;
        transformTime = 0f;

        hpBar.SetBar((float)hp / (float)maxhp);
    }

    // Start is called before the first frame update
    void Start()
    {
        hpText.text = hp + "";
    }

    // Update is called once per frame
    void Update()
    {
        // not initiated
        if (player == null)
            return;

        // check transform 
        if(transforming)
        {
            transformTime -= Time.deltaTime;
            if (transformTime < 0)
            {
                transformTime = 0;
                transforming = false;
                player.EntityDead();
                hide.position = player.position;
                player.gameObject.SetActive(false);
                Destroy(player.gameObject);
                player = hide;
                hide.gameObject.SetActive(true);
                hide.CommitPosition();
            }
        }

        // read input
        bool up = false;
        bool down = false;
        bool left = false;
        bool right = false;
        if (moveDelay <= 0)
        {
            up = Input.GetKeyDown(KeyCode.W);
            down = Input.GetKeyDown(KeyCode.S);
            left = Input.GetKeyDown(KeyCode.A);
            right = Input.GetKeyDown(KeyCode.D);
        }
        if (Input.GetKeyDown(KeyCode.Mouse0) && attackDelay <= 0 && (hide == null || hide == player))
        {
            attackDelay = 0.2f;
            charge = true;
        }

        // calculate input
        float chargeAttack = -1f;
        if (player.GetType() != typeof(Player_Hero))
            charge = false;
        if (charge) chargeTime += Time.deltaTime;
        if(chargeTime > 1)
            chargeTime = 1;
        if ((Input.GetKeyUp(KeyCode.Mouse0) && charge) || chargeTime == 1)
        {
            charge = false;
            chargeAttack = chargeTime;
            chargeTime = 0f;
        }
        attackDelay -= Time.deltaTime;
        moveDelay -= Time.deltaTime;

        // commit to view
        bool move = false;
        if (up && player.position.y < 4)
        {
            move = CheckEntityExist((int)player.position.x, (int)(player.position.y + 1));
            if (move)
            {
                player.position = player.position + new Vector3(0, 1, 0);
                player.CommitPosition();
                moveDelay = 0.1f;
            }
        }
        if (down && player.position.y > 0)
        {
            move = CheckEntityExist((int)player.position.x, (int)(player.position.y - 1));
            if (move)
            {
                player.position = player.position + new Vector3(0, -1, 0);
                player.CommitPosition();
                moveDelay = 0.1f;
            }
        }
        if (left && player.position.x > 0)
        {
            move = CheckEntityExist((int)(player.position.x - 1), (int)player.position.y);
            if (move)
            {
                player.position = player.position + new Vector3(-1, 0, 0);
                player.CommitPosition();
                moveDelay = 0.1f;
            }
        }
        if (right && player.position.x < 4)
        {
            move = CheckEntityExist((int)(player.position.x + 1), (int)player.position.y);
            if (move)
            {
                player.position = player.position + new Vector3(1, 0, 0);
                player.CommitPosition();
                moveDelay = 0.1f;
            }
        }
        if (move)
        {
            if (player.GetType() == typeof(Player_Hero) && !charge)
                player.animator.CrossFade("Move", 0, 0);
            else if (player.GetType() == typeof(Player_Frog))
                player.animator.CrossFade("Move", 0, 0);
        }

        if (charge) player.animator.CrossFade("Charge", 0, 0);
        if (chargeAttack > 0)
        {
            if (player.GetType() == typeof(Player_Hero))
            {
                Player_Hero p = (Player_Hero)player;
                p.Attack(chargeAttack);
            }
        }
    }

    public void Init(GameObject playerobj)
    {
        player = playerobj.GetComponent<Entity>();
    }

    void OnApplicationQuit()
    {
        SqlAccess access = SqlAccess.GetAccess(SqlAccess.battleDB);
        access.Open();
        access.SqlExecute("DELETE FROM MAP;");
        access.ShutDown();
    }

    bool CheckEntityExist(int x, int y)
    {
        bool exist = false;
        // check moveable position
        SqlAccess access = SqlAccess.GetAccess(SqlAccess.battleDB);
        access.Open();
        access.SqlRead("SELECT entity_no FROM MAP WHERE x = " + x + " AND y = " + y);
        if (access.read && access.dataReader.Read() && access.dataReader.FieldCount > 0)
            exist = false;
        else
            exist = true;
        access.ShutDown();

        return exist;
    }

    public static void PlayerDamaged(string damage)
    {
        if (controller == null)
            return;

        controller.hp -= int.Parse(damage);
        if (controller.hp <= 0)
        {
            Entity.GameOver();
            SceneManager.LoadScene("GameOver", LoadSceneMode.Single);
        }
        controller.hpText.text = controller.hp + "";
        controller.hpBar.SetBar((float)controller.hp / (float)controller.maxhp);

        if (controller.player.GetType() == typeof(Player_Hero))
            controller.player.animator.CrossFade("Damage", 0, 0);
    }

    public static void PlayerTransform(GameObject obj, float time)
    {
        if(!controller.transforming)
        {
            controller.transforming = true;
            controller.transformTime = time;
            controller.player.EntityDead();
            controller.hide = controller.player;

            GameObject transform = Instantiate(obj);
            controller.player = transform.GetComponent<Entity>();
            controller.player.position = controller.hide.position;

            controller.player.gameObject.SetActive(true);
            controller.hide.gameObject.SetActive(false);
        }
    }
}
