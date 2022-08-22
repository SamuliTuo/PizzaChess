using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chessboard : MonoBehaviour
{
    [Header("Art Styuff")]
    [SerializeField] private Material tileMat;
    [SerializeField] private Material gardenMat;
    [SerializeField] private Material queueMat;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathSize = 0.3f;
    [SerializeField] private float deathSpacing = 0.3f;
    [SerializeField] private float draggingScale = 0.8f;
    [SerializeField] private float draggingOffset = 1.5f;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    // LOGIC
    private Unit[,] activeUnits;
    public Unit[,] GetUnits() { return activeUnits; }

    [HideInInspector] public GameObject[,] tiles;
    [HideInInspector] public Node[,] nodes;
    private Unit currentlyDragging;
    private const int TILE_COUNT_X = 10;
    private const int TILE_COUNT_Y = 10;
    public Vector2Int GetBoardSize() { return new(TILE_COUNT_X, TILE_COUNT_Y); }
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<Unit> deadUnits_player = new List<Unit>();
    private List<Unit> deadUnits_enemy = new List<Unit>();
    private Camera currentCam;
    private Vector2Int currentHover;

    private void Awake() 
    {
        GenerateGrid(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllUnits();
        PositionAllUnits();
    }
    private void Update()
    {
        if (!currentCam)
        {
            currentCam = Camera.main;
            return;
        }
        if (GameManager.instance.state != GameState.PRE_BATTLE)
        {
            return;
        }

        RaycastHit hit;
        Ray ray = currentCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            // Get the indexes of the tiles I've hit
            Vector2Int hitPosition = LookupTileIndex(hit.transform.gameObject);

            // If hovering a tile after not hovering any tile
            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // If already were hovering a tile, change the previous one
            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            // If we press down on the mouse
            if (Input.GetMouseButtonDown(0))
            {
                if (activeUnits[hitPosition.x, hitPosition.y] != null)
                {
                    // Is it our turn?
                    if (GameManager.instance.state == GameState.PRE_BATTLE)
                    {
                        currentlyDragging = activeUnits[hitPosition.x, hitPosition.y];

                        // Get a list of where i can go, highlight the tiles
                        availableMoves = currentlyDragging.GetAvailableMoves(ref activeUnits, TILE_COUNT_X, TILE_COUNT_Y);
                        HighlightTiles();
                    }
                }
            }

            // If we are releasing the mouse button
            if (currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPos = new Vector2Int(currentlyDragging.x, currentlyDragging.y);

                bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y, ref availableMoves);
                if (!validMove)
                {
                    currentlyDragging.SetPosition(GetTileCenter(previousPos.x, previousPos.y));
                }
                currentlyDragging.SetScale(Vector3.one);
                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }

            if (currentlyDragging && Input.GetMouseButtonUp(0))
            {
                currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.x, currentlyDragging.y));
                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }

        // If we're dragging a piece
        if (currentlyDragging)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance))
            {
                currentlyDragging.SetScale(Vector3.one * draggingScale);
                currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * draggingOffset);
            }
        }
    }
    public bool MoveUnit(Unit unit, Vector2Int targetPos)
    {
        Vector2Int previousPos = new Vector2Int(unit.x, unit.y);

        bool validMove = MoveTo(unit, targetPos.x, targetPos.y, ref unit.availableMoves);
        if (!validMove)
        {
            unit.SetPosition(GetTileCenter(previousPos.x, previousPos.y));
            unit.ResetPath();
            return false;
        }
        else return true;
        //currentlyDragging.SetScale(Vector3.one);
        //currentlyDragging = null;
        //RemoveHighlightTiles();
    }

    // Board
    private void GenerateGrid(float tileSize, int tileCountX, int tileCountY)
    {
        tiles = new GameObject[tileCountX, tileCountY];
        nodes = new Node[tileCountX, tileCountY];
        GenerateKitchen(tileSize, 1, 8, tileCountY);
        GenerateGarden(tileSize, 8, 10, tileCountY);
        GenerateQueue(tileSize, 0, 1, tileCountY);
    }

    private void GenerateKitchen(float tileSize, int fromRow, int toRow, int tileCountY)
    {
        yOffset += transform.position.y;
        int tileCountX = toRow - fromRow;

        for (int x = fromRow; x < toRow; x++)
            for (int y = 0; y < tileCountY; y++)
                tiles[x, y] = GenerateSingleTile(tileSize, x, y, tileMat, "Tile");
    }

    private void GenerateGarden(float tileSize, int fromRow, int toRow, int tileCountY)
    {
        for (int x = fromRow; x < toRow; x++)
            for (int y = 0; y < tileCountY; y++)
                tiles[x, y] = GenerateSingleTile(tileSize, x, y, gardenMat, "Garden");
    }

    private void GenerateQueue(float tileSize, int fromRow, int toRow, int tileCountY)
    {
        for (int x = fromRow; x < toRow; x++)
            for (int y = 0; y < tileCountY; y++)
                tiles[x, y] = GenerateSingleTile(tileSize, x, y, queueMat, "Queue");
    }
    private GameObject GenerateSingleTile(float tileSize, int x, int y, Material material, string layer)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = material;


        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize);
        vertices[1] = new Vector3(x * tileSize, yOffset, (y+1) * tileSize);
        vertices[2] = new Vector3((x+1) * tileSize, yOffset, y * tileSize);
        vertices[3] = new Vector3((x+1) * tileSize, yOffset, (y+1) * tileSize);

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;

        if ((y > 1 && y < 14) && Random.Range(0.00f, 1.00f) > 0.83f)
        {
            tileObject.layer = LayerMask.NameToLayer("Deposit");
            nodes[x, y] = new Node(false, x, y);
            Instantiate(Resources.Load("mineral_pink_mod_01") as GameObject, GetTileCenter(x, y), Quaternion.identity);
        }
        else
        {
            tileObject.layer = LayerMask.NameToLayer(layer);
            nodes[x, y] = new Node(true, x, y);
        }

        tileObject.AddComponent<BoxCollider>().size = new Vector3(tileSize, 0.1f, tileSize);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return tileObject;
    }
    public List<Node> GetNeighbourNodes(Node node, Restriction restriction)
    {
        List<Node> neighbours = new List<Node>();

        switch (restriction)
        {
            case Restriction.NONE:
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0)
                            continue;

                        int checkX = node.x + x;
                        int checkY = node.y + y;

                        if (checkX >= 0 && checkX < TILE_COUNT_X
                         && checkY >= 0 && checkY < TILE_COUNT_Y)
                        {
                            neighbours.Add(nodes[checkX, checkY]);
                        }
                    }
                }
                break;

            case Restriction.STRAIGHTS_ONLY:
                if (node.x - 1 >= 0)
                    neighbours.Add(nodes[node.x - 1, node.y]);
                if (node.x + 1 < TILE_COUNT_X)
                    neighbours.Add(nodes[node.x + 1, node.y]);
                if (node.y - 1 >= 0)
                    neighbours.Add(nodes[node.x, node.y - 1]);
                if (node.y + 1 < TILE_COUNT_Y)
                    neighbours.Add(nodes[node.x, node.y + 1]);
                break;

            case Restriction.DIAGONALS_ONLY:
                if (node.x - 1 >= 0)
                {
                    if (node.y - 1 >= 0)
                        neighbours.Add(nodes[node.x - 1, node.y - 1]);
                    if (node.y + 1 < TILE_COUNT_Y)
                        neighbours.Add(nodes[node.x - 1, node.y + 1]);
                }
                if (node.x + 1 < TILE_COUNT_X)
                {
                    if (node.y - 1 >= 0)
                        neighbours.Add(nodes[node.x + 1, node.y - 1]);
                    if (node.y + 1 < TILE_COUNT_Y)
                        neighbours.Add(nodes[node.x + 1, node.y + 1]);
                }
                break;

            default: break;
        }

        return neighbours;
    }

    // Spawning of the units
    private void SpawnAllUnits()
    {
        activeUnits = new Unit[TILE_COUNT_X, TILE_COUNT_Y];

        int playerTeam = 0;
        int enemyTeam = 1;

        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            activeUnits[i, 0] = SpawnSingleUnit(UnitType.WORKER, playerTeam);
            activeUnits[i, 7] = SpawnSingleUnit(UnitType.WORKER, enemyTeam);
        }
        // Player team
        //activeUnits[0, 0] = SpawnSingleUnit(UnitType.RANGE, playerTeam);
        //activeUnits[1, 0] = SpawnSingleUnit(UnitType.MAGE, playerTeam);
        //activeUnits[2, 0] = SpawnSingleUnit(UnitType.RANGE, playerTeam);
        //activeUnits[3, 0] = SpawnSingleUnit(UnitType.SUMMONER, playerTeam);
        //activeUnits[4, 0] = SpawnSingleUnit(UnitType.SUMMONER, playerTeam);
        //activeUnits[5, 0] = SpawnSingleUnit(UnitType.RANGE, playerTeam);
        //activeUnits[6, 0] = SpawnSingleUnit(UnitType.MAGE, playerTeam);
        //activeUnits[7, 0] = SpawnSingleUnit(UnitType.RANGE, playerTeam);

        //for (int i = 0; i < TILE_COUNT_X; i++)
        //    activeUnits[i, 1] = SpawnSingleUnit(UnitType.WORKER, playerTeam);

        // Enemy team
        /*
        activeUnits[0, 7] = SpawnSingleUnit(UnitType.RANGE, enemyTeam);
        activeUnits[1, 7] = SpawnSingleUnit(UnitType.MAGE, enemyTeam);
        activeUnits[2, 7] = SpawnSingleUnit(UnitType.RANGE, enemyTeam);
        activeUnits[3, 7] = SpawnSingleUnit(UnitType.SUMMONER, enemyTeam);
        activeUnits[4, 7] = SpawnSingleUnit(UnitType.SUMMONER, enemyTeam);
        activeUnits[5, 7] = SpawnSingleUnit(UnitType.RANGE, enemyTeam);
        activeUnits[6, 7] = SpawnSingleUnit(UnitType.MAGE, enemyTeam);
        activeUnits[7, 7] = SpawnSingleUnit(UnitType.RANGE, enemyTeam);
        */
        /*
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            activeUnits[i, 15] = SpawnSingleUnit(UnitType.WORKER, playerTeam);
            activeUnits[i, 14] = SpawnSingleUnit(UnitType.WORKER, playerTeam);
            activeUnits[i, 1] = SpawnSingleUnit(UnitType.WORKER, playerTeam);
            activeUnits[i, 0] = SpawnSingleUnit(UnitType.WORKER, playerTeam);
        }
        */
    }
    private Unit SpawnSingleUnit(UnitType type, int team)
    {
        Unit unit = Instantiate(prefabs[(int)type - 1], transform).GetComponent<Unit>();

        unit.type = type;
        unit.team = team;
        unit.GetComponent<MeshRenderer>().material = teamMaterials[team];

        return unit;
    }

    // Positioning
    private void PositionAllUnits()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (activeUnits[x, y] != null)
                    PositionSingleUnit(x, y, true);
    }
    private void PositionSingleUnit(int x, int y, bool force = false)
    {
        activeUnits[x, y].x = x;
        activeUnits[x, y].y = y;
        activeUnits[x, y].SetPosition(GetTileCenter(x, y), force);
    }
    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) + new Vector3(tileSize * 0.5f, 0, tileSize * 0.5f);
    }

    // Highlight tiles
    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
    }
    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");

        availableMoves.Clear();
    }

    // Operations
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        for (int i = 0; i < moves.Count; i++)
            if (moves[i].x == pos.x && moves[i].y == pos.y)
                return true;

        return false;
    }
    private bool MoveTo(Unit unit, int x, int y, ref List<Vector2Int> moves)
    {
        if (!ContainsValidMove(ref moves, new Vector2(x,y)))
            return false;

        Vector2Int previousPos = new(unit.x, unit.y);

        // Is there another unit on the target pos?
        if (activeUnits[x,y] != null)
        {
            Unit other = activeUnits[x, y];

            if (unit.team == other.team)
            {
                return false;
            }

            // Player unit doing damage
            if (unit.team == 0)
            {
                //other.DoDamage(10);
                deadUnits_enemy.Add(other);
                other.SetScale(Vector3.one * deathSize);
                other.SetPosition(
                    new Vector3(-1 * tileSize, yOffset, 8 * tileSize)
                    + new Vector3(tileSize * 0.5f, 0, tileSize * 0.5f)
                    + (Vector3.back * deathSpacing) * deadUnits_enemy.Count);
            }
            // Enemy unit doing damage to player
            else
            {
                deadUnits_player.Add(other);
                other.SetScale(Vector3.one * deathSize);
                other.SetPosition(
                    new Vector3(8 * tileSize, yOffset, -1 * tileSize)
                    + new Vector3(tileSize * 0.5f, 0, tileSize * 0.5f)
                    + (Vector3.forward * deathSpacing) * deadUnits_player.Count);
            }
        }

        activeUnits[x, y] = unit;
        activeUnits[previousPos.x, previousPos.y] = null;

        PositionSingleUnit(x, y);

        return true;
    }
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (tiles[x,y] == hitInfo)
                    return new Vector2Int(x,y);

        return -Vector2Int.one; // Invalid
    }
}
