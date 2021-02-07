using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class to describe a single piece on the board
public class Piece
{
    public GameObject piece { get; set; }
    public Vector2Int position { get; set; }
    public bool player { get; set; }

    public Piece(GameObject InGamePiece, Vector2Int position, bool player) 
    {
        // The actual gamepiece in unity
        this.piece = InGamePiece;
        // Its relative position
        this.position = position;
        // The player this piece belongs to
        this.player = player;
    }

}
