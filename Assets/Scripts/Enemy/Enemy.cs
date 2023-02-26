using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum EnemyType { Swoop, Aim, Bomber }
    public enum ShootType { Normal, Double, Triple }

    [Header("Health")]
    public int health = 5;

    [Header("Shooting")]
    public float shootRate = 0.5f;
    public ShootType shootType;
    public GameObject projectilePrefab;

    [Header("Movement")]
    public float moveSpeed = 15;
    public float maximumX = 10;
    public float maximumY = 1;
    public float yOffset = 3;
    public float yMultiplier = 0.1f;
    public float turnSpeed = 5;

    [Header("Bomber")]
    public int deathProjectileCount;
    public float timeUntilAutoDeath = 10;

    [Header("Type")]
    public EnemyType type;

    [Header("Death")]
    public GameObject deathEffect;
    public GameObject healthPickup;
    public float healthDropChance;

    private int currentHealth;

    // swoop move
    private int xDirection;
    private int yDirection;
    private Vector2 velocity;

    // bomber
    private Vector2 playerTrackingPos;

    private Animator animator;
    private float aliveTime;
    void Start()
    {
        currentHealth = health;

        xDirection = GetRandomDir();
        yDirection = GetRandomDir();

        if (shootRate > 0)
            InvokeRepeating(nameof(Shoot), 1, shootRate);

        animator = GetComponent<Animator>();

        SoundManager.instance.PlaySound("enemyspawn", 0.5f);
    }

    int GetRandomDir()
    {
        int rng = Random.Range(0, 2);

        if (rng == 0)
            return -1;
        return 1;
    }

    void Update()
    {
        aliveTime += Time.deltaTime;

        if (aliveTime > 0.75f)
            Move();

        if (aliveTime > timeUntilAutoDeath && type == EnemyType.Bomber)
            Die();
    }

    void Shoot()
    {
        int bulletCount = 1;
        if (shootType == ShootType.Double)
            bulletCount = 2;
        else if (shootType == ShootType.Triple)
            bulletCount = 3;

        float randomAngle = Random.Range(-10, 10);

        for (int i = 0; i < bulletCount; i++)
        {
            Vector2 spawnPos = transform.position;
            Vector2 moveDir = Vector2.zero;
            Vector3 spawnRot = Vector3.zero;
            if (type == EnemyType.Swoop)
            {
                spawnPos -= (Vector2)transform.up;

                moveDir.y = -1;

                spawnRot.z = randomAngle;
                if (shootType == ShootType.Double)
                {
                    if (i == 0)
                        spawnRot.z -= 30;
                    else
                        spawnRot.z += 30;
                }
                if (shootType == ShootType.Triple)
                {
                    if (i == 1)
                        spawnRot.z -= 30;
                    else if (i == 2)
                        spawnRot.z += 30;
                }

                moveDir.x = spawnRot.z / 100;
            }
            else if (type == EnemyType.Aim)
            {
                spawnPos -= DirectionTowardsPlayer();

                Vector3 offset = Vector3.zero;
                if (shootType == ShootType.Triple)
                {
                    if (i == 1)
                        offset.y = 5;
                    else if (i == 2)
                        offset.y = -5;
                }

                moveDir = DirectionTowardsPlayer(offset);
            }

            GameObject p = Instantiate(projectilePrefab, spawnPos, Quaternion.Euler(spawnRot));
            p.GetComponent<Projectile>().moveDir = moveDir;
        }

        SoundManager.instance.PlaySound("enemyshoot", 0.15f);
    }

    Vector2 DirectionTowardsPlayer(Vector3 offset)
    {
        GameObject p = GameManager.instance.player;

        return ((p.transform.position + offset) - transform.position).normalized;
    }
    Vector2 DirectionTowardsPlayer() { return DirectionTowardsPlayer(Vector2.zero); }

    void Move()
    {
        switch (type)
        {
            case EnemyType.Swoop:
            case EnemyType.Bomber:
                SwoopMove();
                break;
            case EnemyType.Aim:
                TurnTowardsPlayer();
                break;
        }
    }

    void TurnTowardsPlayer()
    {
        Transform target = GameManager.instance.player.transform;

        Vector3 vectorToTarget = target.position - transform.position;
        float angle = (Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg) + 90;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * turnSpeed);
    }

    void SwoopMove()
    {
        if (transform.position.x < -maximumX)
            xDirection = 1;
        else if (transform.position.x > maximumX)
            xDirection = -1;

        if (transform.position.y < -maximumY + yOffset)
            yDirection = 1;
        else if (transform.position.y > maximumY + yOffset)
            yDirection = -1;

        velocity.x += moveSpeed * xDirection * (Time.deltaTime * moveSpeed / 2.5f);
        velocity.y += (moveSpeed * yDirection * (Time.deltaTime * moveSpeed / 2.5f)) * yMultiplier;

        velocity.x = Mathf.Clamp(velocity.x, -moveSpeed, moveSpeed);
        velocity.y = Mathf.Clamp(velocity.y, -moveSpeed * yMultiplier, moveSpeed * yMultiplier);

        transform.Translate(velocity * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            if (!collision.gameObject.GetComponent<Projectile>().playerSpawned)
                return;

            // damage
            currentHealth--;
            collision.gameObject.GetComponent<Projectile>().Despawn();
            SoundManager.instance.PlaySound("enemyhit", 0.65f, Random.Range(0.95f, 1.05f));
            EffectManager.instance.SetTempNoise(2);

            GameManager.instance.player.GetComponent<PlayerController>().AddGamer();

            // death
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    void Die()
    {
        // boss phase switch
        BossEnemy be = GetComponent<BossEnemy>();
        if (be != null)
        {
            StartCoroutine(be.NextPhase());
            currentHealth = health;
            return;
        }

        SoundManager.instance.PlaySound("enemydie", 0.5f, Random.Range(0.95f, 1.05f));

        // health pickup
        if (Random.Range(0f, 1f) < healthDropChance)
        {
            Instantiate(healthPickup, transform.position, Quaternion.identity);
        }

        // bomber projectiles
        if (type == EnemyType.Bomber)
        {
            SpawnDeathProjectiles();
        }

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        GameManager.instance.EnemyDefeated();
        Destroy(gameObject);
    }

    public void SpawnDeathProjectiles()
    {
        for (int p = 0; p < deathProjectileCount; p++)
        {
            Vector3 spawnPos = transform.position;
            float angle = p * (360 / deathProjectileCount);

            Instantiate(projectilePrefab, spawnPos, Quaternion.Euler(0, 0, angle));
        }
    }

    public void ChangeType(EnemyType newType)
    {
        type = newType;
    }
    
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
