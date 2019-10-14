using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//Auteur : Arthur & Margot
public class GameSystemScript : MonoBehaviour
{
    //[SerializeField]
    //private GameObject AsteroidPrefab;

    //[SerializeField]
    //private GameObject ProjectilePrefab;

    //[SerializeField]
    //private GameObject Player1;

    //[SerializeField]
    //private GameObject Player2;

    [SerializeField]
    private TMP_Dropdown Agent1Dropdown;

    [SerializeField]
    private TMP_Dropdown Agent2Dropdown;

    //private GameState gs;

    //private readonly List<Transform> asteroidsView = new List<Transform>();
    //private readonly List<Transform> projectilesView = new List<Transform>();
    
    //private Transform Player1View;
    //private Transform Player2View;
    private IAgent agentPlayer1;
    private IAgent agentPlayer2;

    [SerializeField]PlayerManager player1;
    [SerializeField]PlayerManager player2;

    public void StartGame()
    {
        //GameStateRules.Init(ref gs);

        //Player1View = Player1.transform;
        //Player2View = Player2.transform;
        


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

        player1.StartGame(agentPlayer1, "AsteroidsPlayer1");
        player2.StartGame(agentPlayer2, "AsteroidsPlayer2");
    }

    private void Update()
    {
        //if (gs.player.isGameOver)
        //{
        //    return;
        //}

        player1.UpdatePlayerState();
        player2.UpdatePlayerState();

        //SyncAsteroidsViews();
        //SyncProjectilesViews();
        //mettre à jour la position des players
        //Player1View.position = gs.player.position;
        //Player2View.position = gs.player[1].position;

        //GameStateRules.Step(ref gs, agentPlayer1.Act(ref gs, GameStateRules.GetAvailableActions(ref gs)));
        
    }

    //private void SyncAsteroidsViews()
    //{
    //    var asteroidToSpawn = gs.asteroids.Length - asteroidsView.Count;

    //    for(int i = 0; i < asteroidToSpawn; i++)
    //    {
    //        var asteroidView = Instantiate(AsteroidPrefab).GetComponent<Transform>();
    //        asteroidsView.Add(asteroidView);
    //    }

    //    for(int i= 0; i< -asteroidToSpawn; i++)
    //    {
    //        Destroy(asteroidsView[asteroidsView.Count - 1].gameObject);
    //        asteroidsView.RemoveAt(asteroidsView.Count- 1);
    //    }

    //    for(int i = 0; i<asteroidsView.Count; i++)
    //    {
    //        asteroidsView[i].position = gs.asteroids[i].position;
    //    }
    //}

    //private void SyncProjectilesViews()
    //{
    //    var projectileToSpawn = gs.projectiles.Length - projectilesView.Count;
    //    for (int i = 0; i < projectileToSpawn; i++)
    //    {
    //        var projectileView = Instantiate(ProjectilePrefab).GetComponent<Transform>();
    //        asteroidsView.Add(projectileView);
    //    }

    //    for (int i = 0; i < -projectileToSpawn; i++)
    //    {
    //        Destroy(projectilesView[projectilesView.Count - 1].gameObject);
    //        projectilesView.RemoveAt(projectilesView.Count - 1);
    //    }

    //    for (int i = 0; i < projectilesView.Count; i++)
    //    {
    //        projectilesView[i].position = gs.projectiles[i].position;
    //    }
    //}
}
