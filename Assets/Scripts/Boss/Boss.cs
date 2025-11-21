using SuperTiled2Unity;
using UnityEngine;
using UnityEngine.UI;
public enum EBossSkillAction
{
    Rush = 0,
    Meteo = 1,
    MAX

}
public class Boss : EnemyBase
{
    BossAnimatorController bossAnim; // 보스 전용 애니메이터 컨트롤러
    private BossData bossData; // 보스 전용 데이터
    private Fuzzy fuzzy;
    private float _skillCooldownTimer = 0f;
    private BossRoomSpawn bossRoomSpawn;

    /// <summary>
    /// 보스 초기화. 부모의 Init을 호출하고 보스만의 로직을 추가합니다.
    /// </summary>
    public override void Init()
    {
        // 보스 전용 캐스팅
        bossAnim = animController as BossAnimatorController;
        bossData = data as BossData;

        base.Init();

        // 보스 전용 초기화
        fuzzy = new Fuzzy();
        _skillCooldownTimer = 0f;

        GetComponent<CircleCollider2D>().enabled = false; // 보스의 충돌체 비 활성화
        bossRoomSpawn = Object.FindFirstObjectByType<BossRoomSpawn>();
    }

    /// <summary>
    /// 보스의 상태 머신을 설정합니다.
    /// </summary>
    protected override void SetState()
    {
        StateMachine = new StateMachine<IEnemyState>();

        // 보스 전용 상태들 등록
        //StateMachine.AddState(new BossIdle(this));
        StateMachine.AddState(new BossMove(this));
        StateMachine.AddState(new BossSkillAttack(this));
        //StateMachine.AddState(new BossCooldown(this));
        StateMachine.AddState(new BossDead(this));
        StateMachine.AddState(new BossSpawn(this));

    }


    public void StartBattle()
    {
        GetComponent<CircleCollider2D>().enabled = true;
        UIManager.Instance.bossUI.SetBossMaxHealth(bossData.maxHealth);
        UIManager.Instance.bossUI.SetBossName(bossData.Name);
        UIManager.Instance.bossUI.SetActiveBossUI(true);
        bossAnim.PlayStartBattle();
        StateMachine.ChangeState<BossSpawn>();
        bossRoomSpawn.StartSpawn();
        bossAnim.PlaySpawn();
    }

    protected override void OnDamaged()
    {
        UIManager.Instance.bossUI.SetBossHealth(bossData.currentHealth);
        GetAnimatorController().PlayDamage();
    }
    protected override void Dead()
    {
        isDead = true;
        UIManager.Instance.bossUI.SetBossHealth(bossData.currentHealth);
        StateMachine.ChangeState<BossDead>();
        bossRoomSpawn.StopSpawn();
        
    }
    // 스킬 쿨타임 관리 로직
    public void UpdateSkillCooldown()
    {


        _skillCooldownTimer += Time.deltaTime;
        if (_skillCooldownTimer >= bossData.attackCooldown)
        {
            StateMachine.ChangeState<BossSkillAttack>();
            _skillCooldownTimer = 0f;
        }
    }


    // 퍼지 로직으로 스킬 결정
    public void DecideSkill()
    {
        // 거리 및 체력 기반 퍼지 평가
        float dist = Vector2.Distance(transform.position, PlayerScript.Instance.GetPlayerTransform().position);
        float bossHP = GetHealthRatio();
        float playerHP = GetPlayerHealthRatio();

        Vector3 intent = fuzzy.Evaluate(dist, bossHP, playerHP);
        float aggr = intent.x;
        float defe = intent.y;
        float balance = intent.z;

        // 퍼지 결과 로그 (디버그용)
        string dominant =
            (aggr > defe && aggr > balance) ? "Aggressive" :
            (defe > aggr && defe > balance) ? "Defensive" : "Balanced";

        Debug.Log($"[Boss Fuzzy Skill] Intent: {dominant} (A:{aggr:F2}, D:{defe:F2}, B:{balance:F2})");

        // 스킬 결정
        EBossSkillAction skillType;
        if (dominant == "Aggressive")
            skillType = EBossSkillAction.Rush;
        else if (dominant == "Defensive")
            skillType = EBossSkillAction.Meteo;
        else
            skillType = (Random.value > 0.5f) ? EBossSkillAction.Rush : EBossSkillAction.Meteo;

        // 스킬 설정
        bossAnim.SetAttackIndex((int)skillType);
        bossData.AttackPatternSet((int)skillType);

        Debug.Log($"[Boss Skill Decision] → {skillType}");
    }
    public override void Parried()
    {

    }
    // 보스 전용 Getter
    public bool CheckPostAttackPauseComplete(float timer) => timer >= bossData.postAttackPauseTime;

    public float GetHealthRatio() => bossData.currentHealth / bossData.maxHealth;
    public float GetPlayerHealthRatio() => PlayerScript.Instance.Health / PlayerScript.Instance.GetPlayerRuntimeStats().maxHealth;

    public bool IsDead() => isDead;

    public void TryAttack()
    {
        if (StateMachine.GetCurrentState() is BossMove)
        {
            StateMachine.ChangeState<BossSkillAttack>();
        }
    }

    public void SetBossImmovable(bool immovable)
    {
        if (immovable)
        {
            // 위치 완전 고정 (벽충돌 유지, 몬스터 밀림 완전 차단)
            rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            // 회전만 고정, 위치는 이동 가능
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

}