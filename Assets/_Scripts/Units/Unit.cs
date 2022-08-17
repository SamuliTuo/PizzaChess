using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType
{
    NONE = 0,
    WORKER = 1,
    MELEE = 2,
    RANGE = 3,
    MAGE = 4,
    SUMMONER = 5
}

public class Unit : MonoBehaviour
{
    public float hp;
    public float attackDamage;
    //spells??!
    public int team;
    public int x;
    public int y;
    public UnitType type;
    public bool goingToAttack = false;
    public bool goingToMove = false;
    public Chessboard board;
    [HideInInspector] public float t;

    public float moveInterval = 1f;
    public float attackSpeed = 0.5f;
    public float attackRange = 1;

    public List<Vector2Int> availableMoves = new List<Vector2Int>();
    
    //private Unit targetUnit;
    private Vector3 desiredPosition;
    private Vector3 desiredScale = Vector3.one;
    private Vector2Int targetMoveTile;

    // Pathing
    public Pathfinding pathfinding;
    public Vector2Int target;
    public Vector2Int[] path;
    public int targetIndex;

    public virtual void SetTargetMoveTile(Vector2Int tile) { targetMoveTile = tile; }


    private void Start()
    {
        t = Random.Range(1.30f, 2.20f);
        board = GameObject.Find("Board").GetComponent<Chessboard>();
        pathfinding = board.GetComponent<Pathfinding>();
    }
    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }

    public void OnPathFound(Vector2Int[] newPath, bool pathSuccesfull)
    {
        if (pathSuccesfull)
        {
            path = newPath;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    public virtual IEnumerator FollowPath()
    {
        Vector2Int currentWaypoint = path[0];
        targetIndex = 0;
        while (true)
        {
            if (goingToMove)
            {
                if (board.MoveUnit(this, path[targetIndex]))
                {
                    targetIndex++;
                    if (targetIndex >= path.Length)
                    {
                        yield break;
                    }

                    currentWaypoint = path[targetIndex];
                    goingToMove = false;
                }
                else
                {
                    yield break;
                }
            }
            yield return null;
        }
    }
    public void ResetPath()
    {
        StopCoroutine("FollowPath");
    }

    public virtual List<Vector2Int> GetAvailableMoves(ref Unit[,] units, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();
        
        // Right
        if (x + 1 < tileCountX)
        {
            // Right
            if (units[x + 1, y] == null)
            {
                if (board.tiles[x + 1, y].layer != LayerMask.NameToLayer("Deposit"))
                {
                    r.Add(new Vector2Int(x + 1, y));
                }
            }
            else if (units[x + 1, y].team != team)
                r.Add(new Vector2Int(x + 1, y));

            // Top right
            if (y + 1 < tileCountY)
                if (units[x + 1, y + 1] == null)
                {
                    if (board.tiles[x + 1, y + 1].layer != LayerMask.NameToLayer("Deposit"))
                    {
                        r.Add(new Vector2Int(x + 1, y + 1));
                    }
                }
                else if (units[x + 1, y + 1].team != team)
                    r.Add(new Vector2Int(x + 1, y + 1));

            // Bottom right
            if (y - 1 >= 0)
                if (units[x + 1, y - 1] == null)
                {
                    if (board.tiles[x + 1, y - 1].layer != LayerMask.NameToLayer("Deposit"))
                    {
                        r.Add(new Vector2Int(x + 1, y - 1));
                    }
                }
                else if (units[x + 1, y - 1].team != team)
                    r.Add(new Vector2Int(x + 1, y - 1));
        }
        // Left
        if (x - 1 >= 0)
        {
            // Left
            if (units[x - 1, y] == null)
            {
                if (board.tiles[x - 1, y].layer != LayerMask.NameToLayer("Deposit"))
                {
                    r.Add(new Vector2Int(x - 1, y));
                }
            }
            else if (units[x - 1, y].team != team)
                r.Add(new Vector2Int(x - 1, y));

            // Top left
            if (y + 1 < tileCountY)
                if (units[x - 1, y + 1] == null)
                {
                    if (board.tiles[x - 1, y + 1].layer != LayerMask.NameToLayer("Deposit"))
                    {
                        r.Add(new Vector2Int(x - 1, y + 1));
                    }
                }
                else if (units[x - 1, y + 1].team != team)
                    r.Add(new Vector2Int(x - 1, y + 1));

            // Bottom left
            if (y - 1 >= 0)
                if (units[x - 1, y - 1] == null)
                {
                    if (board.tiles[x - 1, y - 1].layer != LayerMask.NameToLayer("Deposit"))
                    {
                        r.Add(new Vector2Int(x - 1, y - 1));
                    }
                }
                else if (units[x - 1, y - 1].team != team)
                    r.Add(new Vector2Int(x - 1, y - 1));
        }
        // Up
        if (y + 1 < tileCountY)
        {
            if (units[x, y + 1] == null || units[x, y + 1].team != team)
                if (board.tiles[x, y + 1].layer != LayerMask.NameToLayer("Deposit"))
                    r.Add(new Vector2Int(x, y + 1));
        }
        // Down
        if (y - 1 >= 0)
        {
            if (units[x, y - 1] == null || units[x, y - 1].team != team)
                if (board.tiles[x, y - 1].layer != LayerMask.NameToLayer("Deposit"))
                    r.Add(new Vector2Int(x, y - 1));
        }

        return r;
    }
    public virtual void SetPosition(Vector3 pos, bool force = false)
    {
        desiredPosition = pos;
        if (force)
            transform.position = desiredPosition;
    }
    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if (force)
            transform.localScale = desiredScale;
    }
    public virtual void AI(ref Unit[,] units, Vector2Int boardSize)
    {
        if (t > 0)
        {
            t -= Time.deltaTime;
        }
        else
        {
            ResetPath();
            availableMoves = GetAvailableMoves(ref units, boardSize.x, boardSize.y);
            PathRequestManager.RequestPath(new Vector2Int(x, y), new Vector2Int(0, 0), OnPathFound);//target
            goingToMove = true;
            t = moveInterval * Random.Range(0.83f, 1.15f);
        }
    }
    public virtual void FindTarget()
    {

    }

    public virtual void Move()
    {
        if (goingToMove)
        {
            /*
            //print("current: (" + currentX + ", " + currentY + "), target: " + targetMoveTile);
            board.MoveUnit(this, targetMoveTile); //targetMoveTile);
            goingToMove = false;
            availableMoves = null;
            */
        }
    }
}
