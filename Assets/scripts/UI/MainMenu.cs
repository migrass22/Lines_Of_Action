using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public bool aichosen = false;


    public void Onclick() 
    {
        GameObject go = GameObject.FindGameObjectWithTag("TgTag");
        ToggleGroup ToggleGroupInstance = go.GetComponent<ToggleGroup>();
        Toggle chosen = ToggleGroupInstance.GetComponent<ToggleScript>().CurrentSelected();
        string ChosenName = chosen.name;
        if (ChosenName == "AIvsHuman")
        {
            GameObject tgt = GameObject.FindGameObjectWithTag("tpt");
            ToggleGroup tg = tgt.GetComponent<ToggleGroup>();
            var pressed = tg.GetComponent<ToggleScript>().CurrentSelected();
            ModeNameController.HumanPlayerColor = pressed.name == "White";
        }
        ModeNameController.modetype = ChosenName;
        SceneManager.LoadScene("GameScene");
    }

}
