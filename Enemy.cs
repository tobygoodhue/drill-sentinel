using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float maxHealth; // Enemy max health
    public float currentHealth; // Enemy current health
    public bool isDead;
    
    private Rigidbody rb; // Enemy Rigidbody
    private Transform deathT; // Death Transform - Transform from which the enemy is being hit

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        isDead = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHealth <= 0) // If dead
        {
            Die();
        }
    }

    public void TakeDamage(float damage, Transform addForcePoint)
    {
        deathT = addForcePoint;
        currentHealth -= damage;
    }

    public void Die()
    {
        isDead = true;
        rb.isKinematic = false; // Allow for gravity
        rb.AddForce(deathT.forward * 50f); // Push enemy away from point of death
        print("Enemy dead");
    }
}
