using UnityEngine;

// class to hold moves for ai, with score
public class Move 
{
    // -------------------------------------------------------- Variables ------------------------------------------------------------------
    // Piece up for current move
    public Piece pieceToMove { get; set; }
    // Where to move the piece to
    public Vector2Int moveto { get; set; }
    // Score of the piece
    public int score { get; set; }
    // State of move, is it attacking something or not
    public bool attack { get; set; }


    // --------------------------------------------------------- Constructors ---------------------------------------------------------------
    
    // Normal contructor that recieves all the atributes a move has
    public Move(Piece pieceToMove, Vector2Int moveto, int score, bool attack)
    {
        this.moveto = moveto;
        this.pieceToMove = pieceToMove;
        this.score = score;
        this.attack = attack;
    }

    // Empty contructor that initilizes every variable to 0 or null
    public Move() 
    {
        this.moveto = new Vector2Int(0, 0);
        this.pieceToMove = null;
        this.score = int.MinValue;
        this.attack = false;
    }

    // Copy Constructor to copy a given move value wise (deep copy)
    public Move(Move copythis)
    {
        this.moveto = new Vector2Int(copythis.moveto.x, copythis.moveto.y);
        this.pieceToMove = new Piece(copythis.pieceToMove);
        this.score = copythis.score;
        this.attack = copythis.attack;
    }



    // -------------------------------------------------------- Utility Functions ------------------------------------------------------------

    //A to string function that shows starting location and an end location
    public override string ToString()
    {
        return this.pieceToMove.ToString() + " -> " + this.moveto.ToString();
    }
}
