using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public float maxHealth; // Enemy max health
    public float currentHealth; // Enemy current health
    public bool isDead; // Whether or not enemy is dead
    public Transform curDest; // Current destination
    
    private Rigidbody rb; // Enemy Rigidbody
    private Transform deathT; // Death Transform - Transform from which the enemy is being hit
    private NavMeshAgent agent; // Enemy's NavMeshAgent
    private Transform player; // Player transform

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth; // Set default health
        rb = GetComponent<Rigidbody>(); // Assigning enemy RigidBody
        isDead = false; // Indicating enemy is alive
        player = GameObject.FindGameObjectWithTag("Player").transform; // Setting player transform
        agent = GetComponent<NavMeshAgent>(); // Assigning NavMeshAgent
        curDest = player; // Setting first destination to player
    }

    // Update is called once per frame
    void Update()
    {
        if(!isDead) // Check if enemy is alive
        {
            agent.SetDestination(curDest.position); // If so, move towards target
        }

        if (currentHealth <= 0) // If dead
        {
            Die();
        }
    }

    public void TakeDamage(float damage, Transform addForcePoint) //Damage the enemy will take - Transform at which enemy will be pushed back
    {
        deathT = addForcePoint; // Setting point for adding force away from position of death
        currentHealth -= damage; // Take damage from health
    }

    public void Die()
    {
        isDead = true; // Indicate enemy is dead
        agent.enabled = false; // Disable NavMeshAgent so that RigidBody can take over
        rb.isKinematic = false; // Allow for gravity
        rb.AddForce(deathT.forward * 50f); // Push enemy away from point of death
        print("Enemy dead");
    }
}
