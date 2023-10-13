using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public static int count = 0;
    protected int no;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Canvas canvas;
    [SerializeField]
    protected bool playerTeam;

    public Vector3 position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    public void CommitPosition()
    {
        // sql calculate
        SqlAccess access = SqlAccess.GetAccess(SqlAccess.battleDB);
        access.Open();
        access.SqlRead("SELECT entity_no FROM MAP WHERE entity_no = " + no + ";");
        if (access.read && access.dataReader.FieldCount > 0)
            access.SqlExecute("DELETE FROM MAP WHERE entity_no = " + no + ";");

        access.SqlExecute("INSERT INTO MAP (entity_no, entity_name, x, y, team) VALUES(" + no + ", '" + gameObject.name + "', " + (int)position.x + ", " + (int)position.y + ", " + (playerTeam ? 1 :  0) + "); ");
        access.ShutDown();

        // order layer
        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = (4 - (int)position.y) * 2;
        if (canvas != null)
            canvas.sortingOrder = (4 - (int)position.y) * 2;
    }

    private void Awake()
    {
        no = count++;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void EntityBroadcast(List<string> command)
    {
        var list = FindObjectsOfType(typeof(Entity));
        foreach (Entity receiver in list)
            if(receiver.gameObject.activeSelf && receiver.enabled)
                receiver.EntityBroadcastReceive(command);
    }

    public abstract void EntityBroadcastReceive(List<string> command);

    public abstract void EntityDead();

    public void DeletePosition()
    {
        SqlAccess access = SqlAccess.GetAccess(SqlAccess.battleDB);
        access.Open();
        access.SqlRead("SELECT entity_no FROM MAP WHERE entity_no = " + no + ";");
        if (access.read && access.dataReader.FieldCount > 0)
            access.SqlExecute("DELETE FROM MAP WHERE entity_no = " + no + ";");

        access.ShutDown();
    }

    public static int CheckEnemyCount()
    {
        int val = 0;
        var list = FindObjectsOfType(typeof(Entity));
        foreach (Entity receiver in list)
            if (!receiver.playerTeam)
                val++;
        return val;
    }

    public static int GameOver()
    {
        int val = 0;
        var list = FindObjectsOfType(typeof(Entity));
        foreach (Entity receiver in list)
            receiver.DeletePosition();
        return val;
    }
}
