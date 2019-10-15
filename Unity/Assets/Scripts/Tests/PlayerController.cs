using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float velocity;
    public Vector2 position;

    // Start is called before the first frame update
    void Start()
    {
        position = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKey(KeyCode.Q)) // MOVE LEFT
        //{
        //    var targetVel = velocity - GameState.ACCELERATION_SPEED * 200;
        //    velocity = Mathf.Lerp(velocity, targetVel, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
        //    Debug.Log("test");
        //}

        //else if (Input.GetKey(KeyCode.D))    // MOVE RIGHT
        //{
        //    var targetVel = velocity + GameState.ACCELERATION_SPEED * 200;
        //    velocity = Mathf.Lerp(velocity, targetVel, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
        //}
        //else
        //{
        //    velocity = Mathf.Lerp(velocity, 0, 1 - Mathf.Exp(-GameState.DECELERATION_SPEED));
        //}

        //velocity = Mathf.Clamp(velocity, -GameState.MAX_VELOCITY, GameState.MAX_VELOCITY);
        //position.x += velocity;
        //transform.position = position;
    }
}
