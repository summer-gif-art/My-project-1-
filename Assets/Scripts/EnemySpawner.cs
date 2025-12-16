using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> enemies = new();
    [SerializeField] private int firstDelay = 2;
    [SerializeField] private int burstRate = 5;
    [SerializeField] private int enemiesRate = 1;
    [SerializeField] private int enemiesPerBurst = 5;

    [SerializeField] private List<Transform> SpawnPoints = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        StartCoroutine(SpawnBursts());

    }

    IEnumerator SpawnBursts()
    {
        yield return StartCoroutine(SpawnBurst());

        yield return StartCoroutine(DelayBetweenBurst());

        yield return StartCoroutine(SpawnBurst());
    }

    IEnumerator DelayBetweenBurst()
    {
        yield return new WaitForSeconds(burstRate);
    }

    IEnumerator SpawnBurst()
    {
        for (int i = 0; i < enemiesPerBurst; i++)
        {
            Instantiate(enemies[Random.Range(0, enemies.Count)], SpawnPoints[Random.Range(0, SpawnPoints.Count)].position, Quaternion.identity);
            yield return new WaitForSeconds(enemiesRate);
        }
    }

}
