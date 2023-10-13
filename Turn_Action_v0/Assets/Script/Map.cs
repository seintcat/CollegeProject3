using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Map : MonoBehaviour
{
    static readonly Vector3 playerSidePos = new Vector3(-0.5f, -1.5f, 0), enemySidePos = new Vector3(4.5f, -1.5f, 0);
    static readonly Color playerSideColor = new Color32(255, 255, 255, 255), enemySideColor = new Color32(207, 207, 207, 255);

    [SerializeField]
    PlayerController controller;

    // remove at complete
    [SerializeField]
    GameObject playerPrefab;

    [SerializeField]
    PrefabList dbList;

    [SerializeField]
    RectTransform backgroundPos;
    GameObject backgroundObj, playerSide, enemySide;

    int lastTilesetIndex, stageIndex;

    [SerializeField]
    BarImplement timeBar;
    float maxTime, time;

    bool started;

    static readonly string scoreText = "Score : ";
    [SerializeField]
    TextMeshProUGUI scoreField;
    int score;

    // Start is called before the first frame update
    void Start()
    {
        lastTilesetIndex = 1;
        stageIndex = 1;
        started = false;
        score = 0;
        CreateMap();
    }

    // Update is called once per frame
    void Update()
    {
        time -= Time.deltaTime;
        timeBar.SetBar(time / maxTime);

        if (time < 0 && started)
            MakeMap();
    }

    // read DB and make map
    public void CreateMap()
    {
        // spawn player
        GameObject player = Instantiate(playerPrefab);
        player.transform.position = new Vector3(2, 2, 0);

        // active player controller
        controller.Init(player);

        // make map and play game
        MakeMap();

        started = true;
    }

    public void MakeMap()
    {
        if(Entity.CheckEnemyCount() > 0)
        {
            Entity.GameOver();
            SceneManager.LoadScene("GameOver", LoadSceneMode.Single);
        }
        scoreField.text = scoreText + score;

        if (backgroundObj != null)
            Destroy(backgroundObj);
        if (playerSide != null)
            Destroy(playerSide);
        if (enemySide != null)
            Destroy(enemySide);

        //// edit need
        //int difficulty = 1;
        //List<int> stageList = new List<int>();

        //// select stage, edit need
        SqlAccess access = SqlAccess.GetAccess(SqlAccess.gameDB);
        //access.Open();
        //access.SqlRead("SELECT Key FROM Stage WHERE Difficult = " + difficulty + ";");
        //if (access.read)
        //    while (access.dataReader.Read())
        //        stageList.Add(access.dataReader.GetInt32(0));
        //access.ShutDown();
        //stageIndex = stageList[Random.Range(0, stageList.Count)];

        // make map visual
        access.Open();
        access.SqlRead("SELECT Map.Background, Map.Field, Stage.Time FROM Stage, Map WHERE Stage.Key = " + stageIndex + " AND Stage.Map = Map.Key;");
        if (access.read && access.dataReader.Read())
        {
            // spawn player side
            playerSide = Instantiate(dbList.TilesetList[lastTilesetIndex]);
            playerSide.transform.position = playerSidePos;
            playerSide.GetComponent<SpriteRenderer>().color = playerSideColor;

            // spawn enemy side
            enemySide = Instantiate(dbList.TilesetList[access.dataReader.GetInt32(1)]);
            enemySide.transform.position = enemySidePos;
            enemySide.GetComponent<SpriteRenderer>().color = enemySideColor;
            lastTilesetIndex = access.dataReader.GetInt32(1);

            // spawn background
            backgroundObj = Instantiate(dbList.backgroundList[access.dataReader.GetInt32(0)]);
            backgroundObj.transform.SetParent(backgroundPos);

            maxTime = access.dataReader.GetInt32(2);
            time = maxTime;
            timeBar.SetBar(time / maxTime);
        }
        else
        {
            access.ShutDown();
            stageIndex = 1;
            MakeMap();
            return;
        }
        access.ShutDown();

        // spawn entity
        access.Open();
        access.SqlRead("SELECT Key, x, y FROM Entity WHERE Stage = " + stageIndex + ";");
        if (access.read && access.dataReader.FieldCount > 0)
            while (access.dataReader.Read())
                Instantiate(dbList.entityList[access.dataReader.GetInt32(0)]).transform.position = new Vector3(access.dataReader.GetInt32(1), access.dataReader.GetInt32(2), 0);
        access.ShutDown();

        stageIndex++;
        score++;
    }
}
