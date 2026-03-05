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
        if (anim == null) return;
        anim.SetBool("isRunning", isRunning);
    }

    public void EnemyAttack(bool isAttacking)
    {
        if (anim == null) return;
        anim.SetBool("isAttacking", isAttacking);
    }

    public void EnemyDie()
    {
        if (anim == null) return;
        anim.SetTrigger("Die");
    }

    public void EnemyHitted()
    {
        if (anim == null) return;
        anim.SetTrigger("Hit");
    }

    public void SetPlaybackSpeed(float speed)
    {
        if (anim == null) return;
        anim.speed = Mathf.Max(0f, speed);
    }
}
