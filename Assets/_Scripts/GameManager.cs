using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    NONE = 0,
    PRE_BATTLE = 1,
    BATTLE = 2
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    public GameState state;

    private Chessboard board;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        state = GameState.BATTLE;
        board = GameObject.Find("Board").GetComponent<Chessboard>();
    }


    public Recipe[] TEST_RECIPES;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TaskManager.Instance.AddNewOrder(TEST_RECIPES);
        }

        if (state != GameState.BATTLE)
            return;
        

        // AI
        Unit[,] activeUnits = board.GetUnits();
        for (int x = 0; x < activeUnits.GetLength(0); x++)
            for (int y = 0; y < activeUnits.GetLength(1); y++)
                if (activeUnits[x, y] != null)
                    activeUnits[x, y].AI(ref activeUnits, board.GetBoardSize());    


        //foreach (var unit in activeUnits)
        //if (unit != null) unit.   

        // CheckHP();
        // Respawn(); (se timeri kun pelaaja kuoli -> spawnaa takas)
        // UpdateTimers(); (statuses etc.)
        //foreach (var unit in activeUnits) 
        //    if (unit != null) 
        //        unit.Move();
        // Attack();
        // Graphics(); (animate, facing, etc.)
        // MoveOtherStuffThanUnits() (bullets etc)
        // Lerps/Tweens(); (joku smoothattu liikutus jossain esim)
        // DealDamages();
        // Explode(); (vaikka bullet osuessaan sein‰‰n)
        // LifeTimers(); (esim bulletin)
        // ClearCollisions();
    }
}
