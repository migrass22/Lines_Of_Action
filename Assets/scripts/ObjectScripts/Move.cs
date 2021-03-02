using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// class to hold moves for ai, with score
public class Move 
{
    // piece up for current move
    public Piece p { get; set; }
    // score of the piece
    public int score { get; set; }

    public Move(Piece p, int score)
    {
        this.p = p;
        this.score = score;
    }


}
