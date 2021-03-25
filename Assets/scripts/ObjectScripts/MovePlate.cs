using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlate : MonoBehaviour
{
    // --------------------------------------------------------------- Variables ------------------------------------------------------------------
    // Game object to hold the controller for unity
    public GameObject controller;
    // The reference for the piece im moving on mouse click
    GameObject reference = null;
    // Board positions
    int matrixX, matrixY;
    // Attack = true means can eat a piece
    public bool attack = false;

    // --------------------------------------------------------------- Core Methods ----------------------------------------------------------------

    // Change color of the move plate to red the 
    public void Start()
    {
        if (attack) 
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f, 1f);
        }
    }

    // Move plate was chosen start to move the piece selected and check if piece should be eaten
    public void OnMouseUp()
    {
        // Find the controller for the game
        controller = GameObject.FindGameObjectWithTag("GameController");
        Vector2Int before = new Vector2Int(reference.GetComponent<LOAman>().GetXBoard(), reference.GetComponent<LOAman>().GetYBoard());
        Vector2Int after = new Vector2Int(matrixX, matrixY);
        // Get the model
        Model model = controller.GetComponent<Game>().model;
        // Find the piece im trying to move
        Piece BeforePiece = model.GetPieceByIndex(before);
        controller.GetComponent<Game>().MoveAPieceInUnity(new Move(BeforePiece, after, 0, attack));
    }
    

    // --------------------------------------------------------------- getter / setter --------------------------------------------------------------


    // Set the position for a move plate using a 2d vector
    public void SetCoords(Vector2Int pos) 
    {
        matrixX = pos.x;
        matrixY = pos.y;
    }

    // Set the reference for the piece using a game object
    public void SetReference(GameObject obj) 
    {
        reference = obj;
    }

}
