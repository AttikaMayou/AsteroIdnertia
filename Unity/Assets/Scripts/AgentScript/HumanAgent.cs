using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Auteur : Arthur & Margot

public class HumanAgent : IAgent
{
    public ActionsTypes[] Act(ref GameState gs, ActionsTypes[] availableActions, int playerId)
    {
        ActionsTypes[] actions = new ActionsTypes[3];
        actions[0] = ActionsTypes.Nothing;
        actions[1] = ActionsTypes.Nothing;
        actions[2] = ActionsTypes.NoShoot;

        if (playerId == 0)
        {
            
            if (Input.GetKey(KeyCode.Z))
            {
                actions[0] = ActionsTypes.MoveUp;
            }

            if (Input.GetKey(KeyCode.S))
            {
                actions[0] = ActionsTypes.MoveDown;
            }

            if (Input.GetKey(KeyCode.Q))
            {
                actions[1] = ActionsTypes.RotateLeft;
            }

            if (Input.GetKey(KeyCode.D))
            {
                actions[1] = ActionsTypes.RotateRight;
            }

            if (Input.GetKey(KeyCode.Space))
            {
                actions[2] = ActionsTypes.Shoot;
            }
        }
        else if(playerId == 1)
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                actions[0] = ActionsTypes.MoveUp;
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                actions[0] = ActionsTypes.MoveDown;
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                actions[1] = ActionsTypes.RotateLeft;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                actions[1] = ActionsTypes.RotateRight;
            }

            if (Input.GetKey(KeyCode.Return))
            {
                actions[2] = ActionsTypes.Shoot;
            }
        }
        
        return actions;
    }
}
