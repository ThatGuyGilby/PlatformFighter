using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    private Rigidbody2D rb;

    [Header("Movement")]
    public float moveSpeed;
    public float acceleration;
    public float deceleration;
    public float airAccel;
    public float airDecel;
    public float velPower;
    [Space(10)]
    public float frictionAmount;
    [Space(10)]
    private Vector2 moveInput;
    private Vector2 lastMoveInput;
    public bool canMove = true;
    public bool canWallJump = true;
    public bool canClimb = true;

    [Header("Jump")]
    public float jumpForce;
    public Vector2 wallJumpForce;
    [Range(0, 1)]
    public float jumpCutMultiplier;
    public float wallJumpStopRunTime;
    [Space(5)]
    public float fallGravityMultiplier;
    private float gravityScale;
    [Space(5)]
    private bool isJumping;
    private bool jumpInputReleased;

    [Header("Climb & Slide")]
    public float slideForce;
    private bool isSliding;
    public float climbSpeed;

    [Header("Checks")]
    public Transform groundCheckPoint;
    public Vector2 groundCheckSize;
    [Space(5)]
    public Transform frontWallCheckPoint;
    public Transform backWallCheckPoint;
    public Vector2 wallCheckSize;
    [Space(10)]
    public float jumpCoyoteTime;
    private float lastGroundedTime;
    [Space(5)]
    public float jumpBufferTime;
    private float lastJumpTime;
    [Space(5)]
    public float wallJumpCoyoteTime;
    private float lastOnFrontWallTime;
    private float lastOnBackWallTime;
    [Space(10)]
    private bool isFacingRight = true;

    [Header("Layers & Tags")]
    public LayerMask groundLayer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        gravityScale = rb.gravityScale;

        OnJumpUp();
    }
    
    private void Update()
    {
        #region Run
        if (moveInput.x != 0)
            lastMoveInput.x = moveInput.x;
        if (moveInput.y != 0)
            lastMoveInput.y = moveInput.y;

        if ((lastMoveInput.x > 0 && !isFacingRight) || (lastMoveInput.x < 0 && isFacingRight))
        {
            Turn();
            isFacingRight = !isFacingRight;
        }
        #endregion

        #region Ground
        //checks if set box overlaps with ground
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
            isSliding = true;
        else
            isSliding = false;
        #endregion

        #region Jump
        //checks if the player is grounded or falling and that they have released jump
        if (rb.velocity.y <= 0)
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
            float targetSpeed = moveInput.x * moveSpeed;
            //calculate difference between current velocity and desired velocity
            float speedDif = targetSpeed - rb.velocity.x;

            //change acceleration rate depending on situation
            float accelRate;
            if (lastGroundedTime > 0)
            {
                accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            }
            else
            {
                accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration * airAccel : deceleration * airDecel;
            }

            //applies acceleration to speed difference, the raises to a set power so acceleration increases with higher speeds
            //finally multiplies by sign to reapply direction
            float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

            //applies force force to rigidbody, multiplying by Vector2.right so that it only affects X axis 
            rb.AddForce(movement * Vector2.right);
        }

        #endregion

        #region Friction
        //check if we're grounded and that we are trying to stop (not pressing forwards or backwards)
        if (lastGroundedTime > 0 && !isJumping && Mathf.Abs(moveInput.x) < 0.01f)
        {
            //then we use either the friction amount (~ 0.2) or our velocity
            float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(frictionAmount));
            //sets to movement direction
            amount *= Mathf.Sign(rb.velocity.x);
            //applies force against movement direction
            rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }
        #endregion

        #region Climb & Slide
        //check if we're grounded and that we are trying to stop (not pressing forwards or backwards)
        if ((lastOnBackWallTime > 0 || lastOnFrontWallTime > 0) && canClimb)
        {
            float amount = Mathf.Min(Mathf.Abs(rb.velocity.y), Mathf.Abs(slideForce));
            amount *= Mathf.Sign(rb.velocity.y);
            rb.AddForce(Vector2.up * -amount, ForceMode2D.Impulse);

            if (Input.GetKey(KeyCode.Z))
            {
                rb.velocity = new Vector2(rb.velocity.x, climbSpeed * moveInput.x);
            }
        }
        #endregion

        #region Jump Gravity
        if (rb.velocity.y < 0 && lastGroundedTime <= 0 && !isSliding)
        {
            rb.gravityScale = gravityScale * fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = gravityScale;
        }
        #endregion
    }

    #region Jump
    private void Jump(float jumpForce)
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        lastJumpTime = 0;
        isJumping = true;
        jumpInputReleased = false;
    }

    private void WallJump(float jumpForceX, float jumpForceY)
    {
        //flips x force if facing other direction, since when we Turn() our player the CheckPoints swap around
        if (!isFacingRight)
            jumpForceX *= -1;

        float momentumForce = rb.velocity.x * Mathf.Sign(jumpForceX);

        //apply force, using impluse force mode
        rb.AddForce(new Vector2(jumpForceX + momentumForce, jumpForceY), ForceMode2D.Impulse);
        //rb.velocity = new Vector2(jumpForceX, jumpForceY);
        lastJumpTime = 0;
        isJumping = true;
        jumpInputReleased = false;
    }

    public void OnJump()
    {
        lastJumpTime = jumpBufferTime;
        jumpInputReleased = false;
    }

    public void OnJumpUp()
    {
        if (rb.velocity.y > 0 && isJumping)
        {
            rb.AddForce(Vector2.down * rb.velocity.y * (1 - jumpCutMultiplier), ForceMode2D.Impulse);
        }

        jumpInputReleased = true;
        lastJumpTime = 0;
    }
    #endregion

    #region Inputs
    public void JumpCallback(InputAction.CallbackContext _context)
    {
        if (_context.started)
        {
            lastJumpTime = jumpBufferTime;
        }
        else if (_context.canceled)
        {
            OnJumpUp();
        }
    }

    public void Movement(InputAction.CallbackContext _context)
    {
        Vector2 _inputVector = _context.ReadValue<Vector2>();

        moveInput = _inputVector;
    }
    #endregion

    private IEnumerator StopMovement(float duration)
    {
        canMove = false;
        yield return new WaitForSeconds(duration);
        canMove = true;
    }

    private void Turn()
    {
        //stores scale and flips x axis, flipping the entire gameObject (could also rotate the player)
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(frontWallCheckPoint.position, wallCheckSize);
        Gizmos.DrawWireCube(backWallCheckPoint.position, wallCheckSize);
    }
}
