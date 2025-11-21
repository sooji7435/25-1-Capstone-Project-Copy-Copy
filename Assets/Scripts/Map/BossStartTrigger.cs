using UnityEngine;

public class BossStartTrigger : MonoBehaviour
{
    private bool alreadyStarted = false;
    public Animator animator;
    public BoxCollider2D doorCollider;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (alreadyStarted) return;          // 이미 시작했으면 무시
        if (!other.CompareTag("Player")) return;

        alreadyStarted = true;               // 다시 안 실행되게 막기
        gameObject.SetActive(false);         // 트리거 자체 비활성화
        animator.SetTrigger("close");
        doorCollider.isTrigger = false;
        Boss boss = Object.FindFirstObjectByType<Boss>();
        if (boss != null)
        {
            boss.StartBattle();
            AudioManager.Instance.ChangeBGM("BossBattle");

            Debug.Log("보스전 시작");
        }
    }
}
