using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    public Wave[] waves;
    public float ySpawnPos;

    public int wave;

    public TextMeshProUGUI waveText;

    void Update()
    {
        if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                DebugWave(0);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                DebugWave(1);
            if (Input.GetKeyDown(KeyCode.Alpha3))
                DebugWave(2);
            if (Input.GetKeyDown(KeyCode.Alpha4))
                DebugWave(3);
            if (Input.GetKeyDown(KeyCode.Alpha5))
                DebugWave(4);
            if (Input.GetKeyDown(KeyCode.Alpha6))
                DebugWave(5);
            if (Input.GetKeyDown(KeyCode.Alpha7))
                DebugWave(6);
            if (Input.GetKeyDown(KeyCode.Alpha8))
                DebugWave(7);
            if (Input.GetKeyDown(KeyCode.Alpha9))
                DebugWave(8);
            if (Input.GetKeyDown(KeyCode.Alpha0))
                DebugWave(9);
        }
    }

    void DebugWave(int index)
    {
        StopCoroutine(nameof(SpawnWave));

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
            Destroy(enemy);

        GameManager.instance.enemiesRemaining = 0;

        wave = index;
        SpawnNextWave();
    }

    public void SpawnNextWave()
    {
        if (wave > waves.Length - 1)
            return;

        StartCoroutine(SpawnWave(wave));

        print($"wave {wave}");
        GameManager.instance.player.GetComponent<PlayerController>().FullHeal();

        SoundManager.instance.PlaySound("alarm3", 0.1f);

        wave++;

        waveText.text = $"WAVE {wave}";

        Invoke(nameof(CloseWaveText), 2);
    }

    void CloseWaveText()
    {
        waveText.text = "";
    }

    public IEnumerator SpawnWave(int index)
    {
        Wave wave = waves[index];

        EnemySpawn[] enemies = wave.enemies;

        // dialogue
        if (wave.dialogue != string.Empty)
        {
            GameObject.Find("QueenPanel").GetComponent<Animator>().SetTrigger("Show");
            GameObject.Find("Dialogue").GetComponent<TextMeshProUGUI>().text = wave.dialogue;
        }

        foreach (EnemySpawn enemy in enemies)
        {
            // spawn count
            int count = enemy.spawnCount;
            if (count <= 0)
                count = 1;

            for (int i = 0; i < count; i++)
            {
                if (enemy.prefab != null)
                    GameManager.instance.enemiesRemaining++;
            }
        }

        yield return new WaitForSeconds(3);

        foreach (EnemySpawn enemy in enemies)
        {
            // spawn time
            if (enemy.timeType == EnemySpawn.SpawnTime.AllAtOne || enemy.spawnCount < 2)
                yield return new WaitForSeconds(enemy.spawnTime);

            // spawn count
            int count = enemy.spawnCount;
            if (count <= 0)
                count = 1;

            for (int i = 0; i < count; i++)
            {
                if (enemy.timeType == EnemySpawn.SpawnTime.OneByOne)
                    yield return new WaitForSeconds(enemy.spawnTime);

                // spawn pos
                Vector2 spawnPos = new Vector2(0, ySpawnPos);

                if (enemy.spawnType == EnemySpawn.SpawnPosition.Manual)
                {
                    spawnPos.x = enemy.spawnPos;
                }
                else if (enemy.spawnType == EnemySpawn.SpawnPosition.Random)
                {
                    float maxX = enemy.prefab.GetComponent<Enemy>().maximumX;
                    spawnPos.x = UnityEngine.Random.Range(-maxX, maxX);
                }

                // spawn
                if (enemy.prefab != null)
                    Instantiate(enemy.prefab, spawnPos, Quaternion.identity);
            }
        }
    }

    [Serializable]
    public class Wave
    {
        public EnemySpawn[] enemies;
        [TextArea]
        public string dialogue;
    }

    [Serializable]
    public class EnemySpawn
    {
        public enum SpawnPosition { Manual, Random }
        public enum SpawnTime { AllAtOne, OneByOne }

        [Header("Enemy")]
        public GameObject prefab;
        [Header("Position")]
        public SpawnPosition spawnType;
        public float spawnPos;
        [Header("Time")]
        public float spawnTime;
        public SpawnTime timeType;
        [Header("Count")]
        public int spawnCount;
    }
}
