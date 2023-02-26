using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    Rigidbody2D rb;
    float aliveTime;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        rb.velocity = Vector2.down * 2;

        aliveTime += Time.deltaTime;
        if (aliveTime > 10)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerController>().Heal(4);
            SoundManager.instance.PlaySound("heal", 0.5f);
            Destroy(gameObject);
        }
    }
}
