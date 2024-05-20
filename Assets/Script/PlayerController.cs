using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rb;
    bool isFancingRight= true;

    [Header ("Movement")]
    public float moveSpeed = 5f;
    float horizontalMovement;

    [Header ("Jump")]
    public float jumpPower = 10f;
    public int maxJump = 2;
    int jumpsRemaining;

    [Header ("Ground Check")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;
    bool isGrounded;

    [Header ("Wall Check")]
    public Transform wallCheckPos;
    public Vector2 wallCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask wallLayer;

    [Header ("WallMovement")]
    public float wallSlideSpeed= 2;
    bool isWallSliding;

    //Wall Jumping
    bool isWallJumping;
    float wallJumpDirection;
    float wallJumpTime=0.5f;
    float wallJumpTimer;
    public Vector2 wallJumpPower = new Vector2(5f, 10f);



    [Header ("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float falSpeedlMultiplier = 2f;


    [Header ("Animation")]
    public Animator playerAnimator;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
        GroundCheck();
        Gravity();
        WallSlide();
        ProcessWallJump();
        

        if(!isWallJumping)
        {
            rb.velocity = new Vector2(horizontalMovement * moveSpeed, rb.velocity.y);
            Flip();
        }

        playerAnimator.SetFloat("yVelocity", rb.velocity.y);
        playerAnimator.SetFloat("Speed", rb.velocity.magnitude);
    }

    private void Gravity()
    {
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = baseGravity * falSpeedlMultiplier; //fall faster
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    public void WallSlide()
    {
        if (!isGrounded & WallCheck() &  horizontalMovement != 0)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed)); //Caps fall speed
        }
        else
        {
            isWallSliding = false;
        }
    }
    private void ProcessWallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpDirection = -transform.localScale.x;
            wallJumpTimer = wallJumpTime;

            CancelInvoke(nameof(CancelWallJump));
        }
        else if(wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
        
        }
    }

    private void CancelWallJump()
    {
        isWallJumping = false;
    }
    public void Jump(InputAction.CallbackContext context)
    {
        if (jumpsRemaining > 0)
        {
        if (context.performed)
        {
            //Hold down jump button = full height jump
            rb.velocity = new Vector2(rb.velocity.x, jumpPower);
            jumpsRemaining--;
            playerAnimator.SetTrigger("Jump");
        }
        else if (context.canceled && rb.velocity.y > 0)
        {
            //Light tap on jump button = small jump
            
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
                jumpsRemaining--;
                playerAnimator.SetTrigger("Jump");
         
        }
        }
        //Wall Jumping
        if(context.performed && wallJumpTimer > 0f)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y); //Wall Jump
            wallJumpTimer = 0;
            playerAnimator.SetTrigger("Jump");

            //Force flip
            if(transform.localScale.x != wallJumpDirection)
            {
                isFancingRight = !isFancingRight;
                Vector3 ls = transform.localScale;
                ls.x *= -1f;
                transform.localScale = ls;
            }

            Invoke(nameof(CancelWallJump), wallJumpTime + 0.1f);//Cancel wall jump after 0.1s
        }
    }

    private bool WallCheck()
    {
        return Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0, wallLayer);
    }
    private void GroundCheck()
    {
        if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            jumpsRemaining = maxJump;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
        

    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
    }

    private void Flip()
    {
        if(isFancingRight && horizontalMovement < 0 || !isFancingRight && horizontalMovement > 0)
        {
            isFancingRight = !isFancingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;
        }
       
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }
}
