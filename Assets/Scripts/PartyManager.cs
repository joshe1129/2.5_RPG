using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PartyManager : MonoBehaviour
{
    [SerializeField] private PartyMemberInfo[] allMembers;
    [SerializeField] private List<PartyMember> currentParty;
    [SerializeField] private PartyMemberInfo defaultPartyMember;

    private Vector3 playerPosition;
    public Vector3 playerStartPosition = new Vector3(40f, 0f, 14.5f);

    private static PartyManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        AddMembertoPartyByName(defaultPartyMember.memberName);
        SceneManager.sceneLoaded += OnSceneLoaded; // Escuchar el cambio de escenas

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Destruir el objeto si se carga el menú principal o la escena final
        if (scene.name == "MainMenu")
        {
            Destroy(gameObject);  // Destruye este objeto si es la escena deseada
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;  // Eliminar el evento cuando se destruya el objeto
    }

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


    public List<PartyMember> GetCurrentParty()
    {
        return currentParty;
    }


    public void SaveHealth(int partyMember, int health)
    {
        currentParty[partyMember].currentHealth = health;
    }

    public void SetPosition(Vector3 position)
    {
        playerPosition = position;
    }

    public Vector3 GetPosition()
    {
        return playerPosition;
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