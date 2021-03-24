using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Profiling;
public class AI
{
    // To do:
    // 1. function to copy the current model into a temporary one -> saving only vector2int for positions V
    // 2. using that new model make moves using the function u already have to find the best move V MADE EVEN BETTER
    // 2.5 first move is easy second move by the other player should lower the score of the first move V
    // 3. orgenize please tom ffs
    // Figure out how to save moves VX
    // Currently depth = 4  ttm -> 66 sec
    //                      w/o eval -> ttm -> 50 sec
    // UNDERSTAND HOW TO LOWER THE RECURSION TIME
    // (KILLER HEURISTIC?)

    // -------------------------------- Variables---------------------------------------

    // The controller for the game
    GameObject controller = GameObject.FindGameObjectWithTag("GameController");
    BitBoard b;
    // What player is the ai (black)
    bool player = false;
    // How deep does the search go? deapth 6 ~ 820,000 moves
    public int searchdepth = 4;
    // Killer heuristic data structure 
    public List<Move>[] KillerMoves;

    //--------------------------------- Core Methods-------------------------------------

    private List<Move> GenerateMoves(List<Move> moves, Model m, int deapth)
    {
        List<Move> sortedkillers = new List<Move>();
        for (int i = 0; i < moves.Count; i++)
        {
            moves[i].score = Evaluate(moves[i], m);
        }
        moves.Sort(CompareTwoMoves);
        return moves;
    }

    // Get a given model (in some point of time) and a move
    // Determine score for the given state of game
    public int Evaluate(Move move, Model m)
    {
        int score = 0;

        // Reward having less pieces then enemy
        score += AmountOfPieces(move, m);

        // Calculate ceter of mass of all ai pieces and reward being close to the middle
        score += StructureEvaluation(move, m);

        // Reward begin away from frame of board
        score += BadSquares(move, m);

        // If winning reward higeset number if losing punihs with lowest number
        score += WinninOrLosing(move, m);

        return score;
        //return player ? -score : score;
    }



    public int pvSearch(Move move, int alpha, int beta, int depth, bool currentplayer, Model m)
    {
        m.MakeMove(move);
        if (depth == searchdepth - 1)
        {
            return Evaluate(move, m);
        }

        currentplayer = !currentplayer;
        Move nextmove = new Move();
        List<Piece> indexer = new List<Piece>();
        foreach (Piece p in m.GetPiecesByBool(player))
        {
            indexer.Add(p);
        }
        for (int i = 0; i < indexer.Count; i++)
        {
            nextmove.pieceToMove = new Piece(indexer[i]);
            nextmove.Child = new List<Move>();
            m.FutureMovesImproved(nextmove);
            foreach (Move after in nextmove.Child)
            {
                var score = -pvSearch(after, -alpha - 1, -alpha, depth + 1, currentplayer, m);
                m.UndoChangePosition(after);
                if (alpha < score && score < beta)
                {
                    score = -pvSearch(after, -beta, -score, depth + 1, currentplayer, m);
                    m.UndoChangePosition(after);
                }
                else
                {
                    score = -pvSearch(after, -beta, -alpha, depth + 1, currentplayer, m);
                    m.UndoChangePosition(after);
                }
                alpha = max(alpha, score);
                if (alpha >= beta)
                {
                    //m.UndoChangePosition(move);
                    return alpha;
                }
            }
        }
        return alpha;
    }

    // This is liquid gold
    // For some reason in random some moves cause me to create insane amount of pieces in the temp module - FIX THIS
    public int RecursionEvaluate(Model m, int depth, Move current, int alpha, int beta, bool currentplayer)
    {
        // Make a move on the model - change bitboard and lists
        m.MakeMove(current);
        // Dont stop until you reach the desired deapth
        // (im subtructing one because one move deapth is preformed before the recurion)
        if (depth == searchdepth - 1)
        {
            return Evaluate(current, m);
        }
        currentplayer = !currentplayer;
        if (currentplayer)
        { beta = 10000;}
        else { alpha = -10000; }
        Move nextmove = new Move();
        List<Piece> indexer = new List<Piece>();
        foreach (Piece p in m.GetPiecesByBool(player))
        {
            indexer.Add(p);
        }
        for (int i = 0; i < indexer.Count; i++)
        {
            nextmove.pieceToMove = new Piece(indexer[i]);
            nextmove.Child = new List<Move>();
            //nextmove.Child.AddRange(KillerMoves[depth]);
            m.FutureMovesImproved(nextmove);
            foreach (Move after in nextmove.Child)
            {
                int score = RecursionEvaluate(m, depth + 1, after, alpha, beta, currentplayer);
                m.UndoChangePosition(after);
                if (player)
                {
                    beta = min(beta, score);
                    if (alpha >= beta)
                    {
                        return beta;
                    }
                }
                else
                {
                    alpha = max(alpha, score);
                    if (alpha >= beta)
                    {
                        return alpha;
                    }
                }
            }
        }
        return player ? beta : alpha;
    }


    public void aimove(Model m)
    {
        // Copy model so i can change it
        Model temp = new Model(m);
        Move bestmove = new Move();
        bestmove.score = int.MinValue;
        // Im using stopwatch to calculate the time spent on methods 
        var StopWatch = System.Diagnostics.Stopwatch.StartNew();
        // Go over all the pieces the ai holds (black)
        foreach (Piece p in m.blacks)
        {
            // Copy the current piece since it will be changed
            Piece save = new Piece(p);
            Move move = new Move();
            move.pieceToMove = p;
            // Get the future possible moves for the current piece
            temp.FutureMovesImproved(move);
            foreach (Move nextmove in move.Child)
            {
                var current = pvSearch(nextmove, -1000, 1000, 0, !player, temp);
                // Find the score given to this move
                //var current = RecursionEvaluate(temp, 0, nextmove, -10000, 10000, !player);
                // Save the move if its better score-wise
                if (bestmove.score < current)
                {
                    bestmove.pieceToMove = save;
                    bestmove = nextmove;
                    bestmove.score = current;
                }
                // copy my model again
                temp = new Model(m);
            }
        }
        StopWatch.Stop();
        var elapsedtime = StopWatch.ElapsedMilliseconds;
        Debug.Log("ai move recursion duration : " + elapsedtime / 1000 + " seconds");
        Debug.Log("Move made from square " + bestmove.pieceToMove.position + " to square " + bestmove.moveto);
        // Make the move with my chosen move
        actuallymove(bestmove);
    }

    // ------------------------------- Evalution Methods -----------------------------------------

    // Get a given model (in some point of time) and a move
    // Reward having fewer pieces than the enemy
    private int AmountOfPieces(Move move, Model m)
    {
        return m.GetPiecesByBool(move.pieceToMove.player).Count < m.GetPiecesByBool(!move.pieceToMove.player).Count ? 3 : -3;
    }

    // Get a given model (in some point of time) and a move
    // Reward begin away from frame of board
    private int BadSquares(Move move, Model m)
    {
        return (move.moveto.x == 0 || move.moveto.y == 0 || move.moveto.x == 7 || move.moveto.y == 7) ? -9 : 0;
    }

    // Get a given model (in some point of time) and a move
    // Evaluate the center of mass of the players pieces and reward being close to middle / punish being far
    // O(2n) FOR n = amount of pieces for a player
    // IDEA READ THIS IN THE FUTURE!
    // MUY IMPORTANTE
    // Currently not effective you could save center of mass on model and change it every move
    private int StructureEvaluation(Move move, Model m)
    {
        // Avg x and y of my pieces
        int Myavgx = 0, Myavgy = 0;

        // X and y of the furthest distance position from my center of mass
        int Enemyavgx = 0, Enemyavgy = 0;

        // The furthest distance from the center of mass 
        double Mymaxdist = -100, EnemyMaxdist = -100;

        // Lists of both kinds of pieces
        List<Piece> Mypieces = m.GetPiecesByBool(move.pieceToMove.player);
        List<Piece> EnemyPieces = m.GetPiecesByBool(!move.pieceToMove.player);

        // Go over all pieces and sum all x positions and y positions (both my pieces and enemys)
        for (int i = 0; i < Mypieces.Count || i < EnemyPieces.Count; i++)
        {
            if (i < Mypieces.Count)
            {
                Myavgx += Mypieces[i].position.x;
                Myavgy += Mypieces[i].position.y;
            }
            if (i < EnemyPieces.Count)
            {
                Enemyavgx += EnemyPieces[i].position.x;
                Enemyavgy += EnemyPieces[i].position.y;

            }
        }

        // Calculate average 
        Myavgx /= m.GetPiecesByBool(move.pieceToMove.player).Count;
        Myavgy /= m.GetPiecesByBool(move.pieceToMove.player).Count;
        Enemyavgx /= m.GetPiecesByBool(!move.pieceToMove.player).Count;
        Enemyavgy /= m.GetPiecesByBool(!move.pieceToMove.player).Count;

        // Go over all pieces and calculate distance of all x positions and y positions
        // Find and save the furthest distance
        for (int i = 0; i < Mypieces.Count || i < EnemyPieces.Count; i++)
        {
            if (i < Mypieces.Count)
            {
                double distance = Math.Abs(CalcDistance(Math.Abs(Mypieces[i].position.x - Myavgx), Math.Abs(Mypieces[i].position.y - Myavgy)));
                if (distance > Mymaxdist)
                {
                    Mymaxdist = distance;
                }
            }
            if (i < EnemyPieces.Count)
            {
                double distance = Math.Abs(CalcDistance(Math.Abs(EnemyPieces[i].position.x - Enemyavgx), Math.Abs(EnemyPieces[i].position.y - Enemyavgy)));
                if (distance > EnemyMaxdist)
                {
                    EnemyMaxdist = distance;
                }

            }
        }

        // Reward being close to the middle and reward having a closer formation (my max avg distance is lower then the enemeys)
        return MiddleSquares(Myavgx, Myavgy) + EnemyMaxdist < Mymaxdist ? -7 : 7;
    }

    // Utilty function
    // Get the avg distance of a certain player (x, y) position of avg
    // Evaluate distance from the middle of the board
    private int MiddleSquares(int avgx, int avgy)
    {
        // Punish being far from the middle (squares (3,4) || (4,3) || (3,3) || (4,4))
        int three = Math.Abs(avgx - 3) + Math.Abs(avgy - 3);
        int four = Math.Abs(avgx - 4) + Math.Abs(avgy - 4);
        int threefour = Math.Abs(avgx - 3) + Math.Abs(avgy - 4);
        int fourthree = Math.Abs(avgx - 4) + Math.Abs(avgy - 3);

        // Being far from the middle lowers ur score by how far u are (im choosing the closest option)
        return -min(min(three, four), min(threefour, fourthree));
    }

    // Get a given model (in some point of time) and a move
    // Check if winning or losinng, and return appropriate number
    private int WinninOrLosing(Move move, Model m)
    {
        if (m.checkwin(move.pieceToMove.player))
            return 10000;
        if (m.checkwin(!move.pieceToMove.player))
            return -10000;
        return 0;
    }

    // -------------------------------- Utility Methods ------------------------------------------


    // Get a move
    // Actually make the move in the unity space
    private void actuallymove(Move move)
    {

        controller.GetComponent<Game>().MoveAPieceInUnity(move);
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

    // Get and x and y index
    // Return distance 
    private double CalcDistance(int x, int y)
    {
        return Math.Sqrt(x ^ 2 + y ^ 2);
    }










    //-------------------------- Past Versions (graveyard)----------------------------

    // Ai is always black (for now)
    // make this better - dont have to count all pieces since each turn only 1 moves (6 lines change)
    //public void GetPossibleMoves(Vector2Int dir, Vector2Int now, Move m, Model model)
    //{
    //    Vector2Int pos = now;
    //    pos.x += dir.x;
    //    pos.y += dir.y;
    //    b = model.board;
    //    Vector2Int anti = new Vector2Int(now.x - dir.x, now.y - dir.y);
    //    Vector2Int normalenemy = new Vector2Int(8, 8);
    //    Vector2Int antienemy = new Vector2Int(8, 8);
    //    bool flag1 = true, flag2 = true;

    //    int counter = 1;
    //    while (model.IsOnBoard(pos.x, pos.y) || model.IsOnBoard(anti.x, anti.y))
    //    {
    //        if (CheckGood(pos, model))
    //        {
    //            if (model.board.IsEnemy(pos, player) && flag1)
    //            {
    //                normalenemy.x = pos.x;
    //                normalenemy.y = pos.y;
    //                flag1 = false;
    //            }
    //            counter++;
    //        }
    //        if (CheckGood(anti, model))
    //        {
    //            if (model.board.IsEnemy(anti, player) && flag2)
    //            {
    //                antienemy.x = anti.x;
    //                antienemy.y = anti.y;
    //                flag2 = false;
    //            }
    //            counter++;
    //        }

    //        pos.x += dir.x;
    //        pos.y += dir.y;
    //        anti.x -= dir.x;
    //        anti.y -= dir.y;
    //    }
    //    CreatePossiblesBoard(model, new Vector2Int(now.x + (counter * dir.x), now.y + (counter * dir.y)), normalenemy, flag1, now, m);
    //    CreatePossiblesBoard(model, new Vector2Int(now.x + (counter * -dir.x), now.y + (counter * -dir.y)), antienemy, flag2, now, m);
    //}

    //private bool IsBefore(Vector2Int now, int x, int y, int enemyx, int enemyy)
    //{
    //    int dist1x = x - now.x, dist2x = enemyx - now.x, dist1y = y - now.y, dist2y = enemyy - now.y;
    //    return Math.Abs(dist1x) <= Math.Abs(dist2x) && Math.Abs(dist1y) <= Math.Abs(dist2y);
    //}

    // Add possible moves to somewhere
    // Given a move, find the best move in it (child list)
    //public Move FindBestMove(Move somehthing, Model model)
    //{
    //    int max = -1000;
    //    int score = 0;
    //    Move best = new Move();
    //    foreach (Move m in somehthing.Child)
    //    {
    //        score = evaluate(m, model);
    //        if (score > max)
    //        {
    //            best = m;
    //            best.score = max;
    //        }
    //    }
    //    return best;
    //}
    //public void CreatePossiblesBoard(Model m, Vector2Int pos, Vector2Int enemy, bool flag, Vector2Int p, Move move)
    //{

    //    if (m.IsOnBoard(pos.x, pos.y) && (IsBefore(p, pos.x, pos.y, enemy.x, enemy.y) || flag))
    //    {
    //        if (!m.board.IsPieceHere(pos))
    //        {
    //            move.Child.Add(new Move(move.pieceToMove, pos, 0, false));
    //        }
    //        else if (m.board.IsEnemy(pos, player))
    //        {
    //            move.Child.Add(new Move(move.pieceToMove, pos, 0, true));
    //        }
    //    }
    //}
    //public void getAllDirections(Vector2Int now, Move m, Model model)
    //{
    //    // Initiate all directions according to amound of pieces in the direction
    //    GetPossibleMoves(new Vector2Int(1, 0), now, m, model);
    //    GetPossibleMoves(new Vector2Int(0, 1), now, m, model);
    //    GetPossibleMoves(new Vector2Int(1, 1), now, m, model);
    //    GetPossibleMoves(new Vector2Int(-1, 1), now, m, model);
    //}

    // get vector for position and check of its legit for moves
    //public bool CheckGood(Vector2Int v, Model m)
    //{
    //    if (m.IsOnBoard(v.x, v.y))
    //    {
    //        if (m.board.IsPieceHere(v))
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    //if (!KillerMoves[depth].Contains(after))
    //{
    //    KillerMoves[depth].Add(new Move(after));
    //}
    //else
    //{
    //    after.score += 2;
    //}
    //if (KillerMoves[depth].Contains(after))
    //{
    //    KillerMoves[depth].Remove(after);
    //    after.score -= 2;
    //}
    //else 
    //{
    //    KillerMoves[depth].Add(new Move(after));
    //}

    //// Get 2 moves
    //// Return the subtruction of their scores
    //public static int CompareTwoMoves(Move a, Move b) 
    //{
    //    return a.score - b.score;
    //}
    //// Trying to improve my ai move method 
    //public void BetterAiMove(Model m)
    //{
    //    Model temp = new Model(m);
    //    Move bestmove = new Move(), IteratorMove = new Move();
    //    bestmove.score = -10000;

    //    var StopWatch = System.Diagnostics.Stopwatch.StartNew();
    //    List<Move> moves = GenerateAllMoves(false, IteratorMove, temp);
    //    foreach (Move Node in moves)
    //    {
    //        Move save = new Move(Node);
    //        var current = pvSearch(Node, -10000, 10000, 0, false, temp);
    //        // Save the move if its better score-wise
    //        if (bestmove.score < current)
    //        {
    //            bestmove = new Move(save);
    //            bestmove.score = current;
    //        }
    //        temp = new Model();
    //    }
    //    StopWatch.Stop();
    //    var elapsedtime = StopWatch.ElapsedMilliseconds;
    //    Debug.Log("ai move recursion duration : " + elapsedtime / 1000 + " seconds");
    //    Debug.Log("Move made from square " + bestmove.pieceToMove.position + " to square " + bestmove.moveto);
    //    // Make the move with my chosen move
    //    actuallymove(bestmove);
    //}

    //// Get a player, Get a move and a model
    //// Add every possible move to the child of the given move
    //private List<Move> GenerateAllMoves(bool player, Move move, Model m)
    //{
    //    List<Move> moves = new List<Move>();
    //    move.Child = new List<Move>();
    //    move.Child = moves;
    //    Move temp = new Move();
    //    foreach (Piece p in m.GetPiecesByBool(player))
    //    {
    //        temp = new Move();
    //        temp.pieceToMove = new Piece(p);
    //        m.FutureMovesImproved(temp);
    //        move.Child.AddRange(temp.Child);
    //    }
    //    return move.Child;
    //}





}
