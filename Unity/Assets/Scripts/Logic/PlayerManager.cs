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
    GameObject[] players;

    public List<Transform> asteroidsView = new List<Transform>();
    private readonly List<Transform> projectilesView = new List<Transform>();

    private Transform[] playerViews = new Transform[2];

    private IAgent[] playerAgents = new IAgent[2];

    public void UpdatePlayerState()
    {
        for (var i = 0; i < players.Length; i++)
        {
            if (gs.players[i].isGameOver)
            {
                return;
            }
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
        Debug.Log(gs.projectiles.Length);
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

        Vector3 newPos = new Vector3();
        for (int i = 0; i < projectilesView.Count; i++)
        {
            newPos.x = gs.projectiles[i].position.x;
            newPos.y = 0;
            newPos.z = gs.projectiles[i].position.y;
            Debug.LogWarning(newPos);
            projectilesView[i].position = newPos;
        }
    }

}
