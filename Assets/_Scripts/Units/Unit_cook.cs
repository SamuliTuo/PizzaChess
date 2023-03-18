using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_cook : Unit
{

    // orderID, itemID, Task
    private Tuple<int, int, Task> currentTask = new Tuple<int, int, Task>(-1, -1, null);
    float taskTimer = 0;
    private GameObject carriedObj;

    public override IEnumerator FollowPath() 
    {
        targetIndex = 0;
        while (true) 
        {
            if (timeToMove) 
            {
                if (path.Length <= 0)
                {
                    taskTimer = currentTask.Item3.taskDuration;
                    path = null;
                    yield break;
                }
                if (board.MoveUnit(this, path[targetIndex])) 
                {
                    RotateUnit(path[targetIndex]);
                    if (targetIndex >= path.Length - 1)
                    {
                        taskTimer = currentTask.Item3.taskDuration;
                        path = null;
                        yield break;
                    }

                    targetIndex++;
                    if (targetIndex >= path.Length)
                        yield break;

                    timeToMove = false;
                }
                else
                {
                    ResetPath();
                    yield break;
                }
            }
            yield return null;
        }
    }
    void RotateUnit(Vector2Int lookAt)
    {
        transform.LookAt(board.GetTileCenter(lookAt.x, lookAt.y));
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
        if (taskTimer > 0)
        {
            WorkOnTask();
        }
        else if (t > 0) {
            t -= Time.deltaTime;
        }
        else {
            if (currentTask.Item3 == null)
            {
                currentTask = TaskManager.Instance.GetTask();
                print(currentTask);
            }
            else if (path == null)
            {
                FindTargetAndCreatePath();
            }
            else
            {
                timeToMove = true;
            }
            t = moveInterval * UnityEngine.Random.Range(0.82f, 1.15f);
        }
    }

    void WorkOnTask()
    {
        taskTimer -= Time.deltaTime;
        if (taskTimer <= 0)
        {
            var taskObj = currentTask.Item3.TaskEnded();
            if (taskObj != null)
            {
                carriedObj = Instantiate(
                    taskObj, 
                    transform.position + transform.forward, 
                    Quaternion.identity, 
                    transform);
            }
            else
            {
                Destroy(carriedObj);
                carriedObj = null;
            }
            
            TaskManager.Instance.FinishTask(currentTask.Item1, currentTask.Item2, currentTask.Item3);
            currentTask = new Tuple<int, int, Task>(-1, -1, null);
        }
    }

    void FindTargetAndCreatePath()
    {
        PathRequestManager.RequestFindClosestNode(
            new Vector2Int(x, y),
            currentTask.Item3.targetWorkStation,
            OnPathFound);
        timeToMove = true;
    }
}