using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public float autoshootCooldown;

    public GameObject projectilePrefab;

    [Header("Aim")]
    public float maxShootAngle = 30;
    public float shootAngleOffset = -90;

    private float cooldown;
    private int fireCount;

    Transform head;

    private void Start()
    {
        head = transform.Find("Head");
    }
    void Update()
    {
        Aim();

        // fire
        if (Input.GetButton("Fire1") && cooldown <= 0)
        {
            Shoot();
        }

        cooldown -= Time.deltaTime;
    }

    void Aim()
    {
        var mouse = Input.mousePosition;
        var screenPoint = Camera.main.WorldToScreenPoint(transform.localPosition);
        var offset = new Vector2(mouse.x - screenPoint.x, mouse.y - screenPoint.y);
        var angle = (Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg) + shootAngleOffset;

        if (Mathf.Abs(angle) > maxShootAngle)
            return;

        angle = Mathf.Clamp(angle, -maxShootAngle, maxShootAngle);

        head.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Shoot()
    {
        float c = autoshootCooldown;
        if (GetComponent<PlayerController>().gaming)
            c /= 2;

        cooldown = c;

        fireCount++;

        GameObject projectile = Instantiate(projectilePrefab, transform.position + head.up, Quaternion.identity);

        Vector2 dir = ((head.up + head.position) - head.position).normalized;

        projectile.GetComponent<Projectile>().moveDir = dir;

        SoundManager.instance.PlaySound("playershoot3", 0.6f, Random.Range(0.98f, 1.02f), false, SoundManager.SoundType.SFX);
    }
}
