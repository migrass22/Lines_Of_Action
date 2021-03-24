using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlate : MonoBehaviour
{
    public GameObject controller;
    GameObject reference = null;
    // Board positions
    int matrixX, matrixY;
    // Can eat something by moving 
    public bool attack = false;


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

    public void SetCoords(int x, int y) 
    {
        matrixX = x;
        matrixY = y;
    }

    public void SetReference(GameObject obj) 
    {
        reference = obj;
    }

    public GameObject GetRefrence() 
    {
        return reference;
    }
}
