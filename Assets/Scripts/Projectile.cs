using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Vector2 moveDir;
    public float moveSpeed;
    public bool playerSpawned;

    private float aliveTime;

    private SpriteRenderer sr;

    [SerializeField] GameObject deathEffect;

    private void Start()
    {
        sr = transform.GetChild(0).GetComponent<SpriteRenderer>();

        if (playerSpawned)
            RotateTowardsMouse();
    }

    void RotateTowardsMouse()
    {
        var mouse = Input.mousePosition;
        var screenPoint = Camera.main.WorldToScreenPoint(transform.localPosition);
        var offset = new Vector2(mouse.x - screenPoint.x, mouse.y - screenPoint.y);
        var angle = (Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg) - 90;

        sr.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        transform.Translate(moveSpeed * moveDir * Time.deltaTime);

        if (!sr.isVisible && aliveTime > 1)
        {
            Destroy(gameObject);
        }

        aliveTime += Time.deltaTime;
    }

    public void Despawn()
    {
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
