using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move 
{
    public Piece p { get; set; }
    public int score { get; set; }

    public Move(Piece p, int score)
    {
        this.p = p;
        this.score = score;
    }


}
