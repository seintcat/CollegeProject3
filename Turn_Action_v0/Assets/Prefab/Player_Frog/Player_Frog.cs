using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Frog : Entity
{
    [SerializeField]
    GameObject fx;

    // Start is called before the first frame update
    void Start()
    {
        CommitPosition();
        GameObject _fx = Instantiate(fx);
        _fx.transform.position = transform.position;
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

    public override void EntityDead()
    {
        DeletePosition();
        GameObject _fx = Instantiate(fx);
        _fx.transform.position = transform.position;
    }
}
