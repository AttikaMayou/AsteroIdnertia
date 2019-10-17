using UnityEngine;
using TMPro;
using Random = Unity.Mathematics.Random;

//Auteur : Arthur & Margot

public class GameSystemScript : MonoBehaviour
{
    [SerializeField]
    private GameObject PauseMenu;

    static bool isPaused = false;

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
        Time.timeScale = 1;
        switch (Agent1Dropdown.value)
        {
            case 0:
                agentPlayer1 = new HumanAgent();
                break;
            case 1:
                agentPlayer1 = new RandomAgent { rdm = new Random((uint)Time.frameCount) };
                break;
            case 2:
                agentPlayer1 = new RandomRollOut();
                break;
            case 3:
                agentPlayer1 = new AStarV2();
                break;
            /*case 4:
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
                agentPlayer2 = new RandomAgent { rdm = new Random((uint)Time.frameCount + 150) };
                break;
            case 2:
                agentPlayer2 = new RandomRollOut();
                break;
            case 3:
                agentPlayer2 = new AStarV2();
                break;
            /*case 4:
                agentPlayer2 = new MCTS();
                break;
            case 5:
                agentPlayer2 = new QLearning();
                break;*/
        }
        IAgent[] agents = { agentPlayer1, agentPlayer2 };
        playerManager.StartGame(agents);
        LaunchGame = true;
    }

    private void Update()
    {
        if (LaunchGame)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Time.timeScale = 0;
                Pause();
                isPaused = true;
            }
            else
            {
                playerManager.UpdatePlayerState();
            }
        }

    }

    public void Pause()
    {
        if (isPaused)
        {
            PauseMenu.SetActive(false);
        }
        else
        { 
        PauseMenu.SetActive(true);
        }
    }
}
