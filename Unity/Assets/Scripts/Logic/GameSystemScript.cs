using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameSystemScript : MonoBehaviour
{
    [SerializeField]
    private GameObject AsteroidPrefab;

    [SerializeField]
    private GameObject ProjectilePrefab;

    [SerializeField]
    private GameObject Player1;

    [SerializeField]
    private GameObject Player2;

    [SerializeField]
    private TMP_Dropdown Agent1Dropdown;

    [SerializeField]
    private TMP_Dropdown Agent2Dropdown;

    private GameState gs;

    private readonly List<Transform> asteroidsView = new List<Transform>();
    private readonly List<Transform> projectilesView = new List<Transform>();
    
    private Transform Player1View;
    private Transform Player2View;
    private IAgent agentPlayer1;
    private IAgent agentPlayer2;

    public void StartGame()
    {
        GameStateRules.Init(ref gs);

        Player1View = Player1.transform;
        Player2View = Player2.transform;

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
    }

    private void Update()
    {
        if(gs.players[0].isGameOver || gs.players[1].isGameOver)
        {
            return;
        }

        //mettre à jour la position des players


        GameStateRules.Step(ref gs, GameStateRules.GetAvailableActions(ref gs));

    }

    private void SyncAsteroidsViews()
    {
        var asteroidToSpawn = gs.asteroids.Length - asteroidsView.Count;
        for(int i = 0; i < asteroidToSpawn; i++)
        {
            var asteroidView = Instantiate(AsteroidPrefab).GetComponent<Transform>();
        }
    }
}
