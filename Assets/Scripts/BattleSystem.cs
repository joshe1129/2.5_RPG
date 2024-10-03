using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] private enum BattleState { Start, Selection, Battle, Won, Lost, Run }

    [Header("Battle State")]
    [SerializeField] private BattleState State;

    [Header("SpawnPoints")]
    [SerializeField] private Transform[] partySpawnPoints;
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("Battlers")]
    [SerializeField] private List<BattleEntities> allBattlers = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> playerBattlers = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> enemyBattlers = new List<BattleEntities>();

    [Header("UI")]
    [SerializeField] private GameObject battleMenu;
    [SerializeField] private GameObject GameOverPanel;
    [SerializeField] private GameObject enemySelectionMenu;
    [SerializeField] private GameObject[] enemySelectionButtons;
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private GameObject bottomBattleInfo;
    [SerializeField] private TextMeshProUGUI bottomText;

    private PartyManager partyManager;
    private EnemyManager enemyManager;
    private int currentPartyHero;

    private const string ACTION_MESSAGE = "'s Action: ";
    private const string WIN_MESSAGE = "YOUR PARTY WON THE BATTLE!!!";
    private const string LOST_MESSAGE = "YOUR PARTY HAS BEEN DEFEATED!!!";
    private const string SUCCESSFULLY_RAN_AWAY_MESSAGE = "YOU HAVE RUN AWAY";
    private const string FAIL_RAN_AWAY_MESSAGE = "PARTY FAIL TO RUN";
    private const string SCENE_NAME = "RedDungeonLVL";
    private const int TURN_DURATION = 2;
    private const int RUN_CHANCE = 50;

    void Start()
    {
        partyManager = GameObject.FindFirstObjectByType<PartyManager>();
        enemyManager = GameObject.FindFirstObjectByType<EnemyManager>();

        CreatePartyEntities();
        CreateEnemyEntities();
        ShowBattleMenu();
        DetermineBattleOrder();
    }

    private void CreatePartyEntities()
    {
        List<PartyMember> currentParty = new List<PartyMember>();
        currentParty = partyManager.GetAliveParty();

        for (int i = 0; i < currentParty.Count; i++)
        {
            BattleEntities tempEntity = new BattleEntities();
            tempEntity.SetEntityValues(currentParty[i].memberName, currentParty[i].currentHealth, currentParty[i].maxHealth, currentParty[i].strength, currentParty[i].initiative, currentParty[i].level, true);

            BattleVisuals tempBattleVisual = Instantiate(currentParty[i].memberBattleVisualPrefab, partySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();
            tempBattleVisual.SetStartingValues(currentParty[i].currentHealth, currentParty[i].maxHealth, currentParty[i].level);
            tempEntity.BattleVisuals = tempBattleVisual;

            allBattlers.Add(tempEntity);
            playerBattlers.Add(tempEntity);
        }
    }

    private IEnumerator BattleRoutine()
    {
        enemySelectionMenu.SetActive(false);
        State = BattleState.Battle;
        bottomBattleInfo.SetActive(true);
        enemyManager.hasWonBattle = false;

        for (int i = 0; i < allBattlers.Count; i++)
        {
            if (State == BattleState.Battle && allBattlers[i].CurrentHealth > 0)
            {
                switch (allBattlers[i].BattleAction)
                {
                    case BattleEntities.Action.Attack:
                        yield return StartCoroutine(AttackRoutine(i));
                        break;
                    case BattleEntities.Action.Run:
                        yield return StartCoroutine(RunRoutine());
                        break;
                    default:
                        Debug.LogError("Error - incorrect battle action");
                        break;
                }

            }
        }

        RemoveDeadBattlers();

        if (State == BattleState.Battle)
        {
            bottomBattleInfo.SetActive(false);
            currentPartyHero = 0;
            ShowBattleMenu();
        }

        yield return null;
    }

    private IEnumerator AttackRoutine(int i)
    {
        if (allBattlers[i].IsPlayer)
        {
            BattleEntities currAttacker = allBattlers[i];
            if (allBattlers[currAttacker.Target].CurrentHealth <= 0)
            {
                currAttacker.SetTarget(GetRandomEnemy());
            }
            BattleEntities currTarget = allBattlers[currAttacker.Target];

            AttackAction(currAttacker, currTarget);
            yield return new WaitForSeconds(TURN_DURATION);

            if (currTarget.CurrentHealth <= 0)
            {
                bottomText.text = string.Format("{0} defeated {1}", currAttacker.Name, currTarget.Name);
                yield return new WaitForSeconds(TURN_DURATION);
                enemyBattlers.Remove(currTarget);
                if (enemyBattlers.Count <= 0)
                {
                    State = BattleState.Won;
                    enemyManager.hasWonBattle = true;
                    bottomText.text = WIN_MESSAGE;
                    SceneManager.LoadScene(SCENE_NAME);
                }
            }
        }
        if (i < allBattlers.Count && allBattlers[i].IsPlayer == false)
        {
            BattleEntities currAttacker = allBattlers[i];
            currAttacker.SetTarget(GetRandomPartyMember());
            BattleEntities currTarget = allBattlers[currAttacker.Target];
            AttackAction(currAttacker, currTarget);
            yield return new WaitForSeconds(TURN_DURATION);
            if (currTarget.CurrentHealth <= 0)
            {
                bottomText.text = string.Format("{0} defeated {1}", currAttacker.Name, currTarget.Name);
                yield return new WaitForSeconds(TURN_DURATION);
                playerBattlers.Remove(currTarget);
                if (playerBattlers.Count <= 0)
                {
                    State = BattleState.Lost;
                    bottomText.text = LOST_MESSAGE;
                    yield return new WaitForSeconds(TURN_DURATION);
                    GameOver();
                }
            }
        }
    }

    private void GameOver()
    {        
        bottomBattleInfo.SetActive(false);
        GameOverPanel.SetActive(true);
    }

    private IEnumerator RunRoutine()
    {
        if (State == BattleState.Battle)
        {
            if (Random.Range(1, 101) >= RUN_CHANCE)
            {
                partyManager.SetPosition(partyManager.playerStartPosition);
                bottomText.text = SUCCESSFULLY_RAN_AWAY_MESSAGE;
                State = BattleState.Run;
                allBattlers.Clear();
                yield return new WaitForSeconds(TURN_DURATION);
                SceneManager.LoadScene(SCENE_NAME);
                yield break;

            }
            else
            {
                bottomText.text = FAIL_RAN_AWAY_MESSAGE;
                yield return new WaitForSeconds(TURN_DURATION);
            }
        }
    }

    private void RemoveDeadBattlers()
    {
        for (int i = 0; i < allBattlers.Count; i++)
        {
            if (allBattlers[i].CurrentHealth <= 0)
            {
                allBattlers.RemoveAt(i);
            }
        }
    }

    private void CreateEnemyEntities()
    {
        List<Enemy> currentEnemies = new List<Enemy>();
        currentEnemies = enemyManager.GetCurrentEnemies();

        for (int i = 0; i < currentEnemies.Count; i++)
        {
            BattleEntities tempEntity = new BattleEntities();
            tempEntity.SetEntityValues(currentEnemies[i].enemyName, currentEnemies[i].currentHealth, currentEnemies[i].maxHealth, currentEnemies[i].strength, currentEnemies[i].initiative, currentEnemies[i].level, false);

            BattleVisuals tempBattleVisual = Instantiate(currentEnemies[i].enemyBattleVisuals, enemySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();
            tempBattleVisual.SetStartingValues(currentEnemies[i].maxHealth, currentEnemies[i].maxHealth, currentEnemies[i].level);
            tempEntity.BattleVisuals = tempBattleVisual;

            allBattlers.Add(tempEntity);
            enemyBattlers.Add(tempEntity);
        }
    }

    public void ShowBattleMenu()
    {
        actionText.text = playerBattlers[currentPartyHero].Name + ACTION_MESSAGE;
        battleMenu.SetActive(true);
    }

    public void ShowEnemySelectionMenu()
    {
        battleMenu.SetActive(false);
        SetEnemySelectionButtons();
        enemySelectionMenu.SetActive(true);
    }

    private void SetEnemySelectionButtons()
    {
        for (int i = 0; i < enemySelectionButtons.Length; i++)
        {
            enemySelectionButtons[i].SetActive(false);
        }

        for (int j = 0; j < enemyBattlers.Count; j++)
        {
            enemySelectionButtons[j].SetActive(true);
            enemySelectionButtons[j].GetComponentInChildren<TextMeshProUGUI>().text = enemyBattlers[j].Name;
        }
    }

    public void SelectEnemy(int currentEnemy)
    {
        BattleEntities currentPlayerEntity = playerBattlers[currentPartyHero];
        currentPlayerEntity.SetTarget(allBattlers.IndexOf(enemyBattlers[currentEnemy]));
        currentPlayerEntity.BattleAction = BattleEntities.Action.Attack;
        currentPartyHero++;

        if (currentPartyHero >= playerBattlers.Count)
        {
            StartCoroutine(BattleRoutine());
        }
        else
        {
            enemySelectionMenu.SetActive(false);
            ShowBattleMenu();
        }
    }

    private void AttackAction(BattleEntities currAttacker, BattleEntities currTarget)
    {
        int damage = currAttacker.Strength;// to do: getDamage()  function to increase damage according to lvl, stats or items
        currAttacker.BattleVisuals.PlayAttackAnimation();
        currTarget.CurrentHealth -= damage;
        currTarget.BattleVisuals.PlayHitAnimation();
        currTarget.UpdateUI();
        bottomText.text = string.Format("{0} Attacks {1} for {2} damage", currAttacker.Name, currTarget.Name, damage);
        SaveHealth();
    }

    private int GetRandomPartyMember()
    {
        List<int> partyMembers = new List<int>();
        for (int i = 0; i < allBattlers.Count; i++)
        {
            if (allBattlers[i].IsPlayer == true && allBattlers[i].CurrentHealth > 0)
            {
                partyMembers.Add(i);
            }
        }

        return partyMembers[Random.Range(0, partyMembers.Count)];
    }

    private int GetRandomEnemy()
    {
        List<int> enemyMembers = new List<int>();
        for (int i = 0; i < allBattlers.Count; i++)
        {
            if (allBattlers[i].IsPlayer == false && allBattlers[i].CurrentHealth > 0)
            {
                enemyMembers.Add(i);
            }
        }

        return enemyMembers[Random.Range(0, enemyMembers.Count)];
    }

    private void SaveHealth()
    {
        for (int i = 0; i < playerBattlers.Count; i++)
        {
            partyManager.SaveHealth(i, playerBattlers[i].CurrentHealth);
        }
    }

    private void DetermineBattleOrder()
    {
        allBattlers.Sort((bi1, bi2) => -bi1.Initiative.CompareTo(bi2.Initiative));
    }

    public void SelectRunAction()
    {
        State = BattleState.Selection;
        BattleEntities currentPlayerEntity = playerBattlers[currentPartyHero];
        currentPlayerEntity.BattleAction = BattleEntities.Action.Run;

        battleMenu.SetActive(false);
        currentPartyHero++;

        if (currentPartyHero >= playerBattlers.Count)
        {
            StartCoroutine(BattleRoutine());
        }
        else
        {
            enemySelectionMenu.SetActive(false);
            ShowBattleMenu();
        }

    }

}

[System.Serializable]
public class BattleEntities
{
    public enum Action { Attack, Run }
    public Action BattleAction;

    public string Name;
    public int Level;
    public int CurrentHealth;
    public int MaxHealth;
    public int Strength;
    public int Initiative;
    public bool IsPlayer;
    public BattleVisuals BattleVisuals;
    public int Target;

    public void SetEntityValues(string name, int currentHealth, int maxHealth, int strength, int initiative, int level, bool isPlayer)
    {
        Name = name;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        Strength = strength;
        Initiative = initiative;
        IsPlayer = isPlayer;
        Level = level;
    }

    public void SetTarget(int target)
    {
        Target = target;
    }

    public void UpdateUI()
    {
        BattleVisuals.ChangeHealth(CurrentHealth);
    }
}