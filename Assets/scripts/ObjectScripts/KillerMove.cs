using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillerMove
{
    public int weight = 0;
    public Move move;
    public bool found;


    public KillerMove(Move move, int weight)
    {
        this.move = new Move(move);
        this.weight = weight;
        this.found = false;
    }

    // Increase weight of move
    public void IncreaseWeight()
    {
        this.weight++;
    }
    // decrease weight of move
    public void DecreaseWeight()
    {
        this.weight--;
    }

    // A to string function that shows starting location and an end location
    public override string ToString()
    {
        return this.move.pieceToMove.ToString() + " -> " + this.move.moveto.ToString();
    }
}
