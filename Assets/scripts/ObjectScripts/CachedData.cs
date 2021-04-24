using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CachedData 
{
    public int depth { get; set; }
    public int score { get; set; }
    //public Move bestmove { get; set; }
    public AI.Flag flag { get; set; }


    public CachedData() 
    {
        depth = 0;
        score = 0;
        //bestmove = new Move();
        flag = AI.Flag.VALUE;
    }
}
