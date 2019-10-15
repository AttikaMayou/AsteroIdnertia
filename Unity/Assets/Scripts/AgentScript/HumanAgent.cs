using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Auteur : Arthur & Margot

public class HumanAgent : IAgent
{
    public ActionsTypes Act(ref GameState gs, ActionsTypes[] availableActions, int playerId)
    {
        if(playerId == 0)
        {
            if (Input.GetKey(KeyCode.Z))
            {
                Debug.Log("shoot!");
                return ActionsTypes.Shoot;
            }

            if (Input.GetKey(KeyCode.Q))
            {
                Debug.Log("left!");
                return ActionsTypes.MoveLeft;
            }

            if (Input.GetKey(KeyCode.D))
            {
                Debug.Log("right!");
                return ActionsTypes.MoveRight;
            }
        }
        else if(playerId == 1)
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                Debug.Log("shoot!");
                return ActionsTypes.Shoot;
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                Debug.Log("left!");
                return ActionsTypes.MoveLeft;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                Debug.Log("right!");
                return ActionsTypes.MoveRight;
            }
        }

        return ActionsTypes.Nothing;
    }
}
