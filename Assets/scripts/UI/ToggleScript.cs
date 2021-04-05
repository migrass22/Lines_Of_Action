using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;

public class ToggleScript : MonoBehaviour
{
    GameObject go;
    ToggleGroup ToggleGroupInstance;

    private void Start()
    {
        go = this.gameObject;
    }

    public Toggle CurrentSelected()
    {
        ToggleGroupInstance = go.GetComponent<ToggleGroup>();
        if (ToggleGroupInstance)
        {
            return ToggleGroupInstance.ActiveToggles().FirstOrDefault();
        }
        return null;
    }
}
