using UnityEngine;

/// <summary>
/// BossRLAgent와 함께 사용하는 훈련 전용 보스 스크립트
/// - FSM, UIManager, Animator, BossData 제거
/// - 단순히 체력/거리/상태만 관리
/// - 학습 중 BossRLAgent가 이동과 행동을 제어함
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BossTraing : MonoBehaviour
{
    [Header("Training Stats")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("References")]
    public Transform player;     // 플레이어 참조 (PlayerDummy)
    public Fuzzy fuzzy;          // 퍼지 로직 (AI 행동 판단)
    private Rigidbody2D rb;
    private BossAgent rlAgent;

    private bool isDead = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        fuzzy = new Fuzzy();

        // RL Agent 자동 연결
        rlAgent = GetComponent<BossAgent>();
        if (rlAgent != null)
            rlAgent.player = player;
    }

    void Start()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    void Update()
    {
        // 데미지 시뮬레이션 (랜덤으로 조금씩 체력 감소)
        if (!isDead && Random.value < 0.001f)
        {
            TakeDamage(Random.Range(0.5f, 2f));
        }

        // 혹시 플레이어 없으면 찾아줌
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Boss died (Training)");

        // RL 보상 (패널티)
        if (rlAgent != null)
        {
            rlAgent.AddReward(-1.0f);
            rlAgent.EndEpisode();
        }

        // 리셋
        Invoke(nameof(Respawn), 2f);
    }

    private void Respawn()
    {
        transform.position = new Vector2(Random.Range(-3f, 3f), Random.Range(-2f, 2f));
        currentHealth = maxHealth;
        isDead = false;

        if (rlAgent != null)
            rlAgent.OnEpisodeBegin();
    }

    public float GetHealthNormalized()
    {
        return currentHealth / maxHealth;
    }
}
