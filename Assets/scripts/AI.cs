using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI
{
    GameObject controller = GameObject.FindGameObjectWithTag("GameController");
    BitBoard b;
    Model temp;
    List<Piece> possiblemove = new List<Piece>();

    // Ai is always black
    public void GetPossibleMoves(Vector2Int dir, Piece now) 
    {
        Vector2Int pos = now.position;
        pos.x += dir.x;
        pos.y += dir.y;
        Model model = controller.GetComponent<Game>().model;
        b = model.board;
        Vector2Int anti = new Vector2Int(now.position.x - dir.x, now.position.y - dir.y);
        Vector2Int normalenemy = new Vector2Int(8, 8);
        Vector2Int antienemy = new Vector2Int(8, 8);
        bool flag1 = true, flag2 = true;

        int counter = 1;
        while (model.IsOnBoard(pos.x, pos.y) || model.IsOnBoard(anti.x, anti.y))
        {
            if (now.piece.GetComponent<LOAman>().CheckLegit(pos, model))
            {
                if (model.GetPieceByIndex(pos.x, pos.y).player && flag1)
                {
                    normalenemy.x = pos.x;
                    normalenemy.y = pos.y;
                    flag1 = false;
                }
                counter++;
            }
            if (now.piece.GetComponent<LOAman>().CheckLegit(anti, model))
            {
                if (model.GetPieceByIndex(anti.x, anti.y).player && flag2)
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
        CreatePossiblesBoard(model, new Vector2Int(now.position.x + (counter * dir.x), now.position.y + (counter * dir.y)), normalenemy, flag1, now);
        CreatePossiblesBoard(model, new Vector2Int(now.position.x + (counter * -dir.x), now.position.y + (counter * -dir.y)), antienemy, flag2, now);
    }
    public void CreatePossiblesBoard(Model m, Vector2Int pos, Vector2Int enemy, bool flag,Piece p) 
    {

        if (m.IsOnBoard(pos.x, pos.y) && (p.piece.GetComponent<LOAman>().IsBefore(pos.x, pos.y, enemy.x, enemy.y) || flag))
        {
            if (!m.board.IsPieceHere(pos.x, pos.y))
            {
                possiblemove.Add(new Piece(pos, false));
            }
            else if (m.GetPieceByIndex(pos.x, pos.y).player)
            {
                possiblemove.Add(new Piece(pos, true));
            }
        }
    }
    public void getAllDirections(Piece now)
    {
        // Initiate all directions according to amound of pieces in the direction
        GetPossibleMoves(new Vector2Int(1, 0), now);
        GetPossibleMoves(new Vector2Int(0, 1), now);
        GetPossibleMoves(new Vector2Int(1, 1), now);
        GetPossibleMoves(new Vector2Int(-1, 1), now);
    }

    public void MakeMove(Model m) 
    {
        // Copy everything from the given model to the temporary one
        temp.board = m.board;
        Move best = BestMove(temp, 2);
        actuallymove(m, best);
    }

    public Move BestMove(Model m, int depth) 
    {
        int max = -1000, temp;
        Piece bestPiece = null;
        Piece move = null;
        foreach (Piece p in m.blacks) 
        {
            getAllDirections(p);
            foreach (Piece pos in possiblemove)
            {
                temp = evaluate(b, p.position, pos.position, pos.player, m);
                if (temp > max)
                {
                    max = temp;
                    bestPiece = p;
                    move = pos;
                }
            }
            possiblemove = new List<Piece>();
        }
        //actuallymove(bestPiece, m, move);
        if (depth == 0)
        {
            return new Move(bestPiece, move.position, max, move.player);
        }
        Move back = BestMove(m, depth - 1);
        if (back != null) 
        {
            back.score += max;
        }
        return null;
    }   


    public int evaluate(BitBoard b,Vector2Int before, Vector2Int move, bool attack, Model m) 
    {
        int score = 0;
        if (attack) 
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
        if ((move.x == 4 || move.y == 5) && (move.y == 4 || move.y == 5))
        {
            score += 6;
        }
        else if(move.x == 0 || move.y == 0)
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
                m.board.MakeMove(piece.position.x + piece.position.y * 8 - 1, piece.position.x + piece.position.y * 8 - 1, !controller.GetComponent<Game>().GetCurrentPlayer());
                GameObject.Destroy(piece.piece);
            }
        }

        BeforePiece.piece.GetComponent<LOAman>().SetXBoard(move.moveto.x);
        BeforePiece.piece.GetComponent<LOAman>().SetYBoard(move.moveto.y);
        BeforePiece.piece.GetComponent<LOAman>().SetCorods();
        m.UpdatePosition(BeforePiece, move.moveto);
        BeforePiece.piece.GetComponent<LOAman>().DestroyMovePlates();
        m.board.MakeMove(old, move.moveto.x + move.moveto.y * 8 - 1, controller.GetComponent<Game>().GetCurrentPlayer());
        m.board.SetBitBoard(m.board.GetWhites() | m.board.GetBlacks());
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
}
