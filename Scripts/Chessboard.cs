using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;



#if UNITY_EDITOR
using UnityEditor;
#endif

public enum SpecialMove
{
    None = 0,
    EnPassant,
    Castling,
    Promotion
}


public class Chessboard : MonoBehaviour
{
    [Header("Art Stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material redMaterial;
    [SerializeField] private Material whiteMaterial;
    [SerializeField] private Material blackMaterial;
    [SerializeField] private Material freezeHighlightMaterial;
    private List<Vector2Int> previousFreezePreview = new();
    [SerializeField] private float tileSize = 0.6f;
    [SerializeField] private float yOffset = 0.6f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathSize = 0.4f;
    [SerializeField] private float deathSpacing = 0.2f;
    [SerializeField] private float dragOffset = 0.45f;
    [SerializeField] private GameObject victoryScreen;


    [Header("Prefabs & Materials")]
    public GameObject[] whitePrefabs;
    public GameObject[] blackPrefabs;
    [SerializeField] private Material[] teamMaterials;

    // LOGIC
    private ChessPiece[,] chessPieces;
    private ChessPiece currentlyDragging;
    private List<ChessPiece> deadWhites = new List<ChessPiece>();
    private List<ChessPiece> deadBlacks = new List<ChessPiece>();
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    public bool isWhiteTurn;
    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();
    private SpecialMove specialMove;

    // Special Abilities Logic
    public bool whiteExtraTurn;
    public bool blackExtraTurn;
    public Button extraWhiteTurnButton;
    public Button extraBlackTurnButton;

    // need to access the extra turn button manager here, for refund logic for check situation
    public ExtraTurnButton extraWhiteTurnButtonManager;
    public ExtraTurnButton extraBlackTurnButtonManager;

    public bool whiteTeleportActive = false;
    public bool blackTeleportActive = false;
    public Button blackTeleportButton;
    public Button whiteTeleportButton;

    // Freeze ability logic
    public bool whiteFreezeAbilityActive = false;
    public bool blackFreezeAbilityActive = false;

    [NonSerialized] public int whiteFreezeCharges = 2;
    [NonSerialized] public int blackFreezeCharges = 2;

    public Button whiteFreezeButton;
    public Button blackFreezeButton;
    public TextMeshProUGUI whiteFreezeButtonText;
    public TextMeshProUGUI blackFreezeButtonText;

    private Dictionary<Vector2Int, (int team, int turnsRemaining)> frozenTilesInfo = new();

    // events logic
    public int turnsUntilEvent;
    public int eventElapsedTurns;

    void Awake()
    {
        isWhiteTurn = true;
        whiteExtraTurn = false;
        blackExtraTurn = false;

        // initiate freeze text
        whiteFreezeButtonText.text = $"Activate White Freeze ({whiteFreezeCharges})";
        blackFreezeButtonText.text = $"Activate Black Freeze ({blackFreezeCharges})";

        // initialize event tracking
        turnsUntilEvent = 0;
        eventElapsedTurns = 0;

        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);

        SpawnAllPieces();

        PositionAllPieces();
    }
    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        UpdateGraveyardBobbing();

        // Show or hide the extra turn and teleport button based on the turn
        if (isWhiteTurn)
        {
            extraWhiteTurnButton.gameObject.SetActive(true);
            extraBlackTurnButton.gameObject.SetActive(false);

            whiteTeleportButton.gameObject.SetActive(true);
            blackTeleportButton.gameObject.SetActive(false);

            whiteFreezeButton.gameObject.SetActive(true);
            blackFreezeButton.gameObject.SetActive(false);
        }
        else
        {
            extraWhiteTurnButton.gameObject.SetActive(false);
            extraBlackTurnButton.gameObject.SetActive(true);

            whiteTeleportButton.gameObject.SetActive(false);
            blackTeleportButton.gameObject.SetActive(true);

            whiteFreezeButton.gameObject.SetActive(false);
            blackFreezeButton.gameObject.SetActive(true);
        }

        if ((whiteFreezeAbilityActive || blackFreezeAbilityActive) && Input.GetMouseButtonDown(0))
        {
            if (currentHover.x < 0 || currentHover.y < 0 || currentHover.x >= TILE_COUNT_X - 1 || currentHover.y >= TILE_COUNT_Y - 1)
                return;

            Vector2Int center = currentHover;
            int freezingTeam = isWhiteTurn ? 0 : 1;

            for (int dx = 0; dx < 2; dx++)
            {
                for (int dy = 0; dy < 2; dy++)
                {
                    int x = center.x + dx;
                    int y = center.y + dy;
                    Vector2Int pos = new Vector2Int(x, y);

                    frozenTilesInfo[pos] = (freezingTeam, 1); // frozen for 1 full enemy turn
                    tiles[x, y].GetComponent<Renderer>().material = freezeHighlightMaterial;
                }
            }

            previousFreezePreview.Clear();

            if (isWhiteTurn)
            {
                whiteFreezeAbilityActive = false;
            }
            else
            {
                blackFreezeAbilityActive = false;
            }

            // END TURN IMMEDIATELY after using freeze
            isWhiteTurn = !isWhiteTurn;

            return;
        }




        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            // freeze hover
            if (whiteFreezeAbilityActive || blackFreezeAbilityActive)
            {
                if (currentHover.x >= 0 && currentHover.y >= 0 &&
                    currentHover.x < TILE_COUNT_X - 1 && currentHover.y < TILE_COUNT_Y - 1)
                {
                    PreviewFreezeArea(currentHover);
                }
            }

            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                SetTileHoverState(currentHover, true);
            }

            if (currentHover != hitPosition)
            {
                SetTileHoverState(currentHover, false);
                currentHover = hitPosition;
                SetTileHoverState(currentHover, true);
            }

            // if left click
            if (Input.GetMouseButtonDown(0))
            {
                if (chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    bool isWhitePiece = chessPieces[hitPosition.x, hitPosition.y].team == 0;
                    bool isBlackPiece = chessPieces[hitPosition.x, hitPosition.y].team == 1;

                    // Is it our turn?
                    // Allow selecting your own piece during your turn (including extra turn)
                    if ((isWhiteTurn && isWhitePiece) || (!isWhiteTurn && isBlackPiece))
                    {
                        currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];

                        // Prevent movement if the piece is frozen by the enemy team
                        Vector2Int piecePos = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);
                        if (frozenTilesInfo.ContainsKey(piecePos))
                        {
                            var freezeData = frozenTilesInfo[piecePos];
                            if (freezeData.team != currentlyDragging.team)
                            {
                                Debug.Log("That piece is frozen and cannot move this turn.");
                                currentlyDragging = null;
                                return;
                            }
                        }


                        // Get a list of where I can go, highlight tiles as well
                        availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);

                        // Teleport override, works with pieces in check (i.e. I can't teleport my pawn because it will put my king in check)
                        // Right now, only the knight can teleport
                        if ((isWhiteTurn && whiteTeleportActive) || (!isWhiteTurn && blackTeleportActive))
                        {
                            if (currentlyDragging.type == ChessPieceType.Knight)
                            {
                                availableMoves.Clear();
                                for (int x = 0; x < TILE_COUNT_X; x++)
                                {
                                    for (int y = 0; y < TILE_COUNT_Y; y++)
                                    {
                                        if (chessPieces[x, y] == null)
                                            availableMoves.Add(new Vector2Int(x, y));
                                    }
                                }

                                // Play teleport preview VFX
                            }
                        }


                        Debug.Log($"Clicked on piece: {chessPieces[hitPosition.x, hitPosition.y].name} (team {chessPieces[hitPosition.x, hitPosition.y].team})");
                        Debug.Log($"isWhiteTurn: {isWhiteTurn}, whiteExtraTurn: {whiteExtraTurn}, blackExtraTurn: {blackExtraTurn}");

                        // Get a list of special moves as well
                        specialMove = currentlyDragging.GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves);

                        PreventCheck();

                        HighlightTiles();
                    }
                }
            }


            // if left click released
            if (currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);

                bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);

                if (!validMove)
                {
                    currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                    currentlyDragging = null;
                }
                currentlyDragging = null;
                RemoveHighlightTiles();


            }
        }
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                SetTileHoverState(currentHover, false);
                currentHover = -Vector2Int.one;
            }

            // freeze area handling
            if (whiteFreezeAbilityActive || blackFreezeAbilityActive)
            {
                // Clear freeze preview if mouse moves off board
                foreach (Vector2Int pos in previousFreezePreview)
                {
                    if (!frozenTilesInfo.ContainsKey(pos))
                    {
                        tiles[pos.x, pos.y].GetComponent<Renderer>().material = tileMaterial;
                    }
                }
                previousFreezePreview.Clear();
            }

            if (currentlyDragging && Input.GetMouseButtonUp(0))
            {
                currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }


        // If we're dragging a piece, have it move with the cursor
        if (currentlyDragging)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance))
            {
                currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
            }
        }

    }

    private void SetTileHoverState(Vector2Int pos, bool isHovered)
    {
        GameObject tile = tiles[pos.x, pos.y];

        // If the tile is part of the available moves, change the hover color
        if (availableMoves.Contains(pos))
        {
            if (isHovered)
            {
                // If hovered, change it to hoverMaterial
                tile.GetComponent<Renderer>().material = hoverMaterial;
            }
            else
            {
                // RED if there's an enemy piece
                if (chessPieces[pos.x, pos.y] != null && chessPieces[pos.x, pos.y].team != currentlyDragging.team)
                {

                    tile.GetComponent<Renderer>().material = redMaterial;
                }
                else
                {
                    // If it's not a highlighted tile, just use the hover behavior as normal
                    tile.GetComponent<Renderer>().material = highlightMaterial;
                }
            }
        }
        else
        {
            if (!frozenTilesInfo.ContainsKey(pos))
            {
                tile.layer = LayerMask.NameToLayer(isHovered ? "Hover" : "Tile");
                tile.GetComponent<Renderer>().material = isHovered ? hoverMaterial : tileMaterial;
            }
            else
            {
                // Do NOT change material if frozen — keep freezeHighlightMaterial
                tile.layer = LayerMask.NameToLayer("Tile");
            }

        }
    }


    // Generate the board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;


        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
            }
        }
    }
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    // spawn the pieces
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_X];

        int whiteTeam = 0, blackTeam = 1;

        // White team
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
        }

        // Black team
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);

        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
        }
    }

    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        GameObject prefab = team == 0 ? whitePrefabs[(int)type - 1] : blackPrefabs[(int)type - 1];
        ChessPiece cp = Instantiate(prefab, transform).GetComponent<ChessPiece>();

        cp.type = type;
        cp.team = team;

        // Assign custom material WIP feature for custom colors
        Material teamMat = team == 0 ? whiteMaterial : blackMaterial;
        MeshRenderer renderer = cp.GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.material = teamMat;

        return cp;
    }

    // Positioning
    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    PositionSinglePiece(x, y, true);
                }
            }
        }
    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].SetPosition(GetTileCenter(x, y), force);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    // Highlight available move tiles in green
    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            Vector2Int move = availableMoves[i];
            GameObject tile = tiles[move.x, move.y];

            // Check if there's a piece on that tile and if it's an enemy
            if (chessPieces[move.x, move.y] != null)
            {
                if (chessPieces[move.x, move.y].team != currentlyDragging.team)
                {
                    tile.GetComponent<Renderer>().material = redMaterial; // highlight red for enemy
                }
            }
            else
            {
                tile.GetComponent<Renderer>().material = highlightMaterial; // highlight green for empty
            }
        }
    }

    // freeze highlight tiles
    private void PreviewFreezeArea(Vector2Int center)
    {
        if (center.x < 0 || center.y < 0 || center.x >= TILE_COUNT_X - 1 || center.y >= TILE_COUNT_Y - 1)
            return;

        // Remove old preview highlights
        foreach (Vector2Int pos in previousFreezePreview)
        {
            if (!frozenTilesInfo.ContainsKey(pos)) // only reset if not already frozen
                tiles[pos.x, pos.y].GetComponent<Renderer>().material = tileMaterial;
        }

        previousFreezePreview.Clear();

        // Add new preview highlights
        for (int dx = 0; dx < 2; dx++)
        {
            for (int dy = 0; dy < 2; dy++)
            {
                int x = center.x + dx;
                int y = center.y + dy;

                Vector2Int pos = new Vector2Int(x, y);
                if (!frozenTilesInfo.ContainsKey(pos))
                {
                    tiles[x, y].GetComponent<Renderer>().material = freezeHighlightMaterial;
                }

                previousFreezePreview.Add(pos);
            }
        }
    }

    private void RemoveHighlightTiles()
    {
        // Loop through all available move tiles
        for (int i = 0; i < availableMoves.Count; i++)
        {
            Vector2Int move = availableMoves[i];
            GameObject tile = tiles[move.x, move.y];

            // Change the tile material back to the original tileMaterial
            tile.GetComponent<Renderer>().material = tileMaterial;

            // Set the tile layer back to "Tile"
            tile.layer = LayerMask.NameToLayer("Tile");
        }

        // Clear the available moves list
        availableMoves.Clear();
    }

    // Checkmate
    private void CheckMate(int team)
    {
        DisplayVictory(team);
    }

    [SerializeField] private GameObject whiteWinsPanel;
    [SerializeField] private GameObject blackWinsPanel;

    private void DisplayVictory(int winningTeam)
    {
        victoryScreen.SetActive(true);
        whiteWinsPanel.SetActive(winningTeam == 0);
        blackWinsPanel.SetActive(winningTeam == 1);
    }

    public void OnResetButton()
    {
        // remove UI
        victoryScreen.SetActive(false);
        whiteWinsPanel.SetActive(1 == 0);
        blackWinsPanel.SetActive(0 == 1);

        // Fields reset
        currentlyDragging = null;
        moveList.Clear();
        availableMoves.Clear();

        // Clean up pieces and memory of pieces
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                    Destroy(chessPieces[x, y].gameObject);

                chessPieces[x, y] = null;
            }
        }

        for (int i = 0; i < deadWhites.Count; i++)
        {
            Destroy(deadWhites[i].gameObject);
        }

        for (int i = 0; i < deadBlacks.Count; i++)
        {
            Destroy(deadBlacks[i].gameObject);
        }

        deadWhites.Clear();
        deadBlacks.Clear();

        SpawnAllPieces();
        PositionAllPieces();
        isWhiteTurn = true;
    }

    public void OnExitButtion()
    {
        Debug.Log("Exit button clicked");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    // Special Moves
    private void ProcessSpecialMove()
    {
        if (specialMove == SpecialMove.EnPassant)
        {
            var newMove = moveList[moveList.Count - 1];
            ChessPiece myPawn = chessPieces[newMove[1].x, newMove[1].y];
            var targetPawnPosition = moveList[moveList.Count - 2];
            ChessPiece enemyPawn = chessPieces[targetPawnPosition[1].x, targetPawnPosition[1].y];

            // if pawn and enemypawn are in the same X, en passant was done
            if (myPawn.currentX == enemyPawn.currentX)
            {
                // extra condition to make sure en passant is correct
                if (myPawn.currentY == enemyPawn.currentY - 1 || myPawn.currentY == enemyPawn.currentY + 1)
                {
                    // kill pieces based on team, add to graveyard
                    if (enemyPawn.team == 0)
                    {
                        deadWhites.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deathSize);
                        float centerOffsetX = 7.8f * tileSize;  // Closer to center
                        enemyPawn.SetPosition(new Vector3(centerOffsetX, yOffset + 0.2f, -1 * tileSize + 0.15f)
                            - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2)
                            + (Vector3.forward * deathSpacing) * deadWhites.Count);
                    }

                    else
                    {
                        deadBlacks.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deathSize);
                        float centerOffsetX = 7.8f * tileSize;  // Closer to center
                        enemyPawn.SetPosition(new Vector3(centerOffsetX, yOffset + 0.2f, -1 * tileSize + 0.15f)
                            - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2)
                            + (Vector3.forward * deathSpacing) * deadBlacks.Count);
                    }

                    // if en passant was done, and piece was killed, remove the memory reference
                    chessPieces[enemyPawn.currentX, enemyPawn.currentY] = null;
                }
            }
        }

        if (specialMove == SpecialMove.Promotion)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            ChessPiece targetPawn = chessPieces[lastMove[1].x, lastMove[1].y];

            if (targetPawn.type == ChessPieceType.Pawn)
            {
                // white promotion
                if (targetPawn.team == 0 && lastMove[1].y == 7)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 0);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }

                // black promotion
                if (targetPawn.team == 1 && lastMove[1].y == 0)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 1);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
            }
        }

        if (specialMove == SpecialMove.Castling)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];

            // left rook
            if (lastMove[1].x == 2)
            {
                if (lastMove[1].y == 0) // White side
                {
                    ChessPiece rook = chessPieces[0, 0];
                    chessPieces[3, 0] = rook;
                    PositionSinglePiece(3, 0);
                    chessPieces[0, 0] = null;
                }
                else if (lastMove[1].y == 7) // Black side
                {
                    ChessPiece rook = chessPieces[0, 7];
                    chessPieces[3, 7] = rook;
                    PositionSinglePiece(3, 7);
                    chessPieces[0, 7] = null;
                }
            }

            // right rook
            else if (lastMove[1].x == 6)
            {
                if (lastMove[1].y == 0) // White side
                {
                    ChessPiece rook = chessPieces[7, 0];
                    chessPieces[5, 0] = rook;
                    PositionSinglePiece(5, 0);
                    chessPieces[7, 0] = null;
                }
                else if (lastMove[1].y == 7) // Black side
                {
                    ChessPiece rook = chessPieces[7, 7];
                    chessPieces[5, 7] = rook;
                    PositionSinglePiece(5, 7);
                    chessPieces[7, 7] = null;
                }
            }

        }
    }
    private void PreventCheck()
    {
        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (chessPieces[x, y] != null)
                    if (chessPieces[x, y].type == ChessPieceType.King)
                        if (chessPieces[x, y].team == currentlyDragging.team)
                            targetKing = chessPieces[x, y];

        // since we're sending ref availableMoves, we delete moves that put us in check
        SimulateMoveForSinglePiece(currentlyDragging, ref availableMoves, targetKing);
    }

    private void SimulateMoveForSinglePiece(ChessPiece cp, ref List<Vector2Int> moves, ChessPiece targetKing)
    {
        // save the current values, to reset after the function call
        int actualX = cp.currentX;
        int actualY = cp.currentY;
        List<Vector2Int> movesToRemove = new List<Vector2Int>();

        // going through all the moves, simulate them and check if we're in check
        for (int i = 0; i < moves.Count; i++)
        {
            int simX = moves[i].x;
            int simY = moves[i].y;

            Vector2Int kingPositionThisSim = new Vector2Int(targetKing.currentX, targetKing.currentY);
            // Did we simualte the king's move
            if (cp.type == ChessPieceType.King)
                kingPositionThisSim = new Vector2Int(simX, simY);

            // Copy the [,] and not a reference
            ChessPiece[,] simulation = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
            List<ChessPiece> simAttackingPieces = new List<ChessPiece>();
            for (int x = 0; x < TILE_COUNT_X; x++)
            {
                for (int y = 0; y < TILE_COUNT_Y; y++)
                {
                    if (chessPieces[x, y] != null)
                    {
                        simulation[x, y] = chessPieces[x, y];
                        if (simulation[x, y].team != cp.team)
                            simAttackingPieces.Add(simulation[x, y]);
                    }

                }
            }

            // Simulate that move
            simulation[actualX, actualY] = null;
            cp.currentX = simX;
            cp.currentY = simY;
            simulation[simX, simY] = cp;

            // Did one of the pieces get taken down during our simulation
            var deadPiece = simAttackingPieces.Find(c => c.currentX == simX && c.currentY == simY);
            if (deadPiece != null)
                simAttackingPieces.Remove(deadPiece);

            // Get all the simulated attacking pieces moves
            List<Vector2Int> simMoves = new List<Vector2Int>();
            for (int a = 0; a < simAttackingPieces.Count; a++)
            {
                var pieceMoves = simAttackingPieces[a].GetAvailableMoves(ref simulation, TILE_COUNT_X, TILE_COUNT_Y);
                for (int b = 0; b < pieceMoves.Count; b++)
                    simMoves.Add(pieceMoves[b]);
            }

            // Is the king in trouble? if so, remove the move
            if (ContainsValidMove(ref simMoves, kingPositionThisSim))
            {
                movesToRemove.Add(moves[i]);
            }

            // Restore the actual CP data
            cp.currentX = actualX;
            cp.currentY = actualY;

        }

        // remove from the current available move list
        for (int i = 0; i < movesToRemove.Count; i++)
            moves.Remove(movesToRemove[i]);
    }

    private bool CheckForCheckmate()
    {
        var lastMove = moveList[moveList.Count - 1];
        int targetTeam = (chessPieces[lastMove[1].x, lastMove[1].y].team == 0) ? 1 : 0;

        List<ChessPiece> attackingPieces = new List<ChessPiece>();
        List<ChessPiece> defendingPieces = new List<ChessPiece>();
        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (chessPieces[x, y] != null)
                {
                    if (chessPieces[x, y].team == targetTeam)
                    {
                        defendingPieces.Add(chessPieces[x, y]);
                        if (chessPieces[x, y].type == ChessPieceType.King)
                            targetKing = chessPieces[x, y];
                    }
                    else
                    {
                        attackingPieces.Add(chessPieces[x, y]);
                    }
                }
        // is the king attacked right now?
        List<Vector2Int> currentAvailableMoves = new List<Vector2Int>();
        for (int i = 0; i < attackingPieces.Count; i++)
        {
            var pieceMoves = attackingPieces[i].GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
            for (int b = 0; b < pieceMoves.Count; b++)
                currentAvailableMoves.Add(pieceMoves[b]);
        }

        // are we in check right now?
        if (ContainsValidMove(ref currentAvailableMoves, new Vector2Int(targetKing.currentX, targetKing.currentY)))
        {
            // King is under attack, can we move something to help him?
            for (int i = 0; i < defendingPieces.Count; i++)
            {
                List<Vector2Int> defendingMoves = defendingPieces[i].GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                // since we're sending ref availableMoves, we delete moves that put us in check
                SimulateMoveForSinglePiece(defendingPieces[i], ref defendingMoves, targetKing);

                if (defendingMoves.Count != 0)
                    return false;

            }

            return true; // Checkmate exit
        }

        return false;
    }

    // Operations
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2Int pos)
    {
        for (int i = 0; i < moves.Count; i++)
            if (moves[i].x == pos.x && moves[i].y == pos.y)
                return true;


        return false;
    }
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (tiles[x, y] == hitInfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return -Vector2Int.one; // Invalid
    }

    private bool MoveTo(ChessPiece cp, int x, int y)
    {
        if (!ContainsValidMove(ref availableMoves, new Vector2Int(x, y)))
            return false;

        Vector2Int previousPosition = new Vector2Int(cp.currentX, cp.currentY);

        // is there another piece on the target position?
        if (chessPieces[x, y] != null)
        {
            ChessPiece ocp = chessPieces[x, y];

            if (cp.team == ocp.team)
            {
                return false;
            }

            // if its the enemy team
            // kill it and move it to graveyard
            if (ocp.team == 0)
            {
                if (ocp.type == ChessPieceType.King)
                    CheckMate(1);

                deadWhites.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                float centerOffsetX = 7.8f * tileSize;  // Closer to center
                ocp.SetPosition(new Vector3(centerOffsetX, yOffset + 0.2f, -1 * tileSize + 0.15f)
                    - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2)
                    + (Vector3.forward * deathSpacing) * deadWhites.Count);


            }
            else
            {
                if (ocp.type == ChessPieceType.King)
                    CheckMate(0);

                deadBlacks.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp.SetPosition(
                    new Vector3(-0.8f * tileSize, yOffset + 0.2f, 8 * tileSize - 0.15f)  // centered on left like white's 7.8
                    - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2)
                    + (Vector3.back * deathSpacing) * deadBlacks.Count
                );

            }

        }

        chessPieces[x, y] = cp;
        chessPieces[previousPosition.x, previousPosition.y] = null;
        chessPieces[cp.currentX, cp.currentY] = null;

        PositionSinglePiece(x, y);

        Debug.Log($"Moving {cp} to ({x},{y})");

        // Extra turn and turn handling, with refund logic if move puts opponent in check
        bool movePutOpponentInCheck = MovePutsOpponentInCheck(cp);

        if (isWhiteTurn && whiteExtraTurn)
        {
            if (movePutOpponentInCheck)
            {
                // refund extra turn
                whiteExtraTurn = false;
                isWhiteTurn = false;
                if (extraWhiteTurnButtonManager != null)
                {
                    extraWhiteTurnButtonManager.remainingWhiteExtraTurns++;
                    extraWhiteTurnButtonManager.whiteExtraTurnButtonText.text = $"Activate White Extra Turn ({extraWhiteTurnButtonManager.remainingWhiteExtraTurns})";
                }
                Debug.Log("White's extra turn refunded because it put Black in check.");
            }
            else
            {
                whiteExtraTurn = false; // consume extra turn
            }
        }
        else if (!isWhiteTurn && blackExtraTurn)
        {
            if (movePutOpponentInCheck)
            {
                // refund extra turn
                blackExtraTurn = false;
                isWhiteTurn = true;
                if (extraBlackTurnButtonManager != null)
                {
                    extraBlackTurnButtonManager.remainingBlackExtraTurns++;
                    extraBlackTurnButtonManager.blackExtraTurnButtonText.text = $"Activate Black Extra Turn ({extraBlackTurnButtonManager.remainingBlackExtraTurns})";
                }
                Debug.Log("Black's extra turn refunded because it put White in check.");
            }
            else
            {
                blackExtraTurn = false; // consume extra turn
            }
        }
        else
        {
            isWhiteTurn = !isWhiteTurn; // regular toggle
        }

        // Unfreeze logic: reduce timers and remove expired ones
        List<Vector2Int> toUnfreeze = new();

        foreach (var kvp in frozenTilesInfo)
        {
            (int freezeTeam, int turnsRemaining) = kvp.Value;

            // Only reduce timer if the freezing team is the opponent of the new active team
            if (freezeTeam != (isWhiteTurn ? 0 : 1))
            {
                int newTurnsRemaining = turnsRemaining - 1;
                if (newTurnsRemaining <= 0)
                {
                    tiles[kvp.Key.x, kvp.Key.y].GetComponent<Renderer>().material = tileMaterial;
                    toUnfreeze.Add(kvp.Key);
                }
                else
                {
                    frozenTilesInfo[kvp.Key] = (freezeTeam, newTurnsRemaining);
                }
            }
        }

        foreach (var pos in toUnfreeze)
        {
            frozenTilesInfo.Remove(pos);
        }



        Debug.Log(isWhiteTurn ? "White's turn" : "Black's turn");

        // define movelist
        moveList.Add(new Vector2Int[] { previousPosition, new Vector2Int(x, y) });

        ProcessSpecialMove();

        if (CheckForCheckmate())
        {
            CheckMate(cp.team);
        }

        // trigger rise and materialize effect if teleport was used
        if ((whiteTeleportActive || blackTeleportActive) && cp.type == ChessPieceType.Knight)
        {
            Vector3 tileCenter = GetTileCenter(x, y);

            // if its white, use different duration than black
            if (cp.team == 0)
            {
                cp.PlayTeleportRise(tileCenter, 1.0f, 0.65f);
                cp.PlayMaterializeEffect(2.5f); // runs alongside the rise
            }

            // if its black, use different duration than white
            if (cp.team == 1)
            {
                cp.PlayTeleportRise(tileCenter, 1.0f, 0.65f);
                cp.PlayMaterializeEffect(1.5f); // runs alongside the rise
            }
        }

        // reset teleporting to false after activation
        if (whiteTeleportActive) whiteTeleportActive = false;
        if (blackTeleportActive) blackTeleportActive = false;

        return true;


    }

    private bool MovePutsOpponentInCheck(ChessPiece movedPiece)
    {
        int opponentTeam = movedPiece.team == 0 ? 1 : 0;
        ChessPiece opponentKing = null;
        List<ChessPiece> attackingPieces = new List<ChessPiece>();

        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                ChessPiece piece = chessPieces[x, y];
                if (piece == null) continue;

                if (piece.team == opponentTeam && piece.type == ChessPieceType.King)
                    opponentKing = piece;

                if (piece.team == movedPiece.team)
                    attackingPieces.Add(piece);
            }
        }

        if (opponentKing == null)
            return false;

        foreach (ChessPiece attacker in attackingPieces)
        {
            List<Vector2Int> moves = attacker.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
            if (ContainsValidMove(ref moves, new Vector2Int(opponentKing.currentX, opponentKing.currentY)))
            {
                return true;
            }
        }

        return false;
    }


    private void UpdateGraveyardBobbing()
    {
        // Animate both white and black graveyards
        AnimateGraveyardBobbing(deadWhites);
        AnimateGraveyardBobbing(deadBlacks);
    }

    private void AnimateGraveyardBobbing(List<ChessPiece> pieces)
    {
        foreach (ChessPiece piece in pieces)
        {
            // Calculate the new y position using Mathf.PingPong for smooth oscillation
            float bobbingY = Mathf.PingPong(Time.time * 0.04f, 0.1f) + 0.52f; // bob between 0.6 and 0.7
            piece.transform.position = new Vector3(piece.transform.position.x, bobbingY, piece.transform.position.z);
        }

    }

    // freeze ability button event
    public void OnWhiteFreezeButtonPressed()
    {
        if (whiteFreezeCharges > 0 && isWhiteTurn)
        {
            whiteFreezeAbilityActive = true;
            whiteFreezeCharges--;
            whiteFreezeButtonText.text = $"Activate White Freeze ({whiteFreezeCharges})";
        }
    }

    public void OnBlackFreezeButtonPressed()
    {
        if (blackFreezeCharges > 0 && !isWhiteTurn)
        {
            blackFreezeAbilityActive = true;
            blackFreezeCharges--;
            blackFreezeButtonText.text = $"Activate Black Freeze ({blackFreezeCharges})";
        }
    }

}