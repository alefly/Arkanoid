using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject ballPref;
    [SerializeField] private GameObject blockPref;
    [SerializeField] private GameObject bonusPref;
    [SerializeField] private GameObject platform;
    [SerializeField] private Text textScore;
    [SerializeField] private Text textTimer;
    [SerializeField] private Text gameOverText;

    public PlatformScript platformScript;


    private const float GAPBETWEENBLOCKS = 1f;
    private const int VERTICALCOUNTBLOCKS = 6;
    private const int HORIZONTALCOUNTBLOCKS = 12;
    private const float XLEFTEXTREME = -5.5f;
    private const float YUPEXTREAM = 5f;
    private const int COMPLEXITY = 4; //count wallBlocks div 4
    private const int PERIODICITY = 3;
    private const int BLOCKPOINT = 5;

    //layers
    public const int WALLLAYER = 8;
    public const int PLATFORMLAYER = 9;
    public const int BLOCKLAYER = 10;
    public const int BONUSLAYER = 11;
    public const int BALLLAYER = 12;
    public const int FINISHWALLLAYER = 13;

    public Color SIMPLECOLOR;
    public Color DOUBLEDCOLOR;
    public Color WALLCOLOR;
    public Color WITHEXTENSIONBONUSCOLOR;
    public Color WITHBALLBONUSCOLOR;

    public static bool gameIsStart;
    private int score;
    private List<List<GameObject>> blocksList = new List<List<GameObject>>();
    private int countBonusBall = 0;
    private int countExtensionBonus = 0;
    public int countBall = 0;
    private int countActiveBlocks;
    private Stopwatch timer = new Stopwatch();
    
    public enum BlockType
    {
        none = 0,
        simple = 1,
        doubled = 2,
        wall = 3,
        withExtensionBonus = 4,
        withBallBonus = 5
    }

    public enum BonusType
    {
        none = 0,
        extensionBonus = 1,
        ballBonus = 2
    }

    private static GameController _gameController;

    public static GameController Instance
    {
        get
        {
            if (_gameController == null)
            {
                _gameController = FindObjectOfType<GameController>();
            }
            return _gameController;
        }
    }

    public static SyncClient syncClient;

    public void Start()
    {
        syncClient = FindObjectOfType<SyncClient>();
        blocksList = new List<List<GameObject>>(VERTICALCOUNTBLOCKS);
        BuildBlocks();
    }

    public void Update()
    {
        textTimer.text = string.Format("Timer: {0:00}:{1:00}", timer.Elapsed.Minutes, timer.Elapsed.Seconds);
        var record = syncClient.record > 0 ? $"Your best: {syncClient.record}" : "";
        textScore.text = $"{record}\tNow: {score}";
        ListeningInput();
        if (countActiveBlocks <= 0)
        {
            BuildBlocks();
        }
    }

    private void ListeningInput()
    {
        var x = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
        if (x > platform.transform.position.x)
        {
            platformScript.Move(x);
        }
        if (x < platform.transform.position.x)
        {
            platformScript.Move(x);
        }
        if (Input.GetKey(KeyCode.Mouse0) && !gameIsStart)
        {
            GameStart();
        }
    }

    private void BuildBlocks()
    {
        DestroyBlocks();
        Vector2 blockPos = new Vector2(XLEFTEXTREME, YUPEXTREAM);
        float x = XLEFTEXTREME;
        float y = YUPEXTREAM;
        countActiveBlocks = VERTICALCOUNTBLOCKS * HORIZONTALCOUNTBLOCKS;
        int fourthN = VERTICALCOUNTBLOCKS / 2;
        int fourthM = HORIZONTALCOUNTBLOCKS / 2;
        for (int i = 0; i < fourthN; i++)
        {
            blocksList.Add(new List<GameObject>(HORIZONTALCOUNTBLOCKS));
            for (int j = 0; j < fourthM; j++)
            {
                blocksList[i].Add(Instantiate(blockPref, blockPos, Quaternion.identity));
                blocksList[i][j].GetComponent<BlockScript>().SetType((BlockType)Random.Range(1, 3), false);
                x += GAPBETWEENBLOCKS;
                blockPos += new Vector2(GAPBETWEENBLOCKS, 0);
            }
            blockPos = new Vector2(XLEFTEXTREME, blockPos.y - GAPBETWEENBLOCKS / 2);
            y -= GAPBETWEENBLOCKS / 2;
            x = XLEFTEXTREME;
        }
        for (int i = 0; i < COMPLEXITY; i++)
        {
            int iBlock = Random.Range(0, fourthN);
            iBlock += (iBlock % 2 == 0 ? 0 : 1);
            int jBlock = Random.Range(0, fourthM);
            if (blocksList[iBlock][jBlock].GetComponent<BlockScript>().type != BlockType.wall)
            {
                countActiveBlocks -= 4;
                blocksList[iBlock][jBlock].GetComponent<BlockScript>().SetType(BlockType.wall, false);
            }
        }

        for (int i = 0; i < COMPLEXITY / 2 + 1; i++)
        {
            int iBlock = Random.Range(0, fourthN - 1);
            iBlock += (iBlock % 2 == 0 ? 1 : 0);
            blocksList[iBlock][Random.Range(0, fourthM)].GetComponent<BlockScript>().SetType(BlockType.withExtensionBonus, countExtensionBonus++ % PERIODICITY == 0 ? true : false);
            iBlock = Random.Range(0, fourthN - 1);
            iBlock += (iBlock % 2 == 0 ? 1 : 0);
            blocksList[iBlock][Random.Range(0, fourthM)].GetComponent<BlockScript>().SetType(BlockType.withBallBonus, countBonusBall++ % PERIODICITY == 0 ? true : false);
        }
        x = XLEFTEXTREME + GAPBETWEENBLOCKS * (fourthM);
        y = YUPEXTREAM;
        for (int i = 0; i < fourthN; i++)
        {
            for (int j = fourthM - 1; j >= 0; j--)
            {
                blocksList[i].Add(Instantiate(blocksList[i][j]));
                blocksList[i][HORIZONTALCOUNTBLOCKS - j - 1].transform.position = new Vector3(x, y, 0);
                var blockScript = blocksList[i][HORIZONTALCOUNTBLOCKS - j - 1].GetComponent<BlockScript>();
                blockScript.SetIsBonus(GenerateBonus(blockScript.type));
                x += GAPBETWEENBLOCKS;
            }
            y -= GAPBETWEENBLOCKS / 2;
            x = XLEFTEXTREME + GAPBETWEENBLOCKS * (fourthM);
        }
        x = XLEFTEXTREME;
        for (int i = fourthN - 1; i >= 0; i--)
        {
            blocksList.Add(new List<GameObject>(HORIZONTALCOUNTBLOCKS));
            for (int j = 0; j < HORIZONTALCOUNTBLOCKS; j++)
            {
                blocksList[VERTICALCOUNTBLOCKS - i - 1].Add(Instantiate(blocksList[i][j]));
                blocksList[VERTICALCOUNTBLOCKS - i - 1][j].transform.position = new Vector3(x, y, 0);
                var blockScript = blocksList[VERTICALCOUNTBLOCKS - i - 1][j].GetComponent<BlockScript>();
                blockScript.SetIsBonus(GenerateBonus(blockScript.type));
                x += GAPBETWEENBLOCKS;
            }
            x = XLEFTEXTREME;
            y -= GAPBETWEENBLOCKS / 2;
        }
    }

    bool GenerateBonus(BlockType type)
    {
        switch (type)
        {
            case BlockType.withExtensionBonus:
                return countExtensionBonus++ % PERIODICITY == 0 ? true : false;
            case BlockType.withBallBonus:
                return countBonusBall++ % PERIODICITY == 0 ? true : false;
        }
        return false;
    }

    void GameStart()
    {
        timer.Start();
        gameIsStart = true;
        InstantiateBall();
    }

    void InstantiateBall()
    {
        var ball = Instantiate(ballPref, new Vector3(platform.transform.position.x, platform.transform.position.y + 0.5f, 0), Quaternion.identity);
        ball.GetComponent<BallScript>().StartMove();
        countBall++;
    }

    public void OnCollisionBall(GameObject gameObject, GameObject gameObjectBall)
    {
        Rigidbody2D rigidbody2D = gameObjectBall.GetComponent<Rigidbody2D>();

        switch (gameObject.layer)
        {
            case (BLOCKLAYER):
                BlockScript blockScript = gameObject.GetComponent<BlockScript>();
                if (blockScript.type != BlockType.wall)
                {
                    gameObject.SetActive(false);
                    countActiveBlocks--;
                    score += BLOCKPOINT;
                    switch (blockScript.type)
                    {
                        case BlockType.doubled:
                            score += BLOCKPOINT;
                            break;
                        case BlockType.withExtensionBonus:
                            if (blockScript.isBonus)
                            {
                                var bonus = Instantiate(bonusPref, blockScript.transform.position, Quaternion.identity);
                                bonus.GetComponent<BonusScript>().SetType(BonusType.extensionBonus);
                            }
                            break;
                        case BlockType.withBallBonus:
                            if (blockScript.isBonus)
                            {
                                var bonus = Instantiate(bonusPref, blockScript.transform.position, Quaternion.identity);
                                bonus.GetComponent<BonusScript>().SetType(BonusType.ballBonus);
                            }
                            break;
                    }
                }
                break;
            case FINISHWALLLAYER:
                Destroy(gameObjectBall);
                countBall--;
                if (countBall <= 0)
                {
                    GameOver();
                }
                break;
        }
    }

    public void ActivationBonus(BonusType type)
    {
        switch (type)
        {
            case BonusType.extensionBonus:
                platformScript.Extension();
                break;
            case BonusType.ballBonus:
                InstantiateBall();
                break;
        }
    }

    public void DestroyBlocks()
    {
        for (int i = 0; i < blocksList.Count; i++)
        {
            for (int j = 0; j < blocksList[i].Count; j++)
            {
                Destroy(blocksList[i][j]);
            }
            blocksList[i].Clear();
        }
    }

    public void GameOver()
    {
        timer.Stop();
        if (score > syncClient.record)
        {
            gameOverText.text = $"Wow, I`m proud of you! Your new record {score}";
            syncClient.UpdateRecord(score);
        }
        else
        {
            gameOverText.text = "\t\t\t\t\tGAME OVER :с";
        }
        //platform.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
    }
    
}
