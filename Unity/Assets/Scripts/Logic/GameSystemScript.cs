using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//Auteur : Arthur & Margot
public class GameSystemScript : MonoBehaviour
{
    private IAgent agentPlayer;

    [SerializeField]PlayerManager player;

    private bool LaunchGame = false;

    public void StartGame()
    {
        agentPlayer = new HumanAgent();

        player.StartGame(agentPlayer, player);

        LaunchGame = true;
    }

    private void Update()
    {
        //if (gs.player.isGameOver)
        //{
        //    return;
        //}
        if (LaunchGame)
        {
            player.UpdatePlayerState();
        }
        
    }
}
