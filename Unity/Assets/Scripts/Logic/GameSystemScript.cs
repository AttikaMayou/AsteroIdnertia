using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSystemScript : MonoBehaviour
{
    [SerializeField]
    private GameObject AsteroidPrefab;

    [SerializeField]
    private GameObject ProjectilePrefab;

    [SerializeField]
    private GameObject PlayerPrefab;

    [SerializeField]
    private Dropdown Agent1Dropdown;

    [SerializeField]
    private Dropdown Agent2Dropdown;

    private GameState gs;

    private readonly List<Transform>AsteroidView = new List<Transform>();
    private readonly List<Transform> projectileView = new List<Transform>();
    private Transform PlayerView;
    private IAgent agentPlayer1;
    private IAgent agentPlayer2;

    public void StartGame(int choicePlayer1, int choicePlayer2)
    {
        GameStateRules.Init(ref gs);

        switch (choicePlayer1)
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

        switch (choicePlayer2)
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
    }

    private void Update()
    {
        
    }
}
