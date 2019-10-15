using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//Auteur : Arthur & Margot
public class GameSystemScript : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown Agent1Dropdown;

    [SerializeField]
    private TMP_Dropdown Agent2Dropdown;

    private IAgent agentPlayer1;
    private IAgent agentPlayer2;

    [SerializeField]PlayerManager playerManager;

    private bool LaunchGame = false;

    public void StartGame()
    {
        switch (Agent1Dropdown.value)
        {
            case 0:
                agentPlayer1 = new HumanAgent();
                break;
            case 1:
                agentPlayer1 = new RandomAgent();
                break;
            /*case 2:
                agentPlayer1 = new RandomRollOut();
                break;
            case 3:
                agentPlayer1 = new Dijktra();
                break;
            case 4:
                agentPlayer1 = new MCTS();
                break;
            case 5:
                agentPlayer1 = new QLearning();
                break;*/
        }

        switch (Agent2Dropdown.value)
        {
            case 0:
                agentPlayer2 = new HumanAgent();
                break;
            case 1:
                agentPlayer2 = new RandomAgent();
                break;
            /*case 2:
                agentPlayer2 = new RandomRollOut();
                break;
            case 3:
                agentPlayer2 = new Dijktra();
                break;
            case 4:
                agentPlayer2 = new MCTS();
                break;
            case 5:
                agentPlayer2 = new QLearning();
                break;*/
        }
        IAgent[] agents = { agentPlayer1, agentPlayer2 };
        Debug.Log(agents.Length);
        playerManager.StartGame(agents);
        LaunchGame = true;
    }

    private void Update()
    {

        if (LaunchGame)
        {
            playerManager.UpdatePlayerState();
        }
    }
}
