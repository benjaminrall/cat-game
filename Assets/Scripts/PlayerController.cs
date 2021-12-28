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
    public float wallClimbSpeed = 2f;
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
    private bool _onRightClimbableWall;
    private bool _onLeftClimbableWall;
    private bool _running;
    private bool _climbedWall;
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
        _climbedWall = false;
        _velocity = Vector3.zero;
    }

    private void Update()
    {
        Move();
    }

    // Collision detection
    private void FixedUpdate()
    {
        Transform t = transform;
        Vector3 pos = t.position;
        Vector3 localScale = t.localScale;
        Vector3 verticalOffset = new (0, localScale.y / 2, 0);
        Vector3 horizontalOffset = new (localScale.x / 2, 0, 0);

        Collider[] groundColliders = Physics.OverlapBox(pos - verticalOffset,
            new Vector3((localScale.x - .001f) / 2, groundCollisionRadius, 0), Quaternion.identity, ground);
        Collider[] rightWallColliders = Physics.OverlapBox(pos + horizontalOffset,
            new Vector3(wallCollisionRadius, (localScale.y - 0.001f) / 2, 0), Quaternion.identity, walls);
        Collider[] leftWallColliders = Physics.OverlapBox(pos - horizontalOffset,
            new Vector3(wallCollisionRadius, (localScale.y - 0.001f) / 2, 0), Quaternion.identity, walls);
        Collider[] rightClimbableWallColliders = Physics.OverlapBox(pos + horizontalOffset,
            new Vector3(wallCollisionRadius, (localScale.y - 0.001f) / 2, 0), Quaternion.identity, climbableWalls);
        Collider[] leftClimbableWallColliders = Physics.OverlapBox(pos - horizontalOffset,
            new Vector3(wallCollisionRadius, (localScale.y - 0.001f) / 2, 0), Quaternion.identity, climbableWalls);

        _grounded = groundColliders.Length > 0;
        _onRightWall = rightWallColliders.Length > 0;
        _onLeftWall = leftWallColliders.Length > 0;
        _onRightClimbableWall = rightClimbableWallColliders.Length > 0;
        _onLeftClimbableWall = leftClimbableWallColliders.Length > 0;
    }

    private void Move()
    {
        // Gets inputs
        bool jump = Input.GetButtonDown("Jump");
        bool running = Input.GetButton("Sprint");
        float movement = Input.GetAxisRaw("Horizontal");
        float climbing = Input.GetAxisRaw("Vertical");
        
        // Adjusts input for running and locks movement to maximum in each direction
        _running = running || _running;
        if (movement != 0)
        {
            _facingRight = movement > 0;
            movement = movement > 0 ? 1 : -1;
        }
        else _running = false;
        if (climbing != 0)
        {
            _running = true;
            climbing = climbing > 0 ? 1 : -1;
        }
        
        
        // Movement
        if (_grounded || _climbedWall && !(_onLeftClimbableWall || _onRightClimbableWall))
        {
            float targetSpeed = movement * maxSpeed;
            targetSpeed = _running ? targetSpeed * runningMultiplier : targetSpeed;

            if (_facingRight && _onRightClimbableWall || !_facingRight && _onLeftClimbableWall)
                targetSpeed = 0;
            Vector3 velocity = _rigidbody.velocity;
            Vector3 targetVelocity = new (targetSpeed, velocity.y, velocity.z);

            _rigidbody.velocity = Vector3.SmoothDamp(velocity, targetVelocity, ref _velocity, movementSmoothing);
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
                true when _onRightWall || _onRightClimbableWall => Vector2.zero,
                false when _onLeftWall || _onLeftClimbableWall => Vector2.zero,
                _ => force
            };
            
            _rigidbody.AddForce(force.x, force.y, 0);
        }

        
        // Wall sliding
        if (!_grounded && (_onRightWall || _onLeftWall
                                        || _onRightClimbableWall && movement <= 0 
                                        || _onLeftClimbableWall && movement >= 0))
        {
            Vector3 targetVelocity = new (0, -wallSlideSpeed, 0);
            _rigidbody.velocity =
                Vector3.SmoothDamp(_rigidbody.velocity, targetVelocity, ref _velocity, movementSmoothing);
        }
        
        
        _rigidbody.useGravity = true;
        _climbedWall = false;
        if (_onRightClimbableWall && movement > 0 || _onLeftClimbableWall && movement < 0)
        {
            _rigidbody.useGravity = false;
            
            // Wall hang
            _rigidbody.velocity =
                Vector3.SmoothDamp(_rigidbody.velocity, Vector3.zero, ref _velocity, movementSmoothing);

            // Wall climb
            _climbedWall = climbing != 0;
            Vector3 velocity = _rigidbody.velocity;
            Vector3 targetVelocity = new (velocity.x, climbing * wallClimbSpeed, velocity.z);
            _rigidbody.velocity = Vector3.SmoothDamp(velocity, targetVelocity, ref _velocity, movementSmoothing);
        }
    }
}
