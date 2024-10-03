using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private EnemyInfo[] allEnemies;
    [SerializeField] private List<Enemy> currentEnemies;
    [SerializeField] private EnemyInfo defaultEnemy;
    [SerializeField] private List<GameObject> enemyDGPrefabs = new List<GameObject>();
    private List<Transform> dgSpawnPoints = new List<Transform>();
    [SerializeField] private List<EnemyData> savedDGEnemiesData = new List<EnemyData>();


    private static EnemyManager instance;
    [SerializeField] private Vector3 dgEnemyAttacker;
    public bool hasWonBattle;

    private const float LEVEL_MODIFIER = 0.5f;
    private const int MAX_NUM_ENEMIES_TO_SPAWN = 7;

    private void Awake()
    {
        // Ensure only one instance of this script exists
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {// Destruir el objeto si se carga el menú principal o la escena final
        if (scene.name == "MainMenu")
        {
            Destroy(gameObject);  // Destruye este objeto si es la escena deseada
            return;
        }

        if (scene.name == "RedDungeonLVL")
        {
            FindSpawnPoints();

            if (savedDGEnemiesData.Count == 0)
            {
                SpawnEnemies();
            }
            else
            {
                RespawnEnemies();
            }
        }

    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;  // Eliminar el evento cuando se destruya el objeto
    }


    private void FindSpawnPoints()
    {
        dgSpawnPoints.Clear();
        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
        foreach (GameObject spawnPointObject in spawnPointObjects)
        {
            dgSpawnPoints.Add(spawnPointObject.transform);
        }
    }

    public void GenerateEnemybyEncounter(Encounter[] encounters, int maxNumEnemies)
    {
        currentEnemies.Clear();
        int numEnemies = Random.Range(1, maxNumEnemies + 1);
        for (int i = 0; i < numEnemies; i++)
        {
            Encounter tempEncounter = encounters[Random.Range(0, encounters.Length)];
            int level = Random.Range(tempEncounter.LevelMin, tempEncounter.LevelMax + 1);
            GenerateEnemybyName(tempEncounter.Enemy.enemyName, level);
        }
    }

    private void SpawnEnemies()
    {
        int numEnemiesToSpawn = Random.Range(3, MAX_NUM_ENEMIES_TO_SPAWN + 1);
        for (int i = 0; i < numEnemiesToSpawn; i++)
        {
            Vector3 spawnPosition = dgSpawnPoints[Random.Range(0, dgSpawnPoints.Count)].position;
            GameObject enemyPrefab = enemyDGPrefabs[Random.Range(0, enemyDGPrefabs.Count)];
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            savedDGEnemiesData.Add(new EnemyData(enemy.transform.position, enemy.transform.rotation, enemyDGPrefabs.IndexOf(enemyPrefab)));
        }
    }

    private void RespawnEnemies()
    {
        if (hasWonBattle && dgEnemyAttacker != null)
        {
            // for (int i = savedDGEnemiesData.Count - 1; i >= 0; i--)
            // {
            //     if (savedDGEnemiesData[i].position == dgEnemyAttacker.position)
            //     {
            //         savedDGEnemiesData.RemoveAt(i);
            //     }
            // }
        }

        foreach (EnemyData enemyData in savedDGEnemiesData)
        {
            GameObject enemyPrefab = enemyDGPrefabs[enemyData.prefabIndex];
            GameObject respawnedEnemy = Instantiate(enemyPrefab, enemyData.position, enemyData.rotation);
            if (hasWonBattle && dgEnemyAttacker != null)
            {
                if (respawnedEnemy.transform.position == dgEnemyAttacker)
                {
                    respawnedEnemy.GetComponent<EnemySimpleAI>().TriggerDeath();
                }
            }
            //savedDGEnemiesData.Add(new EnemyData(respawnedEnemy.transform.position, respawnedEnemy.transform.rotation, enemyData.prefabIndex));
        }
    }


    public void SaveDGEnemiesData(Transform enemyAttackerTransform)
    {
        savedDGEnemiesData.Clear();
        dgEnemyAttacker = enemyAttackerTransform.position;
        GameObject[] tempEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject tempEnemy in tempEnemies)
        {
            int prefabIndex = tempEnemy.GetComponent<EnemySimpleAI>().prefabIndex;
            savedDGEnemiesData.Add(new EnemyData(tempEnemy.transform.position, tempEnemy.transform.rotation, prefabIndex));
        }
    }


    public void GenerateEnemybyName(string enemyName, int level)
    {
        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i].enemyName == enemyName)
            {
                Enemy newEnemy = new Enemy();
                newEnemy.enemyName = allEnemies[i].enemyName;
                newEnemy.level = level;
                float levelModifier = (LEVEL_MODIFIER * newEnemy.level);
                newEnemy.maxHealth = Mathf.RoundToInt(allEnemies[i].baseHealth + (allEnemies[i].baseHealth * levelModifier));
                newEnemy.currentHealth = newEnemy.maxHealth;
                newEnemy.strength = Mathf.RoundToInt(allEnemies[i].baseStr + (allEnemies[i].baseStr * levelModifier));
                newEnemy.initiative = Mathf.RoundToInt(allEnemies[i].baseInitiative + (allEnemies[i].baseInitiative * levelModifier));
                newEnemy.enemyBattleVisuals = allEnemies[i].enemyBattleVisualPrefab;
                newEnemy.enemyDungeonVisuals = allEnemies[i].enemyBattleVisualPrefab;

                currentEnemies.Add(newEnemy);
            }
        }
    }

    public List<Enemy> GetCurrentEnemies()
    {
        return currentEnemies;
    }

}

[System.Serializable]
public class Enemy
{
    public string enemyName;
    public int level;
    public int currentHealth;
    public int maxHealth;
    public int strength;
    public int initiative;
    public GameObject enemyBattleVisuals;
    public GameObject enemyDungeonVisuals;
}

[System.Serializable]
public class EnemyData
{
    public Vector3 position;
    public Quaternion rotation;
    public int prefabIndex;

    public EnemyData(Vector3 pos, Quaternion rot, int index)
    {
        position = pos;
        rotation = rot;
        prefabIndex = index;
    }
}