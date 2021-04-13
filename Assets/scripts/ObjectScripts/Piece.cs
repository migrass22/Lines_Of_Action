using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class to describe a single piece on the board
public class Piece
{
    // -------------------------------------------- Variables ----------------------------------------------
    public GameObject piece { get; set; }
    public Vector2Int position { get; set; }
    public bool player { get; set; }

    // ------------------------------------------ Constructors ---------------------------------------------
    public Piece(GameObject InGamePiece, Vector2Int position, bool player) 
    {
        // The actual gamepiece in unity
        this.piece = InGamePiece;
        // Its relative position
        this.position = position;
        // The player this piece belongs to
        this.player = player;
    }

    public Piece(Vector2Int position, bool player) 
    {
        // Its relative position
        this.position = position;
        // The player this piece belongs to
        this.player = player;
    }

    public Piece(Piece copythis)
    {
        this.piece = copythis.piece;
        this.player = copythis.player;
        this.position = new Vector2Int(copythis.position.x, copythis.position.y);
    }

    // ----------------------------------------------- Utility ---------------------------------------------

    public override string ToString()
    {
        return this.position.ToString() + this.player.ToString();
    }

}
