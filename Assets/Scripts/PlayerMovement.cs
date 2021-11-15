using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerCore))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCore playerCore;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float airAccel;
    [SerializeField] private float airDecel;
    [SerializeField] private float velPower;
    [Space(10)]
    [SerializeField] private float frictionAmount;
    [Space(10)]
    private Vector2 moveInput;
    private Vector2 lastMoveInput;
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canWallJump = true;
    [SerializeField] private bool canClimb = true;
    [SerializeField] private bool canDash = true;

    [Header("Jump")]
    [SerializeField] private float jumpForce;
    [SerializeField] private Vector2 wallJumpForce;
    [Range(0, 1)]
    [SerializeField] private float jumpCutMultiplier;
    [SerializeField] private float wallJumpStopRunTime;
    [Space(5)]
    [SerializeField] private float fallGravityMultiplier;
    private float gravityScale;
    [Space(5)]
    private bool isJumping;
    private bool jumpInputReleased;

    [Header("Climb & Slide")]
    [SerializeField] private float slideForce;
    private bool isSliding;
    [SerializeField] private float climbSpeed;

    [Header("Dash")]
    [SerializeField] private float dashForce;

    [Header("Checks")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize;
    [Space(5)]
    [SerializeField] private Transform frontWallCheckPoint;
    [SerializeField] private Transform backWallCheckPoint;
    [SerializeField] private Vector2 wallCheckSize;
    [Space(10)]
    [SerializeField] private float jumpCoyoteTime;
    private float lastGroundedTime;
    [Space(5)]
    [SerializeField] private float jumpBufferTime;
    private float lastJumpTime;
    [Space(5)]
    [SerializeField] private float wallJumpCoyoteTime;
    private float lastOnFrontWallTime;
    private float lastOnBackWallTime;
    [Space(10)]
    public bool isFacingRight = true;

    [Header("Layers & Tags")]
    public LayerMask groundLayer;

    private void Start()
    {
        gravityScale = playerCore.rigidbody2D.gravityScale;
        jumpInputReleased = true;
        lastJumpTime = 0;
    }
    
    private void Update()
    {
        #region Run
        if (moveInput.x != 0)
        {
            lastMoveInput.x = moveInput.x;
        }

        if (moveInput.y != 0)
        {
            lastMoveInput.y = moveInput.y;
        }

        if ((lastMoveInput.x > 0 && !isFacingRight) || (lastMoveInput.x < 0 && isFacingRight))
        {
            Turn();
            isFacingRight = !isFacingRight;
        }
        #endregion

        #region Ground
        //checks if the set hitbox overlaps with ground
        if (Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, groundLayer) && !isJumping)
        {
            //resets countdown timer
            lastGroundedTime = jumpCoyoteTime;
        }
        #endregion

        #region Wall
        //checks if set box overlaps with wall in front of player
        if (lastGroundedTime <= 0)
        {
            if (Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer))
            {
                //resets countdown timer
                lastOnFrontWallTime = wallJumpCoyoteTime;
                lastOnBackWallTime = 0;
            }
            else if (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer))
            {
                //resets countdown timer
                lastOnBackWallTime = wallJumpCoyoteTime;
                lastOnFrontWallTime = 0;
            }
        }

        if (lastOnFrontWallTime > 0 || lastOnBackWallTime > 0)
        {
            isSliding = true;
        }
        else
        {
            isSliding = false;
        }
        #endregion

        #region Jump
        //checks if the player is grounded or falling and that they have released jump
        if (playerCore.rigidbody2D.velocity.y <= 0)
        {
            //if so we are no longer jumping and could jump again
            isJumping = false;
        }

        //checks if was last grounded within coyoteTime and that jump has been pressed within bufferTime
        if (lastJumpTime > 0 && !isJumping && jumpInputReleased)
        {
            if (lastGroundedTime > 0)
            {
                lastGroundedTime = 0;
                Jump(jumpForce);
            }
            else if (lastOnFrontWallTime > 0 && canWallJump)
            {
                lastOnFrontWallTime = 0;
                WallJump(wallJumpForce.x, wallJumpForce.y);
                StopMovement(wallJumpStopRunTime);
            }
            else if (lastOnBackWallTime > 0 && canWallJump)
            {
                lastOnBackWallTime = 0;
                WallJump(-wallJumpForce.x, wallJumpForce.y);
                StopMovement(wallJumpStopRunTime);
            }
        }

        #endregion

        #region Timer
        lastGroundedTime -= Time.deltaTime;
        lastOnFrontWallTime -= Time.deltaTime;
        lastOnBackWallTime -= Time.deltaTime;
        lastJumpTime -= Time.deltaTime;
        #endregion
    }

    private void FixedUpdate()
    {
        #region Run
        if (canMove)
        {
            //calculate the direction we want to move in and our desired velocity
            float _targetSpeed = moveInput.x * moveSpeed;
            //calculate difference between current velocity and desired velocity
            float _speedDif = _targetSpeed - playerCore.rigidbody2D.velocity.x;

            //change acceleration rate depending on situation
            float _accelRate;
            if (lastGroundedTime > 0)
            {
                _accelRate = (Mathf.Abs(_targetSpeed) > 0.01f) ? acceleration : deceleration;
            }
            else
            {
                _accelRate = (Mathf.Abs(_targetSpeed) > 0.01f) ? acceleration * airAccel : deceleration * airDecel;
            }

            //applies acceleration to speed difference, the raises to a set power so acceleration increases with higher speeds
            //finally multiplies by sign to reapply direction
            float _movement = Mathf.Pow(Mathf.Abs(_speedDif) * _accelRate, velPower) * Mathf.Sign(_speedDif);

            //applies force force to rigidbody, multiplying by Vector2.right so that it only affects X axis 
            playerCore.rigidbody2D.AddForce(_movement * Vector2.right);
        }

        #endregion

        #region Friction
        //check if we're grounded and that we are trying to stop (not pressing forwards or backwards)
        if (lastGroundedTime > 0 && !isJumping && Mathf.Abs(moveInput.x) < 0.01f)
        {
            //then we use either the friction amount (~ 0.2) or our velocity
            float _amount = Mathf.Min(Mathf.Abs(playerCore.rigidbody2D.velocity.x), Mathf.Abs(frictionAmount));
            //sets to movement direction
            _amount *= Mathf.Sign(playerCore.rigidbody2D.velocity.x);
            //applies force against movement direction
            playerCore.rigidbody2D.AddForce(Vector2.right * -_amount, ForceMode2D.Impulse);
        }
        #endregion

        #region Climb & Slide
        //check if we're grounded and that we are trying to stop (not pressing forwards or backwards)
        if ((lastOnBackWallTime > 0 || lastOnFrontWallTime > 0) && canClimb)
        {
            float _amount = Mathf.Min(Mathf.Abs(playerCore.rigidbody2D.velocity.y), Mathf.Abs(slideForce));
            _amount *= Mathf.Sign(playerCore.rigidbody2D.velocity.y);
            playerCore.rigidbody2D.AddForce(Vector2.up * -_amount, ForceMode2D.Impulse);

            Debug.LogWarning("Change This Input Now");
            if (Input.GetKey(KeyCode.Z))
            {
                playerCore.rigidbody2D.velocity = new Vector2(playerCore.rigidbody2D.velocity.x, climbSpeed * moveInput.x);
            }
        }
        #endregion

        #region Jump Gravity
        if (playerCore.rigidbody2D.velocity.y < 0 && lastGroundedTime <= 0 && !isSliding)
        {
            playerCore.rigidbody2D.gravityScale = gravityScale * fallGravityMultiplier;
        }
        else
        {
            playerCore.rigidbody2D.gravityScale = gravityScale;
        }
        #endregion
    }

    #region Jump

    /// <summary>
    /// Make the player jump with the desired jump force.
    /// </summary>
    /// <param name="jumpForce">The desired jump force.</param>
    public void Jump(float jumpForce)
    {
        playerCore.rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        lastJumpTime = 0;
        isJumping = true;
        jumpInputReleased = false;
    }

    /// <summary>
    /// Make the player jump with the desired jump force.
    /// </summary>
    /// <param name="jumpForceX">The desired horizontal jump force.</param>
    /// <param name="jumpForceY">The desired vertical jump force.</param>
    private void WallJump(float jumpForceX, float jumpForceY)
    {
        //flips x force if facing other direction, since when we Turn() our player the CheckPoints swap around
        if (!isFacingRight)
        {
            jumpForceX *= -1;
        }

        float _momentumForce = playerCore.rigidbody2D.velocity.x * Mathf.Sign(jumpForceX);

        //apply force, using impluse force mode
        playerCore.rigidbody2D.AddForce(new Vector2(jumpForceX + _momentumForce, jumpForceY), ForceMode2D.Impulse);
        lastJumpTime = 0;
        isJumping = true;
        jumpInputReleased = false;
    }

    /// <summary>
    /// Prepare the variables at the start of a jump.
    /// </summary>
    private void OnJump()
    {
        lastJumpTime = jumpBufferTime;
        jumpInputReleased = false;
    }

    /// <summary>
    /// Cut the player's jump and reset inputs when the jump key is released.
    /// </summary>
    private void OnJumpUp()
    {
        if (playerCore.rigidbody2D.velocity.y > 0 && isJumping)
        {
            playerCore.rigidbody2D.AddForce(Vector2.down * playerCore.rigidbody2D.velocity.y * (1 - jumpCutMultiplier), ForceMode2D.Impulse);
        }

        jumpInputReleased = true;
        lastJumpTime = 0;
    }
    #endregion

    #region Dash
    /// <summary>
    /// Make the player dash with the desired dash force.
    /// </summary>
    /// <param name="_dashForce">The desired dash force.</param>
    /// <param name="_bypassCanDash">Bypass the can dash bool.</param>
    public void Dash(float _dashForce = 0, bool _bypassCanDash = false)
    {
        if (!canDash && !_bypassCanDash) return;

        // Get the direction to dash in
        Vector3 _direction = Vector2.right;

        if (!isFacingRight)
        {
            _direction = -Vector2.right;
        }

        // Apply the dash force to the player
        if (_dashForce == 0) _dashForce = dashForce;
        playerCore.rigidbody2D.AddForce(_direction * _dashForce, ForceMode2D.Impulse);
    }
    #endregion

    #region Inputs
    /// <summary>
    /// Handle the jump input.
    /// </summary>
    /// <param name="_context">The input callback context.</param>
    public void JumpCallback(InputAction.CallbackContext _context)
    {
        if (_context.started)
        {
            // If the jump key has been pressed start the jump.
            lastJumpTime = jumpBufferTime;
        }
        else if (_context.canceled)
        {
            // If the jump key has been released cut the jump.
            OnJumpUp();
        }
    }

    /// <summary>
    /// Handle the movement input.
    /// </summary>
    /// <param name="_context">The input callback context.</param>
    public void Movement(InputAction.CallbackContext _context)
    {
        Vector2 _inputVector = _context.ReadValue<Vector2>();

        moveInput = _inputVector;
    }

    /// <summary>
    /// Handle the dash input.
    /// </summary>
    /// <param name="_context">The input callback context.</param>
    public void Dash(InputAction.CallbackContext _context)
    {
        if (_context.started)
        {
            Dash();
        }
    }
    #endregion

    /// <summary>
    /// Stop the player moving for a desired duration.
    /// </summary>
    /// <param name="_duration">The desired duration.</param>
    /// <returns></returns>
    public IEnumerator StopMovement(float _duration)
    {
        canMove = false;
        yield return new WaitForSeconds(_duration);
        canMove = true;
    }

    /// <summary>
    /// Flip the x scale.
    /// </summary>
    private void Turn()
    {
        //stores scale and flips x axis, flipping the entire gameObject (could also rotate the player)
        Vector3 _scale = transform.localScale;
        _scale.x *= -1;
        transform.localScale = _scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(frontWallCheckPoint.position, wallCheckSize);
        Gizmos.DrawWireCube(backWallCheckPoint.position, wallCheckSize);
    }
}
