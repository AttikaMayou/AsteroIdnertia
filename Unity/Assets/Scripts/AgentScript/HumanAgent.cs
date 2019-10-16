using System.Collections;
using Unity.Collections;
using UnityEngine;

//Auteur : Arthur & Margot
//Modifications : Attika

public class HumanAgent : IAgent
{
    public ActionsTypes Act(ref GameState gs, NativeArray<ActionsTypes> availableActions, int playerId)
    {
        if (playerId == 0)
        {
            //Shoot
            if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.Z))
            {
                return ActionsTypes.MoveUpS;
            }

            if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.S))
            {
                return ActionsTypes.MoveDownS;
            }

            if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.Q))
            {
                return ActionsTypes.RotateLeftS;
            }

            if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.D))
            {
                return ActionsTypes.RotateRightS;
            }

            if (Input.GetKey(KeyCode.Z))
            {
                return ActionsTypes.MoveUpNS;
            }

            if (Input.GetKey(KeyCode.S))
            {
                return ActionsTypes.MoveDownNS;
            }

            if (Input.GetKey(KeyCode.Q))
            {
                return ActionsTypes.RotateLeftNS;
            }

            if (Input.GetKey(KeyCode.D))
            {
                return ActionsTypes.RotateRightNS;
            }


        }
        else if(playerId == 1)
        {
            //Shoot
            if (Input.GetKey(KeyCode.Return) && Input.GetKey(KeyCode.UpArrow))
            {
                return ActionsTypes.MoveUpS;
            }

            if (Input.GetKey(KeyCode.Return) && Input.GetKey(KeyCode.DownArrow))
            {
                return ActionsTypes.MoveDownS;
            }
            if (Input.GetKey(KeyCode.Return) && Input.GetKey(KeyCode.LeftArrow))
            {
                return ActionsTypes.RotateLeftS;
            }
            if (Input.GetKey(KeyCode.Return) && Input.GetKey(KeyCode.RightArrow))
            {
                return ActionsTypes.RotateRightS;
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                return ActionsTypes.MoveUpNS;
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                return ActionsTypes.MoveDownNS;
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                return ActionsTypes.RotateLeftNS;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                return ActionsTypes.RotateRightNS;
            }
        }

        return ActionsTypes.Nothing;
    }
}
