using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Auteur : Arthur & Margot

public class HumanAgent : IAgent
{
    public ActionsTypes Act(ref GameState gs, ActionsTypes[] availableActions)
    {
        //tirer
        if(Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.UpArrow))
        {
            Debug.Log("shoot!");
            return ActionsTypes.Shoot;
        }

        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftArrow))
        {
            Debug.Log("left!");
            return ActionsTypes.MoveLeft;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            Debug.Log("right!");
            return ActionsTypes.MoveRight;
        }

        return ActionsTypes.Nothing;
    }
}
