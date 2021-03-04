using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for all logical expression
public class Model
{
    // Hold pieces of each color that are in the game
    public List<Piece> whites { get; set; }
    public List<Piece> blacks { get; set; }
    // BitBoard object to do all logical actions
    public BitBoard board { get; set; }
    
    // Ctor for the model, will also be used to restart the game
    public void InitModel() 
    {
        whites = new List<Piece>();
        blacks = new List<Piece>();
        board = new BitBoard();
    }

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
    public Piece GetPieceByIndex(int x, int y)
    {
        // Save the position
        Vector2Int position = new Vector2Int(x, y);
        // Use bit operations to decide if its a white piece
        if (board.IsWhitePiece(x, y))
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
        else if (board.IsBlackPiece(x, y))
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

    // Check if that player won get the current player bool
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
            int index = pos.x + 8 * pos.y;
            // Check if said position hasnt been checked before
            if ((board.Getcheckedthis() & board.TurnIndexToBitBoard(index - 1)) == 0)
            {
                // Find the amount of of adjacent of pieces
                number = board.FindLines(index - 1, currentplayer);
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
    // Get a Certain piece and remove it from its list
    internal void RemovePiece(Piece p)
    {
        // Use its "player" to get the right list
        GetPiecesByBool(p.player).Remove(p);
    }

    // Get a piece and a position, update its position
    public void UpdatePosition(Piece p, Vector2Int after)
    {
        p.position = after;
    }

    // Get a piece and update its position using the position its really on 
    public void SetPostion(Piece p)
    {
        // Get the board location of said piece
        LOAman lm = p.piece.GetComponent<LOAman>();
        // Add to that pieces list the new position
        GetPiecesByBool(p.player)[GetPiecesByBool(p.player).IndexOf(p)].position = new Vector2Int(lm.GetXBoard(), lm.GetYBoard()); ;
    }

    // Get 2 indexes and check if they are legal
    public bool IsOnBoard(int x, int y)
    {
        return (x < 8 && x >= 0) && (y < 8 && y >= 0);
    }

    public void ChangePiecePosition(Move move) 
    {
        foreach (Piece p in GetPiecesByBool(move.pieceToMove.player))
        {
            if (p == move.pieceToMove) 
            {
                p.position = move.moveto;
                break;
            }
        }
        if (move.attack) 
        {
            GetPiecesByBool(!move.pieceToMove.player).Remove(GetPieceByIndex(move.moveto.x, move.moveto.y));
        }
        board.MakeMove(move.pieceToMove.position, move.moveto, move.pieceToMove.player);
    }

    public void UndoChangePosition(Move move) 
    {
        foreach (Piece p in GetPiecesByBool(move.pieceToMove.player))
        {
            if (p == move.pieceToMove)
            {
                p.position = move.moveto;
                break;
            }
        }
        if (move.attack)
        {
            GetPiecesByBool(!move.pieceToMove.player).Add(new Piece(move.moveto, !move.pieceToMove.player));
        }
        board.MakeMove(move.pieceToMove.position, move.moveto, move.pieceToMove.player);
        
    }

}

