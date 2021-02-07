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
    }

    // Used to transform given indexes of this pieces x and y to an actual good spot on the board
    public void SetCorods()
    {
        // Play around with this values to make them fir my board
        float x = xBoard, y = yBoard;
        x *= 1.255f;
        y *= 1.255f;
        x += -4.39f;
        y += -4.39f;
        // Put the piece in front of the board not behind it
        this.transform.position = new Vector3(x,y,-1f);
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
        if (controller.GetComponent<Game>().IsGameOver())
        {
            controller.GetComponent<Game>().CleanUp();
            controller.GetComponent<Game>().InitializeGame();
        }
        else if (controller.GetComponent<Game>().GetCurrentPlayer() == player)
        {
            DestroyMovePlates();
            InitMovePlates();
        }
    }

    public void InitMovePlates()
    {
        // Initiate all directions according to amound of pieces in the direction
        LineMovePlate(new Vector2Int(1,0));
        LineMovePlate(new Vector2Int(0, 1));
        LineMovePlate(new Vector2Int(1, 1));
        LineMovePlate(new Vector2Int(-1, 1));
    }

    private void LineMovePlate(Vector2Int dir)
    {
        Model model = controller.GetComponent<Game>().model;
        int x = xBoard + dir.x;
        int y = yBoard + dir.y;
        int antiY = yBoard - dir.y;
        int antiX = xBoard - dir.x;
        int enemy1x = 8, enemy1y= 8, enemy2x= 8, enemy2y =8;
        bool flag1 = true, flag2 = true;

        int counter = 1;
        while (model.IsOnBoard(x, y) || model.IsOnBoard(antiX,antiY)) 
        {
            if (model.IsOnBoard(x, y))
            {
                if (model.board.IsPieceHere(x, y))
                {
                    if (model.GetPieceByIndex(x, y).player != player && flag1)
                    {
                        enemy1x = x;
                        enemy1y = y;
                        flag1 = false;
                    }
                    counter++;
                }
            }

            if (model.IsOnBoard(antiX, antiY))
            {
                if (model.board.IsPieceHere(antiX, antiY))
                {
                    if (model.GetPieceByIndex(antiX, antiY).player != player && flag2)
                    {
                        enemy2x = antiX;
                        enemy2y = antiY;
                        flag2 = false;
                    }
                    counter++;
                }
            }
            x += dir.x;
            y += dir.y;
            antiX -= dir.x;
            antiY -= dir.y;
        }


        x = xBoard + (counter * dir.x);
        y = yBoard + (counter * dir.y);
        if (model.IsOnBoard(x, y) && (IsBefore(x,y,enemy1x,enemy1y) || flag1))
        {
            if (!model.board.IsPieceHere(x, y))
            {
                MovePlateSpawn(x, y, false);
            }
            else if (model.GetPieceByIndex(x, y).player != player)
            {
                MovePlateSpawn(x, y, true);
            }
        }


        antiX = xBoard + (counter * - dir.x);
        antiY = yBoard + (counter * - dir.y);
        if (model.IsOnBoard(antiX, antiY) && (IsBefore(antiX,antiY,enemy2x,enemy2y) || flag2))
        {
            if (!model.board.IsPieceHere(antiX, antiY))
            {
                MovePlateSpawn(antiX, antiY, false);
            }
            else if (model.GetPieceByIndex(antiX, antiY).player != player)
            {
                MovePlateSpawn(antiX, antiY, true);
            }
        }
    }

    private bool IsBefore(int x, int y, int enemyx, int enemyy) 
    {
        int dist1x = x - xBoard, dist2x = enemyx - xBoard, dist1y = y - yBoard, dist2y = enemyy - yBoard;
        return Math.Abs(dist1x) <= Math.Abs(dist2x) && Math.Abs(dist1y) <= Math.Abs(dist2y);
    }

    private void MovePlateSpawn(int matirxX, int matirxY, bool attack)
    {
        float x = matirxX;
        float y = matirxY;
        x *= 1.255f;
        y *= 1.255f;
        x += -4.39f;
        y += -4.39f;
        GameObject mp = Instantiate(moveplate, new Vector3(x, y, -3f), Quaternion.identity);
        MovePlate mpScript = mp.GetComponent<MovePlate>();
        if (attack)
        {
            mpScript.attack = true;
        }
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matirxX, matirxY);
        
    }

    public void DestroyMovePlates() 
    {
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        for (int i = 0; i < movePlates.Length; i++)
        {
            Destroy(movePlates[i]);
        }
    }

}
