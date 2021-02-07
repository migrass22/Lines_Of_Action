using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    private int turncounter = 0;
    public Model model = new Model();
    // true = white
    private bool currentplayer = true;
    private bool gameover = false;
    public GameObject piece;
    // Start is called before the first frame update
    void Start()
    {
        InitializeGame();
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
        model.InitModel();
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
        model.InitModel();
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

    // Change the current player and update turn counter and message 
    public void NextTurn()
    {
        // increment
        turncounter++;
        // Change player
        currentplayer = !currentplayer;
        // Update the turn counter
        UpdateTurns(currentplayer, turncounter);
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
    public void Winner(bool player)
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
}

