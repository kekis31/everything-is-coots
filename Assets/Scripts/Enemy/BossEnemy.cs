using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossEnemy : MonoBehaviour
{
    public enum Mode { Swoop, Aim, Dash }

    [Header("Mode switch")]
    public float modeSwitchRate;

    [Header("Bullet hell")]
    public float sprayRate;

    [Header("Laser beam")]
    public float seekTime;
    public float seekSpeed;
    public float topSpeed;
    private bool seekingPlayer;
    public LayerMask player;

    [Header("Allies")]
    public GameObject allyPrefab;

    private Enemy e;
    private Mode mode;
    private int phase;

    private GameObject bossPanel;
    void Start()
    {
        e = GetComponent<Enemy>();

        InvokeRepeating(nameof(SwitchMode), modeSwitchRate, modeSwitchRate);

        SoundManager.instance.StopAllSounds();
        SoundManager.instance.PlaySound("bossspawn");
        SoundManager.instance.PlaySound("miukumaukuboss", 0.05f, 1, true, SoundManager.SoundType.Music);

        SoundManager.instance.PlaySound("miukumauku_boss_high", 0.075f, 1, true, SoundManager.SoundType.Music);
        SoundManager.instance.ChangeVolume("miukumauku_boss_high", 0);

        bossPanel = GameObject.Find("BPanel");
        bossPanel.transform.localScale = Vector3.one;
    }

    // Update is called once per frame
    void Update()
    {
        if (seekingPlayer)
        {
            SeekPlayer();
        }

        Image i = bossPanel.transform.GetChild(0).GetComponent<Image>();
        i.fillAmount = Mathf.Lerp(i.fillAmount, (float)e.GetCurrentHealth() / e.health, Time.deltaTime * 2);
    }

    void SeekPlayer()
    {
        Vector2 seekPos = new(GameManager.instance.player.transform.position.x, transform.position.y);

        Vector3 targetPos = Vector3.Lerp(transform.position, seekPos, seekSpeed * Time.deltaTime);
        targetPos.x = Mathf.Clamp(targetPos.x, transform.position.x - topSpeed, transform.position.x + topSpeed);

        transform.position = targetPos;
    }

    void TurnTowardsPlayer()
    {
        Transform target = GameManager.instance.player.transform;

        Vector3 vectorToTarget = target.position - transform.position;
        float angle = (Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg) + 90;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * 75f);
    }

    IEnumerator ActivateLazer()
    {
        e.CancelInvoke("Shoot");

        seekingPlayer = true;

        SoundManager.instance.PlaySound("seek", 0.8f, seekTime / 2);

        yield return new WaitForSeconds(seekTime);

        seekingPlayer = false;
        transform.Find("LazerBeam").GetComponent<Animator>().SetTrigger("Beam");
        SoundManager.instance.PlaySound("laserbeam3", 0.8f);

        yield return new WaitForSeconds(0.5f);

        RaycastHit2D ray = Physics2D.BoxCast(transform.position, Vector2.one * 2, 0, Vector2.down, 15, player);
        if (ray.collider != null)
        {
            ray.collider.GetComponent<PlayerController>().Die();
        }

        yield return new WaitForSeconds(1.5f);

        e.enabled = true;
        e.ChangeType(Enemy.EnemyType.Swoop);
        mode = Mode.Swoop;
        InvokeRepeating(nameof(SwitchMode), modeSwitchRate, modeSwitchRate);
        e.InvokeRepeating("Shoot", 1, e.shootRate);
    }

    void SwitchMode()
    {
        switch (mode)
        {
            case Mode.Swoop:
                e.ChangeType(Enemy.EnemyType.Aim);
                mode = Mode.Aim;
                break;
            case Mode.Aim:
                e.enabled = false;
                mode = Mode.Dash;
                StartCoroutine(ActivateLazer());
                CancelInvoke(nameof(SwitchMode));
                break;
        }
    }

    void SpawnAlly()
    {
        float spawnPos = Random.Range(-9, 9);

        Instantiate(allyPrefab, new Vector3(spawnPos, 2, 0), Quaternion.identity);
    }

    public IEnumerator NextPhase()
    {
        CancelInvoke(nameof(SwitchMode));
        e.enabled = false;
        e.CancelInvoke("Shoot");
        CancelInvoke(nameof(SpawnAlly));
        transform.rotation = Quaternion.Euler(0, 0, 0);

        print($"phase {phase}");

        SoundManager.instance.PlaySound("bomb", 0.6f);

        yield return new WaitForSeconds(1);

        GetComponent<Animator>().SetTrigger("PhaseSwitch");
        SoundManager.instance.PlaySound("bossphase2");

        yield return new WaitForSeconds(1);

        switch (phase)
        {
            case 0:
                e.shootType = Enemy.ShootType.Triple;
                break;
            case 1:
                e.moveSpeed *= 1.5f;
                seekSpeed *= 1.5f;
                SpawnAlly();
                SpawnAlly();
                InvokeRepeating(nameof(SpawnAlly), 5, 5);
                break;
            case 2:
                e.shootRate /= 1.5f;
                SpawnAlly();
                SpawnAlly();
                SpawnAlly();
                InvokeRepeating(nameof(SpawnAlly), 4.5f, 4.5f);
                break;
            case 4:
                e.InvokeRepeating("SpawnDeathProjectiles", 3, sprayRate);
                break;
            case 5:
                SoundManager.instance.StopAllSounds();
                SoundManager.instance.PlaySound("bossdie", 1.5f);
                GetComponent<Animator>().SetTrigger("Die");
                GameManager.instance.Invoke("Exit", 5);
                e.CancelInvoke("SpawnDeathProjectiles");
                yield return null;
                break;
        }

        e.enabled = true;
        mode = Mode.Swoop;
        e.ChangeType(Enemy.EnemyType.Swoop);
        InvokeRepeating(nameof(SwitchMode), modeSwitchRate, modeSwitchRate);
        e.InvokeRepeating("Shoot", 0, e.shootRate);

        phase++;
    }
}
