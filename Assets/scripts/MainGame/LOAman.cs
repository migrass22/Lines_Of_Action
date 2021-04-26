using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LOAman : MonoBehaviour
{
    // Main "Game" class that controlls everything
    public GameObject controller;
    // Game object of a single moveplate
    public GameObject moveplate;
    // The current position of the piece
    private Vector2Int pos = new Vector2Int(-1,-1);
    // True = white -> current piece's color
    public bool player = true;
    // Sprites of both pieces
    public Sprite BlackPawn;
    public Sprite WhitePawn;
    // The main model of the game
    public Model model;
    // The actuall object of the current unity piece
    public Piece p;
    // Flag to save for the first time the current piece
    public bool flag = true;
    // Actuall unity space position (end position if piece needs to be moved)
    private Vector2 starting = new Vector2(-1, -1);
    private Vector2 moveto = new Vector2(-1, -1);


    // Using update method to animate the movement of the piece on the board
    public void Update()
    {
        // Since update happens every second only enter this block code once the piece has to move
        if (moveto.y != -1 && moveto.x != -1) 
        {
            // If the position is no already achived by current piece
            if (this.transform.position.x != moveto.x || this.transform.position.y != moveto.y)
            {
                // Create a step to move the piece a portion of the way as so to animate it each interval of update
                float step = 10f * Time.deltaTime;
                // Update position on board and in script
                this.transform.position = Vector3.MoveTowards(new Vector3(starting.x, starting.y, -1f), new Vector3(moveto.x, moveto.y, -1f), step);
                starting.x = this.transform.position.x;
                starting.y = this.transform.position.y;
            }
            else 
            {
                // After getting to end lcoation save current index
                float x = moveto.x;
                float y = moveto.y;
                x /= 1.255f;
                y /= 1.255f;
                x -= -4.39f;
                y -= -4.39f;
                SetXBoard((int)x);
                SetYBoard((int)y);
                // Change end position to not enter this block code again
                moveto.x = -1;
                moveto.y = -1;
            }
        }
    }

    // Used to make a piece show on screen
    public void Activate()
    {
        // Get the main script
        controller = GameObject.FindGameObjectWithTag("GameController");
        // make the piece show on the right spot with the right sprite for this current piece
        SetCorods();
        // Based on this players color give the right sprite
        if (this.player)
        {
            this.GetComponent<SpriteRenderer>().sprite = WhitePawn;
        }
        else 
        {
            this.GetComponent<SpriteRenderer>().sprite = BlackPawn;
        }
        // Save main game model
        model = controller.GetComponent<Game>().model;
    }

    // Used to transform given indexes of this pieces x and y to an actual good spot on the board
    public void SetCorods()
    {
        // Play around with this values to make them for my board
        float x = pos.x, y = pos.y;
        x *= 1.255f;
        y *= 1.255f;
        x += -4.39f;
        y += -4.39f;
        
        // Put the piece in front of the board not behind it
        this.transform.position = new Vector3(x, y, -1f);
    }

    public int GetXBoard()
    {
        return pos.x;
    }

    public int GetYBoard()
    {
        return pos.y;
    }

    public void SetXBoard(int x)
    {
        pos.x = x;
    }

    public void SetYBoard(int y)
    {
        pos.y = y;
    }

    // When a piece is pressed check for endgame
    // If not end game then generate appropriate move plates for the human player to press
    private void OnMouseUp()
    {
        bool endgame = controller.GetComponent<Game>().IsGameOver();
        // Activate only the first time, find the piece and save it to this unity piece
        if (flag) 
        {
            p = model.GetPieceByIndex(pos);
            flag = false;
        }
        // If the game ended and the piece is pressed restart the game
        if (endgame)
        {
            controller.GetComponent<Game>().RestartGame();
        }
        // If this is the current players piece and we dont use ai then continue
        if (player == controller.GetComponent<Game>().GetCurrentPlayer() && !endgame)
        {
            // Destroy any last placed move plates on the board
            DestroyMovePlates();
            // Create and save all possible moves (maximum 8 moves) to this piece using number arrays
            List<Move> moves = new List<Move>(8);
            model.FutureMovesImproved(p, moves);
            //m.PossibleMovesImproved(p);
            foreach (Move possiblemove in moves)
            {
                // Create said move plate based on the gereated move
                MovePlateSpawn(possiblemove);
            }

        }
    }

    // Create the move plate sprites using the positions deemed possible to move to
    private void MovePlateSpawn(Move m)
    {
        // Change the given indexes on the board to location in unity space that match the boards location
        float x = m.moveto.x;
        float y = m.moveto.y;
        x *= 1.255f;
        y *= 1.255f;
        x += -4.39f;
        y += -4.39f;
        // Create a move plate
        GameObject mp = Instantiate(moveplate, new Vector3(x, y, -3f), Quaternion.identity);
        MovePlate mpScript = mp.GetComponent<MovePlate>();
        // Call script to change move plates color if move is an attack move
        if (m.attack)
        {
            mpScript.attack = true;
        }
        // Save the origin piece game object in the move plate script
        mpScript.SetReference(gameObject);
        // Set move plate location according to move
        mpScript.SetCoords(m.moveto);
        
    }

    // Destroy all existing move plates in the unity space
    public void DestroyMovePlates() 
    {
        // Get all move plates using tags and unity function
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        for (int i = 0; i < movePlates.Length; i++)
        {
            // Destroy each and every one
            Destroy(movePlates[i]);
        }
    }

    // Based on a selected move, start moving the corrosponding piece in unity
    public void StartMoving(Move move) 
    {
        // Save starting position
        starting = pos;
        starting *= 1.255f;
        starting.x +=  - 4.39f;
        starting.y +=  - 4.39f;
        // Save end position
        moveto = move.moveto;
        moveto *= 1.255f;
        moveto.x += -4.39f;
        moveto.y += -4.39f;
    }


    // ----------------------------------------------- GraveYard -------------------------------------------
    //private void LineMovePlate(Vector2Int dir)
    //{
    //    Model model = controller.GetComponent<Game>().model;
    //    int x = xBoard + dir.x;
    //    int y = yBoard + dir.y;
    //    int antiY = yBoard - dir.y;
    //    int antiX = xBoard - dir.x;
    //    int enemy1x = 8, enemy1y= 8, enemy2x= 8, enemy2y =8;
    //    bool flag1 = true, flag2 = true;

    //    int counter = 1;
    //    while (model.IsOnBoard(x, y) || model.IsOnBoard(antiX,antiY)) 
    //    {
    //        if (model.IsOnBoard(x, y))
    //        {
    //            if (model.board.IsPieceHere(new Vector2Int(x,y)))
    //            {
    //                if (model.board.IsEnemy(new Vector2Int(x, y), player) && flag1)
    //                {
    //                    enemy1x = x;
    //                    enemy1y = y;
    //                    flag1 = false;
    //                }
    //                counter++;
    //            }
    //        }

    //        if (model.IsOnBoard(antiX, antiY))
    //        {
    //            if (model.board.IsPieceHere(new Vector2Int(antiX, antiY)))
    //            {
    //                if (model.GetPieceByIndex(antiX, antiY).player != player && flag2)
    //                {
    //                    enemy2x = antiX;
    //                    enemy2y = antiY;
    //                    flag2 = false;
    //                }
    //                counter++;
    //            }
    //        }
    //        x += dir.x;
    //        y += dir.y;
    //        antiX -= dir.x;
    //        antiY -= dir.y;
    //    }


    //    x = xBoard + (counter * dir.x);
    //    y = yBoard + (counter * dir.y);
    //    MakeTypePlates(new Vector2Int(x, y), new Vector2Int(enemy1x, enemy1y), flag1);


    //    antiX = xBoard + (counter * - dir.x);
    //    antiY = yBoard + (counter * - dir.y);
    //    MakeTypePlates(new Vector2Int(antiX, antiY), new Vector2Int(enemy2x, enemy2y), flag2);
    //}
    //public void InitMovePlates()
    //{
    //    // Initiate all directions according to amound of pieces in the direction
    //    LineMovePlate(new Vector2Int(1,0));
    //    LineMovePlate(new Vector2Int(0, 1));
    //    LineMovePlate(new Vector2Int(1, 1));
    //    LineMovePlate(new Vector2Int(-1, 1));
    //}
    //// Get a piece and return where it can go to using move arrays
    //public void PossibleMovesImproved()
    //{
    //    if (p.amountOfMoves == 0)
    //    {
    //        // y position is amount of pieces in this num of col
    //        // x position is number of pieces in this num of row
    //        int colmove = m.col[p.position.x];
    //        int rowmove = m.row[p.position.y];
    //        // Turn a position to the index of correct diagonal
    //        int pdiagmove = m.pdiagonal[p.position.x - p.position.y + 7];
    //        int sdiagmove = m.sdiagonal[p.position.y + p.position.x];

    //        // Check for col moves of piece
    //        OneLineMoves(p, new Vector2Int(p.position.x, p.position.y + colmove), new Vector2Int(0, 1));
    //        OneLineMoves(p, new Vector2Int(p.position.x, p.position.y - colmove), new Vector2Int(0, -1));

    //        // Check for row moves of piece
    //        OneLineMoves(p, new Vector2Int(p.position.x + rowmove, p.position.y), new Vector2Int(1, 0));
    //        OneLineMoves(p, new Vector2Int(p.position.x - rowmove, p.position.y), new Vector2Int(-1, 0));

    //        // Check for the primary diagonal of the piece
    //        OneLineMoves(p, new Vector2Int(p.position.x + pdiagmove, p.position.y + pdiagmove), new Vector2Int(1, 1));
    //        OneLineMoves(p, new Vector2Int(p.position.x - pdiagmove, p.position.y - pdiagmove), new Vector2Int(-1, -1));

    //        // Check for the secondery diagonal of the piece
    //        OneLineMoves(p, new Vector2Int(p.position.x + sdiagmove, p.position.y - sdiagmove), new Vector2Int(1, -1));
    //        OneLineMoves(p, new Vector2Int(p.position.x - sdiagmove, p.position.y + sdiagmove), new Vector2Int(-1, 1));

    //    }

    //}

    //// Get a piece, an endpoint and a direction
    //// Add a new move to the pieces possible moves if said move is possible
    //public void OneLineMoves(Piece p, Vector2Int endPoint, Vector2Int dir) 
    //{
    //    // Check for the column of this piece
    //    // End point is on the board?
    //    if (m.IsOnBoard(endPoint.x, endPoint.y))
    //    {
    //        // Are there enemy pieces i jump over?
    //        if (!m.board.IsEnemyBeforeHere(p.position, endPoint, dir, player))
    //        {
    //            // Is there a piece at the end?
    //            if (m.board.IsPieceHere(endPoint))
    //            {
    //                // Is this piece an enemy Piece?
    //                if (m.board.IsEnemy(endPoint, player))
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
    //// Decide based on positions if the plate created should be attack one or normal one
    //public void MakeTypePlates(Vector2Int pos, Vector2Int enemy, bool flag)
    //{

    //    if (m.IsOnBoard(pos.x, pos.y) && (IsBefore(pos.x, pos.y, enemy.x, enemy.y) || flag))
    //    {
    //        if (!m.board.IsPieceHere(pos))
    //        {
    //            MovePlateSpawn(new Move(pos, false));

    //        }
    //        else if (m.GetPieceByIndex(pos).player != player)
    //        {
    //            MovePlateSpawn(new Move(pos, true));
    //        }
    //    }
    //}
    //public bool IsBefore(int x, int y, int enemyx, int enemyy) 
    //{
    //    int dist1x = x - xBoard, dist2x = enemyx - xBoard, dist1y = y - yBoard, dist2y = enemyy - yBoard;
    //    return Math.Abs(dist1x) <= Math.Abs(dist2x) && Math.Abs(dist1y) <= Math.Abs(dist2y);
    //}
    //// get vector for position and check of its legit for moves
    //public bool CheckLegit(Vector2Int v)
    //{
    //    if (model.IsOnBoard(v.x, v.y))
    //    {
    //        if (model.board.IsPieceHere(v))
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
}

