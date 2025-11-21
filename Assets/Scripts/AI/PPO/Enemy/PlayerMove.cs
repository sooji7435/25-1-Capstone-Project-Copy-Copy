using UnityEngine;

/// <summary>
/// 🔹 강화학습용 타깃 이동 AI
/// - 일정 반경 내에서 랜덤하게 이동
/// - 장애물(Wall, Obstacle) 충돌 시 반대 방향으로 회피
/// - 이동 경로는 완전 랜덤
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    [Header("Move Settings")]
    public float moveSpeed = 1.5f;             // 이동 속도
    public float dirChangeInterval = 2f;       // 방향 전환 주기
    public float moveRadius = 6f;              // 이동 반경
    public LayerMask obstacleMask;             // 장애물 감지용 (Wall 등)

    private Rigidbody2D rb;
    private Vector2 origin;
    private Vector2 moveDir;
    private float timer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        origin = transform.position;
        PickNewDirection();
    }

    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        // 방향 주기적으로 전환
        if (timer >= dirChangeInterval)
        {
            PickNewDirection();
            timer = 0f;
        }

        // 장애물 충돌 감지 (앞쪽 Ray)
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDir, 0.5f, obstacleMask);
        if (hit.collider)
        {
            // 반대 방향으로 회피
            moveDir = Vector2.Reflect(moveDir, hit.normal).normalized;
            timer = 0f; // 바로 방향 전환 타이머 리셋
        }

        // 원점으로부터 일정 반경 벗어나면 되돌아감
        if (Vector2.Distance(transform.position, origin) > moveRadius)
        {
            Vector2 toCenter = (origin - (Vector2)transform.position).normalized;
            moveDir = Vector2.Lerp(moveDir, toCenter, 0.6f).normalized;
        }

        // 이동
        rb.linearVelocity = moveDir * moveSpeed;

        // 시각화용 Debug Line
        Debug.DrawRay(transform.position, moveDir * 1.0f, Color.cyan);
    }

    // 랜덤 방향 선택
    void PickNewDirection()
    {
        moveDir = Random.insideUnitCircle.normalized;
    }

#if UNITY_EDITOR
    // 이동 반경 표시
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.3f, 0.3f);
        Gizmos.DrawWireSphere(Application.isPlaying ? origin : transform.position, moveRadius);
    }
#endif
}
