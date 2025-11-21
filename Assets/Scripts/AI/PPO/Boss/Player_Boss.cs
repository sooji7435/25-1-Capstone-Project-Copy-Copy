using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player_Boss : MonoBehaviour
{
    public float moveSpeed = 3f;      // 이동 속도
    public float changeDirTime = 3f;    // 방향 바꾸는 주기
    public float areaLimit = 6f;        // 배회 범위 (맵 중심 기준 반경)

    private Rigidbody2D rb;
    private Vector2 moveDir;
    private float timer;
    private Vector2 center;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        center = transform.position; // 현재 위치를 기준점으로 설정
        SetRandomDirection();
    }

    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        // 주기적으로 방향 바꾸기
        if (timer >= changeDirTime)
        {
            SetRandomDirection();
            timer = 0f;
        }

        // 이동
        rb.linearVelocity = moveDir * moveSpeed;

        // 중심에서 너무 멀리 가면 반대 방향으로 되돌림
        Vector2 offset = (Vector2)transform.position - center;
        if (offset.magnitude > areaLimit)
        {
            moveDir = -offset.normalized;
        }
    }

    void SetRandomDirection()
    {
        // 랜덤 방향 설정 (정규화)
        moveDir = Random.insideUnitCircle.normalized;
    }
}
