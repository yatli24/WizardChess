using System.Collections.Generic;
using UnityEngine;

public class Queen : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        // Down
        for (int i = currentY - 1; i >= 0; i--)
        {
            if (board[currentX, i] == null)
            {
                r.Add(new Vector2Int(currentX, i));
            }
            else
            {
                if (IsTileAvailable(ref board, currentX, i))
                    r.Add(new Vector2Int(currentX, i));
                break;
            }
        }

        // Up
        for (int i = currentY + 1; i < tileCountY; i++)
        {
            if (board[currentX, i] == null)
            {
                r.Add(new Vector2Int(currentX, i));
            }
            else
            {
                if (IsTileAvailable(ref board, currentX, i))
                    r.Add(new Vector2Int(currentX, i));
                break;
            }
        }

        // Left
        for (int i = currentX - 1; i >= 0; i--)
        {
            if (board[i, currentY] == null)
            {
                r.Add(new Vector2Int(i, currentY));
            }
            else
            {
                if (IsTileAvailable(ref board, i, currentY))
                    r.Add(new Vector2Int(i, currentY));
                break;
            }
        }

        // Right
        for (int i = currentX + 1; i < tileCountX; i++)
        {
            if (board[i, currentY] == null)
            {
                r.Add(new Vector2Int(i, currentY));
            }
            else
            {
                if (IsTileAvailable(ref board, i, currentY))
                    r.Add(new Vector2Int(i, currentY));
                break;
            }
        }

        // Top Right
        for (int x = currentX + 1, y = currentY + 1; x < tileCountX && y < tileCountY; x++, y++)
        {
            // Check if the tile is within bounds and either empty or occupied by an opponent's piece
            if (x >= 0 && x < tileCountX && y >= 0 && y < tileCountY)
            {
                if (board[x, y] == null)
                {
                    r.Add(new Vector2Int(x, y));
                }
                else
                {
                    if (board[x, y].team != team) // Opponent's piece
                    {
                        r.Add(new Vector2Int(x, y));
                    }
                    break; // Stop if a friendly piece is encountered
                }
            }
            else
            {
                break; // Stop if out of bounds
            }
        }

        // Top Left
        for (int x = currentX - 1, y = currentY + 1; x >= 0 && y < tileCountY; x--, y++)
        {
            // Check if the tile is within bounds and either empty or occupied by an opponent's piece
            if (x >= 0 && x < tileCountX && y >= 0 && y < tileCountY)
            {
                if (board[x, y] == null)
                {
                    r.Add(new Vector2Int(x, y));
                }
                else
                {
                    if (board[x, y].team != team) // Opponent's piece
                    {
                        r.Add(new Vector2Int(x, y));
                    }
                    break; // Stop if a friendly piece is encountered
                }
            }
            else
            {
                break; // Stop if out of bounds
            }
        }

        // Bottom Right
        for (int x = currentX + 1, y = currentY - 1; x < tileCountX && y >= 0; x++, y--)
        {
            // Check if the tile is within bounds and either empty or occupied by an opponent's piece
            if (x >= 0 && x < tileCountX && y >= 0 && y < tileCountY)
            {
                if (board[x, y] == null)
                {
                    r.Add(new Vector2Int(x, y));
                }
                else
                {
                    if (board[x, y].team != team) // Opponent's piece
                    {
                        r.Add(new Vector2Int(x, y));
                    }
                    break; // Stop if a friendly piece is encountered
                }
            }
            else
            {
                break; // Stop if out of bounds
            }
        }

        // Bottom Left
        for (int x = currentX - 1, y = currentY - 1; x >= 0 && y >= 0; x--, y--)
        {
            // Check if the tile is within bounds and either empty or occupied by an opponent's piece
            if (x >= 0 && x < tileCountX && y >= 0 && y < tileCountY)
            {
                if (board[x, y] == null)
                {
                    r.Add(new Vector2Int(x, y));
                }
                else
                {
                    if (board[x, y].team != team) // Opponent's piece
                    {
                        r.Add(new Vector2Int(x, y));
                    }
                    break; // Stop if a friendly piece is encountered
                }
            }
            else
            {
                break; // Stop if out of bounds
            }
        }

        return r;
    }
}
