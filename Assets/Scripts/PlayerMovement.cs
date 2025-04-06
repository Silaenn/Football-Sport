using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Joystick joystick;
    [SerializeField] float speed = 5f;
    [SerializeField] float sprintSpeed = 8f;
    [SerializeField] float kickForce = 10f;
    [SerializeField] float kickRange = 1f;
    [SerializeField] float dribbleRange = 0.5f;
    [SerializeField] Button sprintButton;
    GameObject ball;
    Rigidbody2D playerRb;
    Rigidbody2D ballRb;
    Animator animator;
    Vector2 moveDirection;
    bool isDribbling = false;
    bool isSprinting = false;
    bool justKicked = false;
    Vector2 lastDirection = Vector2.right;

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        ball = GameObject.FindWithTag("Ball");
        animator = GetComponent<Animator>();
        if (ball != null)
        {
            ballRb = ball.GetComponent<Rigidbody2D>();
            if (ballRb != null)
            {
                ballRb.bodyType = RigidbodyType2D.Kinematic;
                ballRb.linearDamping = 2f;
                ballRb.angularDamping = 1f;
            }
            else
            {
                Debug.LogError("Ball has no Rigidbody2D!");
            }
        }
    }

    void Update()
    {
        if (ball != null && ballRb != null)
        {
            float currentDribbleRange = isSprinting ? dribbleRange * 1.5f : dribbleRange;
            float distance = Vector2.Distance(transform.position, ball.transform.position);

            if (distance <= currentDribbleRange && !isDribbling && !justKicked)
            {
                isDribbling = true;
            }
            else if (distance > currentDribbleRange && isDribbling)
            {
                isDribbling = false;
                ballRb.bodyType = RigidbodyType2D.Dynamic;
            }

            if (ballRb.bodyType == RigidbodyType2D.Dynamic)
            {
                Vector2 velocity = ballRb.linearVelocity;
                float rollSpeed = velocity.magnitude * 200f;
                ball.transform.Rotate(0, 0, rollSpeed * Time.deltaTime * (velocity.x > 0 ? -1 : 1));
            }
        }

        isSprinting = (sprintButton != null && EventSystem.current.currentSelectedGameObject == sprintButton.gameObject) || Input.GetKey(KeyCode.LeftShift);

        if (justKicked) justKicked = false;
    }

    void FixedUpdate()
    {
        moveDirection = joystick.Direction;
        if (moveDirection.magnitude < 0.1f) moveDirection = Vector2.zero;

        if (moveDirection != Vector2.zero)
        {
            lastDirection = moveDirection.normalized;
            float currentSpeed = isSprinting ? sprintSpeed : speed;
            playerRb.linearVelocity = moveDirection * currentSpeed;

            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (isDribbling && ball != null && ballRb != null && !justKicked)
            {
                float rollSpeed = moveDirection.magnitude * currentSpeed * 200f;
                ball.transform.Rotate(0, 0, rollSpeed * Time.fixedDeltaTime * (moveDirection.x > 0 ? -1 : 1));
            }
        }
        else
        {
            playerRb.linearVelocity = Vector2.zero;
        }

        animator.SetBool("isRunning", moveDirection != Vector2.zero);

        if (isDribbling && ball != null && ballRb != null && !justKicked)
        {
            float dribbleOffset = isSprinting ? 0.6f : 0.5f;
            Vector2 targetPos = (Vector2)transform.position + lastDirection * dribbleOffset;
            ballRb.bodyType = RigidbodyType2D.Kinematic;
            ball.transform.position = targetPos;
            ballRb.linearVelocity = Vector2.zero;
        }
    }

    public void KickBall()
    {
        if (ball == null || ballRb == null)
        {
            Debug.Log("Ball or Rigidbody2D is null!");
            return;
        }

        Vector2 ballPos = ball.transform.position;
        Vector2 playerPos = transform.position;
        float distance = Vector2.Distance(playerPos, ballPos);

        if (distance <= kickRange)
        {
            Debug.Log("Kick Start");
            isDribbling = false;
            justKicked = true;
            ballRb.bodyType = RigidbodyType2D.Dynamic;
            Vector2 kickDirection = joystick.Direction != Vector2.zero ? joystick.Direction.normalized : lastDirection.normalized;
            ballRb.linearVelocity = Vector2.zero;
            ballRb.AddForce(kickDirection * kickForce, ForceMode2D.Impulse);
            Debug.Log("Kick Applied, Direction: " + kickDirection + ", Force: " + kickForce + ", Velocity: " + ballRb.linearVelocity);
        }
        else
        {
            Debug.Log("Kick failed, distance too far: " + distance);
        }
    }
}