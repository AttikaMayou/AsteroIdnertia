using System.Collections;
using Unity.Collections;
using UnityEngine;

//Auteur : Arthur & Margot

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

            else if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.S))
            {
                return ActionsTypes.MoveDownS;
            }

            else if(Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.Q))
            {
                return ActionsTypes.RotateLeftS;
            }

            else if(Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.D))
            {
                return ActionsTypes.RotateRightS;
            }

            else if (Input.GetKey(KeyCode.Z) && Input.GetKey(KeyCode.D))
            {
                return ActionsTypes.RotateRightUp;
            }

            else if (Input.GetKey(KeyCode.Z) && Input.GetKey(KeyCode.Q))
            {
                return ActionsTypes.RotateLeftUp;
            }

            else if(Input.GetKey(KeyCode.Z))
            {
                return ActionsTypes.MoveUpNS;
            }

            else if(Input.GetKey(KeyCode.S))
            {
                return ActionsTypes.MoveDownNS;
            }

            else if(Input.GetKey(KeyCode.Q))
            {
                return ActionsTypes.RotateLeftNS;
            }

            else if(Input.GetKey(KeyCode.D))
            {
                return ActionsTypes.RotateRightNS;
            }

            else if (Input.GetKey(KeyCode.Space))
            {
                return ActionsTypes.NothingS;
            }


        }
        else if(playerId == 1)
        {
            //Shoot
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.UpArrow))
            {
                return ActionsTypes.MoveUpS;
            }

            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.DownArrow))
            {
                return ActionsTypes.MoveDownS;
            }
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.LeftArrow))
            {
                return ActionsTypes.RotateLeftS;
            }
            if (Input.GetKey(KeyCode.RightControl) && Input.GetKey(KeyCode.RightArrow))
            {
                return ActionsTypes.RotateRightS;
            }


            else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.RightArrow))
            {
                return ActionsTypes.RotateRightUp;
            }

            else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftArrow))
            {
                return ActionsTypes.RotateLeftUp;
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

            else if (Input.GetKey(KeyCode.RightControl))
            {
                return ActionsTypes.NothingS;
            }
        }

        return ActionsTypes.Nothing;
    }
}
