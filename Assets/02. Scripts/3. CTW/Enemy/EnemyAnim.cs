using UnityEngine;

public class EnemyAnim : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void EnemyRunning(bool isRunning)
    {
        anim.SetBool("isRunning", isRunning);
    }

    public void EnemyAttack(bool isAttacking)
    {
        anim.SetBool("isAttacking", isAttacking);
    }

    public void EnemyDie()
    {
        anim.SetTrigger("Die");
    }

    public void EnemyHitted()
    {
        anim.SetTrigger("Hit");
    }
}
