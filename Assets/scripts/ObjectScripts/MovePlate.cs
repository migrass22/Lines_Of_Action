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

    public void OnMouseUp()
    {
        Vector2Int before = new Vector2Int(reference.GetComponent<LOAman>().GetXBoard(), reference.GetComponent<LOAman>().GetYBoard());
        Vector2Int after = new Vector2Int(matrixX, matrixY);
        controller = GameObject.FindGameObjectWithTag("GameController");
        Model model = controller.GetComponent<Game>().model;
        Piece BeforePiece = model.GetPieceByIndex(before.x, before.y);
        Piece piece = model.GetPieceByIndex(after.x, after.y);
        int old = before.x + 8 * before.y - 1;
        if (attack) 
        {
            model.RemovePiece(piece);
            model.board.MakeMove(matrixX + matrixY * 8 - 1, matrixX + matrixY * 8 - 1, !controller.GetComponent<Game>().GetCurrentPlayer());
            Destroy(piece.piece);
        }

        reference.GetComponent<LOAman>().SetXBoard(matrixX);
        reference.GetComponent<LOAman>().SetYBoard(matrixY);
        reference.GetComponent<LOAman>().SetCorods();
        model.UpdatePosition(BeforePiece, after);
        reference.GetComponent<LOAman>().DestroyMovePlates();
        model.board.MakeMove(old, matrixX + matrixY * 8 - 1, controller.GetComponent<Game>().GetCurrentPlayer());
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
