using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody _rigidbody;

    [Header("Movement Parameters")] 
    public float maxSpeed = 10f;
    public float runningMultiplier = 1.2f;
    [Space] 
    public float jumpForce = 400f;
    public Vector2 stationaryJumpMultiplier;
    [Space] 
    public float wallSlideSpeed = 1;
    [Space] 
    [Range(0, .3f)] public float movementSmoothing = .05f;
    [Space]
    
    [Header("Collision Parameters")]
    public LayerMask ground;
    public float groundCollisionRadius = .15f;
    [Space]
    public LayerMask walls;
    public LayerMask climbableWalls;
    public float wallCollisionRadius = .2f;

    // Movement flags
    private bool _grounded;
    private bool _onRightWall;
    private bool _onLeftWall;
    private bool _running;
    private bool _facingRight;
    
    private Vector3 _velocity;

    private void Awake()
    {
        // Initialisation of private attributes
        _rigidbody = GetComponent<Rigidbody>();
        _grounded = false;
        _onRightWall = false;
        _onLeftWall = false;
        _running = false;
        _facingRight = true;
        _velocity = Vector3.zero;
    }

    private void Update()
    {
        // Gets inputs
        bool jump = Input.GetButtonDown("Jump");
        bool running = Input.GetButton("Sprint");
        float movement = Input.GetAxisRaw("Horizontal");

        // Adjusts input for running and locks movement to maximum in each direction
        _running = running || _running;
        if (movement != 0)
            movement = movement > 0 ? 1 : -1;
        else _running = false;
        
        // Movement
        if (_grounded)
        {
            float targetSpeed = movement * maxSpeed;
            targetSpeed = _running ? targetSpeed * runningMultiplier : targetSpeed;
            
            Vector3 velocity = _rigidbody.velocity;
            Vector3 targetVelocity = new (targetSpeed, velocity.y, velocity.z);
            
            _rigidbody.velocity = Vector3.SmoothDamp(velocity, targetVelocity, ref _velocity, movementSmoothing);

            if (movement != 0)
                _facingRight = movement > 0;
        }

        // Jumping
        if (_grounded && jump)
        {
            _grounded = false;
            Vector2 force = Vector2.zero;
            if (_running)
            {
                force.y = jumpForce;
            }
            else
            {
                _rigidbody.velocity = Vector3.zero;
                force.x = stationaryJumpMultiplier.x * jumpForce * (_facingRight ? 1 : -1);
                force.y = stationaryJumpMultiplier.y * jumpForce;
            }

            force = _facingRight switch
            {
                true when _onRightWall => Vector2.zero,
                false when _onLeftWall => Vector2.zero,
                _ => force
            };
            
            _rigidbody.AddForce(force.x, force.y, 0);
        }

        if (!_grounded && (_onRightWall || _onLeftWall))
        {
            Vector3 targetVelocity = new (0, -wallSlideSpeed, 0);
            _rigidbody.velocity =
                Vector3.SmoothDamp(_rigidbody.velocity, targetVelocity, ref _velocity, movementSmoothing);
        }
    }

    // Collision detection
    private void FixedUpdate()
    {
        Transform t = transform;
        Vector3 pos = t.position;
        Vector3 localScale = t.localScale;
        Vector3 verticalOffset = new (0, localScale.y / 2, 0);
        Vector3 horizontalOffset = new (localScale.x / 2, 0, 0);
        
        Collider[] groundColliders = Physics.OverlapSphere(pos - verticalOffset, groundCollisionRadius, ground);
        Collider[] rightWallColliders = Physics.OverlapSphere(pos + horizontalOffset, wallCollisionRadius, walls);
        Collider[] leftWallColliders = Physics.OverlapSphere(pos - horizontalOffset, wallCollisionRadius, walls);

        _grounded = groundColliders.Length > 0;
        _onRightWall = rightWallColliders.Length > 0;
        _onLeftWall = leftWallColliders.Length > 0;
    }
}
