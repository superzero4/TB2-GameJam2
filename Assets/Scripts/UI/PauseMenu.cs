using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject buttonPanel;

    public void Quit()
    {
        Application.Quit();
    }

    public void Resume()
    {
        buttonPanel.SetActive(false);
    }
}
