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
    bool player = false;
    int searchdepth = 1;

    // Copy a model to a second model
    public void CopyModel(Model m, Model copy)
    {
        copy.InitModel();
        foreach (Piece p in m.whites)
        {
            copy.whites.Add(new Piece(p.position, p.player));
        }
        foreach (Piece p in m.blacks)
        {
            copy.blacks.Add(new Piece(p.position, p.player));
        }
        m.board.copyboard(copy.board); 
    }

    // Trying to improve the move generation
    
    // Method to change amount of pieces in move arrays
    // After a move is made
    public void UpdateArrayNumbers(Vector2Int start, Vector2Int end, Vector2Int dir, bool attack, Model m) 
    {
        // Reapet everything below this for every other direction
        // Check if the move was made in a row
        if (dir == new Vector2Int(1, 0) || dir == new Vector2Int(-1, 0))
        {
            if (attack)
            {
                // Only if a piece was eaten then the amount of pieces goes down
                m.row[end.x]--;
            }
            // Change every array if the direction is a row
            m.col[start.y]--;
            m.col[end.y]++;

            m.pdiagonal[start.x - start.y + 7]--;
            m.pdiagonal[end.x - end.y + 7]++;

            m.sdiagonal[start.y - start.x + 7]--;
            m.sdiagonal[end.y - end.x + 7]++;
        }
        // Check if the move is made in a col
        else if (dir == new Vector2Int(0, -1) || dir == new Vector2Int(0, 1))
        {
            if (attack)
            {
                m.col[end.y]--;
            }

            m.row[start.x]--;
            m.row[end.x]++;

            m.pdiagonal[start.x - start.y + 7]--;
            m.pdiagonal[end.x - end.y + 7]++;

            m.sdiagonal[start.y - start.x + 7]--;
            m.sdiagonal[end.y - end.x + 7]++;
        }
        // Check if the move is a primary diagonal
        else if (dir == new Vector2Int(1, 1) || dir == new Vector2Int(-1, -1))
        {
            if (attack)
            {
                m.pdiagonal[end.x - end.y + 7]--;
            }

            m.row[start.x]--;
            m.row[end.x]++;

            m.col[start.y]--;
            m.col[end.y]++;

            m.sdiagonal[start.y - start.x + 7]--;
            m.sdiagonal[end.y - end.x + 7]++;
        }
        // Check if the move is a secondary diagonal 
        else if (dir == new Vector2Int(1, -1) || dir == new Vector2Int(-1, 1)) 
        {
            if (attack)
            {
                m.sdiagonal[end.y - end.x + 7]--;
            }

            m.row[start.x]--;
            m.row[end.x]++;

            m.col[start.y]--;
            m.col[end.y]++;

            m.pdiagonal[start.x - start.y + 7]--;
            m.pdiagonal[end.x - end.y + 7]++;
        }
    }

    // Get a piece and return where it can go to using move arrays
    public void PossibleMovesImproved(Piece p, Model m)
    {
        // y position is amount of pieces in this num of col
        // x position is number of pieces in this num of row
        int colmove = m.col[p.position.y];
        int rowmove = m.row[p.position.x];
        // Turn a position to the index of correct diagonal
        int pdiagmove = m.pdiagonal[p.position.x - p.position.y + 7];
        int sdiagmove = m.sdiagonal[p.position.y - p.position.x + 7];

        // Check for col moves of piece
        OneLineMoves(p, new Vector2Int(p.position.x, p.position.y + colmove), new Vector2Int(0, 1), m);
        OneLineMoves(p, new Vector2Int(p.position.x, p.position.y - colmove), new Vector2Int(0, -1), m);

        // Check for row moves of piece
        OneLineMoves(p, new Vector2Int(p.position.x + rowmove, p.position.y), new Vector2Int(1, 0), m);
        OneLineMoves(p, new Vector2Int(p.position.x - rowmove, p.position.y), new Vector2Int(-1, 0), m);

        // Check for the primary diagonal of the piece
        OneLineMoves(p, new Vector2Int(p.position.x + pdiagmove, p.position.y + pdiagmove), new Vector2Int(1, 1), m);
        OneLineMoves(p, new Vector2Int(p.position.x - pdiagmove, p.position.y - pdiagmove), new Vector2Int(-1, -1), m);

        // Check for the secondery diagonal of the piece
        OneLineMoves(p, new Vector2Int(p.position.x + sdiagmove, p.position.y - sdiagmove), new Vector2Int(1, -1), m);
        OneLineMoves(p, new Vector2Int(p.position.x - sdiagmove, p.position.y + sdiagmove), new Vector2Int(-1, 1), m);
    }

    // Get a piece, an endpoint and a direction
    // Add a new move to the pieces possible moves if said move is possible
    public void OneLineMoves(Piece p, Vector2Int endPoint, Vector2Int dir, Model m)
    {
        // Check for the column of this piece
        // End point is on the board?
        if (m.IsOnBoard(endPoint.x, endPoint.y))
        {
            // Are there enemy pieces i jump over?
            if (!m.board.IsEnemyBeforeHere(p.position, endPoint, dir, player))
            {
                // Is there a piece at the end?
                if (m.board.IsPieceHere(endPoint))
                {
                    // Is this piece an enemy Piece?
                    if (m.board.IsEnemy(p.position, player))
                    {
                        // Create a new attack move at this point, save on the piece,
                        p.possibles[p.amountOfMoves++] = new Move(p, endPoint, 0, true);
                    }
                }
                else
                {
                    // No piece at end point -> create a new normal move
                    p.possibles[p.amountOfMoves++] = new Move(p, endPoint, 0, false);
                }
            }
        }
    }

    // Ai is always black
    // make this better - dont have to count all pieces since each turn only 1 moves (6 lines change)
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
            if (!m.board.IsPieceHere(pos))
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
            if (m.board.IsPieceHere(v))
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
        if ((move.moveto.x == 4 && move.moveto.y == 3) || (move.moveto.y == 4 && move.moveto.x == 3))
        {
            score += 6;
        }
        else if (move.moveto.x == 0 || move.moveto.y == 0)
        {
            score -= 4;
        }
        if (m.checkwin(false))
        {
            score += int.MaxValue;
        }
        else 
        {
            score += int.MinValue;
        }
        return score;
    }

    public void actuallymove(Model m, Move move)
    {
        Piece p = m.GetPieceByIndex(move.pieceToMove.position.x, move.pieceToMove.position.y);
        //Vector2Int before = new Vector2Int(p.piece.GetComponent<LOAman>().GetXBoard(),
        //    p.piece.GetComponent<LOAman>().GetYBoard());
        Vector2Int before = p.position;
        controller = GameObject.FindGameObjectWithTag("GameController");
        Piece BeforePiece = m.GetPieceByIndex(before.x, before.y);
        Piece piece = m.GetPieceByIndex(move.moveto.x, move.moveto.y);
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

    public int RecursionMove(Model m, int depth, Move current, int alpha, int beta)
    {
        if (depth == searchdepth)
        {
            return evaluate(current, m);
        }
        // Make a move on the model - change bitboard and lists
        m.ChangePiecePosition(current);
        player = !player;
        Move nextmove = new Move();
        List<Piece> indexer=new List<Piece>();
        indexer.AddRange(m.GetPiecesByBool(player));
        for (int i = 0; i < indexer.Count; i++)
        {
            Piece p = indexer[i];
            nextmove.pieceToMove = p;
            nextmove.Child = new List<Move>();
            getAllDirections(p.position, nextmove, m);
            foreach (Move after in nextmove.Child)
            {

                if (player)
                {
                    beta = min(beta, RecursionMove(m, depth + 1, after, alpha, beta));
                    if (beta <= alpha)
                    {
                        return beta;
                    }
                }
                else 
                {
                    alpha = max(alpha, RecursionMove(m, depth + 1, after, alpha, beta));

                    if (alpha >= beta)
                    {
                        return alpha;
                    }
                }
                m.UndoChangePosition(current);
            }
        }
        return player ? beta : alpha;
    }

    // Get 2 numbers and return the smaller one
    private int min(int a, int b) 
    {
        return a > b ? b : a;
    }
    
    // Get 2 numbers and return the bigger one 
    private int max(int a, int b)
    {
        return a < b ? b : a;
    }
    public void aimove(Model m)
    {
        Model temp = new Model();
        CopyModel(m, temp);
        Move move = new Move();
        Move bestmove = new Move();
        bestmove.score = int.MinValue;
        int current = int.MinValue;
        int counter = 0;
        foreach (Piece p in m.GetPiecesByBool(false)) 
        {
            move.pieceToMove = p;
            getAllDirections(p.position, move, m);
            counter += 1;
            foreach (Move nextmove in move.Child) 
            {
                current = RecursionMove(temp, 0, nextmove, int.MinValue, int.MaxValue);
                if (bestmove.score < current) 
                {
                    bestmove = nextmove;
                    bestmove.score = current;
                }
            }
            move.Child = new List<Move>();
            CopyModel(m, temp);
        }
        actuallymove(m, bestmove);
    }


}
