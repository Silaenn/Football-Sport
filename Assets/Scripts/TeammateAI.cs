using UnityEngine;

public class TeammateAI : MonoBehaviour
{
    [SerializeField] float speed = 4f;
    [SerializeField] float openSpaceRadius = 5f;
    [SerializeField] GameObject passIndicator;
    Rigidbody2D rb;
    GameObject ball;
    Transform playerWithBall;
    bool isRequestingPass = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ball = GameObject.FindWithTag("Ball");
        passIndicator.SetActive(false);
    }

    void Update()
    {
        if (playerWithBall != null)
        {
            MoveToOpenSpace();
            CheckPassOpportunity();
        }
    }

    public void SetPlayerWithBall(Transform player)
    {
        playerWithBall = player;
    }

    void MoveToOpenSpace()
    {
        Vector2 targetPos = FindOpenSpace();
        Vector2 moveDirection = (targetPos - (Vector2)transform.position).normalized;
        rb.MovePosition(rb.position + moveDirection * speed * Time.deltaTime);
    }

    Vector2 FindOpenSpace()
    {
        Vector2 ballPos = ball.transform.position;
        Vector2 randomOffset = Random.insideUnitCircle * openSpaceRadius;
        Vector2 targetPos = ballPos + randomOffset;

        if (Vector2.Distance(targetPos, ballPos) > openSpaceRadius)
        {
            targetPos = ballPos + (randomOffset.normalized * openSpaceRadius);
        }

        return targetPos;
    }

    void CheckPassOpportunity()
    {
        float distanceToBall = Vector2.Distance(transform.position, ball.transform.position);

        if (distanceToBall < openSpaceRadius && !Physics2D.Raycast(transform.position, (ball.transform.position - transform.position).normalized, distanceToBall, LayerMask.GetMask("Opponent")))
        {
            isRequestingPass = true;
            passIndicator.SetActive(true);
        }
        else
        {
            isRequestingPass = false;
            passIndicator.SetActive(false);
        }
    }

}
