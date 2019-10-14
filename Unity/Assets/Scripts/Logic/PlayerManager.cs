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

    private readonly List<Transform> asteroidsView = new List<Transform>();
    private readonly List<Transform> projectilesView = new List<Transform>();

    private Transform playerView;
    private IAgent agent;


    public void UpdatePlayerState()
    {
        if (gs.player.isGameOver)
        {
            return;
        }

        //SyncAsteroidsViews();
        //SyncProjectilesViews();
        //mettre à jour la position du players
        //PlayerView.position = gs.player.position;

        //GameStateRules.Step(ref gs, agent.Act(ref gs, GameStateRules.GetAvailableActions(ref gs)));
    }


    public void StartGame(IAgent agent, string tag)
    {
        this.agent = agent;
        GameStateRules.Init(ref gs, tag);
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

        for (int i = 0; i < asteroidsView.Count; i++)
        {
            asteroidsView[i].position = gs.asteroids[i].position;
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
