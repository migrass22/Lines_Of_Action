using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
// Class for all logical expression
public class Model
{
    //----------------------------------- Variables ---------------------------------------


    // Hold pieces of each color that are in the game
    public List<Piece> whites { get; set; }
    public List<Piece> blacks { get; set; }
    // BitBoard object to do all logical actions
    public BitBoard board { get; set; }

    // Create 4 arrays for each line possible to move in, u need 2 more functions to change each array 
    // when move is made and when move is undone
    public int[] row = new int[8];

    public int[] col = new int[8];

    // Primary diagonal       
    public int[] pdiagonal = new int[15];

    // Secondary diagonal
    public int[] sdiagonal = new int[15];

    // Trying to cut time down by saving the avg position of both players
    public Vector2Int WhiteAvg;
    public Vector2Int BlackAvg;
    public int BlackDist;
    public int WhiteDist;

    // ----------------------------------- Constructors ----------------------------------

    // Ctor for the model, will also be used to restart the game
    public Model() 
    {
        whites = new List<Piece>(12);
        blacks = new List<Piece>(12);
        board = new BitBoard();
        int []temprow = { 6, 2, 2, 2, 2, 2, 2, 6 };
        this.row = temprow;
        int[] tempcol = { 6, 2, 2, 2, 2, 2, 2, 6 };
        this.col = tempcol;
        int[] temppdiagnoal = { 0, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 0 };
        this.pdiagonal = temppdiagnoal;
        int[] tempsdiagonal = { 0, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 0 };
        this.sdiagonal = tempsdiagonal;
        WhiteAvg = new Vector2Int(42,42);
        BlackAvg = new Vector2Int(42,42);
        WhiteDist = 328;
        BlackDist = 328;

    }

    // Copy ctor for a model to a second model
    public Model(Model copythis)
        {
            // Init model as empty
            this.whites = new List<Piece>(12);
            this.blacks = new List<Piece>(12);
            this.board = new BitBoard();

            // Deep copy each piece
            foreach (Piece p in copythis.whites)
            {
                this.whites.Add(new Piece(new Vector2Int(p.position.x, p.position.y), p.player));
            }
            foreach (Piece p in copythis.blacks)
            {
                this.blacks.Add(new Piece(new Vector2Int(p.position.x, p.position.y), p.player));
            }

            // Copy the board as well
            this.board = new BitBoard(copythis.board);
            // Copy the arrays that count the amount of pieces in each col row and diagnoals
            CopyNumberArrays(copythis);

            WhiteAvg = new Vector2Int(copythis.WhiteAvg.x, copythis.WhiteAvg.y) ;
            BlackAvg = new Vector2Int(copythis.BlackAvg.x, copythis.BlackAvg.y);
            WhiteDist = copythis.WhiteDist;
            BlackDist = copythis.BlackDist;
        }

    // Copy number arrays from a given model to this model
    private void CopyNumberArrays(Model copythis) 
    {
        copythis.col.CopyTo(this.col,0);
        copythis.row.CopyTo(this.row, 0);
        copythis.sdiagonal.CopyTo(this.sdiagonal, 0);
        copythis.pdiagonal.CopyTo(this.pdiagonal, 0);
    }


    // ----------------------------------- Getter / Setter -----------------------------


    // Get a bool of a player and return the list of the pieces for that player
    public List<Piece> GetPiecesByBool(bool player)
    {
        if (player)
        {
            return whites;
        }
        else
        {
            return blacks;
        }
    }

    // Get 2 indexes on the board and return the piece in the corrosponding place, if the index is empty return null
    public Piece GetPieceByIndex(Vector2Int position)
    {
        // Save the position
        // Use bit operations to decide if its a white piece
        if (board.IsWhitePiece(position))
        {
            // If yes go over list and find which one is the piece
            foreach (Piece p in whites)
            {
                if (p.position == position)
                {
                    return p;
                }
            }
        }
        // Use bit operations to decide if its a black piece
        else if (board.IsBlackPiece(position))
        {
            // If yes go over list and find which one is the piece
            foreach (Piece p in blacks)
            {
                if (p.position == position)
                {
                    return p;
                }
            }
        }
        // this position does not hold any kind of piece so return null
        return null;
    }

    // ----------------------------------- Core game methods ------------------------------

    // Get a player's bool
    // Check if the given player won 
    public bool checkwin(bool currentplayer)
    {
        int number = 0;
        board.InitCheckedThis();
        // If the number of any players piece is 1 than the game is finished
        if (GetPiecesByBool(currentplayer).Count == 1) { return true; }
        // Get a single piece
        Piece p = GetPiecesByBool(currentplayer)[0];
        // Save the current pieces position and the corrospondaning index
        Vector2Int pos = p.position;
        int index = board.PositionToIndex(pos);
        // Check if said position hasnt been checked before
        if ((board.checkedthis & board.TurnIndexToBitBoard(index)) == 0)
        {
            // Find the amount of of adjacent of pieces
            number = board.FindLines(index, currentplayer);
            if (number == GetPiecesByBool(currentplayer).Count)
            {
                return true;
            }
            else 
            {
                return false;
            }
            
        }
        
        return false;
    }

    // Second try of the make move method
    public void MakeMove(Move move) 
    {
        // First take care of the main data structure - lists of pieces 
        ChangePosition(move);

        // Make the said move on the bit board
        board.MakeMove(move);

        // Update array numbers so i can make future moves
        UpdateArrayNumbers(move);

        // Finally update the average position for the moving piece
        UpdateAvgPos(move);
    }

    // Get a move
    // Change the position of the piece thats supposed to move and also remove an enemy piece if it was eaten
    private void ChangePosition(Move move)
    {
        // Find the actuall piece
        Piece actuallPiece = FindPiece(move.pieceToMove);
        move.pieceToMove = new Piece(actuallPiece);
        actuallPiece.position = new Vector2Int(move.moveto.x, move.moveto.y);

        // If move was an attack move then remove the other players piece from the list
        if (move.attack)
        {
            GetPiecesByBool(!actuallPiece.player).Remove(GetPieceByIndex(move.moveto));
        }
    }

    // Method to change amount of pieces in move arrays
    // After a move is made
    public void UpdateArrayNumbers(Move move)
    {
        Vector2Int start = move.pieceToMove.position;
        Vector2Int end = move.moveto;
        Vector2Int dir = new Vector2Int(end.x - start.x, end.y - start.y);
        // Translate move into a direction
        if (dir.x != 0)
        {
            if (dir.x > 0) { dir.x /= dir.x; }
            if (dir.x < 0) { dir.x /= -dir.x; }
        }
        if (dir.y != 0)
        {
            if (dir.y > 0) { dir.y /= dir.y; }
            if (dir.y < 0) { dir.y /= -dir.y; }
        }

        // Do everything below this for every other direction
        // Things can change 6 times or 4 times depending if eating piece or not
        // Check if the move was made in a row
        if (dir == new Vector2Int(1, 0) || dir == new Vector2Int(-1, 0))
        {
            if (move.attack)
            {
                // Only if a piece was eaten then the amount of pieces goes down
                row[start.y]--;
                sdiagonal[start.y + start.x]--;
                pdiagonal[start.y - start.x + 7]--;
                col[start.x]--;
            }
            else
            {
                // Change every array if the direction is a row
                col[start.x]--;
                col[end.x]++;

                pdiagonal[start.y - start.x + 7]--;
                pdiagonal[end.y - end.x + 7]++;

                sdiagonal[start.y + start.x]--;
                sdiagonal[end.y + end.x]++;
            }

        }
        // Check if the move is made in a col
        else if (dir == new Vector2Int(0, -1) || dir == new Vector2Int(0, 1))
        {
            if (move.attack)
            {
                col[start.x]--;
                row[start.y]--;
                pdiagonal[start.y - start.x + 7]--;
                sdiagonal[start.y + start.x]--;
            }
            else
            {
                row[start.y]--;
                row[end.y]++;

                pdiagonal[start.y - start.x + 7]--;
                pdiagonal[end.y - end.x + 7]++;

                sdiagonal[start.y + start.x]--;
                sdiagonal[end.y + end.x]++;
            }

        }
        // Check if the move is a primary diagonal
        else if (dir == new Vector2Int(1, 1) || dir == new Vector2Int(-1, -1))
        {
            if (move.attack)
            {
                pdiagonal[start.y - start.x + 7]--;
                row[start.y]--;
                col[start.x]--;
                sdiagonal[start.y + start.x]--;
            }
            else
            {
                row[start.y]--;
                row[end.y]++;

                col[start.x]--;
                col[end.x]++;

                sdiagonal[start.y + start.x]--;
                sdiagonal[end.y + end.x]++;
            }

        }
        // Check if the move is a secondary diagonal 
        else if (dir == new Vector2Int(1, -1) || dir == new Vector2Int(-1, 1))
        {
            if (move.attack)
            {
                sdiagonal[start.y + start.x]--;
                row[start.y]--;
                col[start.x]--;
                pdiagonal[start.y - start.x + 7]--;

            }
            else
            {
                row[start.y]--;
                row[end.y]++;

                col[start.x]--;
                col[end.x]++;

                pdiagonal[start.y - start.x + 7]--;
                pdiagonal[end.y - end.x + 7]++;
            }
        }
    }

    // Something to update the avg position of pieces of a certain color
    private void UpdateAvgPos(Move move)
    {
        bool player = move.pieceToMove.player;
        Vector2Int Avg = GetCurrentAvg(player);
        // Add the current position moved to and sub the position i came from
        Avg.x = Math.Abs((Avg.x) + move.moveto.x - move.pieceToMove.position.x);
        Avg.y = Math.Abs((Avg.y) + move.moveto.y - move.pieceToMove.position.y);
        SetCurrentAvg(player, Avg);
        // Update other players avg if it was eaten
        if (move.attack)
        {
            Avg = GetCurrentAvg(!player);
            Avg.x = Math.Abs(Avg.x - move.moveto.x);
            Avg.y = Math.Abs(Avg.y - move.moveto.y);
            SetCurrentAvg(!player, Avg);
        }

    }

    // Get a certain player
    // Find all the moves that player can make at the current state of the board
    public List<Move> GenerateAllMoves(bool player)
    {
        List<Move> moves = new List<Move>(96);
        foreach (Piece p in GetPiecesByBool(player))
        {
            FutureMovesImproved(p, moves);
        }
        return moves;
    }

    // Get a piece and return where it can go to using move arrays
    public List<Move> FutureMovesImproved(Piece p, List<Move> futuremoves)
    {
        Vector2Int position = p.position;
        // y position is amount of pieces in this num of col
        // x position is number of pieces in this num of row
        int colmove = col[position.x];
        int rowmove = row[position.y];
        // Turn a position to the index of correct diagonal
        int pdiagmove = pdiagonal[position.y - position.x + 7];
        int sdiagmove = sdiagonal[position.y + position.x];

        // Check for col moves of piece
        MoveInAline(p, futuremoves, new Vector2Int(position.x, position.y + colmove), new Vector2Int(0, 1));
        MoveInAline(p, futuremoves, new Vector2Int(position.x, position.y - colmove), new Vector2Int(0, -1));

        // Check for row moves of piece
        MoveInAline(p, futuremoves, new Vector2Int(position.x + rowmove, position.y), new Vector2Int(1, 0));
        MoveInAline(p, futuremoves, new Vector2Int(position.x - rowmove, position.y), new Vector2Int(-1, 0));

        // Check for the primary diagonal of the piece
        MoveInAline(p, futuremoves, new Vector2Int(position.x + pdiagmove, position.y + pdiagmove), new Vector2Int(1, 1));
        MoveInAline(p, futuremoves, new Vector2Int(position.x - pdiagmove, position.y - pdiagmove), new Vector2Int(-1, -1));

        // Check for the secondery diagonal of the piece
        MoveInAline(p, futuremoves, new Vector2Int(position.x + sdiagmove, position.y - sdiagmove), new Vector2Int(1, -1));
        MoveInAline(p, futuremoves, new Vector2Int(position.x - sdiagmove, position.y + sdiagmove), new Vector2Int(-1, 1));
        return futuremoves;
    }

    // Get a piece, an endpoint and a direction
    // Add a new move to the pieces possible moves if said move is possible
    public void MoveInAline(Piece p, List<Move> futuremoves, Vector2Int endPoint, Vector2Int dir)
    {
        // Check for the column of this piece
        // End point is on the board?
        if (IsOnBoard(endPoint.x, endPoint.y))
        {
            // Are there enemy pieces i jump over?
            if (!board.IsEnemyBeforeHere(p.position, endPoint, dir, p.player))
            {
                // Is there a piece at the end?
                if (board.IsPieceHere(endPoint))
                {
                    // Is this piece an enemy Piece?
                    if (board.IsEnemy(endPoint, p.player))
                    {
                        // Create a new attack move at this point, save on the piece,
                        futuremoves.Add(new Move(new Piece(p), endPoint, 0, true));
                    }
                }
                else
                {
                    // No piece at end point -> create a new normal move
                    futuremoves.Add(new Move(new Piece(p), endPoint, 0, false));
                }
            }
        }
    }

    // ------------------------------------ Core ai methods --------------------------------

    // Second try of the undo move method
    public void Undomove(Move move) 
    {
        // First take care of of the main data structure
        UndoChangePosition(move);

        // Make the said move on the bit board
        board.undoMove(move);

        // Lastly update array numbers
        UndoUpdateArrayNumbers(move);

        // Undo the update on the average move
        UndoUpdateAvgPos(move);
    }

    // Get a move
    // Reverse teh effects of the move on the number arrays
    private void UndoUpdateArrayNumbers(Move move)
    {
        Vector2Int position = move.pieceToMove.position;
        if (move.attack)
        {
            this.col[position.x]++;
            this.row[position.y]++;
            this.pdiagonal[position.y - position.x + 7]++;
            this.sdiagonal[position.y + position.x]++;
        }
        else 
        {
            Piece reveresePiece = new Piece(move.moveto, move.pieceToMove.player);
            Move reverseMove = new Move(reveresePiece, position, 0, false);
            UpdateArrayNumbers(reverseMove);
        }
    }

    // Get a move
    // Reverse the effects of the move 
    public void UndoChangePosition(Move move) 
    {
        // If the move was an attack move
        if (move.attack)
        {
            // Recreate the lost piece and add it to the list of pieces
            Piece eatenPiece = new Piece(null, move.moveto, !move.pieceToMove.player);
            GetPiecesByBool(!move.pieceToMove.player).Add(eatenPiece);
        }

        // Get the piece
        Piece actuallPiece = GetPieceByIndex(move.moveto);

        // Change its position
        actuallPiece.position = move.pieceToMove.position;
    }

    // Something to update the avg position of pieces of a certain color
    private void UndoUpdateAvgPos(Move move)
    {
        bool player = move.pieceToMove.player;
        Vector2Int Avg = GetCurrentAvg(player);
        // Add the current position moved to and sub the position i came from
        Avg.x = Math.Abs(Avg.x - move.moveto.x + move.pieceToMove.position.x);
        Avg.y = Math.Abs(Avg.y - move.moveto.y + move.pieceToMove.position.y);
        SetCurrentAvg(player, Avg);

        // Update other players avg if it was eaten
        if (move.attack)
        {
            Avg = GetCurrentAvg(!player);
            Avg.x = Math.Abs(Avg.x + move.moveto.x);
            Avg.y = Math.Abs(Avg.y + move.moveto.y);
            SetCurrentAvg(!player, Avg);
        }

    }


    // -------------------------------------------- Utility ------------------------------------------------

    // Get a player
    // Return the current average of that player
    public Vector2Int GetCurrentAvg(bool player) 
    {
        if (player)
        { return WhiteAvg; }
        else { return BlackAvg; }

    }

    // Get a Vector2Int
    // Set one of the avg positions to this value
    private void SetCurrentAvg(bool player, Vector2Int avg)
    {
        if (player)
        {
            WhiteAvg = avg;
        }
        else 
        {
            BlackAvg = avg;
        }
    }

    // Get a copy of a piece
    // Find the actuall object in the model
    private Piece FindPiece(Piece p)
    {
        foreach (Piece foundPiece in GetPiecesByBool(p.player))
        {
            if (foundPiece.position == p.position)
            {
                return foundPiece;
            }

        }

        // If not found return null
        return null;
    }

    // Get 2 indexes and check if they are legal
    public bool IsOnBoard(int x, int y)
    {
        return (x < 8 && x >= 0) && (y < 8 && y >= 0);
    }

    // Get a Certain piece and remove it from its list
    public void RemovePiece(Piece p)
    {
        // Use its "player" to get the right list
        GetPiecesByBool(p.player).Remove(p);
    }


    //-------------------------------------------- GraveYard ----------------------------------------------
    // Get a piece and return where it can go to using move arrays
    //public void PossibleMovesImproved(Piece p)
    //{
    //    p.possibles = new Move[8];
    //    p.amountOfMoves = 0;
    //    // y position is amount of pieces in this num of col
    //    // x position is number of pieces in this num of row
    //    int colmove = col[p.position.x];
    //    int rowmove = row[p.position.y];

    //    // Turn a position to the index of correct diagonal
    //    int pdiagmove = pdiagonal[p.position.x - p.position.y + 7];
    //    int sdiagmove = sdiagonal[p.position.y + p.position.x];

    //    // Check for col moves of piece
    //    OneLineMoves(p, new Vector2Int(p.position.x, p.position.y + colmove), new Vector2Int(0, 1));
    //    OneLineMoves(p, new Vector2Int(p.position.x, p.position.y - colmove), new Vector2Int(0, -1));

    //    // Check for row moves of piece
    //    OneLineMoves(p, new Vector2Int(p.position.x + rowmove, p.position.y), new Vector2Int(1, 0));
    //    OneLineMoves(p, new Vector2Int(p.position.x - rowmove, p.position.y), new Vector2Int(-1, 0));

    //    // Check for the primary diagonal of the piece
    //    OneLineMoves(p, new Vector2Int(p.position.x + pdiagmove, p.position.y + pdiagmove), new Vector2Int(1, 1));
    //    OneLineMoves(p, new Vector2Int(p.position.x - pdiagmove, p.position.y - pdiagmove), new Vector2Int(-1, -1));

    //    // Check for the secondery diagonal of the piece
    //    OneLineMoves(p, new Vector2Int(p.position.x + sdiagmove, p.position.y - sdiagmove), new Vector2Int(1, -1));
    //    OneLineMoves(p, new Vector2Int(p.position.x - sdiagmove, p.position.y + sdiagmove), new Vector2Int(-1, 1));
    //}

    //// Get a piece, an endpoint and a direction
    //// Add a new move to the pieces possible moves if said move is possible
    //public void OneLineMoves(Piece p, Vector2Int endPoint, Vector2Int dir)
    //{
    //    // Check for the column of this piece
    //    // End point is on the board?
    //    if (IsOnBoard(endPoint.x, endPoint.y))
    //    {
    //        // Are there enemy pieces i jump over?
    //        if (!board.IsEnemyBeforeHere(p.position, endPoint, dir, p.player))
    //        {
    //            // Is there a piece at the end?
    //            if (board.IsPieceHere(endPoint))
    //            {
    //                // Is this piece an enemy Piece?
    //                if (board.IsEnemy(endPoint, p.player))
    //                {
    //                    // Create a new attack move at this point, save on the piece,
    //                    p.possibles[p.amountOfMoves++] = new Move(p, endPoint, 0, true);
    //                }
    //            }
    //            else
    //            {
    //                // No piece at end point -> create a new normal move
    //                p.possibles[p.amountOfMoves++] = new Move(p, endPoint, 0, false);
    //            }
    //        }
    //    }
    //}
    //// Get a piece and update its position using the position its really on 
    //public void SetPostion(Piece p)
    //{
    //    // Get the board location of said piece
    //    LOAman lm = p.piece.GetComponent<LOAman>();
    //    // Add to that pieces list the new position
    //    GetPiecesByBool(p.player)[GetPiecesByBool(p.player).IndexOf(p)].position = new Vector2Int(lm.GetXBoard(), lm.GetYBoard()); ;
    //}
    //// Get a move 
    //// Make the move and update every data structure i use including removing pieces eaten
    //public void MakeMove(Move move, int depth)
    //{
    //    Piece p = GetPieceByIndex(move.pieceToMove.position);
    //    // If this is an attack move remove the piece eaten
    //    if (move.attack)
    //    {
    //        GetPiecesByBool(!move.pieceToMove.player).Remove(GetPieceByIndex(move.moveto));
    //    }
    //    // Update number arrays after a move is made
    //    //UpdateArrayNumbers(move.pieceToMove.position, move.moveto, move.attack);
    //    // Update all bit boards after a move is made
    //    //board.MakeMove(move.pieceToMove.position, move.moveto, move.pieceToMove.player);
    //    //p.position = new Vector2Int(move.moveto.x, move.moveto.y);
    //    try
    //    {
    //        //board.MakeMove(move.pieceToMove.position, move.moveto, move.pieceToMove.player);
    //        p.position = new Vector2Int(move.moveto.x, move.moveto.y);
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.LogError(" failed at " + move + " depth failed :" + depth);
    //        Debug.LogError("all pieces:");
    //        for (int i = 0; i < this.whites.Count; i++)
    //        {
    //            Debug.LogError(whites[i]);
    //        }
    //        for (int i = 0; i < this.blacks.Count; i++)
    //        {
    //            Debug.LogError(blacks[i]);
    //        }
    //        Debug.Break();
    //    }

    //}
    //public void UndoChangesArrayNumbers(Move before, Vector2Int after) 
    //{
    //    if (before.attack)
    //    {
    //        Vector2Int pos = before.pieceToMove.position;
    //        sdiagonal[pos.y + pos.x]++;
    //        row[pos.y]++;
    //        col[pos.x]++;
    //        pdiagonal[pos.y - pos.x + 7]++;
    //    }
    //    else 
    //    {
    //        //UpdateArrayNumbers(after, before.pieceToMove.position, false);
    //    }


    //}

}


