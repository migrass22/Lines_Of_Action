using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Profiling;
public class AI
{
    /* To do:
    // 1. function to copy the current model into a temporary one -> saving only vector2int for positions V
    // 2. using that new model make moves using the function u already have to find the best move V MADE EVEN BETTER
    // 2.5 first move is easy second move by the other player should lower the score of the first move V
    // 3. orgenize please tom ffs
    // Figure out how to save moves VX

    // Currently depth = 4  ttm -> 66 sec
    //                      w/o eval -> ttm -> 50 sec
    // UPGRADE GOT TO DEPTH 

    // UNDERSTAND HOW TO LOWER THE RECURSION TIME
    // (KILLER HEURISTIC?)

    // -------------------------------- Variables---------------------------------------

    // The controller for the game*/

    //// Trying out transposition table
    //Hashtable transposition = new Hashtable();

    // Just the model of the game
    Model mainModel;

    // Controller for the game
    GameObject controller = GameObject.FindGameObjectWithTag("GameController");

    // Search depth for ai
    private int searchDepth = 4;

    // Qs search depth
    private int qsDepth = 1;

    // Constructor for the ai
    public AI(Model m)
    {
        this.mainModel = new Model(m);
    }

    // Base function to start the ai and make a move
    public void StartAi()
    {
        // Variable to save the best move made
        Move bestMove = new Move();

        // Current score of the move
        int current = 0;

        // List of all possible moves for the current player (black)
        List<Move> possibleMoves = mainModel.GenerateAllMoves(false);

        // Trying out move ordering
        SimpleMoveOrdering(possibleMoves, mainModel);

        // Count preformence of the ai
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Temporary model to play on with the ai
        Model temp = new Model(mainModel);

        foreach (Move move in possibleMoves)
        {
            // make the move on the model
            temp.MakeMove(move);
            // send recursion to evaluate final node
            current = pvSearch(move, -1000, 1000, 0, false, temp);
            // undo the move
            temp.Undomove(move);

            // If the current move is better then the best one yet than save it
            if (current > bestMove.score)
            {
                bestMove = new Move(move);
            }
        }
        stopwatch.Stop();
        // Document for future improvment
        Debug.Log("Time spent deciding the move - " + stopwatch.ElapsedMilliseconds / 1000 + " seconds" + "\n Current depth - " + searchDepth);
        Debug.Log("Move made - " + bestMove);

        // Actually make a move 
        actuallymove(bestMove);
    }



    // --------------------------------------------- recursions ------------------------------------------------------------------------------


    // Try #4 on the recursion (just testing now) -> this is the new liquid gold (this is diamond)
    private int BaseReucrsion(int depth, bool currentplayer, Model m)
    {
        // If im at a leaf node or game has ended go back
        if (depth == searchDepth - 1 || m.checkwin(currentplayer))
        {
            return 1;
        }
        int current = 0;

        // Switch to other player
        currentplayer = !currentplayer;

        // Generate all possible moves
        List<Move> moves = m.GenerateAllMoves(currentplayer);

        // Go over all the moves generated
        foreach (Move nextmove in moves)
        {
            // Make possible move
            m.MakeMove(nextmove);
            // Continue down the search tree
            current = BaseReucrsion(depth + 1, currentplayer, m);
            // Undo move made
            m.Undomove(nextmove);
        }
        return current;
    }

    // Try #2 negascout
    private int pvSearch(Move move, int alpha, int beta, int depth, bool currentplayer, Model m)
    {

        // If im at a leaf node or game has ended go back
        if (depth == searchDepth - 1 || m.checkwin(currentplayer))
        {
            return Evaluate(move, m);
        }
        int current = 0;

        // Switch to other player
        currentplayer = !currentplayer;

        // Generate all possible moves
        List<Move> moves = m.GenerateAllMoves(currentplayer);

        // Implementing move ordering
        SimpleMoveOrdering(moves, m);

        // Go over all the moves generated
        foreach (Move nextmove in moves)
        {
            if (depth == searchDepth - 1 && nextmove.attack)
            {
                current = QuietSearch(nextmove, 0, m);
            }
            else 
            {
                // Make possible move
                m.MakeMove(nextmove);
                // Continue down the search tree
                current = -pvSearch(nextmove, -alpha - 1, -alpha, depth + 1, currentplayer, m);
                // Undo move made
                m.Undomove(nextmove);
                // if the current score is 
                if (alpha < current && current < beta)
                {
                    m.MakeMove(nextmove);
                    current = -pvSearch(nextmove, -beta, -current, depth + 1, currentplayer, m);
                    m.Undomove(nextmove);
                }
                else
                {
                    m.MakeMove(nextmove);
                    current = -pvSearch(nextmove, -beta, -alpha, depth + 1, currentplayer, m);
                    m.Undomove(nextmove);
                }
            }



            alpha = max(alpha, current);
            if (alpha >= beta)
            {
                return alpha;
            }
        }
        return alpha;

    }

    // Trying out Quiescence search
    private int QuietSearch(Move move, int depth, Model m) 
    {
        if (move.attack || depth == qsDepth || m.checkwin(move.pieceToMove.player)) 
        {
            return Evaluate(move, m);
        }
        Model temp = new Model(m);
        return pvSearch(move, -1000, 1000, depth, move.pieceToMove.player, temp);
    }

    // fix this
    // For some reason in random some moves cause me to create insane amount of pieces in the temp module - FIX THIS
    public int RecursionEvaluate(Model m, int depth, Move current, int alpha, int beta, bool currentplayer)
    {
        // Make a move on the model - change bitboard and lists
        //m.MakeMove(current, depth);
        // Dont stop until you reach the desired deapth
        // (im subtructing one because one move deapth is preformed before the recurion)
        if (depth == searchDepth - 1)
        {
            return Evaluate(current, m);
        }
        //lastmove = current;
        currentplayer = !currentplayer;
        if (currentplayer)
        { beta = 10000; }
        else { alpha = -10000; }
        Move nextmove = new Move();
        List<Piece> indexer = new List<Piece>();
        foreach (Piece p in m.GetPiecesByBool(currentplayer))
        {
            indexer.Add(p);
        }
        for (int i = 0; i < indexer.Count; i++)
        {
            nextmove.pieceToMove = new Piece(indexer[i]);
            nextmove.Child = new List<Move>();
            //nextmove.Child.AddRange(KillerMoves[depth]);
            //m.FutureMovesImproved(nextmove);
            foreach (Move after in nextmove.Child)
            {
                int score = RecursionEvaluate(m, depth + 1, after, alpha, beta, currentplayer);
                m.UndoChangePosition(after);
                if (currentplayer)
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
        return currentplayer ? beta : alpha;
    }


    // ------------------------------- Evalution Methods -----------------------------------------


    // Get a given model (in some point of time) and a move
    // Determine score for the given state of game
    public int Evaluate(Move move, Model m)
    {
        int score = 0;

        // Average position of all the pieces with this color
        Vector2Int avgpos = AvgPos(move.pieceToMove.player, m);

        // Reward having less pieces then enemy
        score += AmountOfPieces(move, m);

        // Calculate ceter of mass of all ai pieces and reward being close to the middle
        score += MiddleSquares(avgpos);

        //// Punish backtracking moves
        //score += MovePlayed(move);

        // Trying out new things
        score += SimpleStructureEval(avgpos, move, m);

        //// Calculate the structure of my pieces
        //score += StructureEvaluation(move, avgpos, m);

        // Reward begin away from frame of board
        score += BadSquares(move, m);

        // If winning reward higeset number if losing punihs with lowest number
        score += WinninOrLosing(move, m);

        return score;
        //return player ? -score : score;
    }

    // Get a move list 
    // Give every move a score and sort them
    public void SimpleMoveOrdering(List<Move> moves, Model m)
    {
        foreach (Move move in moves)
        {
            move.score = MiddleSquares(move.moveto) + AmountOfPieces(move, m) + BadSquares(move, m);
        }
        moves.Sort(delegate (Move p1, Move p2)
        {
            int compareScore = p1.score.CompareTo(p2.score);
            return compareScore;
        });
    }

    /*    // Get a move
        // If move was last played punish ai
        private int MovePlayed(Move move)
        {
            Move lastmove = move.pieceToMove.player ? LastWhiteMove : LastBlackMove;
            return ((move.moveto == lastmove.pieceToMove.position) && (move.pieceToMove.position == lastmove.moveto)) ? -200 : 0;
        }*/


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
        return (move.moveto.x == 0 || move.moveto.y == 0 || move.moveto.x == 7 || move.moveto.y == 7) ? -10 : 0;
    }

    private int SimpleStructureEval(Vector2Int avg, Move move, Model m)
    {
        return Math.Abs(CalcDistance(move.pieceToMove.position) - CalcDistance(avg)) * -2;
    }

    // Get a given model (in some point of time) and a move
    // Evaluate the center of mass of the player's pieces
    private int StructureEvaluation(Move move, Vector2Int avgpos, Model m)
    {

        // The furthest distance from the center of mass 
        double Mymaxdist = -100;
        // All the pieces of a certain color
        List<Piece> Mypieces = m.GetPiecesByBool(move.pieceToMove.player);

        // Go over all pieces and calculate distance of all x positions and y positions
        // Find and save the furthest distance
        for (int i = 0; i < Mypieces.Count; i++)
        {
            // Calculate distance of a piece from the average position
            double distance = CalcDistance(Math.Abs(Mypieces[i].position.x - avgpos.x), Math.Abs(Mypieces[i].position.y - avgpos.y));
            // Update if found a bigger distance
            if (distance > Mymaxdist)
            {
                Mymaxdist = distance;
            }
        }

        // Reward being close to the middle and reward having a closer formation (my max avg distance is lower then the enemeys)
        return -(int)Mymaxdist;
    }


    // Get the avg position of a certain player
    // Evaluate distance from the middle of the board
    private int MiddleSquares(Vector2Int pos)
    {
        // Punish being far from the middle (squares (3,4) || (4,3) || (3,3) || (4,4))
        int three = CalcDistance(Math.Abs(pos.x - 3), Math.Abs(pos.y - 3));
        int four = CalcDistance(Math.Abs(pos.x - 4), Math.Abs(pos.y - 4));
        int threefour = CalcDistance(Math.Abs(pos.x - 3), Math.Abs(pos.y - 4));
        int fourthree = CalcDistance(Math.Abs(pos.x - 4), Math.Abs(pos.y - 3));

        // Being far from the middle lowers ur score by how far u are (im choosing the closest option)
        return max(max(three, four), max(threefour, fourthree) * -4);
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
    private int CalcDistance(int x, int y)
    {
        return (int)Math.Sqrt(x ^ 2 + y ^ 2);
    }

    // Just the same function only with a vector
    private int CalcDistance(Vector2Int pos)
    {
        return (int)Math.Sqrt(pos.x ^ 2 + pos.y ^ 2);
    }

    // Get a model
    // Return the avg position for all the pieces of a certain color
    private Vector2Int AvgPos(bool Myplayer, Model m)
    {
        // Avg x and y of my pieces
        int Myavgx = 0, Myavgy = 0;
        // Lists of both kinds of pieces
        List<Piece> Mypieces = m.GetPiecesByBool(Myplayer);
        List<Piece> EnemyPieces = m.GetPiecesByBool(!Myplayer);

        // Go over all pieces and sum all x positions and y positions (both my pieces and enemys)
        for (int i = 0; i < Mypieces.Count || i < EnemyPieces.Count; i++)
        {
            if (i < Mypieces.Count)
            {
                Myavgx += Mypieces[i].position.x;
                Myavgy += Mypieces[i].position.y;
            }
        }

        // Calculate average 
        Myavgx /= m.GetPiecesByBool(Myplayer).Count;
        Myavgy /= m.GetPiecesByBool(Myplayer).Count;
        return new Vector2Int(Myavgx, Myavgy);
    }

    // Get a move
    // Actually make the move in the unity space
    private void actuallymove(Move move)
    {
        controller.GetComponent<Game>().MoveAPieceInUnity(move);
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

    //private List<Move> GenerateMoves(List<Move> moves, Model m, int deapth)
    //{
    //    List<Move> sortedkillers = new List<Move>();
    //    for (int i = 0; i < moves.Count; i++)
    //    {
    //        moves[i].score = Evaluate(moves[i], m);
    //    }
    //    moves.Sort(CompareTwoMoves);
    //    return moves;
    //}
    //// Get a list of pieces and the model
    //// Return a list of all the pieces of the current player
    //private void AddAllPieces(List<Piece> indexer, Model m, bool currentplayer)
    //{
    //    foreach (Piece p in m.GetPiecesByBool(currentplayer))
    //    {
    //        indexer.Add(p);
    //    }
    //}

    // Get a model
    // Make an initial search and sent out the actual recursion
    //public void aimove(Model m)
    //{
    //    // Copy model so i can change it
    //    Model temp = new Model(m);
    //    Move bestmove = new Move();
    //    bestmove.score = int.MinValue;
    //    // Im using stopwatch to calculate the time spent on methods 
    //    var StopWatch = System.Diagnostics.Stopwatch.StartNew();
    //    // Go over all the pieces the ai holds (black)
    //    foreach (Piece p in m.blacks)
    //    {
    //        // Copy the current piece since it will be changed
    //        Piece save = new Piece(p);
    //        Move move = new Move();
    //        move.pieceToMove = new Piece(p);
    //        // Get the future possible moves for the current piece
    //        //temp.FutureMovesImproved(move);
    //        foreach (Move nextmove in move.Child)
    //        {
    //            temp.MakeMove(nextmove, 0);
    //            //var current = pvSearch(nextmove, -1000, 1000, 0, false, temp);
    //            var current = RecursionTest(0, false, temp);

    //            // Find the score given to this move
    //            //var current = RecursionEvaluate(temp, 0, nextmove, -10000, 10000, !player);
    //            temp.UndoChangePosition(nextmove);
    //            // Save the move if its better score-wise
    //            if (bestmove.score < current)
    //            {
    //                bestmove.pieceToMove = save;
    //                bestmove = nextmove;
    //                bestmove.score = current;
    //            }
    //            // copy my model again
    //            temp = new Model(m);
    //        }
    //    }
    //    StopWatch.Stop();
    //    var elapsedtime = StopWatch.ElapsedMilliseconds;
    //    Debug.Log("ai move recursion duration : " + elapsedtime / 1000 + " seconds");
    //    Debug.Log("Move made from square " + bestmove.pieceToMove.position + " to square " + bestmove.moveto);
    //    // Make the move with my chosen move
    //    actuallymove(bestmove);
    //}

    // Better Recursion to find best score using nega scout
    //public int pvSearch(Move move, int alpha, int beta, int depth, bool currentplayer, Model m)
    //{
    //    m.MakeMove(move, depth);
    //    if (depth == searchdepth - 1 || m.checkwin(move.pieceToMove.player))
    //    {
    //        return Evaluate(move, m);
    //    }
    //    if (currentplayer)
    //    {
    //        LastWhiteMove = move;
    //    }
    //    else
    //    {
    //        LastBlackMove = move;
    //    }
    //    currentplayer = !currentplayer;
    //    Move nextmove = new Move();
    //    List<Piece> indexer = new List<Piece>();
    //    foreach (Piece p in m.GetPiecesByBool(currentplayer))
    //    {
    //        indexer.Add(p);
    //    }
    //    for (int i = 0; i < indexer.Count; i++)
    //    {
    //        nextmove.pieceToMove = new Piece(indexer[i]);
    //        nextmove.Child = new List<Move>();
    //       // m.FutureMovesImproved(nextmove);
    //        foreach (Move after in nextmove.Child)
    //        {
    //            var score = -pvSearch(after, -alpha - 1, -alpha, depth + 1, currentplayer, m);
    //            m.UndoChangePosition(after);
    //            if (alpha < score && score < beta)
    //            {
    //                score = -pvSearch(after, -beta, -score, depth + 1, currentplayer, m);
    //                m.UndoChangePosition(after);
    //            }
    //            else
    //            {
    //                score = -pvSearch(after, -beta, -alpha, depth + 1, currentplayer, m);
    //                m.UndoChangePosition(after);
    //            }
    //            alpha = max(alpha, score);
    //            if (alpha >= beta)
    //            {
    //                return alpha;
    //            }
    //        }
    //    }
    //    return alpha;
    //}
}
