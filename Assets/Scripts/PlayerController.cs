using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    bool canAttack = true;
    public float moveSpeed = 1f;
    public float collisionOffset = 0.05f;
    public ContactFilter2D movementFilter;
    Vector2 movementInput;
    SpriteRenderer spriteRenderer;
    Animator animator;
    Rigidbody2D rb;
    List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    bool canMove = true;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {   //If movement input != 0, try to move
        if (canMove)
        {
            if (movementInput != Vector2.zero)
            {
                bool success = PlayerMovement(movementInput);
                if (!success)
                {
                    success = PlayerMovement(new Vector2(movementInput.x, 0));
                    if (!success)
                    {
                        success = PlayerMovement(new Vector2(0, movementInput.y));
                    }
                }
                animator.SetInteger("AnimState", success ? 1 : 0);
            }
            else
            {
                animator.SetInteger("AnimState", 0);
            }

            //Set direction of sprite to movement direction
            if (movementInput.x < 0)
            {
                spriteRenderer.flipX = true;
            }
            else if (movementInput.x > 0)
            {
                spriteRenderer.flipX = false;
            }
        }
    }

    private bool PlayerMovement(Vector2 direction)
    {
        //Check out for potential collisions
        int count = rb.Cast
        (
                direction,
                movementFilter,
                castCollisions,
                moveSpeed * Time.fixedDeltaTime + collisionOffset
        );
        if (count == 0)
        {
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
            return true;
        }
        else
        {
            return false;
        }
    }

    void OnMove(InputValue movementValue)
    {
        movementInput = movementValue.Get<Vector2>();
    }

    void OnFire()
    {
        if (!canAttack) return; // If attacking, no further attacks allowed.

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Attack1") && stateInfo.normalizedTime < 1.0f)
        {
            return;
        }

        canAttack = false; // Block next Attack
        animator.SetTrigger("Attack1");
        StartCoroutine(ResetAttackCooldown());
    }

    IEnumerator ResetAttackCooldown()
    {
        yield return new WaitForSeconds(0.5f); // Delay 0.5s before another attack
        canAttack = true;
    }

    //Lock Movement while attack
    public void LockMovement()
    {
        canMove = false;
    }
    public void UnLockMovement()
    {
        canMove = true;
    }
}
