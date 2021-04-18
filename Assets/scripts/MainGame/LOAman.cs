using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LOAman : MonoBehaviour
{
    public GameObject controller;
    public GameObject moveplate;
    private int xBoard = -1, yBoard = -1;
    // true = white
    public bool player = true;
    public Sprite BlackPawn;
    public Sprite WhitePawn;
    public Model m;
    public Piece p;
    public bool flag = true;
    private float movetox = -1, movetoy = -1;
    private float startingx = -1, startingy = -1;

    public void Update()
    {
        if (movetox != -1 && movetoy != -1) 
        {
            if (this.transform.position.x != movetox || this.transform.position.y != movetoy)
            {
                float step = 10f * Time.deltaTime;

                this.transform.position = Vector3.MoveTowards(new Vector3(startingx, startingy, -1f), new Vector3(movetox, movetoy, -1f), step);
                startingx = this.transform.position.x;
                startingy = this.transform.position.y;
            }
            else 
            {
                float x = movetox;
                float y = movetoy;
                x /= 1.255f;
                y /= 1.255f;
                x -= -4.39f;
                y -= -4.39f;
                SetXBoard((int)x);
                SetYBoard((int)y);
                movetox = -1;
                movetoy = -1;
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
        // Changed this bool when i made this LOAman object
        if (this.player)
        {
            this.GetComponent<SpriteRenderer>().sprite = WhitePawn;
        }
        else 
        {
            this.GetComponent<SpriteRenderer>().sprite = BlackPawn;
        }
        m = controller.GetComponent<Game>().model;
    }

    // Used to transform given indexes of this pieces x and y to an actual good spot on the board
    public void SetCorods()
    {
        // Play around with this values to make them for my board
        float x = xBoard, y = yBoard;
        x *= 1.255f;
        y *= 1.255f;
        x += -4.39f;
        y += -4.39f;
        
        // Put the piece in front of the board not behind it
        this.transform.position = new Vector3(x, y, -1f);
    }

    public int GetXBoard()
    {
        return xBoard;
    }

    public int GetYBoard()
    {
        return yBoard;
    }

    public void SetXBoard(int x)
    {
        xBoard = x;
    }

    public void SetYBoard(int y)
    {
        yBoard = y;
    }

    private void OnMouseUp()
    {
        bool endgame = controller.GetComponent<Game>().IsGameOver();
        // Activate only the first time, find the piece and save it
        if (flag) 
        {
            p = m.GetPieceByIndex(new Vector2Int(xBoard, yBoard));
            flag = false;
        }
        if (endgame)
        {
            controller.GetComponent<Game>().RestartGame();
        }
        // If this is the current players piece and we dont use ai then continue
        if (player == controller.GetComponent<Game>().GetCurrentPlayer() && !endgame)
        {
            DestroyMovePlates();
            List<Move> moves = new List<Move>(96);
            m.FutureMovesImproved(p, moves);
            //m.PossibleMovesImproved(p);
            foreach (Move possiblemove in moves)
            {
                MovePlateSpawn(possiblemove);
            }

        }
    }

    // get vector for position and check of its legit for moves
    public bool CheckLegit(Vector2Int v)
    {
        if (m.IsOnBoard(v.x, v.y))
        {
            if (m.board.IsPieceHere(v))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsBefore(int x, int y, int enemyx, int enemyy) 
    {
        int dist1x = x - xBoard, dist2x = enemyx - xBoard, dist1y = y - yBoard, dist2y = enemyy - yBoard;
        return Math.Abs(dist1x) <= Math.Abs(dist2x) && Math.Abs(dist1y) <= Math.Abs(dist2y);
    }

    // Create the move plate sprites using the positions deemed possible to move to
    private void MovePlateSpawn(Move m)
    {
        float x = m.moveto.x;
        float y = m.moveto.y;
        x *= 1.255f;
        y *= 1.255f;
        x += -4.39f;
        y += -4.39f;
        GameObject mp = Instantiate(moveplate, new Vector3(x, y, -3f), Quaternion.identity);
        MovePlate mpScript = mp.GetComponent<MovePlate>();
        if (m.attack)
        {
            mpScript.attack = true;
        }
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(m.moveto);
        
    }

    public void DestroyMovePlates() 
    {
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        for (int i = 0; i < movePlates.Length; i++)
        {
            Destroy(movePlates[i]);
        }
    }

    public void StartMoving(Move move) 
    {

        startingy = yBoard;
        startingx = xBoard;
        startingx *= 1.255f;
        startingy *= 1.255f;
        startingx += -4.39f;
        startingy += -4.39f;

        movetox = move.moveto.x;
        movetoy = move.moveto.y;
        movetox *= 1.255f;
        movetoy *= 1.255f;
        movetox += -4.39f;
        movetoy += -4.39f;
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


}

