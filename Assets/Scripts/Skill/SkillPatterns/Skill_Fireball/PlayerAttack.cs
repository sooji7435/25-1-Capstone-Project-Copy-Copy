using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public int damage;
    public bool disableOnEnemyHit;

    [SerializeField] private LayerMask wallLayer;

    public int GetDamage() => damage;
    public void SetDamage(int damage) => this.damage = damage;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & wallLayer) != 0)
        {
            //Debug.Log("ddddd");
            gameObject.SetActive(false);
            return;
        }

        if (other.CompareTag("Wall"))
        {
            //Debug.Log("aaaaaaaaaaa");
            gameObject.SetActive(false);
            return;
        }
    }

}
