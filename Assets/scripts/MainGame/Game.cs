// MADE BY TOM PELEG

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour
{


    // ------------------------------------------- Variables ----------------------------------------------------


    // Counter for amount of turns that passed
    private int turncounter = 1;
    // My Model to hold data and logical functions
    public Model model = new Model();
    // True = white
    private bool currentplayer = true;
    // True = gameover
    private bool gameover = false;
    // Prefab piece used to create the board
    public GameObject piece;
    // Change this to activate ai
    private bool ActivateBlackAi;
    private bool ActivateWhiteAi;
    private bool currentai;
    // Time for every turn
    float MOVING_TIME = 1;

    // Bool for turn ending
    public bool turnended = false;

    // Set up game according to what was selected in the main menu
    public static string mode;

    // Depth for ai vs human
    private static int AivsHumanDepth = 4;
    private static int AivsAiDepthb = 5;
    private static int AivsAiDepthw = 1;

    float timeToTestSimpaleCube;

    AI ai, otherai ;
    // ------------------------------------------ View Methods --------------------------------------------------


    // Start is called beforse the first frame update
    void Start()
    {
        timeToTestSimpaleCube = MOVING_TIME;
        mode = ModeNameController.modetype;
        TurnOffTexts();
        ActivateOption(mode);
        InitializeGame();
    }
    private void Update()
    {
        
        timeToTestSimpaleCube -= Time.deltaTime;
        if (timeToTestSimpaleCube <= 0)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            timeToTestSimpaleCube = MOVING_TIME;
            // If the game isnt over
            if (!IsGameOver())
            {
                // Activate if 2 ai mode is set 
                if (ActivateBlackAi && ActivateWhiteAi)
                {
                    currentai = !currentai;
                    if (currentai)
                    {
                        otherai.mainModel = model;
                        otherai.StartAi();
                    }
                    else
                    {
                        ai.mainModel = model;
                        ai.StartAi();
                    }
                    // increment
                    turncounter++;
                    // Change player
                    currentplayer = !currentplayer;
                    // Update the turn counter
                    UpdateTurns();
                    stopwatch.Stop();
                    Debug.Log("Time to make final move - " + stopwatch.ElapsedMilliseconds / 1000 + " seconds");
                }
                // Activate ai if ai mode is set
                else if (ActivateBlackAi && turnended)
                {
                    // increment
                    turncounter++;
                    // Change player
                    currentplayer = !currentplayer;
                    // Update the turn counter
                    UpdateTurns();
                    if (!currentplayer)
                    {
                        ai.mainModel = model;
                        ai.StartAi();
                        currentplayer = !currentplayer;
                    }
                    turnended = false;
                    stopwatch.Stop();
                    Debug.Log("Time to make final move - " + stopwatch.ElapsedMilliseconds / 1000 + " seconds");
                }

            }
        }
        if (turnended && !gameover && !ActivateBlackAi && !ActivateWhiteAi)
        {
            // increment
            turncounter++;
            // Change player
            currentplayer = !currentplayer;
            // Update the turn counter
            UpdateTurns();
            turnended = false;
        }
    }

    // Get a toggle script and activate the selected game mode chosen on the main menu
    private void ActivateOption(string s)
    {
        switch (s)
        {
            // Ai vs Ai
            case "PureAi":
                currentai = true;
                ActivateBlackAi = true;
                ai = new AI(model, currentai, AivsAiDepthw);
                ActivateWhiteAi = true;
                otherai = new AI(model, !currentai, AivsAiDepthb);
                break;
            // Ai vs human player
            case "AIvsHuman":
                ActivateWhiteAi = false;
                currentai = false;
                ai = new AI(model, currentai, AivsHumanDepth);
                ActivateBlackAi = true;
                currentplayer = true;
                break;
            // Human player vs human player
            case "1V1":
                ActivateWhiteAi = false;
                ActivateBlackAi = false;
                currentplayer = true;
                break;
            default:
                break;
        };
    }

    // Restart the db and the pieces to be in the first positions again
    public void CleanUp()
    {
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

    public void RestartGame() 
    {
        CleanUp();
        TurnOffTexts();
        InitializeGame();
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
        currentplayer = true;
        gameover = false;
        turncounter = 1;
        turnended = false;
        UpdateTurns();
    }

    // Disable all texts and restart the turn text 
    public void TurnOffTexts()
    {
        // Starting at turn 1
        GameObject.FindGameObjectWithTag("TurnText").GetComponent<Text>().text = "Turn:" + turncounter + "\nWhite player move";
        // Disable both winner text and the restart text
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().enabled = false;
        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().enabled = false;
        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().enabled = false;
        GameObject.FindGameObjectWithTag("RestartButton").GetComponent<Button>().interactable = false;

    }

    // Update the turn text for the current player and turn
    public void UpdateTurns()
    {
        // Display message for current player
        if (currentplayer)
        {
            GameObject.FindGameObjectWithTag("TurnText").GetComponent<Text>().text = "Turn:" + turncounter + "\nWhite player move";
        }
        else
        {
            GameObject.FindGameObjectWithTag("TurnText").GetComponent<Text>().text = "Turn:" + turncounter + "\nBlack player move";
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
        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().text = "PRESS TO RESTART";
        GameObject.FindGameObjectWithTag("RestartButton").GetComponent<Button>().interactable = true;


    }

    // If both players won then this is a tie
    private void Tie() 
    {
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().enabled = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().text = "TIE! GOOD GAME";
        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().enabled = true;
        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().text = "PRESS TO RESTART";
        GameObject.FindGameObjectWithTag("RestartButton").GetComponent<Button>().interactable = true;
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
            if (model.checkwin(!currentplayer))
            {
                Tie();
            }
            else 
            {
                // Player won so end the game and show winner message
                Winner(currentplayer);
            }

        }
    }

    // Go back to main menu if button is pressed
    public void GoBack()
    {
        SceneManager.LoadScene("MainMenu");
    }


    // ---------------------------------------- Getter and Setter -----------------------------------------------


    // Get the current player 
    public bool GetCurrentPlayer()
    {
        return currentplayer;
    }

    // Check if the game is over
    public bool IsGameOver()
    {
        return gameover;
    }


    // ------------------------------------------- Core Methods-------------------------------------------------


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
            // Actually destroy said piece from board
            Destroy(AfterPiece.piece);
        }
    }

    // Get a move
    // Update all dbs in the model (currently array numbers and bit board)
    private void UpdateAllDbsAndPieces(Move move) 
    {
        // Piece to move
        Piece BeforePiece = model.GetPieceByIndex(move.pieceToMove.position);
        // Update array numbers and position of the pieces
        model.MakeMove(move);

        BeforePiece.piece.GetComponent<LOAman>().StartMoving(move);

        // Update the position in the unity space of the piece im moving
        //BeforePiece.piece.GetComponent<LOAman>().SetXBoard(move.moveto.x);
        //BeforePiece.piece.GetComponent<LOAman>().SetYBoard(move.moveto.y);

        // Remove move plates since move is made
        BeforePiece.piece.GetComponent<LOAman>().DestroyMovePlates();
        // Make said move on the bit board
    }


}

