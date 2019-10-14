using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Auteur : Arthur
public class PauseScript : MonoBehaviour
{
    [SerializeField]
    private GameObject PauseMenu;

    bool isPaused = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }

    public void Pause()
    {
        if (isPaused)
        {
            //TODO: pause the game
            PauseMenu.SetActive(false);
        }
        else
        {
            //TODO: Resume the game
            PauseMenu.SetActive(true);
        }
    }
}
