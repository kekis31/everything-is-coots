using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [HideInInspector]
    public GameObject player;
    private PlayerController pc;
    private float gamingTime;

    [HideInInspector]
    public int enemiesRemaining;

    private Image healthPanel;
    private Image gamerPanel;

    public static int savedWave;

    private GameObject deathPanel;
    private EnemySpawner spawner;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else 
            Destroy(gameObject);
        
        player = GameObject.FindGameObjectWithTag("Player");
        pc = player.GetComponent<PlayerController>();
    }

    void Start()
    {
        if (savedWave != 9)
        {
            SoundManager.instance.PlaySound("miukumaukugameplay_intro", 0.05f, 1, false, SoundManager.SoundType.Music);
            Invoke(nameof(PlayGameplayThemeLoop), 11.75f);
        }

        spawner = GameObject.Find("EnemySpawner").GetComponent<EnemySpawner>();

        healthPanel = GameObject.Find("Health").GetComponent<Image>();
        gamerPanel = GameObject.Find("Gamer").GetComponent<Image>();

        spawner.wave = savedWave;
        spawner.Invoke("SpawnNextWave", 3);

        deathPanel = GameObject.Find("DeathPanel");
        deathPanel.SetActive(false);
    }

    void Update()
    {
        healthPanel.fillAmount = Mathf.Lerp(healthPanel.fillAmount, 
            ((float)pc.health / pc.maxHealth), 
            Time.deltaTime * 2);

        gamerPanel.fillAmount = Mathf.Lerp(gamerPanel.fillAmount,
            pc.gamerAmount / 100,
            Time.deltaTime * 2);

        string normaltheme = "miukumaukugameplay";
        string hightheme = "miukumaukugameplay_high";

        if (spawner.wave >= 9)
        {
            normaltheme = "miukumaukuboss";
            hightheme = "miukumauku_boss_high";
        }



        if (pc.gaming)
        {
            gamingTime += Time.deltaTime * 0.05f;

            SoundManager.instance.ChangeVolume(hightheme, 1);
            SoundManager.instance.ChangeVolume(normaltheme, 0);
            EffectManager.instance.SetTempNoise(5);
        }
        else
        {
            gamingTime -= Time.deltaTime / 2;

            SoundManager.instance.ChangeVolume(hightheme, 0);
            SoundManager.instance.ChangeVolume(normaltheme, 1);
        }

        gamingTime = Mathf.Clamp(gamingTime, 0, 0.5f);

        EffectManager.instance.SetLensDistortion(gamingTime);
    }

    void PlayGameplayThemeLoop()
    {
        SoundManager.instance.PlaySound("miukumaukugameplay", 0.05f, 1, true, SoundManager.SoundType.Music);
        SoundManager.instance.PlaySound("miukumaukugameplay_high", 0.075f, 1, true, SoundManager.SoundType.Music);
        SoundManager.instance.ChangeVolume("miukumaukugameplay_high", 0);
    }

    public void EnemyDefeated()
    {
        enemiesRemaining--;

        if (enemiesRemaining == 0)
            GameObject.Find("EnemySpawner").GetComponent<EnemySpawner>().SpawnNextWave();
    }

    public void Restart()
    {
        SoundManager.instance.StopAllSounds();
        savedWave = 0;

        SceneManager.LoadScene("SampleScene");
    }

    public void RetryWave()
    {
        SoundManager.instance.StopAllSounds();
        int w = GameObject.Find("EnemySpawner").GetComponent<EnemySpawner>().wave;
        if (w > 9)
            w = 9;

        savedWave = w;

        SceneManager.LoadScene("SampleScene");
    }

    public void Exit()
    {
        SoundManager.instance.StopAllSounds();
        savedWave = 0;

        SceneManager.LoadScene("Title");
    }

    public void OnPlayerDeath()
    {
        deathPanel.SetActive(true);

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (var enemy in enemies)
        {
            Destroy(enemy);
        }

        Destroy(spawner);
    }
}
