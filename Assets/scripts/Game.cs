using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    // Counter for amount of turns that passed
    private int turncounter = 0;
    // My Model to hold data and logical functions
    public Model model = new Model();
    // True = white
    private bool currentplayer = true;
    // True = gameover
    private bool gameover = false;
    public GameObject piece;
    // Change this to activate ai
    private bool ActivateAi;

    // Start is called before the first frame update
    void Start()
    {
        InitializeGame();
        // Change here to activate ai
        ActivateAi = true;
    }

    // Restart the db and the pieces to be in the first positions again
    public void CleanUp() 
    {
        // Turn off all texts and restart the turn text to 0
        TurnOffTexts();
        // Make sure to destroy all the pieces on the board before starting a new game
        foreach (Piece go in model.whites)
        {
            Destroy(go.piece);
        }
        foreach (Piece go in model.blacks)
        {
            Destroy(go.piece);
        }
        model = new Model();
    }

    // Init normal black positions
    public void CreateBlackPiecesNormal()
    {
        // Starting positions for black
        model.blacks.AddRange(new Piece[]{Create(false, 1, 0), Create(false, 2, 0), Create(false, 3, 0),
                           Create(false, 4, 0), Create(false, 5, 0), Create(false, 6, 0),
                           Create(false, 1, 7), Create(false, 2, 7), Create(false, 3, 7),
                           Create(false, 4, 7), Create(false, 5, 7), Create(false, 6, 7)});
    }

    // Init normal white positions
    public void CreateWhitePiecesNormal()
    {
        // Starting positions for white
        model.whites.AddRange(new Piece[]{Create(true, 0, 1), Create(true, 0, 2), Create(true, 0, 3),
                           Create(true, 0, 4), Create(true, 0, 5), Create(true, 0, 6),
                           Create(true, 7, 1), Create(true, 7, 2), Create(true, 7, 3),
                           Create(true, 7, 4), Create(true, 7, 5), Create(true, 7, 6)});
    }

    // Add a new Piece to the board and activate it to show on screen, also create a Piece object and return it
    public Piece Create(bool player, int x, int y)
    {
        // Create the new gameobject
        GameObject obj = Instantiate(piece, new Vector3(0, 0, -1), Quaternion.identity);
        // Activate it using loaman script
        LOAman lm = obj.GetComponent<LOAman>();
        lm.player = player;
        lm.SetXBoard(x);
        lm.SetYBoard(y);
        lm.Activate();
        // Using everything we have create a new piece object and return it
        Piece CurrentPiece = new Piece(obj, new Vector2Int(x, y), player);
        return CurrentPiece;
    }

    // Starting up the game creating all 24 pieces and making them show on screen and save on lists
    public void InitializeGame()
    {
        model = new Model();
        // If this is second game make sure to get last pieces out
        CreateWhitePiecesNormal();
        // If this is second game make sure to get last pieces out
        CreateBlackPiecesNormal();
        // Game always begins with white to move, since we want to display the turns correctly we make black be the starting position
        // So it will change in NextTurn function
        currentplayer = false;
        gameover = false;
        turncounter = 0;
        // Display first turn 
        NextTurn();
    }

    public bool GetCurrentPlayer() 
    {
        return currentplayer;
    }

    public bool IsGameOver() 
    {
        return gameover;
    }

    // Disable all texts and restart the turn text 
    public void TurnOffTexts()
    {
        // Starting at turn 1
        GameObject.FindGameObjectWithTag("TurnText").GetComponent<Text>().text = "Turn:" + 1 + "\nWhite player move";
        // Disable both winner text and the restart text
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().enabled = false;
        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().enabled = false;
    }

    // Update the turn text for the current player and turn
    public void UpdateTurns(bool currentplayer, int turn)
    {
        // Display message for current player
        if (currentplayer)
        {
            GameObject.FindGameObjectWithTag("TurnText").GetComponent<Text>().text = "Turn:" + turn + "\nWhite player move";
        }
        else
        {
            GameObject.FindGameObjectWithTag("TurnText").GetComponent<Text>().text = "Turn:" + turn + "\nBlack player move";
        }
    }

    // Get a boolean for a player and decide if he won the game or not
    private void Winner(bool player)
    {
        gameover = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().enabled = true;
        if (player)
        {
            GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().text = "White player is the winner GOOD GAME";
        }
        else
        {
            GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().text = "Black player is the winner GOOD GAME";
        }
        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().enabled = true;
        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().text = "Press to restart";
    }

    // Get a Move
    // Move piece on screen and update all databases also remove a piece if it gets eaten
    // Also check for win and update messages if won game
    public void MoveAPieceInUnity(Move move) 
    {
        // Check if this move is an attacking one - if so remove the piece and update dbs
        AttackingPiece(move);

        // Update all databases including the piece im moving and the arraynumbers and bit boards
        UpdateAllDbsAndPieces(move);

        // Check if the player won after the move was made
        if (model.checkwin(currentplayer))
        {
            // Player won so end the game and show winner message
            Winner(currentplayer);
        }
        else
        {
            // if the game isnt over then continue to the next turn
            NextTurn();
        }
    }

    // -------------------------------- Utility Methods------------------------------------------

    // Get a move and check if its attacking
    // If attackng remove the piece its attacking and update dbs
    private void AttackingPiece(Move move) 
    {
        // If attacking then theres a piece at the endpoint of the move
        if (move.attack)
        {
            // Get the piece using position -> this is o(n^2) get this better maybe??
            Piece AfterPiece = model.GetPieceByIndex(move.moveto);
            // Remove the piece from lists in model
            model.RemovePiece(AfterPiece);
            // Make a move that deletes the eaten piece from bitboards
            model.board.MakeMove(AfterPiece.position, AfterPiece.position, !currentplayer);
            // Actually destroy said piece from board
            Destroy(AfterPiece.piece);
        }
    }

    // Get a move
    // Update all dbs in the model (currently array numbers and bit board)
    private void UpdateAllDbsAndPieces(Move move) 
    {
        // Piece to move
        Piece BeforePiece = move.pieceToMove;
        // Shorten the things i need to write
        Vector2Int before = move.pieceToMove.position; 
        // Update array numbers and position of the pieces
        model.MakeMove(move);
        // Update the position in the unity space of the piece im moving
        BeforePiece.piece.GetComponent<LOAman>().SetXBoard(move.moveto.x);
        BeforePiece.piece.GetComponent<LOAman>().SetYBoard(move.moveto.y);
        BeforePiece.piece.GetComponent<LOAman>().SetCorods();
        // Remove move plates since move is made
        BeforePiece.piece.GetComponent<LOAman>().DestroyMovePlates();
        // Make said move on the bit board
    }

    // Change the current player and update turn counter and message 
    public void NextTurn()
    {
        // If the game isnt over
        if (!IsGameOver())
        {
            // increment
            turncounter++;
            // Change player
            currentplayer = !currentplayer;
            // Update the turn counter
            UpdateTurns(currentplayer, turncounter);
            // Activate ai if ai mode is set
            if (!currentplayer && ActivateAi)
            {
                AI ai = new AI();
                ai.aimove(model);
            }
        }
    }
}

