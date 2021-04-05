using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public void Onclick() 
    {
        GameObject go = GameObject.FindGameObjectWithTag("TgTag");
        ToggleGroup ToggleGroupInstance = go.GetComponent<ToggleGroup>();
        Toggle chosen = ToggleGroupInstance.GetComponent<ToggleScript>().CurrentSelected();
        string ChosenName = chosen.name;
        ModeNameController.modetype = ChosenName;
        SceneManager.LoadScene("GameScene");
    }

}
