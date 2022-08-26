using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum Restriction { NONE = 0, STRAIGHTS_ONLY = 1, DIAGONALS_ONLY = 2 }

public class Pathfinding : MonoBehaviour
{
    PathRequestManager requestManager;
    Chessboard board;

    private void Awake()
    {
        requestManager = GetComponent<PathRequestManager>();
        board = GetComponent<Chessboard>();
    }

    public void StartFindPath(
        Vector2Int startPos, 
        Vector2Int targetPos, 
        Restriction restriction)
    {
        StartCoroutine(FindPath(startPos, targetPos, restriction));
    }

    public IEnumerator FindPath(
        Vector2Int startPos, 
        Vector2Int targetPos, 
        Restriction restriction) 
    {
        Vector2Int[] waypoints = new Vector2Int[0];
        bool pathSuccess = false;

        Node startNode = board.nodes[startPos.x, startPos.y];
        Node targetNode = board.nodes[targetPos.x, targetPos.y];

        Heap<Node> openSet = new Heap<Node>(board.nodes.Length);
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);
        
        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                pathSuccess = true;
                break;
            }

            foreach (Node neighbour in board.GetNeighbourNodes(currentNode, restriction))
            {
                if (!neighbour.walkable
                    || closedSet.Contains(neighbour)
                    || (board.GetUnits()[neighbour.x, neighbour.y] != null 
                        && board.nodes[neighbour.x, neighbour.y] != board.nodes[targetPos.x, targetPos.y]))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                    else
                        openSet.UpdateItem(neighbour);
                }
            }
        }
        yield return null;
        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
        }
        requestManager.FinishedProcessingPath(waypoints, pathSuccess); 
    }

    public IEnumerator FindClosestTileOfLayer(
        Vector2Int startPos,
        string targetLayerName,
        Restriction restriction
        )
    {
        Vector2Int[] waypoints = new Vector2Int[0];
        bool pathSuccess = false;

        Node startNode = board.nodes[startPos.x, startPos.y];
        Node targetNode = null;
        Heap<Node> openSet = new Heap<Node>(board.nodes.Length);
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (board.tiles[currentNode.x, currentNode.y].layer == LayerMask.NameToLayer(targetLayerName))
            {
                pathSuccess = true;
                targetNode = board.nodes[currentNode.x, currentNode.y];
                break;
            }

            foreach (Node neighbour in board.GetNeighbourNodes(currentNode, restriction))
            {
                if (!neighbour.walkable
                    || closedSet.Contains(neighbour)
                    || board.GetUnits()[neighbour.x, neighbour.y] != null)
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                    else
                        openSet.UpdateItem(neighbour);
                }
            }
        }
        yield return null;
        if (pathSuccess && targetNode != null)
        {
            waypoints = RetracePath(startNode, targetNode);
        }
        requestManager.FinishedProcessingPath(waypoints, pathSuccess);
    }

    Vector2Int[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector2Int[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);

        return waypoints;
    }

    Vector2Int[] SimplifyPath(List<Node> path)
    {
        List<Vector2Int> waypoints = new List<Vector2Int>();
        //Vector2 directionOld = Vector2.zero;

        for (int i = 0; i < path.Count; i++)
        {
            //Vector2 directionNew = new(path[i-1].x - path[i].x, path[i-1].y - path[i].y);
            //if (directionNew != directionOld)
            //{
                waypoints.Add(new(path[i].x, path[i].y));
            //}
            //directionOld = directionNew;
        }
        return waypoints.ToArray();
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.x - nodeB.x);
        int distY = Mathf.Abs(nodeA.y - nodeB.y);

        if (distX > distY)
            return 14*distY + 10* (distX-distY);
        return 14*distX + 10* (distY-distX);
    }
}
