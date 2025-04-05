using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Joystick joystick;
    [SerializeField] float speed = 5f;
    [SerializeField] float kickForce = 10f;
    [SerializeField] float kickRange = 1f;
    GameObject ball;
    Rigidbody2D rb;
    Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ball = GameObject.FindWithTag("Ball");
        animator = GetComponent<Animator>();
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
    }

    public void KickBall()
    {
        if (ball == null) return;

        Vector2 ballPos = ball.transform.position;
        Vector2 playerPos = transform.position;
        float distance = Vector2.Distance(ballPos, playerPos);

        if (distance <= kickRange)
        {
            Vector2 kickDirection = joystick.Direction != Vector2.zero ? joystick.Direction : Vector2.right;
            Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
            ballRb.linearVelocity = kickDirection.normalized * kickForce;
        }
    }
}
