using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RPGInterfaces;
using RPG.Core;

/// <summary>
/// Singleton manager that handles enemy generation, spawning, and persistence in dungeons.
/// Saves enemy state when exiting battles and respawns defeated enemies based on encounter data.
/// Manages enemy data persistence between scene loads.
/// </summary>
public class EnemyManager : MonoBehaviour, IEnemyManager
{
    [SerializeField] private EnemyInfo[] allEnemies;
    [SerializeField] private List<Enemy> currentEnemies;
    [SerializeField] private EnemyInfo defaultEnemy;
    [SerializeField] private List<GameObject> enemyDGPrefabs = new List<GameObject>();
    [SerializeField] private List<Transform> dgSpawnPoints = new List<Transform>();
    [SerializeField] private List<EnemyData> savedDGEnemiesData = new List<EnemyData>();
    [SerializeField] private Vector3 dgEnemyAttacker;


    private static EnemyManager instance;
    public bool hasWonBattle;
    public bool HasWonBattle { get => hasWonBattle; set => hasWonBattle = value; }

    private const float LEVEL_MODIFIER = 0.25f;
    private const int MAX_NUM_ENEMIES_TO_SPAWN = 5;

    /// <summary>
    /// Ensures only one instance of EnemyManager exists using the singleton pattern.
    /// Makes this object persistent across scene loads and registers for scene load events.
    /// </summary>
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        ServiceLocator.RegisterService<IEnemyManager>(this);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// Handles scene load events and spawns or respawns enemies based on the loaded scene.
    /// Destroys the manager if the main menu is loaded.
    /// </summary>
    /// <param name="scene">The scene that was loaded.</param>
    /// <param name="mode">The scene load mode (single or additive).</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GameConstants.SCENE_MAIN_MENU)
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

    /// <summary>
    /// Unregisters the scene load event when this manager is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;  // Eliminar el evento cuando se destruya el objeto
        if (instance == this)
        {
            ServiceLocator.UnregisterService<IEnemyManager>();
        }
    }

    /// <summary>
    /// Finds all spawn points tagged "SpawnPoint" in the current scene and caches their transforms.
    /// </summary>
    private void FindSpawnPoints()
    {
        dgSpawnPoints.Clear();
        if (dgSpawnPoints.Count == 0)
        {
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag(GameConstants.TAG_SPAWN_POINT);
            foreach (GameObject spawnPoint in spawnPoints)
            {
                dgSpawnPoints.Add(spawnPoint.transform);
            }
        }
    }

    /// <summary>
    /// Generates random enemies based on the provided encounter data.
    /// Randomly selects from the available encounters and creates enemies within the specified level range.
    /// </summary>
    /// <param name="encounters">Array of encounter configurations with enemy types and level ranges.</param>
    /// <param name="maxNumEnemies">The maximum number of enemies to generate.</param>
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

    /// <summary>
    /// Spawns random enemies in the dungeon at designated spawn points.
    /// Only spawns one enemy per encounter.
    /// </summary>
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

    /// <summary>
    /// Respawns previously saved enemies at their original positions after a battle.
    /// Removes defeated enemies if the player won the battle.
    /// </summary>
    private void RespawnEnemies()
    {
        List<EnemyData> newSavedData = new List<EnemyData>();

        foreach (EnemyData enemyData in savedDGEnemiesData)
        {
            GameObject enemyPrefab = enemyDGPrefabs[enemyData.prefabIndex];
            GameObject respawnedEnemy = Instantiate(enemyPrefab, enemyData.position, enemyData.rotation);
            if (hasWonBattle && respawnedEnemy.transform.position == dgEnemyAttacker)
            {
                respawnedEnemy.GetComponent<EnemySimpleAI>().TriggerDeath();
            }
            else
            {
                newSavedData.Add(new EnemyData(respawnedEnemy.transform.position, respawnedEnemy.transform.rotation, enemyData.prefabIndex));
            }
        }

        savedDGEnemiesData = newSavedData;

        hasWonBattle = false;
    }

    /// <summary>
    /// Saves the current state of all dungeon enemies before transitioning to a battle.
    /// Records their positions, rotations, and prefab indices for respawning later.
    /// </summary>
    /// <param name="enemyAttackerTransform">The transform of the enemy that initiated the battle.</param>
    public void SaveDGEnemiesData(Transform enemyAttackerTransform)
    {
        savedDGEnemiesData.Clear();
        dgEnemyAttacker = enemyAttackerTransform.position;
        GameObject[] tempEnemies = GameObject.FindGameObjectsWithTag(GameConstants.TAG_ENEMY);
        foreach (GameObject tempEnemy in tempEnemies)
        {
            int prefabIndex = tempEnemy.GetComponent<EnemySimpleAI>().prefabIndex;
            savedDGEnemiesData.Add(new EnemyData(tempEnemy.transform.position, tempEnemy.transform.rotation, prefabIndex));
        }
    }

    /// <summary>
    /// Generates an enemy instance by name and level, calculating stats based on the level modifier.
    /// Adds the generated enemy to the current enemy list.
    /// </summary>
    /// <param name="enemyName">The name of the enemy type to generate.</param>
    /// <param name="level">The level of the enemy to generate.</param>
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

    /// <summary>
    /// Returns the list of currently active enemies in battle.
    /// </summary>
    /// <returns>The list of current enemies.</returns>
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