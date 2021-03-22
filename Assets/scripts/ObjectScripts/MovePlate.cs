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
        // Create position before the move and after the move
        Vector2Int before = new Vector2Int(reference.GetComponent<LOAman>().GetXBoard(), reference.GetComponent<LOAman>().GetYBoard());
        Vector2Int after = new Vector2Int(matrixX, matrixY);
        // Find the controller for the game
        controller = GameObject.FindGameObjectWithTag("GameController");
        // Get the model
        Model model = controller.GetComponent<Game>().model;
        // Find the piece im trying to move
        Piece BeforePiece = model.GetPieceByIndex(before.x, before.y);
        // Piece is moved so possible moves should be reset 
        BeforePiece.possibles = new Move[8];
        BeforePiece.amountOfMoves = 0;
        Piece AfterPiece = model.GetPieceByIndex(after.x, after.y);
        // Check for attack on the piece
        if (attack) 
        {
            model.RemovePiece(AfterPiece);
            model.board.MakeMove(after, after, !controller.GetComponent<Game>().GetCurrentPlayer());
            Destroy(AfterPiece.piece);
        }
        model.UpdateArrayNumbers(before, after, attack);
        reference.GetComponent<LOAman>().SetXBoard(matrixX);
        reference.GetComponent<LOAman>().SetYBoard(matrixY);
        reference.GetComponent<LOAman>().SetCorods();
        model.UpdatePosition(BeforePiece, after);
        reference.GetComponent<LOAman>().DestroyMovePlates();
        model.board.MakeMove(before, after, controller.GetComponent<Game>().GetCurrentPlayer());
        model.board.SetBitBoard(model.board.GetWhites() | model.board.GetBlacks());
        if (model.checkwin(controller.GetComponent<Game>().GetCurrentPlayer()))
        {
            controller.GetComponent<Game>().Winner(controller.GetComponent<Game>().GetCurrentPlayer());
        }
        else 
        {
            if (!controller.GetComponent<Game>().IsGameOver())
            {
                controller.GetComponent<Game>().NextTurn();
            }

        }

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
