using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Joystick joystick;
    [SerializeField] float speed = 5f;
    [SerializeField] float kickForce = 10f;
    [SerializeField] float kickRange = 1f;
    [SerializeField] float dribbleRange = 0.5f;
    GameObject ball;
    Rigidbody2D rb;
    Animator animator;
    bool isDribbling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ball = GameObject.FindWithTag("Ball");
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (ball != null)
        {
            float distance = Vector2.Distance(transform.position, ball.transform.position);

            if (distance <= dribbleRange && !isDribbling)
            {
                StartDribbling();
            }
            else if (distance > dribbleRange && isDribbling)
            {
                StopDribbling(true);
            }
        }
    }


    void FixedUpdate()
    {
        Vector2 moveDirection = joystick.Direction;

        rb.linearVelocity = moveDirection * speed;

        if (moveDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        bool isRunning = moveDirection != Vector2.zero;
        animator.SetBool("isRunning", isRunning);

        if (isDribbling && ball != null)
        {
            Vector2 dribbleOffset = moveDirection != Vector2.zero ? moveDirection : Vector2.right;
            ball.transform.position = (Vector2)transform.position + dribbleOffset.normalized * 0.5f;
            ball.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
    }

    public void KickBall()
    {
        if (ball == null) return;

        Vector2 ballPos = ball.transform.position;
        Vector2 playerPos = transform.position;
        float distance = Vector2.Distance(ballPos, playerPos);

        if (distance <= kickRange)
        {
            StopDribbling(true);
            Vector2 kickDirection = joystick.Direction != Vector2.zero ? joystick.Direction : Vector2.right;
            Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
            ballRb.linearVelocity = kickDirection.normalized * kickForce;
        }
    }

    void StopDribbling(bool isKicking)
    {
        isDribbling = false;

        if (ball != null && !isKicking)
        {
            Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
            Vector2 lastDirection = joystick.Direction != Vector2.zero ? joystick.Direction : Vector2.right;
            ballRb.linearVelocity = lastDirection.normalized * speed * 0.5f;
        }
    }

    void StartDribbling()
    {
        isDribbling = true;
    }

}
