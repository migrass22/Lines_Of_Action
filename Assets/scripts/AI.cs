using System;
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
    Model temp;
    List<Piece> possiblemove = new List<Piece>();
    bool player = false;

    public Model CopyModel(Model m) 
    {
        Model mnew = new Model();
        mnew.InitModel();
        foreach (Piece p in m.whites) 
        {
            mnew.whites.Add(new Piece(p.position, p.player));
        }
        foreach (Piece p in m.blacks)
        {
            mnew.blacks.Add(new Piece(p.position, p.player));
        }
        mnew.board = m.board;
        return mnew;
    }


    // Ai is always black
    public void GetPossibleMoves(Vector2Int dir, Vector2Int now) 
    {
        Vector2Int pos = now;
        pos.x += dir.x;
        pos.y += dir.y;
        Model model = temp;
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
        CreatePossiblesBoard(model, new Vector2Int(now.x + (counter * dir.x), now.y + (counter * dir.y)), normalenemy, flag1, now);
        CreatePossiblesBoard(model, new Vector2Int(now.x + (counter * -dir.x), now.y + (counter * -dir.y)), antienemy, flag2, now);
    }

    private bool IsBefore(Vector2Int now,int x, int y, int enemyx, int enemyy)
    {
        int dist1x = x - now.x, dist2x = enemyx - now.x, dist1y = y - now.y, dist2y = enemyy - now.y;
        return Math.Abs(dist1x) <= Math.Abs(dist2x) && Math.Abs(dist1y) <= Math.Abs(dist2y);
    }
    public void CreatePossiblesBoard(Model m, Vector2Int pos, Vector2Int enemy, bool flag,Vector2Int p) 
    {

        if (m.IsOnBoard(pos.x, pos.y) && (IsBefore(p, pos.x, pos.y, enemy.x, enemy.y) || flag))
        {
            if (!m.board.IsPieceHere(pos.x, pos.y))
            {
                possiblemove.Add(new Piece(pos, false));
            }
            else if (m.board.IsEnemy(pos, player))
            {
                possiblemove.Add(new Piece(pos, true));
            }
        }
    }

    public void getAllDirections(Vector2Int now)
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
        temp = CopyModel(m);

        Move best = RecursionMove(temp, 2, new Move()) ;


        actuallymove(m, best);
    }

    public Move FindBestMove(Model m) 
    {
        int max = -1000, temp;
        Piece bestPiece = null;
        Piece move = null;
        foreach (Piece p in m.GetPiecesByBool(player)) 
        {
            getAllDirections(p.position);
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
        return new Move(bestPiece, move.position, max, move.player);
    }

    // Will call findbestmove, make moves and undo them
    public Move RecursionMove(Model m, int depth, Move current)
    {
        Move best = FindBestMove(m);
        bool nowplayer = player;
        int start = best.pieceToMove.position.x + best.pieceToMove.position.y * 8 - 1;
        int end = best.moveto.x + best.moveto.y * 8 - 1;
        m.board.MakeMove(start, end, player);
        // add method to change the list of pieces
        m.ChangePiecePosition(best.pieceToMove.position, best.moveto, nowplayer);


        player = !player;
        if (depth == 0)
        {
            current.score += addscore(best.score, nowplayer);
            return current;
        }
        Move two = RecursionMove(m, depth - 1, best);
        current.score += addscore(two.score, nowplayer);
        return current;
    }

    private int addscore(int score, bool nowplayer) 
    {
        if (nowplayer)
        {
            return -score;
        }
        else 
        {
            return score;
        }
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
