using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public Vector2 velocity;
    public Vector2 maxVelocity;
    public float deacceleration;

    private bool horizontalFlipped;
    private bool verticalFlipped;

    private Vector2 lastInput;
    private Vector2 timeSinceLastInputChange;

    [HideInInspector]
    public int health;
    [Header("Health")]
    public int maxHealth;

    // gamer mode
    [HideInInspector] public float gamerAmount;
    [HideInInspector] public bool gaming;
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        float hori = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        UpdateLastInput();

        if (horizontalFlipped)
            hori = -hori;
        if (verticalFlipped)
            vert = -vert;

        Vector2 moveDir = new Vector2(hori, vert);

        GetComponent<Animator>().SetFloat("Move", (hori + 1) / 2);

        velocity += moveDir * moveSpeed;
        
        velocity.x = Mathf.Clamp(velocity.x, -maxVelocity.x, maxVelocity.x);
        velocity.y = Mathf.Clamp(velocity.y, -maxVelocity.y, maxVelocity.y);

        if (hori == 0)
        {
            if (velocity.x < 0)
                velocity.x += deacceleration * Time.deltaTime;
            else if (velocity.x > 0)
                velocity.x -= deacceleration * Time.deltaTime;
        }
        if (vert == 0)
        {
            if (velocity.y < 0)
                velocity.y += deacceleration * Time.deltaTime;
            else if (velocity.y > 0)
                velocity.y -= deacceleration * Time.deltaTime;
        }

        transform.Translate(velocity * Time.deltaTime);

        //g amer mode
        if (gamerAmount >= 100 && Input.GetKeyDown(KeyCode.E) && !gaming)
            ActivateGamerMode();

        if (Application.isEditor && Input.GetKeyDown(KeyCode.G))
        {
            gamerAmount = 99;
            AddGamer();
        }
        
        if (Mathf.Abs(transform.position.x) > 20 || Mathf.Abs(transform.position.y) > 10)
        {
            transform.position = new(0, -4, 0);
        }
    }

    void ActivateGamerMode()
    {
        SoundManager.instance.PlaySound("gamer2");

        gaming = true;

        FullHeal();

        GameObject.Find("GamerText").GetComponent<TextMeshProUGUI>().text = "GAMER MODE ACTIVE";

        Invoke(nameof(DeactivateGamerMode), 10);
    }

    void DeactivateGamerMode()
    {
        gaming = false;

        gamerAmount = 0;

        GameObject.Find("GamerText").GetComponent<TextMeshProUGUI>().text = "";
    }

    void UpdateLastInput()
    {
        float hori = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        timeSinceLastInputChange.x += Time.deltaTime;
        timeSinceLastInputChange.y += Time.deltaTime;

        if (hori != lastInput.x)
        {
            horizontalFlipped = false;
            timeSinceLastInputChange.x = 0;
        }
        if (vert != lastInput.y)
        {
            verticalFlipped = false;
            timeSinceLastInputChange.y = 0;
        }

        lastInput = new(hori, vert);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // yandere dev moment
        if (collision.gameObject.CompareTag("WallH"))
        {
            if (timeSinceLastInputChange.x < 0.15f)
                return;

            velocity.x = -velocity.x;
            horizontalFlipped = !horizontalFlipped;
        }
        else if (collision.gameObject.CompareTag("WallV"))
        {
            if (timeSinceLastInputChange.y < 0.15f)
                return;

            velocity.y = -velocity.y;
            verticalFlipped = !verticalFlipped;
        }
        else if (collision.gameObject.CompareTag("Projectile"))
        {
            if (collision.gameObject.GetComponent<Projectile>().playerSpawned == true)
                return;

            // damage
            health--;
            collision.gameObject.GetComponent<Projectile>().Despawn();
            SoundManager.instance.PlaySound("playerhit", 0.6f);
            EffectManager.instance.SetTempNoise(3);

            RemoveGamer(3);

            // death
            if (health < 0)
            {
                Die();
            }
        }
    }

    public void Die()
    {
        health = 0;

        GameManager.instance.OnPlayerDeath();

        SoundManager.instance.StopAllSounds();
        SoundManager.instance.PlaySound("playerdie", 1);

        Destroy(gameObject);
    }

    public void FullHeal()
    {
        health = maxHealth;
    }

    public void AddGamer()
    {
        if (gamerAmount >= 100)
            return;

        gamerAmount++;

        if (gamerAmount >= 100)
        {
            gamerAmount = 100;
            GameObject.Find("GamerText").GetComponent<TextMeshProUGUI>().text = "GAMER MODE READY\n[E] TO ACTIVATE";
            SoundManager.instance.PlaySound("gamer");
        }
    }

    public void RemoveGamer(float value)
    {
        if (gamerAmount >= 100)
            return;

        gamerAmount -= value;

        if (gamerAmount < 0)
            gamerAmount = 0;
    }

    public void Heal(float amount)
    {
        health++;

        if (health > maxHealth)
            health = maxHealth;
    }
}
