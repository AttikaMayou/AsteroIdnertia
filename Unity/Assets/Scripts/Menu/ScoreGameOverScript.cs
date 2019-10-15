using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreGameOverScript : MonoBehaviour
{
    [SerializeField]
    private GameObject menuEndGame;

    [SerializeField]
    private Player player1;

    [SerializeField]
    private Player player2;

    public void EndGame()
    {
        if(player1.isGameOver)
        {
            //player2 victory
            Debug.Log("player1 deaaaaad");
            menuEndGame.SetActive(true);
        }
        else if(player2.isGameOver)
        {
            //player1 victory
        }
    }

    public void Update()
    {
        EndGame();
    }

    public void DisplayScore(Player player1, Player player2)
    {

    }
}
