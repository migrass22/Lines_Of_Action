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
    public int[] row = { 6, 2, 2, 2, 2, 2, 2, 6 };

    public int[] col = { 6, 2, 2, 2, 2, 2, 2, 6 };

    // Primary diagonal
    public int[] pdiagonal = { 0, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 0 };

    // Secondary diagonal
    public int[] sdiagonal = { 0, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 0 };

    // ----------------------------------- Constructors ----------------------------------

    // Ctor for the model, will also be used to restart the game
    public Model() 
    {
        whites = new List<Piece>();
        blacks = new List<Piece>();
        board = new BitBoard();
        int []temprow = { 6, 2, 2, 2, 2, 2, 2, 6 };
        this.row = temprow;
        int[] tempcol = { 6, 2, 2, 2, 2, 2, 2, 6 };
        this.col = tempcol;
        int[] temppdiagnoal = { 0, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 0 };
        this.pdiagonal = temppdiagnoal;
        int[] tempsdiagonal = { 0, 2, 2, 2, 2, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 0 };
        this.sdiagonal = tempsdiagonal;
    }

    // Copy ctor for a model to a second model
    public Model(Model copythis)
    {
        // Init model as empty
        this.whites = new List<Piece>();
        this.blacks = new List<Piece>();
        this.board = new BitBoard();

        // Deep copy each piece
        foreach (Piece p in copythis.whites)
        {
            this.whites.Add(new Piece(p.position, p.player));
        }
        foreach (Piece p in copythis.blacks)
        {
            this.blacks.Add(new Piece(p.position, p.player));
        }
        this.board.CopyThisBoard(copythis.board);
        CopyNumberArrays(copythis);
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

    // Get a piece and a position, update its position
    public void SetPiecePosition(Piece p, Vector2Int after)
    {
        p.position = after;
    }

    // Get a Certain piece and remove it from its list
    public void RemovePiece(Piece p)
    {
        // Use its "player" to get the right list
        GetPiecesByBool(p.player).Remove(p);
    }

    // Get a player's bool
    // Check if the given player won 
    public bool checkwin(bool currentplayer)
    {
        int max = 0;
        int number = 0;
        board.InitCheckedThis();
        // If the number of any players piece is 1 than the game is finished
        if (GetPiecesByBool(currentplayer).Count == 1) { return true; }
        // Go over the pieces of the player im checking
        foreach (Piece p in GetPiecesByBool(currentplayer))
        {
            // Save the current pieces position and the corrospondaning index
            Vector2Int pos = p.position;
            int index = board.PositionToIndex(pos);
            // Check if said position hasnt been checked before
            if ((board.Getcheckedthis() & board.TurnIndexToBitBoard(index)) == 0)
            {
                // Find the amount of of adjacent of pieces
                number = board.FindLines(index, currentplayer);
                // If the number is bigger than saved max than change it
                if (max < number)
                {
                    max = number;
                }
            }
        }
        // Check for the current players pieces if the max number found is the same as the count of the pieces the game is over
        if (currentplayer)
        {
            if (max == whites.Count) { return true; }
        }
        else
        {
            if (max == blacks.Count) { return true; }
        }
        return false;
    }

    // Get 2 indexes and check if they are legal
    public bool IsOnBoard(int x, int y)
    {
        return (x < 8 && x >= 0) && (y < 8 && y >= 0);
    }

    // Method that gets a move 
    // Make the move and update every data structure i use including removing pieces eaten
    public void MakeMove(Move move) 
    {
        // If this is an attack move remove the piece eaten
        if (move.attack)
        {
            GetPiecesByBool(!move.pieceToMove.player).Remove(GetPieceByIndex(move.moveto));
        }
        Piece p = GetPieceByIndex(move.pieceToMove.position);
        // Update number arrays after a move is made
        UpdateArrayNumbers(move.pieceToMove.position, move.moveto, move.attack);
        // Update all bit boards after a move is made
        board.MakeMove(move.pieceToMove.position, move.moveto, move.pieceToMove.player);
        p.position = new Vector2Int(move.moveto.x, move.moveto.y);
        //try
        //{
        //    board.MakeMove(move.pieceToMove.position, move.moveto, move.pieceToMove.player);
        //    p.position = new Vector2Int(move.moveto.x, move.moveto.y);
        //} 
        //catch (Exception e) 
        //{
        //    Debug.LogError(" failed at " + move);
        //}

    }

    public void UndoChangePosition(Move move) 
    {
        if (move.attack)
        {
            GetPiecesByBool(!move.pieceToMove.player).Add(new Piece(move.moveto, !move.pieceToMove.player));
        }
        Piece p = GetPieceByIndex(move.moveto);
        board.undomove(move.moveto, move.pieceToMove.position, move.pieceToMove.player, move.attack);
        UndoChangesArrayNumbers(new Move(move.pieceToMove.position, move.attack), move);
        p.position = new Vector2Int(move.pieceToMove.position.x, move.pieceToMove.position.y);
        move.Child = new List<Move>();

    }


    // Trying to improve the move generation


    // Method to change amount of pieces in move arrays
    // After a move is made
    public void UpdateArrayNumbers(Vector2Int start, Vector2Int end, bool attack)
    {

        Vector2Int dir = new Vector2Int(end.x - start.x, end.y - start.y);
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

        // Reapet everything below this for every other direction
        // Things can change 6 times or 4 times depending if eating piece or not
        // Check if the move was made in a row
        if (dir == new Vector2Int(1, 0) || dir == new Vector2Int(-1, 0))
        {
            if (attack)
            {
                // Only if a piece was eaten then the amount of pieces goes down
                row[end.y]--;
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
            if (attack)
            {
                col[end.x]--;
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
            if (attack)
            {
                pdiagonal[end.y - end.x + 7]--;
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
            if (attack)
            {
                sdiagonal[end.y + end.x]--;
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

    public void UndoChangesArrayNumbers(Move before, Move after) 
    {
        UpdateArrayNumbers(after.moveto, before.moveto, false);
        if (before.attack) 
        {
            sdiagonal[after.moveto.y + after.moveto.x]++;
            row[after.moveto.y]++;
            col[after.moveto.x]++;
            pdiagonal[after.moveto.y - after.moveto.x + 7]++;
        }
    }

    // Get a piece and return where it can go to using move arrays
    public void FutureMovesImproved(Move move)
    {
        Vector2Int position = move.pieceToMove.position;
        // y position is amount of pieces in this num of col
        // x position is number of pieces in this num of row
        int colmove = col[position.x];
        int rowmove = row[position.y];
        // Turn a position to the index of correct diagonal
        int pdiagmove = pdiagonal[position.y - position.x + 7];
        int sdiagmove = sdiagonal[position.y + position.x];

        // Check for col moves of piece
        MoveInAline(move, new Vector2Int(position.x, position.y + colmove), new Vector2Int(0, 1));
        MoveInAline(move, new Vector2Int(position.x, position.y - colmove), new Vector2Int(0, -1));

        // Check for row moves of piece
        MoveInAline(move, new Vector2Int(position.x + rowmove, position.y), new Vector2Int(1, 0));
        MoveInAline(move, new Vector2Int(position.x - rowmove, position.y), new Vector2Int(-1, 0));

        // Check for the primary diagonal of the piece
        MoveInAline(move, new Vector2Int(position.x + pdiagmove, position.y + pdiagmove), new Vector2Int(1, 1));
        MoveInAline(move, new Vector2Int(position.x - pdiagmove, position.y - pdiagmove), new Vector2Int(-1, -1));

        // Check for the secondery diagonal of the piece
        MoveInAline(move, new Vector2Int(position.x + sdiagmove, position.y - sdiagmove), new Vector2Int(1, -1));
        MoveInAline(move, new Vector2Int(position.x - sdiagmove, position.y + sdiagmove), new Vector2Int(-1, 1));

    }

    // Get a piece, an endpoint and a direction
    // Add a new move to the pieces possible moves if said move is possible
    public void MoveInAline(Move move, Vector2Int endPoint, Vector2Int dir)
    {
        // Check for the column of this piece
        // End point is on the board?
        if (IsOnBoard(endPoint.x, endPoint.y))
        {
            // Are there enemy pieces i jump over?
            if (!board.IsEnemyBeforeHere(move.pieceToMove.position, endPoint, dir, move.pieceToMove.player))
            {
                // Is there a piece at the end?
                if (board.IsPieceHere(endPoint))
                {
                    // Is this piece an enemy Piece?
                    if (board.IsEnemy(endPoint, move.pieceToMove.player))
                    {
                        // Create a new attack move at this point, save on the piece,
                        move.Child.Add(new Move(move.pieceToMove, endPoint, 0, true));
                    }
                }
                else
                {
                    // No piece at end point -> create a new normal move
                    move.Child.Add(new Move(move.pieceToMove, endPoint, 0, false));
                }
            }
        }
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

}


