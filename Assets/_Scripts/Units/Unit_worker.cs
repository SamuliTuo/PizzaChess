using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_worker : Unit
{
    [SerializeField] private float workerMoveInterval = 1f;

    private Node targetDeposit = null;
    bool atDeposit = false;

    public override IEnumerator FollowPath() 
    {
        Vector2Int currentWaypoint = path[0];
        targetIndex = 0;
        while (true) 
        {
            if (timeToMove) 
            {
                if (board.MoveUnit(this, path[targetIndex])) 
                {
                    if (board.tiles[path[targetIndex].x, path[targetIndex].y].layer == LayerMask.NameToLayer("Deposit")) 
                    {
                        atDeposit = true;
                        yield break;
                    }

                    targetIndex++;
                    if (targetIndex >= path.Length)
                        yield break;

                    currentWaypoint = path[targetIndex];
                    timeToMove = false;
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
            if (atDeposit) {
                // Mine();
                // miningInterval?
            }
            else {
                if (SearchForDeposit())
                {
                    availableMoves = GetAvailableMoves(ref units, boardSize.x, boardSize.y);
                    ResetPath();
                    PathRequestManager.RequestPath(
                        new Vector2Int(x, y),
                        new(targetDeposit.x, targetDeposit.y),
                        OnPathFound);
                    timeToMove = true;
                }
            }
            t = moveInterval * Random.Range(0.82f, 1.15f);
        }
    }

    bool SearchForDeposit()
    {
        // HUOM \\
        // HUOM \\
        /// T‰n sijaan voisi k‰ytt‰‰ sit‰ D* FLOODAUS menetelm‰‰ 
        ///  ja sill‰ etsi‰ deposit, joka on ruutuja pitkin k‰vellen 
        ///  l‰himp‰n‰ nappulaa.

        float dist = 100000;
        targetDeposit = null;
        for (int x = 0; x < board.GetBoardSize().x; x++) {
            for (int y = 0; y < board.GetBoardSize().y; y++) {
                if (board.GetUnits()[x,y] != null)
                    continue;
                
                var tile = board.tiles[x,y];
                if (tile.layer == LayerMask.NameToLayer("Deposit"))
                {
                    float tiledist = Vector3.Distance(transform.position, tile.transform.position);
                    if (dist > tiledist)
                    {
                        dist = tiledist;
                        targetDeposit = board.nodes[x,y];
                    }
                }
            }
        }
        if (targetDeposit == null)
            return false;
        else 
            return true;
    }
}

/*
public override void AI(Unit[,] units, Vector2Int boardSize)
{
    if (workerT > 0)
    {
        workerT -= Time.deltaTime * Random.Range(0.8f, 1.2f);
    }
    else if (targetDeposit == null)
    {
        if (SearchForDeposit())
        {
            //MoveToDeposit();
        }
        else
        {
            //MoveAroundRandomly();
        }
    }
    else
    {
        //MoveToDeposit();
        availableMoves = GetAvailableMoves(ref units, boardSize.x, boardSize.y);
        if (availableMoves.Count > 0)
        {
            SetTargetMoveTile(availableMoves[Random.Range(0, availableMoves.Count)]);
            goingToMove = true;
        }
        workerT = workerMoveInterval + Random.Range(-0.33f, 0.33f);


    }
}
*/

