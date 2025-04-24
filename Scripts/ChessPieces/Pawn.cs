using UnityEngine;
using System.Collections.Generic;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1;

        // One in front
        if (currentY + direction >= 0 && currentY + direction < tileCountY &&
            board[currentX, currentY + direction] == null)
        {
            r.Add(new Vector2Int(currentX, currentY + direction));
        }

        // Two in front from starting position
        if (team == 0 && currentY == 1 || team == 1 && currentY == 6)
        {
            if (board[currentX, currentY + direction] == null &&
                board[currentX, currentY + direction * 2] == null)
            {
                r.Add(new Vector2Int(currentX, currentY + direction * 2));
            }
        }

        // Capture diagonally right
        int targetX = currentX + 1;
        int targetY = currentY + direction;
        if (targetX < tileCountX && targetY >= 0 && targetY < tileCountY)
        {
            if (IsTileAvailable(ref board, targetX, targetY))
            {
                r.Add(new Vector2Int(targetX, targetY));
            }
        }

        // Capture diagonally left
        targetX = currentX - 1;
        if (targetX >= 0 && targetY >= 0 && targetY < tileCountY)
        {
            if (IsTileAvailable(ref board, targetX, targetY))
            {
                r.Add(new Vector2Int(targetX, targetY));
            }
        }

        return r;
    }

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        int direction = (team == 0) ? 1 : -1; // direction is 1 if white team, -1 if black team
        if ((team == 0 && currentY == 6) || (team == 1 && currentY == 1))
        {
            return SpecialMove.Promotion;
        }

        // En passant
        if (moveList.Count > 0)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            if (board[lastMove[1].x, lastMove[1].y].type == ChessPieceType.Pawn)
            {
                // if the last move was +2 in either direction (black/white)
                if (Mathf.Abs(lastMove[0].y - lastMove[1].y) == 2)
                {
                    // if move was from other team
                    if (board[lastMove[1].x, lastMove[1].y].team != team)
                    {
                        // if both pawns are on the same Y
                        if (lastMove[1].y == currentY)
                        {
                            // landed left
                            if (lastMove[1].x == currentX - 1)
                            {
                                availableMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }

                            // landed right
                            if (lastMove[1].x == currentX + 1)
                            {
                                availableMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }
                        }
                    }
                }
            }
        }

        return SpecialMove.None;
    }
}