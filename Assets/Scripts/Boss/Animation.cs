using UnityEngine;

public class BossRoomDoor : MonoBehaviour
{
    public float frameRate = 0.05f;
    public Sprite[] sprites;       // 문 애니메이션 전체 (23~32 포함)
    public int startFrame = 23;
    public int endFrame = 32;

    private SpriteRenderer sr;
    private float timer = 0f;
    private int index;
    private bool isPlaying = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprites[startFrame];   // 처음엔 열린 상태 프레임
    }

    void Update()
    {
        if (!isPlaying) return;

        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            timer = 0f;
            index++;

            if (index > endFrame)
            {
                isPlaying = false;
                return;
            }

            sr.sprite = sprites[index];
        }
    }

    public void PlayClose()
    {
        index = startFrame;
        sr.sprite = sprites[startFrame];
        isPlaying = true;
    }
}
