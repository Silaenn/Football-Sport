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
    [SerializeField] float dribbleOffset = 0.1f;
    [SerializeField] float passForce = 8f;
    [SerializeField] Button sprintButton;
    [SerializeField] Button passButton;
    GameObject ball;
    Rigidbody2D playerRb;
    Rigidbody2D ballRb;
    Animator animator;
    Vector2 moveDirection;
    Vector2 lastDirection = Vector2.right;
    bool isDribbling = false;
    bool isSprinting = false;
    bool justKicked = false;
    float kickCooldown = 0.2f;
    float lastKickTime = -1f;
    float currentSpeed;

    TeammateAI[] teammates;
    PlayerMovement currentPlayer;

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
                ballRb.bodyType = RigidbodyType2D.Dynamic;
                ballRb.linearDamping = 2f;
                ballRb.angularDamping = 1f;
            }
            else
            {
                Debug.LogError("Ball has no Rigidbody2D!");
            }
        }

        if (sprintButton != null)
        {
            sprintButton.onClick.AddListener(() => isSprinting = true);
        }

        if (passButton != null)
        {
            passButton.onClick.AddListener(PassBall);
        }

        teammates = FindObjectsOfType<TeammateAI>();
        currentPlayer = this;
    }

    void Update()
    {
        MovePlayer();
        HandleDribbling();
        UpdateTeammates();
    }

    private void MovePlayer()
    {
        moveDirection = joystick.Direction;
        if (moveDirection.magnitude < 0.1f) moveDirection = Vector2.zero;

        if (moveDirection != Vector2.zero)
        {
            lastDirection = moveDirection.normalized;
            currentSpeed = isSprinting ? sprintSpeed : speed;
            Vector2 newPosition = playerRb.position + moveDirection * currentSpeed * Time.deltaTime;
            playerRb.MovePosition(newPosition);

            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        isSprinting = Input.GetKey(KeyCode.LeftShift) || (sprintButton != null && isSprinting);
        if (Input.GetKeyUp(KeyCode.LeftShift) || (sprintButton != null && !sprintButton.interactable))
        {
            isSprinting = false;
        }

        animator.SetBool("isRunning", moveDirection != Vector2.zero);
        if (justKicked) justKicked = false;
    }

    private void HandleDribbling()
    {
        if (ball == null || ballRb == null) return;

        Vector2 ballPos = ball.transform.position;
        Vector2 playerPos = transform.position;
        float distance = Vector2.Distance(playerPos, ballPos);

        if (distance <= dribbleRange && moveDirection != Vector2.zero && !justKicked)
        {
            isDribbling = true;
        }

        if (isDribbling)
        {
            Vector2 dribbleDirection = moveDirection != Vector2.zero ? moveDirection.normalized : lastDirection.normalized;
            dribbleOffset = isSprinting ? 0.4f : 0.2f;
            dribbleRange = isSprinting ? 0.4f : 0.3f;
            Vector2 targetBallPos = playerPos + dribbleDirection * (dribbleRange + dribbleOffset);

            ballRb.position = Vector2.Lerp(ballPos, targetBallPos, Time.deltaTime * 15f);
            ballRb.linearVelocity = Vector2.zero;
            ballRb.angularVelocity = 0f;

            distance = Vector2.Distance(playerPos, ballRb.position);

            if (distance > dribbleRange * 2f || moveDirection == Vector2.zero)
            {
                isDribbling = false;
            }
        }

    }

    public void KickBall()
    {
        if (Time.time - lastKickTime < kickCooldown) return;
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
            isDribbling = false; // Hentikan dribbling saat menendang
            justKicked = true;
            Vector2 kickDirection = joystick.Direction != Vector2.zero ? joystick.Direction.normalized : lastDirection.normalized;
            ballRb.linearVelocity = Vector2.zero;
            ballRb.AddForce(kickDirection * kickForce, ForceMode2D.Impulse);
            lastKickTime = Time.time;
            Debug.Log("Kick Applied, Direction: " + kickDirection + ", Force: " + kickForce + ", Velocity: " + ballRb.linearVelocity);
        }
        else
        {
            Debug.Log("Kick failed, distance too far: " + distance);
        }
    }

    void UpdateTeammates()
    {
        if (isDribbling)
        {
            foreach (var teammate in teammates)
            {
                teammate.SetPlayerWithBall(transform); // Beri tahu teammate siapa yang pegang bola
            }
        }
        else
        {
            foreach (var teammate in teammates)
            {
                teammate.SetPlayerWithBall(null); // Hentikan AI jika tidak ada yang dribble
            }
        }
    }

    public void PassBall()
    {
        if (Time.time - lastKickTime < kickCooldown) return;
        if (ball == null || ballRb == null) return;

        // Cari teammate terdekat yang meminta umpan
        TeammateAI target = null;
        float minDistance = float.MaxValue;
        foreach (var teammate in teammates)
        {
            float distance = Vector2.Distance(transform.position, teammate.transform.position);
            if (distance < minDistance && !Physics2D.Raycast(transform.position, (teammate.transform.position - transform.position).normalized, distance, LayerMask.GetMask("Opponent")))
            {
                minDistance = distance;
                target = teammate;
            }
        }

        if (target != null)
        {
            isDribbling = false;
            justKicked = true;
            Vector2 passDirection = (target.transform.position - transform.position).normalized;
            ballRb.linearVelocity = Vector2.zero;
            ballRb.AddForce(passDirection * passForce, ForceMode2D.Impulse);
            lastKickTime = Time.time;

            // Ganti kontrol ke teammate
            SwitchControlToTeammate(target);
        }
    }

    void SwitchControlToTeammate(TeammateAI target)
    {
        // Nonaktifkan PlayerMovement di pemain saat ini
        PlayerMovement currentPlayer = GetComponent<PlayerMovement>();
        currentPlayer.enabled = false;

        // Aktifkan TeammateAI di pemain saat ini (jadikan AI)
        TeammateAI currentAI = gameObject.GetComponent<TeammateAI>();
        if (currentAI == null)
        {
            currentAI = gameObject.AddComponent<TeammateAI>();
        }
        currentAI.enabled = true;

        // Aktifkan PlayerMovement di target
        PlayerMovement targetPlayer = target.GetComponent<PlayerMovement>();
        if (targetPlayer == null)
        {
            targetPlayer = target.gameObject.AddComponent<PlayerMovement>();
            targetPlayer.joystick = joystick;
            targetPlayer.speed = speed;
            targetPlayer.sprintSpeed = sprintSpeed;
            targetPlayer.kickForce = kickForce;
            targetPlayer.kickRange = kickRange;
            targetPlayer.dribbleRange = dribbleRange;
            targetPlayer.dribbleOffset = dribbleOffset;
            targetPlayer.passForce = passForce;
            targetPlayer.sprintButton = sprintButton;
            targetPlayer.passButton = passButton;
        }
        targetPlayer.enabled = true;

        // Nonaktifkan TeammateAI di target
        target.enabled = false;

        // Perbarui referensi teammates
        teammates = FindObjectsOfType<TeammateAI>();
    }
}