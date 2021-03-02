﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// class to hold moves for ai, with score
public class Move 
{
    // Piece up for current move
    public Piece pieceToMove { get; set; }
    // Where to move the piece to
    public Vector2Int moveto { get; set; }
    // Score of the piece
    public int score { get; set; }
    // State of move, is it attacking something or not
    public bool attack { get; set; }

    // Ctor
    public Move(Piece pieceToMove, Vector2Int moveto, int score, bool attack)
    {
        this.moveto = moveto;
        this.pieceToMove = pieceToMove;
        this.score = score;
        this.attack = attack;
    }


}