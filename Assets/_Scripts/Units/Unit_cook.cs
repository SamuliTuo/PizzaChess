using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_cook : Unit
{
    [SerializeField] private float workerMoveInterval = 1f;

    private Node targetDeposit = null;
    bool atTarget = false;

    public override IEnumerator FollowPath() 
    {
        Vector2Int currentWaypoint = path[0];
        targetIndex = 0;
        while (true) 
        {
            if (goingToMove) 
            {
                if (board.MoveUnit(this, path[targetIndex])) 
                {
                    if (targetIndex >= path.Length - 1)
                    {
                        atTarget = true;
                        yield break;
                    }

                    targetIndex++;
                    if (targetIndex >= path.Length)
                        yield break;

                    currentWaypoint = path[targetIndex];
                    goingToMove = false;
                }
                else
                    yield break;
            }
            yield return null;
        }
    }

    public override List<Vector2Int> GetAvailableMoves(ref Unit[,] units, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new();

        // Right
        if (x + 1 < tileCountX)
        {
            // Right
            if (units[x + 1, y] == null)
                r.Add(new Vector2Int(x + 1, y));

            // Top right
            if (y + 1 < tileCountY)
                if (units[x + 1, y + 1] == null)
                    r.Add(new Vector2Int(x + 1, y + 1));

            // Bottom right
            if (y - 1 >= 0)
                if (units[x + 1, y - 1] == null)
                    r.Add(new Vector2Int(x + 1, y - 1));
        }
        // Left
        if (x - 1 >= 0)
        {
            // Left
            if (units[x - 1, y] == null)
                r.Add(new Vector2Int(x - 1, y));

            // Top left
            if (y + 1 < tileCountY)
                if (units[x - 1, y + 1] == null)
                    r.Add(new Vector2Int(x - 1, y + 1));

            // Bottom left
            if (y - 1 >= 0)
                if (units[x - 1, y - 1] == null)
                    r.Add(new Vector2Int(x - 1, y - 1));
        }
        // Up
        if (y + 1 < tileCountY)
        {
            if (units[x, y + 1] == null)
                r.Add(new Vector2Int(x, y + 1));
        }
        // Down
        if (y - 1 >= 0)
        {
            if (units[x, y - 1] == null)
                r.Add(new Vector2Int(x, y - 1));
        }

        return r;
    }

    public override void AI(ref Unit[,] units, Vector2Int boardSize) {
        if (t > 0) {
            t -= Time.deltaTime;
        }
        else {
            if (atTarget)
            {
                goingToMove = true;
                // Mine();
                // miningInterval?
            }
            else
            {
                //ResetPath();
                PathRequestManager.RequestFindClosestNode(
                    new Vector2Int(x, y),
                    NodeType.COUNTER,
                    OnPathFound);
                goingToMove = true;
                atTarget = true;
            }
            t = moveInterval * Random.Range(0.82f, 1.15f);
        }
    }
}