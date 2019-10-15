using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    private GameObject AsteroidPrefab;

    [SerializeField]
    private GameObject ProjectilePrefab;

    private GameState gs;

    [SerializeField]
    GameObject player;

    public List<Transform> asteroidsView = new List<Transform>();
    private readonly List<Transform> projectilesView = new List<Transform>();

    private Transform playerView;
    private IAgent agent;


    public void UpdatePlayerState()
    {
        if (gs.player1.isGameOver)
        {
            return;
        }

        SyncAsteroidsViews();
        SyncProjectilesViews();
        //mettre à jour la position du players
        playerView.position = gs.player1.position;

        GameStateRules.Step(ref gs, agent.Act(ref gs, GameStateRules.GetAvailableActions(ref gs)));
    }

    public void StartGame(IAgent agent, PlayerManager player)
    {
        Debug.Log("Start Game called for player " + this.player.name);
        this.agent = agent;
        GameStateRules.Init(ref gs, player);
        playerView = player.transform;
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

        Vector3 newPos = new Vector3();
        for (int i = 0; i < asteroidsView.Count; i++)
        {
            newPos.x = gs.asteroids[i].position.x;
            newPos.y = 0;
            newPos.z = gs.asteroids[i].position.y;
            asteroidsView[i].position = newPos;
        }
    }

    private void SyncProjectilesViews()
    {
        var projectileToSpawn = gs.projectiles.Length - projectilesView.Count;
        for (int i = 0; i < projectileToSpawn; i++)
        {
            var projectileView = Instantiate(ProjectilePrefab).GetComponent<Transform>();
            asteroidsView.Add(projectileView);
        }

        for (int i = 0; i < -projectileToSpawn; i++)
        {
            Destroy(projectilesView[projectilesView.Count - 1].gameObject);
            projectilesView.RemoveAt(projectilesView.Count - 1);
        }

        for (int i = 0; i < projectilesView.Count; i++)
        {
            projectilesView[i].position = gs.projectiles[i].position;
        }
    }

}
