using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        // Directions the King can move in (8 directions)
        int[] dx = { 1, 1, 1, 0, -1, -1, -1, 0 };
        int[] dy = { 0, 1, -1, 1, 1, 0, -1, -1 };

        for (int i = 0; i < dx.Length; i++)
        {
            int targetX = currentX + dx[i];
            int targetY = currentY + dy[i];

            if (targetX >= 0 && targetX < tileCountX && targetY >= 0 && targetY < tileCountY)
            {
                ChessPiece targetPiece = board[targetX, targetY];

                if (targetPiece == null || targetPiece.team != team)
                {
                    r.Add(new Vector2Int(targetX, targetY));
                }
            }
        }

        return r;
    }

    // castling
    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {

        SpecialMove r = SpecialMove.None;

        var kingMove = moveList.Find(m => m[0].x == 4 && m[0].y == ((team == 0) ? 0 : 7));
        var leftRook = moveList.Find(m => m[0].x == 0 && m[0].y == ((team == 0) ? 0 : 7));
        var rightRook = moveList.Find(m => m[0].x == 7 && m[0].y == ((team == 0) ? 0 : 7));

        if(kingMove == null && currentX == 4)
        {
            // White team
            if(team == 0)
            {
                // Left rook
                // if leftrook never moved
                if(leftRook == null)
                {
                    // if a rook is in 0,0
                    if (board[0,0].type == ChessPieceType.Rook)
                    {
                        // and the team of that rook is white
                        if (board[0,0].team == 0)
                        {
                            // if all spaces between king and rook are empty
                            if (board[3,0]==null)
                            {
                                if (board[2, 0] == null)
                                {
                                    if (board[1, 0] == null)
                                    {
                                        // castle is possible, king can move 2 to the left
                                        availableMoves.Add(new Vector2Int(2, 0));
                                        r = SpecialMove.Castling;
                                    }
                                }
                                    
                            }
                        }
                    }
                }

                // Right rook
                if (leftRook == null)
                {
                    if (board[7, 0].type == ChessPieceType.Rook)
                    {
                        if (board[7, 0].team == 0)
                        {
                            if (board[5, 0] == null)
                            {
                                if (board[6, 0] == null)
                                {
                                    availableMoves.Add(new Vector2Int(6, 0));
                                    r = SpecialMove.Castling;
                                }

                            }
                        }
                    }
                }
            }


            // Black team

            else
            {
                // Left rook
                // if leftrook never moved
                if (leftRook == null)
                {
                    // if a rook is in 0,0
                    if (board[0, 7].type == ChessPieceType.Rook)
                    {
                        // and the team of that rook is white
                        if (board[0, 7].team == 1)
                        {
                            // if all spaces between king and rook are empty
                            if (board[3, 7] == null)
                            {
                                if (board[2, 7] == null)
                                {
                                    if (board[1, 7] == null)
                                    {
                                        // castle is possible, king can move 2 to the left
                                        availableMoves.Add(new Vector2Int(2, 7));
                                        r = SpecialMove.Castling;
                                    }
                                }

                            }
                        }
                    }
                }

                // Right rook
                if (rightRook == null)
                {
                    if (board[7, 7].type == ChessPieceType.Rook)
                    {
                        if (board[7, 7].team == 1)
                        {
                            if (board[5, 7] == null)
                            {
                                if (board[6, 7] == null)
                                {
                                    availableMoves.Add(new Vector2Int(6, 7));
                                    r = SpecialMove.Castling;
                                }

                            }
                        }
                    }
                }
            }
        }


        return r;
    }
}
