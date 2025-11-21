using UnityEngine;

public class BossDoorAnimatorTrigger : MonoBehaviour
{
    public Animator animator;       // 문 Animator
    private bool alreadyStarted = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (alreadyStarted) return;            // 이미 열렸으면 무시
        if (!other.CompareTag("Player")) return;

        animator.SetTrigger("open");      // 애니메이션 재생
        alreadyStarted = true;                 // 다시는 열리지 않도록 설정

    }
}
