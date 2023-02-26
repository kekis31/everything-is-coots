using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary : MonoBehaviour
{
    public float minDistance;
    public float maxAlpha;
    BoxCollider2D c;
    SpriteRenderer sr;

    private void Start()
    {
        c = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Transform p = GameManager.instance.player.transform;

        float distance = Vector2.Distance(c.ClosestPoint(p.position), p.position);

        distance = 1 - Mathf.Clamp((distance - minDistance) / minDistance, 0, 1);
        
        sr.color = new(0, 1, 1, distance * maxAlpha);
    }
}
