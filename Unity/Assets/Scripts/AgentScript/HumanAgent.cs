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
                return ActionsTypes.MoveUp;
            }

            if (Input.GetKey(KeyCode.S))
            {
                return ActionsTypes.MoveDown;
            }

            if (Input.GetKey(KeyCode.Q))
            {
                return ActionsTypes.RotateLeft;
            }

            if (Input.GetKey(KeyCode.D))
            {
                return ActionsTypes.RotateRight;
            }

            if (Input.GetKey(KeyCode.Space))
            {
                return ActionsTypes.Shoot;
            }
        }
        else if(playerId == 1)
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                return ActionsTypes.MoveUp;
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                return ActionsTypes.MoveDown;
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                return ActionsTypes.RotateLeft;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                return ActionsTypes.RotateRight;
            }

            if (Input.GetKey(KeyCode.KeypadEnter))
            {
                return ActionsTypes.Shoot;
            }
        }

        return ActionsTypes.Nothing;
    }
}
