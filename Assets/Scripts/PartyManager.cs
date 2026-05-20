using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RPGInterfaces;
using RPG.Core;

/// <summary>
/// Singleton manager that handles party member data and persistence across scenes.
/// Tracks current party composition, saves/loads character stats, and manages player position between scenes.
/// Destroyed when returning to main menu.
/// </summary>
public class PartyManager : MonoBehaviour, IPartyManager
{
    [SerializeField] private PartyMemberInfo[] allMembers;
    [SerializeField] private List<PartyMember> currentParty;
    [SerializeField] private PartyMemberInfo defaultPartyMember;
    [SerializeField] public Vector3 playerStartPosition = new Vector3(0f, 0f, 0f);

    private static PartyManager instance;
    private Vector3 playerPosition;

    /// <summary>
    /// Initializes the PartyManager singleton, sets up the default party member, and registers for scene load events.
    /// </summary>
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        ServiceLocator.RegisterService<IPartyManager>(this);
        AddMembertoPartyByName(defaultPartyMember.memberName);
        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    /// <summary>
    /// Handles scene load events and destroys the PartyManager if the main menu is loaded.
    /// </summary>
    /// <param name="scene">The scene that was loaded.</param>
    /// <param name="mode">The scene load mode.</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GameConstants.SCENE_MAIN_MENU)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Unregisters the scene load event when this manager is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this)
        {
            ServiceLocator.UnregisterService<IPartyManager>();
        }
    }

    /// <summary>
    /// Adds a new party member to the active party by searching for their data by name.
    /// Initializes the party member with default stats from their ScriptableObject definition.
    /// </summary>
    /// <param name="memberName">The name of the party member to add.</param>
    public void AddMembertoPartyByName(string memberName)
    {
        for (int i = 0; i < allMembers.Length; i++)
        {
            if (allMembers[i].memberName == memberName)
            {
                PartyMember newPartyMember = new PartyMember();
                newPartyMember.memberName = allMembers[i].memberName;
                newPartyMember.level = allMembers[i].startingLevel;
                newPartyMember.currentHealth = allMembers[i].baseHealth;
                newPartyMember.maxHealth = newPartyMember.currentHealth;
                newPartyMember.strength = allMembers[i].baseStr;
                newPartyMember.initiative = allMembers[i].baseInitiative;
                newPartyMember.sprite = allMembers[i].memberHUDSprite;
                newPartyMember.memberBattleVisualPrefab = allMembers[i].memberBattleVisualPrefab;
                newPartyMember.memberOverworldVisualPrefab = allMembers[i].memberOverworldVisualPrefab;
                currentParty.Add(newPartyMember);
            }
        }
    }

    /// <summary>
    /// Returns a list of currently alive party members (with health > 0).
    /// Used by the battle system to determine valid participants in combat.
    /// </summary>
    /// <returns>A list of party members with positive health.</returns>
    public List<PartyMember> GetAliveParty()
    {
        List<PartyMember> aliveParty = new List<PartyMember>();
        aliveParty = currentParty;
        for (int i = 0; i < aliveParty.Count; i++)
        {
            if (aliveParty[i].currentHealth <= 0)
            {
                aliveParty.RemoveAt(i);
            }
        }
        return aliveParty;
    }

    /// <summary>
    /// Returns the current list of active party members.
    /// </summary>
    /// <returns>The list of party members currently in the party.</returns>
    public List<PartyMember> GetCurrentParty()
    {
        return currentParty;
    }

    /// <summary>
    /// Updates the health value of a specific party member after a battle.
    /// </summary>
    /// <param name="partyMember">The index of the party member to update.</param>
    /// <param name="health">The new health value to set.</param>
    public void SaveHealth(int partyMember, int health)
    {
        currentParty[partyMember].currentHealth = health;
    }

    /// <summary>
    /// Sets the player's position, typically saved when transitioning to a battle.
    /// </summary>
    /// <param name="position">The position to save.</param>
    public void SetPosition(Vector3 position)
    {
        playerPosition = position;
    }

    /// <summary>
    /// Retrieves the previously saved player position.
    /// Used to restore the player's location after exiting a battle.
    /// </summary>
    /// <returns>The saved player position.</returns>
    public Vector3 GetPosition()
    {
        return playerPosition;
    }

    /// <summary>
    /// Retrieves the default starting position for the player.
    /// </summary>
    /// <returns>The default start position.</returns>
    public Vector3 GetPlayerStartPosition()
    {
        return playerStartPosition;
    }
}

[System.Serializable]
public class PartyMember
{
    public string memberName;
    public int level;
    public int currentHealth;
    public int maxHealth;
    public int strength;
    public int initiative;
    public int currExp;
    public int maxExp;
    public Sprite sprite;
    public GameObject memberBattleVisualPrefab;
    public GameObject memberOverworldVisualPrefab;
}