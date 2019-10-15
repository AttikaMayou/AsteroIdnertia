using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    private GameObject AsteroidPrefab;

    [SerializeField]
    private GameObject ProjectilePrefab;

    private GameState gs;

    [SerializeField]
    GameObject[] players;

    [SerializeField]
    private GameObject menuEndGame;

    [SerializeField]
    private TMP_Text playerEndGame;

    public List<Transform> asteroidsView = new List<Transform>();
    private readonly List<Transform> projectilesView = new List<Transform>();

    private Transform[] playerViews = new Transform[2];

    private IAgent[] playerAgents = new IAgent[2];

    public void StartGame(IAgent[] agents)
    {
        Debug.Log(players.Length);
        for (var i = 0; i < players.Length; i++)
        {
            playerAgents[i] = agents[i];
            playerViews[i] = players[i].transform;
        }

        GameStateRules.Init(ref gs, this);
    }

    public void UpdatePlayerState()
    {
        if (gs.players[0].isGameOver || gs.players[1].isGameOver)
        {
            if (gs.players[0].isGameOver)
            {
                playerEndGame.text = "Player 2 wins !";
            }

            if (gs.players[1].isGameOver)
            {
                playerEndGame.text = "Player 1 wins !";
            }

            menuEndGame.SetActive(true);

            return;

        }

        SyncAsteroidsViews();
        SyncProjectilesViews();
        //mettre à jour la position du players
        for (int i = 0; i < players.Length; i++)
        {
            playerViews[i].position = gs.players[i].position;
        }


        GameStateRules.Step(ref gs, playerAgents[0].Act(ref gs, GameStateRules.GetAvailableActions(ref gs), 0), playerAgents[1].Act(ref gs, GameStateRules.GetAvailableActions(ref gs), 1));
        
    }

    private void SyncAsteroidsViews()
    {
        var asteroidToSpawn = gs.asteroids.Length - asteroidsView.Count;

        for (int i = 0; i < asteroidToSpawn; i++)
        {
            var asteroidView = Instantiate(AsteroidPrefab).GetComponent<Transform>();
            asteroidsView.Add(asteroidView);
        }

        for (int i = 0; i < -asteroidToSpawn; i++)
        {
            Destroy(asteroidsView[asteroidsView.Count - 1].gameObject);
            asteroidsView.RemoveAt(asteroidsView.Count - 1);
        }

        for (int i = 0; i < asteroidsView.Count; i++)
        {
            Vector3 newPos = new Vector3(gs.asteroids[i].position.x, 0.0f, gs.asteroids[i].position.y);
            asteroidsView[i].position = newPos;
        }
    }

    private void SyncProjectilesViews()
    {
        var projectileToSpawn = gs.projectiles.Length - projectilesView.Count;

        for (int i = 0; i < projectileToSpawn; i++)
        {
            var projectileView = Instantiate(ProjectilePrefab/*, new Vector3(0, 0, 5), Quaternion.identity*/);
            //projectilesView[i].position = gs.projectiles[i].position;
            projectilesView.Add(projectileView.transform);
        }

        for (int i = 0; i < -projectileToSpawn; i++)
        {
            Destroy(projectilesView[projectilesView.Count - 1].gameObject);
            projectilesView.RemoveAt(projectilesView.Count - 1);
        }

        for (int i = 0; i < projectilesView.Count; i++)
        {
            Vector3 newPos = new Vector3(gs.projectiles[i].position.x, 0.0f, gs.projectiles[i].position.y);
            projectilesView[i].position = newPos;
        }
    }

    public void ResetGameOver()
    {
        gs.players[0].isGameOver = false;
        gs.players[1].isGameOver = false;
    }
}
