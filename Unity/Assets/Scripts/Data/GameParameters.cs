using UnityEngine;

[System.Serializable]
public struct GameParametersStruct
{
    public long ShootDelay;
    public float PlayerRadius;
    public float InitialPlayerSpeed;
    public float AccelerationSpeed;
    public float DecelerationSpeed;
    public float MaxVelocity;
    public float RotationAccelerationSpeed;
    public float RotationDecelerationSpeed;
    public float MaxRotationVelocity;
    public float ProjectileRadius;
    public float ProjectileSpeed;
    public float AsteroidRadius;
    public float AsteroidMinimumSpeed;
    public float AsteroidMaximumSpeed;
    public float Boundary;
    public float screenSideBoundary;
    public float screenBoundary;
    public int StepScoreDelay;
    public int StepScore;
    public int AsteroidDestructionScore;
    public int EnemyDestroyScore;
    public Unity.Mathematics.Random Rdm;
    /// <summary>
    /// x = min, y = max
    /// </summary>
    public Vector2 ScreenBordersBoundaryX;
    /// <summary>
    /// x = min, y = max
    /// </summary>
    public Vector2 ScreenBordersBoundaryY;
}

public class GameParameters : MonoBehaviour
{
    #region singleton
    private static GameParameters _instance;

    // SINGLETON
    public static GameParameters Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameParameters>();
                _instance.Parameters.Rdm = new Unity.Mathematics.Random((uint)Random.Range(0, 11111111));
                if (_instance == null)
                {
                    throw new System.Exception("There is no active object of type " + typeof(GameParameters).ToString() + " in the scene");

                }
            }
            return _instance;
        }
    }
    #endregion

    [SerializeField]
    public GameParametersStruct Parameters;
}
