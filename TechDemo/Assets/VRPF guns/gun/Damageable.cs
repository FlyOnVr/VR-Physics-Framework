using UnityEngine;

public class Damageable : MonoBehaviour
{
    [Header("Damage Settings")]
    public int shotsUntilDeath = 3;         // hits to die from?
    public Collider targetCollider;         // opional: collider to detect hits 
    public Animator targetAnimator;         //  Put animator to destroy when taking amount of hits
    public bool destroyAnimator = true;     // If true, destroys the assigned Animator

    private int currentHits = 0;

    private void Start()
    {
        
        if (targetCollider == null)
        {
            targetCollider = GetComponent<Collider>();
            if (targetCollider == null)
            {
                Debug.LogWarning($"{gameObject.name} no collider assigned to damageable");
            }
        }

       
        if (targetAnimator == null)
        {
            targetAnimator = GetComponent<Animator>();
        }
    }

    public void TakeDamage(int damage)
    {
        currentHits += damage;

        Debug.Log($"{gameObject.name} took {damage} damage. Total hits: {currentHits}/{shotsUntilDeath}");

        if (currentHits >= shotsUntilDeath)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died!");

        // animator is removed for ragdolls
        if (destroyAnimator && targetAnimator != null)
        {
            Destroy(targetAnimator);
        }
    }
}
