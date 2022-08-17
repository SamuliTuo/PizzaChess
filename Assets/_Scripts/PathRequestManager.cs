using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathRequestManager : MonoBehaviour
{
    Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
    PathRequest currentPathRequest;

    static PathRequestManager instance;
    Pathfinding pathfinding;

    bool isProcessingPath;

    private void Awake()
    {
        instance = this;
        pathfinding = GetComponent<Pathfinding>();
    }

    public static void RequestPath(
        Vector2Int pathStart,
        Vector2Int pathEnd,
        Action<Vector2Int[], bool> callback,
        Restriction restriction = Restriction.NONE)
    {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback, restriction);
        instance.pathRequestQueue.Enqueue(newRequest);
        instance.TryProcessNext();
    }

    void TryProcessNext()
    {
        if (!isProcessingPath && pathRequestQueue.Count > 0)
        {
            currentPathRequest = pathRequestQueue.Dequeue();
            isProcessingPath = true;
            pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd, currentPathRequest.restriction);
        }
    }

    public void FinishedProcessingPath(Vector2Int[] path, bool success)
    {
        currentPathRequest.callback(path, success);
        isProcessingPath = false;
        TryProcessNext();
    }

    struct PathRequest {
        public Vector2Int pathStart;
        public Vector2Int pathEnd;
        public Action<Vector2Int[], bool> callback;
        public Restriction restriction;

        public PathRequest(Vector2Int _start, Vector2Int _end, Action<Vector2Int[], bool> _callback, Restriction _restriction)
        {
            pathStart = _start;
            pathEnd = _end;
            callback = _callback;
            restriction = _restriction;
        }
    }
}
