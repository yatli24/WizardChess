using UnityEngine;
using System.Collections.Generic;

public class Knight : ChessPiece
{
    public Chessboard Chessboard;
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        // All 8 possible L-shaped moves for a knight
        Vector2Int[] knightMoves = new Vector2Int[]
        {
        new Vector2Int(+1, +2),
        new Vector2Int(+2, +1),
        new Vector2Int(-1, +2),
        new Vector2Int(-2, +1),
        new Vector2Int(-1, -2),
        new Vector2Int(-2, -1),
        new Vector2Int(+1, -2),
        new Vector2Int(+2, -1),
        };

        foreach (Vector2Int move in knightMoves)
        {
            int x = currentX + move.x;
            int y = currentY + move.y;

            // Check if inside the board
            if (x >= 0 && x < tileCountX && y >= 0 && y < tileCountY)
            {
                // Either empty or enemy piece
                if (board[x, y] == null || board[x, y].team != team)
                    r.Add(new Vector2Int(x, y));
            }
        }

        return r;
    }
}
