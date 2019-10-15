using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                if (_instance == null)
                {
                    throw new System.Exception("There is no active object of type " + typeof(GameParameters).ToString() + " in the scene");

                }
            }
            return _instance;
        }
    }
    #endregion

    [Header("Player Settings")]
    [SerializeField] private long _shootDelay = 30;
    public long ShootDelay { get => _shootDelay; }

    [SerializeField] private float _playerRadius = 0.5f;
    public float PlayerRadius { get => _playerRadius; }

    [SerializeField] private float _initialPlayerSpeed = 0.1f;
    public float InitialPlayerSpeed { get => _initialPlayerSpeed; }

    [Header("Player Inertia Settings")]
    [SerializeField] private float _accelerationSpeed = 0.2f;
    public float AccelerationSpeed { get => _accelerationSpeed; }

    [SerializeField] private float _decelerationSpeed = 0.025f;
    public float DecelerationSpeed { get => _decelerationSpeed; }

    [SerializeField] private float _maxVelocity = 0.2f;
    public float MaxVelocity { get => _maxVelocity; }

    [SerializeField] private float _rotationAccelerationSpeed = 0.2f;
    public float RotationAccelerationSpeed { get => _rotationAccelerationSpeed; }

    [SerializeField] private float _rotationDecelerationSpeed = 0.025f;
    public float RotationDecelerationSpeed { get => _rotationDecelerationSpeed; }

    [SerializeField] private float _maxRotationVelocity = 0.05f;
    public float MaxRotationVelocity { get => _maxRotationVelocity; }

    

    [Header("Projectile Settings")]
    [SerializeField] private float _projectileRadius = 0.2f;
    public float ProjectileRadius { get => _projectileRadius; }

    [SerializeField] private float _projectileSpeed = 0.08f;
    public float ProjectileSpeed { get => _projectileSpeed; }




    [Header("Asteroids Settings")]
    [SerializeField] private float _asteroidRadius = 5.0f;
    public float AsteroidRadius { get => _asteroidRadius; }


    [SerializeField] private float _asteroidMinimumSpeed = 10.0f;
    public float AsteroidMinimumSpeed { get => _asteroidMinimumSpeed; }


    [SerializeField] private float _asteroidMaximumSpeed = 30.0f;
    public float AsteroidMaximumSpeed { get => _asteroidMaximumSpeed; }

    [SerializeField] private float _minimalBoundary = -150f;
    public float MinimalBoundary { get => _minimalBoundary; }

    [SerializeField] private float _maximalBoundary = 150f;
    public float MaximalBoundary { get => _maximalBoundary; }

    [Header("Score Settings")]
    [SerializeField] private int _stepScore = 1;
    public int StepScore { get => _stepScore; }

    [SerializeField] private int _asteroidDestroyScore = 20;
    public int AsteroidDestructionScore { get => _asteroidDestroyScore; }

    [SerializeField] private int _enemyDestroyScore = 500;
    public int EnemyDestroyScore { get => _enemyDestroyScore; }
}
