using System;
using System.Collections.Generic;
using UnityEngine;

public class AI
{

    // -------------------------------- Variables---------------------------------------

    // Trying out transposition table
    Dictionary<string, CachedData> table;


    // Enum for different types of ai's
    public enum AItypes
    {
        NegaScout, NegaMax, DebugMode
    }   

    private AItypes type = AItypes.DebugMode;


    // Data structure to save sizes of distances
    private int[] DistanceArray = new int[64];
    // Data structure to give points to certain squares on the board
    public int[] PositionArray = { -20, -10, -10, -10, -10, -10, -10, -20,
                                    -10, 5, 5, 5, 5, 5, 5, -10,
                                    -10, 5, 10, 10, 10, 10, 5, -10,
                                    -10, 5, 10, 20, 20, 10, 5, -10,
                                    -10, 5, 10, 20, 20, 10, 5, -10,
                                    -10, 5, 10, 10, 10, 10, 5, -10,
                                    -10, 5, 5, 5, 5, 5, 5, -10,
                                    -20, -10, -10, -10, -10, -10, -10, -20,};
                                   

    // Just the model of the game
    public Model mainModel;

    public Model temp;

    public int turncounter = 1;

    // Controller for the game
    GameObject controller = GameObject.FindGameObjectWithTag("GameController");

    // Search depth for ai
    public int searchDepth;

    // What player the ai is playing as
    public bool aiplayer;

    // Killer moves
    private List<KillerMove>[] killers;

    // Static variable for evaluate functions
    private const int MyPositionMultiplier = 1200;
    private const int EnemyPositionMultiplier = 800;
    private int nodes = 0;
    private int leaves = 0;
    private int tthits = 0;
    private bool qsflag = false;
    private long time = 0;
    private long alltime = 0;
    private long evaltime = 0;
    private long MoveMaking = 0;
    private long MoveUndoing = 0;
    private long StartingAI = 0;
    private long transtime = 0;
    private long functioncalls = 0;
    private long counter = 0;
    
    // Constructor for the ai
    public AI(Model m, bool player, int searchdepth)
    {
        this.aiplayer = player;
        this.searchDepth = searchdepth;
        this.mainModel = new Model(m);
        this.killers = new List<KillerMove>[searchDepth];
        for (int i = 0; i < searchDepth; i++)
        {
            killers[i] = new List<KillerMove>();
        }
        BuildDistanceArr();
    }

    // Constructor for the ai with the option to decide what type of ai to use
    public AI(Model m, bool player, int searchdepth, AItypes TypeOfAi)
    {
        this.aiplayer = player;
        this.searchDepth = searchdepth;
        this.mainModel = new Model(m);
        this.type = TypeOfAi;
        this.killers = new List<KillerMove>[searchDepth];
        for (int i = 0; i < searchDepth; i++)
        {
            killers[i] = new List<KillerMove>();
        }
        BuildDistanceArr();
    }

    // Build distance array based on vectors
    private void BuildDistanceArr() 
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                DistanceArray[TurnPosToIndex(new Vector2Int(j,i))] = (int)Math.Sqrt((double)CalcDistance(new Vector2Int(j,i)));
            }
        }
    }

    // Base function to start the ai and make a move
    public void StartAi()
    {
        table = new Dictionary<string, CachedData>(10000000);

        // Variable to save the best move made
        Move bestMove = new Move();
        bestMove.score = int.MinValue;
        
        // Current score of the move
        int current = 0;

        // List of all possible moves for the current player (black)
        List<Move> possibleMoves = mainModel.GenerateAllMoves(aiplayer);

        // Trying out move ordering
        SimpleMoveOrdering(possibleMoves);

        // Temporary model to play on with the ai
        temp = new Model(mainModel);

        foreach (Move move in possibleMoves)
        {
            // make the move on the model
            temp.MakeMove(move);

            current = ActivateTypeOfAi();

            // undo the move
            temp.Undomove(move);

            // If the current move is better then the best one yet than save it
            if (current > bestMove.score)
            {
                bestMove = new Move(move);
                bestMove.score = current;
            }
        }
        // Make an actuall move using the best move given by chosen algorithem
        actuallymove(bestMove);
    }
    // Base function to start the ai and make a move
    public void StartAiWithDebug()
    {
        counter++;
        nodes = 0;
        leaves = 0;
        MoveMaking = 0;
        MoveUndoing = 0;
        StartingAI = 0;
        transtime = 0;
        functioncalls = 0;
        tthits = 0;
        table = new Dictionary<string, CachedData>(10000000);

        // Variable to save the best move made
        Move bestMove = new Move();
        bestMove.score = int.MinValue;

        // Current score of the move
        int current = 0;
        var sw2 = System.Diagnostics.Stopwatch.StartNew();

        // List of all possible moves for the current player (black)
        List<Move> possibleMoves = mainModel.GenerateAllMoves(aiplayer);

        // Trying out move ordering
        SimpleMoveOrdering(possibleMoves);
        sw2.Stop();
        time = sw2.ElapsedMilliseconds;
        // Count preformence of the ai
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Temporary model to play on with the ai
        temp = new Model(mainModel);

        foreach (Move move in possibleMoves)
        {
            sw2 = System.Diagnostics.Stopwatch.StartNew();
            // make the move on the model
            temp.MakeMove(move);
            sw2.Stop();
            MoveMaking += sw2.ElapsedMilliseconds;
            sw2 = System.Diagnostics.Stopwatch.StartNew();
            functioncalls++;
            current = ActivateTypeOfAi();
            sw2.Stop();
            StartingAI += sw2.ElapsedMilliseconds;

            sw2 = System.Diagnostics.Stopwatch.StartNew();
            // undo the move
            temp.Undomove(move);
            sw2.Stop();
            MoveUndoing += sw2.ElapsedMilliseconds;

            // If the current move is better then the best one yet than save it
            if (current > bestMove.score)
            {
                bestMove = new Move(move);
                bestMove.score = current;
            }
        }

        stopwatch.Stop();
        alltime += stopwatch.ElapsedMilliseconds;
        // Document for future improvment
        Debug.Log("Move made - " + bestMove + " score: " + bestMove.score);
        Debug.Log("amount of nodes " + nodes);
        Debug.Log("amount of leaves " + leaves);
        Debug.Log("average time spent deciding the move - " + alltime / counter + " milliseconds" + "\n Current depth - " + searchDepth);
        Debug.Log("amount of time spent on starting ai " + StartingAI.ToString() + " milliseconds");
        Debug.Log("amount of time spent on move generation " + time.ToString() + " milliseconds");
        Debug.Log("amount of time spent on move making " + MoveMaking.ToString() + " milliseconds");
        Debug.Log("amount of time spent on move undoing " + MoveUndoing.ToString() + " milliseconds");
        Debug.Log("amount of time spent on transposition table " + transtime.ToString() + " milliseconds");
        Debug.Log("amount of function calls " + functioncalls.ToString() + " calls");
        Debug.Log("amount of time spent on evaluation " + evaltime.ToString() + " milliseconds");
        Debug.Log("amount of tt hits " + tthits.ToString() + " times");
        sw2 = System.Diagnostics.Stopwatch.StartNew();
        // Actually make a move 
        actuallymove(bestMove);
        sw2.Stop();
        Debug.Log("amount of time spent on actually moving " + sw2.ElapsedMilliseconds.ToString() + " milliseconds");

    }

    // Function to handle different calls for different ai types     
    private int ActivateTypeOfAi()
    {
        switch (type)
        {
            case (AItypes.NegaMax):
                return -NegaMax(-10000, 10000, !aiplayer, 1);
            case (AItypes.NegaScout):
                return -NegaScout(-10000, 10000, !aiplayer, 1);
            case(AItypes.DebugMode):
                return -NegaScoutWithDebugging(-10000, 10000, !aiplayer, 1);
            default:
                break;
        }
        return 0;
    }


    // --------------------------------------------- recursions ------------------------------------------------------------------------------

    // Try #1 on negamax recrsion -> (no evaluate ) depth 4 = <1 sec | d 5 5 = 6 sec | d 6 = 21 sec 1,600,000 positions searched
    private int NegaMax(int alpha, int beta, bool currentplayer, int depth)
    {
        int a = alpha;
        char[] chars = new char[16];
        HashCode();
        if (table.TryGetValue(new string(chars), out CachedData cd) && cd.depth <= depth)
        {
            return cd.score;
        }

        cd = new CachedData();


        if (depth == searchDepth || temp.checkwin(currentplayer) || temp.checkwin(!currentplayer))
        {
            leaves++;
            return Evaluate(!currentplayer, depth);
        }
        nodes++;

        int eval = -10000;

        // Generate all possible moves
        List<Move> moves = temp.GenerateAllMoves(currentplayer);

        // Implementing move ordering
        SimpleMoveOrdering(moves);

        // Go over all the moves generated
        foreach (Move nextmove in moves)
        {

            temp.MakeMove(nextmove);
            eval = max(eval, -NegaMax(-beta, -alpha, !currentplayer, depth + 1));
            temp.Undomove(nextmove);

            alpha = max(alpha, eval);
            // Cutoff
            if (alpha >= beta)
            {
                break;
            }
        }
        cd.depth = depth;
        table[new string(chars)] = cd;
        return eval;
    }

    // Try #3 on negascout recursion -> (no evaluate) depth 4 = <1 sec | d 5 = 5 sec | d 6 = 20 sec 1,520,000 positions searched
    private int NegaScout(int alpha, int beta, bool currentplayer, int depth)
    {
        int eval = int.MinValue;
        int originalalpha = alpha;
        string key = HashCode();
        if (table.TryGetValue(key, out CachedData cd) && cd.depth <= depth)
        {
            tthits++;
            return cd.score;
        }


        CachedData cd2 = new CachedData();

        if (depth == searchDepth || temp.checkwin(currentplayer) || temp.checkwin(!currentplayer))
        {
            int score = 0;
            leaves++;
            score = Evaluate(currentplayer, depth);
            return score;
        }
        nodes++;

        bool flag = true;

        // Generate all possible moves
        List<Move> moves = temp.GenerateAllMoves(currentplayer);

        // Implementing move ordering
        SimpleMoveOrdering(moves);

        foreach (Move nextmove in moves)
        {

            if (flag)
            {
                flag = false;
                temp.MakeMove(nextmove);

                eval = -NegaScout(-beta, -alpha, !currentplayer, depth + 1);

                temp.Undomove(nextmove);
            }
            else
            {
                temp.MakeMove(nextmove);

                eval = -NegaScout(-(alpha + 1), -alpha, !currentplayer, depth + 1);

                temp.Undomove(nextmove);

                // if the current score is 
                if (alpha < eval && eval < beta)
                {
                    temp.MakeMove(nextmove);

                    eval = -NegaScout(-beta, -eval, !currentplayer, depth + 1);

                    temp.Undomove(nextmove);
                }
            }
            alpha = max(eval, alpha);
            // Cutoff
            if (alpha >= beta)
            {
                break;
            }
        }
        if (alpha > originalalpha && alpha < beta)
        {
            cd2.score = alpha;
            cd2.depth = depth;
            table[key] = cd2;
        }
        return alpha;
    }
    // Try #3 on negascout recursion -> (no evaluate) depth 4 = <1 sec | d 5 = 5 sec | d 6 = 20 sec 1,520,000 positions searched
    private int NegaScoutWithDebugging(int alpha, int beta, bool currentplayer, int depth)
    {
        int eval = int.MinValue;
        var sw2 = System.Diagnostics.Stopwatch.StartNew();
        int originalalpha = alpha;
        string key = HashCode();
        if (table.TryGetValue(key, out CachedData cd) && cd.depth <= depth)
        {
            return cd.score;
        }


        CachedData cd2 = new CachedData();
        sw2.Stop();
        transtime += sw2.ElapsedMilliseconds;

        if (depth == searchDepth || temp.checkwin(currentplayer) || temp.checkwin(!currentplayer))
        {
            int score = 0;
            var sw3 = System.Diagnostics.Stopwatch.StartNew();
            leaves++;
            score = Evaluate(currentplayer, depth);
            sw3.Stop();
            evaltime += sw3.ElapsedMilliseconds;
            return score;
        }
        nodes++;

        bool flag = true;
        sw2 = System.Diagnostics.Stopwatch.StartNew();

        // Generate all possible moves
        List<Move> moves = temp.GenerateAllMoves(currentplayer);

        // Implementing move ordering
        SimpleMoveOrdering(moves);
        sw2.Stop();
        time += sw2.ElapsedMilliseconds;
        foreach (Move nextmove in moves)
        {

            if (flag)
            {
                flag = false;
                var sw4 = System.Diagnostics.Stopwatch.StartNew();
                temp.MakeMove(nextmove);
                sw4.Stop();
                MoveMaking += sw4.ElapsedMilliseconds;
                functioncalls++;
                eval = -NegaScout(-beta, -alpha, !currentplayer, depth + 1);
                var sw5 = System.Diagnostics.Stopwatch.StartNew();
                temp.Undomove(nextmove);
                sw5.Stop();
                MoveUndoing += sw5.ElapsedMilliseconds;
            }
            else
            {
                var sw4 = System.Diagnostics.Stopwatch.StartNew();
                temp.MakeMove(nextmove);
                sw4.Stop();
                MoveMaking += sw4.ElapsedMilliseconds;
                functioncalls++;
                eval = -NegaScout(-(alpha + 1), -alpha, !currentplayer, depth + 1);
                var sw5 = System.Diagnostics.Stopwatch.StartNew();
                temp.Undomove(nextmove);
                sw5.Stop();
                MoveUndoing += sw5.ElapsedMilliseconds;

                // if the current score is 
                if (alpha < eval && eval < beta)
                {
                    sw4 = System.Diagnostics.Stopwatch.StartNew();
                    temp.MakeMove(nextmove);
                    sw4.Stop();
                    MoveMaking += sw4.ElapsedMilliseconds;
                    functioncalls++;
                    eval = -NegaScout(-beta, -eval, !currentplayer, depth + 1);
                    sw5 = System.Diagnostics.Stopwatch.StartNew();
                    temp.Undomove(nextmove);
                    sw5.Stop();
                    MoveUndoing += sw5.ElapsedMilliseconds;
                }
            }
            alpha = max(eval, alpha);
            // Cutoff
            if (alpha >= beta)
            {
                break;
            }
        }
        if (alpha > originalalpha && alpha < beta)
        {
            sw2 = System.Diagnostics.Stopwatch.StartNew();
            cd2.score = alpha;
            cd2.depth = depth;
            table[key] = cd2;
            sw2.Stop();
            transtime += sw2.ElapsedMilliseconds;
        }

        return alpha;
    }

    // Trying out Quiescence search
    private int Quiescencesearch(bool currentplayer, int depth)
    {
        if (!qsflag || temp.checkwin(currentplayer))
        {
            return Evaluate(currentplayer, depth);
        }
        return -NegaScout(-10000, 10000, currentplayer, depth);
    }


    // ------------------------------- Evalution and move Methods -----------------------------------------


    // Get a given model (in some point of time) and a move
    // Determine score for the given state of game
    public int Evaluate(bool currentplayer, int depth) 
    {
        int score = 1;

        if (turncounter > 5)
        {
            score += ImprovedWinCheck(currentplayer, depth);
        }

        score += CentralisationScore(currentplayer);
        score -= CentralisationScore(!currentplayer);

        // Turn saved average position into its actuall value (currently the sum of all positions)
        Vector2Int avgpos = temp.GetCurrentAvg(currentplayer) / temp.GetPiecesByBool(currentplayer).Count;
        // Better position is a position where the hezion is closest to the avg position
        score += SumOfDistances(avgpos, currentplayer, currentplayer);
        // Turn saved average position into its actuall value (currently the sum of all positions)
        Vector2Int enemyavgpos = temp.GetCurrentAvg(!currentplayer) / temp.GetPiecesByBool(!currentplayer).Count;
        // Better position is a position where the hezion is closest to the avg position
        score -= SumOfDistances(enemyavgpos, !currentplayer, currentplayer);

        return score;
    }

    // Get a move list 
    // Give every move a score and sort them
    private void SimpleMoveOrdering(List<Move> moves)
    {
        moves.Sort(delegate (Move p1, Move p2)
        {
            int compareScore = p1.score.CompareTo(p2.score);
            return compareScore;
        });
    }

    // Get a list of moves and the current depth
    // Add to the list the moves in the killer moves list if the move is legal in current model
    private void AddKillerMoves(List <Move> moves, int depth) 
    {
        for (int i = killers[depth].Count-1; i > -1; i--)
        {
            if (killers[depth][i].found) 
            {
                moves.Insert(0, killers[depth][i].move);
            }
            killers[depth][i].found = false;
        }
    }

    // Get an average position and a move
    // Return a score based on the distance of the given move from the average position
    public int SumOfDistances(Vector2Int avg, bool currentplayer , bool player)
    {
        double sumofdistances = GetSumOfDistances(avg, player);

        return player == currentplayer ? (int)((1 / sumofdistances) * MyPositionMultiplier) : (int)((1 / sumofdistances)  * EnemyPositionMultiplier);
    }
    
    // Get the current player and a depth
    // Check win for each of them and return appropriate score relative to depth
    // Earlier wins get a bigger score
    private int ImprovedWinCheck(bool player, int depth)
    {
        if (temp.checkwin(player))
        {
            return (searchDepth - depth + 1) * 10000;
        }
        else if(temp.checkwin(!player))
        {
            return (searchDepth - depth + 1) * -10000;
        }
        return 0;
    }

    // Get the current player 
    // Return the sum of all scores of the positions of the pieces on the board and return the average
    public int CentralisationScore(bool currentplayer) 
    {
        int sum = 0;
        foreach (Piece p in temp.GetPiecesByBool(currentplayer).Values) 
        {
            sum += PositionArray[TurnPosToIndex(p.position)];
        }
        return sum / temp.GetPiecesByBool(currentplayer).Count;
    }


    // -------------------------------- Utility Methods ------------------------------------------


    // Get a position on a board
    // return the index it will have on the board
    private int TurnPosToIndex(Vector2Int pos)
    {
        return pos.x + pos.y * 8;
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

    // Get a model, an average position and a player
    // Return the sum of distnaces of all pieces of the player given  
    private int GetSumOfDistances(Vector2Int avg, bool player) 
    {
        int i = 0;
        int sum = 0;
        foreach (Piece p in temp.GetPiecesByBool(player).Values) 
        {
            Vector2Int minus = avg - p.position;
            minus.x = Math.Abs(minus.x);
            minus.y = Math.Abs(minus.y);
            sum += DistanceArray[TurnPosToIndex(minus)];
            i++;
        }

        return sum;
    }

    // Just the same function only with a vector
    private int CalcDistance(Vector2Int pos)
    {
        int dist = 0;
        if ((dist = (int)(Math.Pow((double)pos.x, 2) + Math.Pow((double)pos.y, 2))) == 0) 
        {
            return 1;
        }
        return dist;
    }

    // Get a move
    // Actually make the move in the unity space
    private void actuallymove(Move move)
    {
        controller.GetComponent<Game>().MoveAPieceInUnity(move);
        turncounter++;
    }

    // Get an array of chars and return the hash value of it
    private string HashCode() 
    {
        return temp.board.blacks.ToString() + temp.board.whites.ToString();
    }

    //-------------------------- Past Versions (graveyard)----------------------------
    // Get cached value and a list of possible
    // 
    //private void AddBestMove(CachedData cd, List<Move> moves) 
    //{
    //    if (moves.Contains(cd.bestmove)) 
    //    {
    //        moves.Remove(cd.bestmove);
    //        moves.Insert(0, cd.bestmove);
    //    }
    //}
    //// Get a move and a model and try to score the connectivity of the pieces of the moving piece
    //private int ConnectivityScore(Move move, Model m)
    //{
    //    int max = 0;
    //    int number = 0;
    //    int amountOfGroups = 0;
    //    m.board.InitCheckedThis();
    //    // If the number of any players piece is 1 than the game is finished
    //    if (m.GetPiecesByBool(move.pieceToMove.player).Count == 1) { return 10000; }
    //    // Go over the pieces of the player im checking
    //    foreach (Piece p in m.GetPiecesByBool(move.pieceToMove.player).Values)
    //    {
    //        // Save the current pieces position and the corrospondaning index
    //        Vector2Int pos = p.position;
    //        int index = m.board.PositionToIndex(pos);
    //        // Check if said position hasnt been checked before
    //        if ((m.board.checkedthis & m.board.TurnIndexToBitBoard(index)) == 0)
    //        {
    //            // Find the amount of of adjacent of pieces
    //            number = m.board.FindLines(index, move.pieceToMove.player);
    //            // amount of times i run the search is amount of groups
    //            amountOfGroups++;
    //            // If the number is bigger than saved max than change it
    //            if (max < number)
    //            {
    //                max = number;
    //                if (max == m.GetPiecesByBool(move.pieceToMove.player).Count)
    //                {
    //                    return max + amountOfGroups * -2;
    //                }

    //            }

    //        }
    //    }
    //    return max;

    //}
    //// Get a given model (in some point of time) and a move
    //// Check if winning or losinng and if not any of those evaluate the connectivity, return appropriate number
    //private int WinninLosingConnectivity(Move move, Model m)
    //{
    //    int maxme = ConnectivityScore(move, m);
    //    if (maxme == m.GetPiecesByBool(move.pieceToMove.player).Count)
    //    {
    //        return 10000;
    //    }
    //    else if (m.checkwin(!move.pieceToMove.player))
    //    {
    //        return -10000;
    //    }
    //    float proportion = maxme / m.GetPiecesByBool(move.pieceToMove.player).Count;
    //    return (int)proportion;
    //}
    //// Get and x and y index
    //// Return distance 
    //private int CalcDistanceBetween2Points(Vector2Int first, Vector2Int second)
    //{
    //    return (int)(Vector2Int.Distance(first, second)) * -5;
    //}
    // Get a move
    // If move was last played punish ai
    //private int MovePlayed(Move move)
    //{
    //    if (GetMyLastMove() != null)
    //    {
    //        Move lastmove = move.pieceToMove.player ? LastWhiteMove : LastBlackMove;
    //        return ((move.moveto == lastmove.pieceToMove.position) && (move.pieceToMove.position == lastmove.moveto)) ? 15 : 0;
    //    }
    //    return 0;
    //}
    //// Get a given model (in some point of time) and a move
    //// Determine score for the given state of game
    //public int Evaluate(Move move, Model m)
    //{
    //    int score = 1;
    //    int amount = m.GetPiecesByBool(move.pieceToMove.player).Count;
    //    // Average position of all the pieces with this color
    //    Vector2Int avgpos = m.GetCurrentAvg(move.pieceToMove.player);
    //    avgpos.x /= amount;
    //    avgpos.y /= amount;

    //    //score += SimpleWinningLosing(move.pieceToMove.player, m);

    //    // Reward having less pieces then enemy
    //    //score += AmountOfPieces(move, m);

    //    //Calculate ceter of mass of all ai pieces and reward being close to the middle
    //    score += MiddleSquares(move.moveto);

    //    //// Trying out new things
    //    //score += CloserToAvg(avgpos, move);

    //    // Favoring attack for now 
    //    if (move.attack) 
    //    {
    //        score *= 2;
    //    }

    //    // Reward begin away from frame of board
    //    score -= BadSquares(move.moveto);

    //    //// If winning reward higeset number if losing punihs with lowest number
    //    //score += WinninLosingConnectivity(move, m) / 2;

    //    return score;
    //}


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
    // Try #2 negascout
    //private int pvSearch(Move move, int alpha, int beta, int depth, bool currentplayer, Model m)
    //{

    //    // If im at a leaf node or game has ended go back
    //    if (depth == searchDepth - 1 || m.checkwin(currentplayer))
    //    {
    //        return Evaluate(move, m);
    //    }
    //    int current = 0;
    //    // Switch to other player
    //    currentplayer = !currentplayer;

    //    // Generate all possible moves
    //    List<Move> moves = m.GenerateAllMoves(currentplayer);

    //    // Implementing move ordering
    //    SimpleMoveOrdering(moves, currentplayer, m);


    //    // Go over all the moves generated
    //    foreach (Move nextmove in moves)
    //    {

    //        if (depth == searchDepth - 1 && nextmove.attack)
    //        {
    //            current = QuietSearch(nextmove, 0, m);
    //        }
    //        else
    //        {
    //            positions++;
    //            // Make possible move
    //            m.MakeMove(nextmove);
    //            // Continue down the search tree
    //            current = -pvSearch(nextmove, -alpha - 1, -alpha, depth + 1, currentplayer, m);
    //            // Undo move made
    //            m.Undomove(nextmove);
    //            // if the current score is 
    //            if (alpha < current && current < beta)
    //            {
    //                m.MakeMove(nextmove);
    //                current = -pvSearch(nextmove, -beta, -current, depth + 1, currentplayer, m);
    //                m.Undomove(nextmove);
    //            }
    //            else
    //            {
    //                m.MakeMove(nextmove);
    //                current = -pvSearch(nextmove, -beta, -alpha, depth + 1, currentplayer, m);
    //                m.Undomove(nextmove);
    //            }
    //        }

    //        alpha = max(alpha, current);
    //        if (alpha >= beta)
    //        {
    //            return alpha;
    //        }
    //    }
    //    return alpha;

    //}
    //// Get a given model (in some point of time) and a move
    //// Evaluate the center of mass of the player's pieces
    //private int StructureEvaluation(Move move, Vector2Int avgpos, Model m)
    //{

    //    // The furthest distance from the center of mass 
    //    double Mymaxdist = -100;
    //    // All the pieces of a certain color
    //    List<Piece> Mypieces = m.GetPiecesByBool(move.pieceToMove.player);

    //    // Go over all pieces and calculate distance of all x positions and y positions
    //    // Find and save the furthest distance
    //    for (int i = 0; i < Mypieces.Count; i++)
    //    {
    //        // Calculate distance of a piece from the average position
    //        double distance = CalcDistance(Math.Abs(Mypieces[i].position.x - avgpos.x), Math.Abs(Mypieces[i].position.y - avgpos.y));
    //        // Update if found a bigger distance
    //        if (distance > Mymaxdist)
    //        {
    //            Mymaxdist = distance;
    //        }
    //    }

    //    // Reward being close to the middle and reward having a closer formation (my max avg distance is lower then the enemeys)
    //    return -(int)Mymaxdist;
    //}
    //// Get a model
    //// Return the avg position for all the pieces of a certain color
    //private Vector2Int AvgPos(bool Myplayer, Model m)
    //{
    //    // Avg x and y of my pieces
    //    int Myavgx = 0, Myavgy = 0;
    //    // Lists of both kinds of pieces
    //    List<Piece> Mypieces = m.GetPiecesByBool(Myplayer);
    //    List<Piece> EnemyPieces = m.GetPiecesByBool(!Myplayer);

    //    // Go over all pieces and sum all x positions and y positions (both my pieces and enemys)
    //    for (int i = 0; i < Mypieces.Count || i < EnemyPieces.Count; i++)
    //    {
    //        if (i < Mypieces.Count)
    //        {
    //            Myavgx += Mypieces[i].position.x;
    //            Myavgy += Mypieces[i].position.y;
    //        }
    //    }

    //    // Calculate average 
    //    Myavgx /= m.GetPiecesByBool(Myplayer).Count;
    //    Myavgy /= m.GetPiecesByBool(Myplayer).Count;
    //    return new Vector2Int(Myavgx, Myavgy);
    //}
    // fix this
    // For some reason in random some moves cause me to create insane amount of pieces in the temp module - FIX THIS -> fixed
    //public int RecursionEvaluate(Model m, int depth, Move current, int alpha, int beta, bool currentplayer)
    //{
    //    // Make a move on the model - change bitboard and lists
    //    //m.MakeMove(current, depth);
    //    // Dont stop until you reach the desired deapth
    //    // (im subtructing one because one move deapth is preformed before the recurion)
    //    if (depth == searchDepth - 1)
    //    {
    //        return Evaluate(current.pieceToMove.player, m);
    //    }
    //    //lastmove = current;
    //    currentplayer = !currentplayer;
    //    if (currentplayer)
    //    { beta = 10000; }
    //    else { alpha = -10000; }
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
    //        //nextmove.Child.AddRange(KillerMoves[depth]);
    //        //m.FutureMovesImproved(nextmove);
    //        foreach (Move after in nextmove.Child)
    //        {
    //            int score = RecursionEvaluate(m, depth + 1, after, alpha, beta, currentplayer);
    //            m.UndoChangePosition(after);
    //            if (currentplayer)
    //            {
    //                beta = min(beta, score);
    //                if (alpha >= beta)
    //                {
    //                    return beta;
    //                }
    //            }
    //            else
    //            {
    //                alpha = max(alpha, score);
    //                if (alpha >= beta)
    //                {
    //                    return alpha;
    //                }
    //            }
    //        }
    //    }
    //    return currentplayer ? beta : alpha;
    //}
    // Get an average position and a move
    // Return a score based on the distance of the given move from the average position
    //private int CloserToAvg(Vector2Int avg, Move move)
    //{
    //    Vector2Int minus = avg - move.moveto;
    //    minus.x = Math.Abs(minus.x);
    //    minus.y = Math.Abs(minus.y);
    //    return (int)(1 / DistanceArray[TurnPosToIndex(minus)]) * MyPositionMultiplier;
    //}
    //private int SimpleWinningLosing(bool player, int depth) 
    //{
    //    if (depth < searchDepth) 
    //    {
    //        Debug.Log("here");
    //    }

    //    if (temp.checkwin(player))
    //    {
    //        return ((searchDepth - depth) + 1) * 10000;
    //    }
    //    else if (temp.checkwin(!player))
    //    {
    //        return ((searchDepth - depth) + 1) * -10000;
    //    }
    //    return 0;
    //}

    //// Get and x and y index
    //// Return distance 
    //private int CalcDistance(int x, int y)
    //{
    //    return (int)(Math.Pow((double)x, 2) + Math.Pow((double)y, 2));
    //}
    //// Get a player, a move and a model 
    //// Count the amount of groups on the board using bit board and return score based on amount (1 group is a win)
    //private int GroupScore(bool player, bool currentplayer)
    //{
    //    double amountOfGroups = 0;
    //    temp.board.InitCheckedThis();
    //    // If the number of any players piece is 1 than the game is finished
    //    if (temp.GetPiecesByBool(player).Count == 1) { return 10000; }
    //    // Go over the pieces of the player im checking
    //    foreach (Piece p in temp.GetPiecesByBool(player).Values)
    //    {
    //        // Save the current pieces position and the corrospondaning index
    //        Vector2Int pos = p.position;
    //        int index = temp.board.PositionToIndex(pos);
    //        // Check if said position hasnt been checked before
    //        if ((temp.board.checkedthis & temp.board.TurnIndexToBitBoard(index)) == 0)
    //        {
    //            // Find the amount of of adjacent of pieces
    //            var number = temp.board.FindLines(index, player);
    //            // amount of times i run the search is amount of groups
    //            amountOfGroups++;
    //        }
    //    }

    //    if (amountOfGroups == 1)
    //    {
    //        return -10000;
    //    }

    //      double score = player == currentplayer ? 1 / amountOfGroups * MyGroupScoreMultiplier : 1 / amountOfGroups * EnemyGroupScoreMultiplier;

    //    return (int)score;
    //}
    //// Try #4 on negascout recursion -> (no evaluate) depth 4 = <1 sec | d 5 = 5 sec | d 6 = 20 sec 1,520,000 positions searched
    //private int NegaScoutOld(int alpha, int beta, bool currentplayer, int depth)
    //{
    //    if (depth == searchDepth || temp.checkwin(currentplayer))
    //    {
    //        return Evaluate(currentplayer, depth);
    //    }

    //    int a, b, t, i = 0;
    //    a = alpha;
    //    b = beta;

    //    // Generate all possible moves
    //    List<Move> moves = temp.GenerateAllMoves(currentplayer);

    //    // Implementing move ordering
    //    SimpleMoveOrdering(moves);

    //    // Go over all the moves generated
    //    foreach (Move nextmove in moves)
    //    {
    //        i++;
    //        temp.MakeMove(nextmove);
    //        t = -NegaScoutOld(-b, -a, !currentplayer, depth+1);
    //        temp.Undomove(nextmove);
    //        if (t > a && t< beta && i > 1 && depth < searchDepth-1) 
    //        {
    //            temp.MakeMove(nextmove);
    //            a = -NegaScoutOld(-beta, -t, !currentplayer, depth + 1);
    //            temp.Undomove(nextmove);
    //        }
    //        a = max(a, t);

    //        nodes++;
    //        if (alpha >= beta)
    //        {
    //            return a;
    //        }
    //        b = a + 1;
    //    }
    //    return a;
    //}
    //// Try #4 on the recursion (just testing now) -> this is the new liquid gold (this is diamond)
    //private int BaseReucrsion(int depth, bool currentplayer)
    //{
    //    // If im at a leaf node or game has ended go back
    //    if (depth == searchDepth || temp.checkwin(currentplayer) || temp.checkwin(currentplayer))
    //    {
    //        return Evaluate(currentplayer, depth);
    //    }
    //    int current = 0;
    //    // Switch to other player
    //    currentplayer = !currentplayer;

    //    // Generate all possible moves
    //    List<Move> moves = temp.GenerateAllMoves(currentplayer);

    //    // Go over all the moves generated
    //    foreach (Move nextmove in moves)
    //    {
    //        // Make possible move
    //        temp.MakeMove(nextmove);
    //        // Continue down the search tree
    //        current = BaseReucrsion(depth + 1, currentplayer);
    //        // Undo move made
    //        temp.Undomove(nextmove);

    //        if (currentplayer) 
    //        {

    //        }

    //    }
    //    return current;
    //}
    //// Try # 3 evaluate
    //private int NewEvaluate(bool currentplayer, int depth) 
    //{
    //    int score = 1;

    //    if (turncounter > 5)
    //    {
    //        score += ImprovedWinCheck(currentplayer, depth);
    //    }

    //    score += CentralisationScore(currentplayer);
    //    score -= CentralisationScore(!currentplayer);

    //    // Turn saved average position into its actuall value (currently the sum of all positions)
    //    Vector2Int avgpos = temp.GetCurrentAvg(currentplayer) / temp.GetPiecesByBool(currentplayer).Count;
    //    // Better position is a position where the hezion is closest to the avg position
    //    score += SumOfDistances(avgpos, currentplayer, currentplayer);
    //    // Turn saved average position into its actuall value (currently the sum of all positions)
    //    Vector2Int enemyavgpos = temp.GetCurrentAvg(!currentplayer) / temp.GetPiecesByBool(!currentplayer).Count;
    //    // Better position is a position where the hezion is closest to the avg position
    //    score -= SumOfDistances(enemyavgpos, !currentplayer, currentplayer);

    //    return score;
    //}
    //// Try #1 on negamax recrsion -> (no evaluate ) depth 4 = <1 sec | d 5 5 = 6 sec | d 6 = 21 sec 1,600,000 positions searched
    //private int negamax2(bool currentplayer, int depth)
    //{
    //    if (depth == searchDepth || temp.checkwin(currentplayer) || temp.checkwin(!currentplayer))
    //    {
    //        leaves++;
    //        return Evaluate(currentplayer, depth) ;
    //    }
    //    nodes++;

    //    int eval = -100000;

    //    // Generate all possible moves
    //    List<Move> moves = temp.GenerateAllMoves(currentplayer);

    //    // Implementing move ordering
    //    SimpleMoveOrdering(moves);

    //    // Go over all the moves generated
    //    foreach (Move nextmove in moves)
    //    {
    //        temp.MakeMove(nextmove);
    //        eval = max(eval, -negamax2(!currentplayer, depth + 1));
    //        temp.Undomove(nextmove);
    //    }
    //    return eval;
    //}
}
