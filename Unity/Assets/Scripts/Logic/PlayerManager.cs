﻿using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Entities;

//Auteur : Félix
//Modifications : Arthur, Margot et Attika

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

    [SerializeField]
    private TMP_Text finaleScore1;

    [SerializeField]
    private TMP_Text finaleScore2;

    [SerializeField]
    private GameObject menuInGame;

    [SerializeField]
    private TMP_Text scorePlayer1;

    [SerializeField]
    private TMP_Text scorePlayer2;

    private readonly List<Transform> asteroidsView = new List<Transform>();
    private readonly List<Transform> projectilesView = new List<Transform>();

    private Transform[] playerViews = new Transform[2];

    private IAgent[] playerAgents = new IAgent[2];

    public void StartGame(IAgent[] agents)
    {
        for (var i = 0; i < players.Length; i++)
        {
            playerAgents[i] = agents[i];
        //    playerViews[i] = players[i].transform;
        }

        GameStateRules.Init(ref GameParameters.Instance.Parameters, ref gs, this);
    }

    public void UpdatePlayerState()
    {
        if (gs.players[0].isGameOver || gs.players[1].isGameOver)
        {
            menuInGame.SetActive(false);

            if (gs.players[0].isGameOver)
            {
                playerEndGame.text = "Player 2 wins !";
            }

            if (gs.players[1].isGameOver)
            {
                playerEndGame.text = "Player 1 wins !";
            }

            ResetGameOver();

            finaleScore1.text = scorePlayer1.text;
            finaleScore2.text = scorePlayer2.text;

            menuEndGame.SetActive(true);

            return;

        }

        //Update score display
        var score1 = (int)gs.players[0].score;
        var score2 = (int)gs.players[1].score;
        scorePlayer1.text = score1.ToString();
        scorePlayer2.text = score2.ToString();

        //SyncAsteroidsViews();
        World.Active.GetExistingSystem<GameSystem>().UpdateAsteroidsViews(ref gs);
        World.Active.GetExistingSystem<GameSystem>().UpdateProjectilesViews(ref gs);
       // SyncProjectilesViews();
        //mettre à jour la position du players
        //for (int i = 0; i < players.Length; i++)
        //{
        //    Vector3 lookDir = new Vector3(gs.players[i].lookDirection.x, 0, gs.players[i].lookDirection.y);
        //    playerViews[i].position = new Vector3(gs.players[i].position.x, 0, gs.players[i].position.y);
        //    playerViews[i].rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        //}

        World.Active.GetExistingSystem<GameSystem>().UpdatePlayersViews(ref gs);


        GetBoundaries();
        GameStateRules.Step(ref GameParameters.Instance.Parameters, ref gs, playerAgents[0].Act(ref gs, GameStateRules.GetAvailableActions(ref gs), 0),
                                    playerAgents[1].Act(ref gs, GameStateRules.GetAvailableActions(ref gs), 1));

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
            Vector3 dir = gs.projectiles[projectilesView.Count - 1].direction;
            dir = new Vector3(dir.x, projectileView.transform.rotation.eulerAngles.y, dir.y);
            projectileView.transform.rotation = Quaternion.LookRotation(Vector3.down, dir);
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
        Player oldPlayer1 = gs.players[0];
        Vector2 position1 = new Vector2(170f, 170f);
        gs.players[0] = GameStateRules.createPlayer(oldPlayer1.score, oldPlayer1.speed, position1,
    oldPlayer1.lastShootStep, false, oldPlayer1.velocity, oldPlayer1.rotationVelocity, oldPlayer1.lookDirection);

        Player oldPlayer2 = gs.players[1];
        Vector2 position2 = new Vector2(170f, 170f);
        gs.players[1] = GameStateRules.createPlayer(oldPlayer2.score, oldPlayer2.speed, position2,
    oldPlayer2.lastShootStep, false, oldPlayer2.velocity, oldPlayer2.rotationVelocity, oldPlayer2.lookDirection);

        gs.asteroids.Clear();
        gs.currentGameStep = 0;
        //gs.projectiles.Dispose();
    }

    void GetBoundaries()
    {
        Camera cam = Camera.main;
        Vector3 leftUpRay = new Vector3(0, 0);
        Vector3 rightDownRay = new Vector3(Screen.width, Screen.height);
        RaycastHit hit;

        Ray ray = cam.ScreenPointToRay(leftUpRay);
        if (Physics.Raycast(ray, out hit))
        {
            leftUpRay = hit.point;            
        }

        ray = cam.ScreenPointToRay(rightDownRay);
        if (Physics.Raycast(ray, out hit))
        {
            rightDownRay = hit.point;
        }
        
        Vector2 horizontalBoundary = new Vector2(leftUpRay.x, rightDownRay.x);
        Vector2 verticalBoundary = new Vector2(leftUpRay.z, rightDownRay.z);

        GameParameters.Instance.Parameters.ScreenBordersBoundaryX = horizontalBoundary;
        GameParameters.Instance.Parameters.ScreenBordersBoundaryY = verticalBoundary;

    }
}
