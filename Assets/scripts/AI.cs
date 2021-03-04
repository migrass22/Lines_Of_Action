﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI
{
    // To do:
    // 1. function to copy the current model into a temporary one -> saving only vector2int for positions
    // 2. using that new model make moves using the function u already have to find the best move
    // 2.5 first move is easy second move by the other player should lower the score of the first move
    // Figure out how to save moves
    GameObject controller = GameObject.FindGameObjectWithTag("GameController");
    BitBoard b;
    bool player = false;
    int searchdepth = 0;
    // Copy a model to a second model
    public void CopyModel(Model m, Model copy)
    {
        foreach (Piece p in m.whites)
        {
            copy.whites.Add(new Piece(p.position, p.player));
        }
        foreach (Piece p in m.blacks)
        {
            copy.blacks.Add(new Piece(p.position, p.player));
        }
        copy.board = m.board; }

    // Ai is always black
    public void GetPossibleMoves(Vector2Int dir, Vector2Int now, Move m, Model model)
    {
        Vector2Int pos = now;
        pos.x += dir.x;
        pos.y += dir.y;
        b = model.board;
        Vector2Int anti = new Vector2Int(now.x - dir.x, now.y - dir.y);
        Vector2Int normalenemy = new Vector2Int(8, 8);
        Vector2Int antienemy = new Vector2Int(8, 8);
        bool flag1 = true, flag2 = true;

        int counter = 1;
        while (model.IsOnBoard(pos.x, pos.y) || model.IsOnBoard(anti.x, anti.y))
        {
            if (CheckGood(pos, model))
            {
                if (model.board.IsEnemy(pos, player) && flag1)
                {
                    normalenemy.x = pos.x;
                    normalenemy.y = pos.y;
                    flag1 = false;
                }
                counter++;
            }
            if (CheckGood(anti, model))
            {
                if (model.board.IsEnemy(anti, player) && flag2)
                {
                    antienemy.x = anti.x;
                    antienemy.y = anti.y;
                    flag2 = false;
                }
                counter++;
            }

            pos.x += dir.x;
            pos.y += dir.y;
            anti.x -= dir.x;
            anti.y -= dir.y;
        }
        CreatePossiblesBoard(model, new Vector2Int(now.x + (counter * dir.x), now.y + (counter * dir.y)), normalenemy, flag1, now, m);
        CreatePossiblesBoard(model, new Vector2Int(now.x + (counter * -dir.x), now.y + (counter * -dir.y)), antienemy, flag2, now, m);
    }

    private bool IsBefore(Vector2Int now, int x, int y, int enemyx, int enemyy)
    {
        int dist1x = x - now.x, dist2x = enemyx - now.x, dist1y = y - now.y, dist2y = enemyy - now.y;
        return Math.Abs(dist1x) <= Math.Abs(dist2x) && Math.Abs(dist1y) <= Math.Abs(dist2y);
    }

    // Add possible moves to somewhere
    public void CreatePossiblesBoard(Model m, Vector2Int pos, Vector2Int enemy, bool flag, Vector2Int p, Move move)
    {

        if (m.IsOnBoard(pos.x, pos.y) && (IsBefore(p, pos.x, pos.y, enemy.x, enemy.y) || flag))
        {
            if (!m.board.IsPieceHere(pos.x, pos.y))
            {
                move.Child.Add(new Move(move.pieceToMove, pos, 0, false));
            }
            else if (m.board.IsEnemy(pos, player))
            {
                move.Child.Add(new Move(move.pieceToMove, pos, 0, true));
            }
        }
    }

    public void getAllDirections(Vector2Int now, Move m, Model model)
    {
        // Initiate all directions according to amound of pieces in the direction
        GetPossibleMoves(new Vector2Int(1, 0), now, m, model);
        GetPossibleMoves(new Vector2Int(0, 1), now, m, model);
        GetPossibleMoves(new Vector2Int(1, 1), now, m, model);
        GetPossibleMoves(new Vector2Int(-1, 1), now, m, model);
    }

    // get vector for position and check of its legit for moves
    public bool CheckGood(Vector2Int v, Model m)
    {
        if (m.IsOnBoard(v.x, v.y))
        {
            if (m.board.IsPieceHere(v.x, v.y))
            {
                return true;
            }
        }
        return false;
    }

    public int evaluate(Move move, Model m)
    {
        int score = 0;
        if (move.attack)
        {
            score += 4;
        }
        if (m.blacks.Count >= m.whites.Count)
        {
            score += 5;
        }
        else
        {
            score -= 5;
        }
        if ((move.moveto.x == 4 || move.moveto.y == 5) && (move.moveto.y == 4 || move.moveto.y == 5))
        {
            score += 6;
        }
        else if (move.moveto.x == 0 || move.moveto.y == 0)
        {
            score -= 4;
        }
        return score;
    }

    public void actuallymove(Model m, Move move)
    {
        Vector2Int before = new Vector2Int(move.pieceToMove.piece.GetComponent<LOAman>().GetXBoard(),
            move.pieceToMove.piece.GetComponent<LOAman>().GetYBoard());
        controller = GameObject.FindGameObjectWithTag("GameController");
        Piece BeforePiece = m.GetPieceByIndex(before.x, before.y);
        Piece piece = m.GetPieceByIndex(move.moveto.x, move.moveto.y);
        int old = before.x + 8 * before.y - 1;
        if (piece != null)
        {
            // white piece
            if (piece.player)
            {
                m.RemovePiece(piece);
                m.board.MakeMove(piece.position, piece.position, !controller.GetComponent<Game>().GetCurrentPlayer());
                GameObject.Destroy(piece.piece);
            }
        }

        BeforePiece.piece.GetComponent<LOAman>().SetXBoard(move.moveto.x);
        BeforePiece.piece.GetComponent<LOAman>().SetYBoard(move.moveto.y);
        BeforePiece.piece.GetComponent<LOAman>().SetCorods();
        m.UpdatePosition(BeforePiece, move.moveto);
        BeforePiece.piece.GetComponent<LOAman>().DestroyMovePlates();
        m.board.MakeMove(before, move.moveto, controller.GetComponent<Game>().GetCurrentPlayer());
        if (m.checkwin(controller.GetComponent<Game>().GetCurrentPlayer()))
        {
            controller.GetComponent<Game>().Winner(controller.GetComponent<Game>().GetCurrentPlayer());
        }
        else
        {
            if (!controller.GetComponent<Game>().IsGameOver())
            {
                controller.GetComponent<Game>().NextTurn();
            }

        }
    }

    // Given a move, find the best move in it (child list)
    public Move FindBestMove(Move somehthing, Model model)
    {
        int max = -1000;
        int score = 0;
        Move best = new Move();
        foreach (Move m in somehthing.Child)
        {
            score = evaluate(m, model);
            if (score > max)
            {
                best = m;
                best.score = max;
            }
        }
        return best;
    }

    public int RecursionMove(Model m, int depth, Move current)
    {
        if (depth == searchdepth)
        {
            return evaluate(current, m);
        }
        // Make a move on the model - change bitboard and lists
        m.ChangePiecePosition(current);
        m.board.MakeMove(current.pieceToMove.position, current.moveto, player);
        player = !player;
        int score;
        int bestscore = player ? int.MaxValue : int.MinValue;
        Move nextmove = new Move();
        List<Piece> indexer = m.GetPiecesByBool(player);
        for (int i = 0; i < indexer.Count; i++)
        {
            Piece p = indexer[i];
            nextmove.pieceToMove = p;
            getAllDirections(p.position, nextmove, m);
            foreach (Move after in nextmove.Child)
            {
                score = RecursionMove(m, depth + 1, after);
                if (player)
                {
                    if (bestscore > score)
                    {
                        bestscore = score;
                    }
                }
                else 
                {
                    if (bestscore < score)
                    {
                        bestscore = score;
                    }
                }
                m.UndoChangePosition(current);
                m.board.MakeMove(current.moveto, current.pieceToMove.position, player);
            }
        }
        return bestscore;
    }

    public void aimove(Model m)
    {
        Model temp = new Model();
        temp.InitModel();
        CopyModel(m, temp);
        Move move = new Move();
        Move bestmove = new Move();
        bestmove.score = int.MinValue;
        int current = int.MinValue;
        foreach (Piece p in temp.GetPiecesByBool(false)) 
        {
            move.pieceToMove = p;
            getAllDirections(p.position, move, m);
            foreach (Move nextmove in move.Child) 
            {
                current = RecursionMove(temp, 0, nextmove);
                if (bestmove.score < current) 
                {
                    bestmove = nextmove;
                    bestmove.score = current;
                }
            }
            move.Child = new List<Move>();
        }
        actuallymove(m, bestmove);
    }


}
