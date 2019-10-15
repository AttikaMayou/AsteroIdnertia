using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Auteur : Arthur & Margot

public class HumanAgent : IAgent
{
    public ActionsTypes Act(ref GameState gs, ActionsTypes[] availableActions)
    {
        //tirer
        if(Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log("shoot!");
            return ActionsTypes.Shoot;
        }

        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Debug.Log("left!");
            return ActionsTypes.MoveLeft;
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            Debug.Log("right!");
            return ActionsTypes.MoveRight;
        }

        return ActionsTypes.Nothing;
    }
}
