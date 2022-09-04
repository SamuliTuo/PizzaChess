using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_melee1 : Unit
{
    Unit targetUnit = null;

    // FollowPath is started when a path is found
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

    public override void AI(ref Unit[,] units, Vector2Int boardSize)
    {
        if (t > 0)
        {
            t -= Time.deltaTime;
        }
        else
        {
            //if (targetUnit == null)
            //{
                if (FindANewTarget(ref units))
                {
                    availableMoves = GetAvailableMoves(ref units, boardSize.x, boardSize.y);
                    ResetPath();
                    PathRequestManager.RequestPath(
                        new Vector2Int(x, y),
                        new(targetUnit.x, targetUnit.y),
                        OnPathFound,
                        Restriction.STRAIGHTS_ONLY);
                    timeToMove = true;
                }
            //}
            t = moveInterval * Random.Range(0.82f, 1.15f);
        }
    }

    bool FindANewTarget(ref Unit[,] units)
    {
        // HUOM \\
        // HUOM \\
        /// T‰n sijaan voisi k‰ytt‰‰ sit‰ D* FLOODAUS menetelm‰‰ 
        ///  ja sill‰ etsi‰ deposit, joka on ruutuja pitkin k‰vellen 
        ///  l‰himp‰n‰ nappulaa.

        float dist = 100000;
        targetUnit = null;
        for (int x = 0; x < board.GetBoardSize().x; x++)
        {
            for (int y = 0; y < board.GetBoardSize().y; y++)
            {
                var u = units[x, y];
                if (u != null && u.team != team)
                {
                    float unitDist = Vector3.Distance(transform.position, u.transform.position);
                    if (dist > unitDist)
                    {
                        dist = unitDist;
                        targetUnit = board.GetUnits()[x, y];
                    }
                }
            }
        }
        if (targetUnit == null)
            return false;
        else
            return true;
    }

    public override List<Vector2Int> GetAvailableMoves(ref Unit[,] units, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new();

        // Right
        if (x + 1 < tileCountX)
        {
            // Right
            if (units[x + 1, y] == null && board.tiles[x + 1, y].layer != LayerMask.NameToLayer("Deposit"))
                r.Add(new Vector2Int(x + 1, y));

            // Top right
            if (y + 1 < tileCountY && board.tiles[x + 1, y + 1].layer != LayerMask.NameToLayer("Deposit"))
                if (units[x + 1, y + 1] == null)
                    r.Add(new Vector2Int(x + 1, y + 1));

            // Bottom right
            if (y - 1 >= 0 && board.tiles[x + 1, y - 1].layer != LayerMask.NameToLayer("Deposit"))
                if (units[x + 1, y - 1] == null)
                    r.Add(new Vector2Int(x + 1, y - 1));
        }
        // Left
        if (x - 1 >= 0)
        {
            // Left
            if (units[x - 1, y] == null && board.tiles[x - 1, y].layer != LayerMask.NameToLayer("Deposit"))
                r.Add(new Vector2Int(x - 1, y));

            // Top left
            if (y + 1 < tileCountY)
                if (units[x - 1, y + 1] == null && board.nodes[x - 1, y + 1].walkable)
                    r.Add(new Vector2Int(x - 1, y + 1));

            // Bottom left
            if (y - 1 >= 0)
                if (units[x - 1, y - 1] == null && board.nodes[x - 1, y - 1].walkable)
                    r.Add(new Vector2Int(x - 1, y - 1));
        }
        // Up
        if (y + 1 < tileCountY)
        {
            if (units[x, y + 1] == null && board.tiles[x, y + 1].layer != LayerMask.NameToLayer("Deposit"))
                r.Add(new Vector2Int(x, y + 1));
        }
        // Down
        if (y - 1 >= 0)
        {
            if (units[x, y - 1] == null && board.tiles[x, y - 1].layer != LayerMask.NameToLayer("Deposit"))
                r.Add(new Vector2Int(x, y - 1));
        }

        return r;
    }
}
