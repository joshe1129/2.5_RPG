using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGInterfaces;

/// <summary>
/// Controls the turn-based battle system including turn order, action execution, and win/loss conditions.
/// Coordinates between managers, UI, and combat logic.
/// </summary>
public class BattleSystem : MonoBehaviour
{
    private enum BattleState { Start, Selection, Battle, Won, Lost, Run }

    [Header("Battle State")]
    [SerializeField] private BattleState State;

    [Header("SpawnPoints")]
    [SerializeField] private Transform[] partySpawnPoints;
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("UI Reference")]
    [SerializeField] private BattleUIManager uiManager;

    [Header("Battlers")]
    [SerializeField] private List<BattleEntities> allBattlers = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> playerBattlers = new List<BattleEntities>();
    [SerializeField] private List<BattleEntities> enemyBattlers = new List<BattleEntities>();

    private IPartyManager partyManager;
    private IEnemyManager enemyManager;

    private int currentPartyHero;

    private const string WIN_MESSAGE = "Your Party Won The Battle!!!";
    private const string LOST_MESSAGE = "YOUR PARTY HAS BEEN DEFEATED!!!";
    private const string SUCCESSFULLY_RAN_AWAY_MESSAGE = "YOU HAVE RUN AWAY";
    private const string FAIL_RAN_AWAY_MESSAGE = "PARTY FAIL TO RUN";
    private const int TURN_DURATION = 2;
    private const int RUN_CHANCE = 50;

    private void Start()
    {
        partyManager = ServiceLocator.GetService<IPartyManager>();
        enemyManager = ServiceLocator.GetService<IEnemyManager>();

        if (uiManager != null)
        {
            uiManager.OnAttackSelected += HandleAttackSelection;
            uiManager.OnRunSelected += HandleRunSelection;
            uiManager.OnEnemyTargetSelected += HandleEnemySelection;
            uiManager.OnEnemyHoverEntered += HandleEnemyHoverEntered;
            uiManager.OnEnemyHoverExited += HandleEnemyHoverExited;
            uiManager.OnForceAllEnemySpotlightsOff += HandleForceAllEnemySpotlightsOff;
        }
        else
        {
            Debug.LogError("[BattleSystem] BattleUIManager is not assigned!");
        }

        CreatePartyEntities();
        CreateEnemyEntities();
        DetermineBattleOrder();

        currentPartyHero = 0;
        if (uiManager != null && playerBattlers.Count > 0)
        {
            playerBattlers[currentPartyHero].BattleVisuals.ToggleSpotlight(true);
            uiManager.ShowBattleMenu(playerBattlers[currentPartyHero].Name, playerBattlers[currentPartyHero].Portrait);
        }
    }

    private void OnDestroy()
    {
        if (uiManager != null)
        {
            uiManager.OnAttackSelected -= HandleAttackSelection;
            uiManager.OnRunSelected -= HandleRunSelection;
            uiManager.OnEnemyTargetSelected -= HandleEnemySelection;
            uiManager.OnEnemyHoverEntered -= HandleEnemyHoverEntered;
            uiManager.OnEnemyHoverExited -= HandleEnemyHoverExited;
            uiManager.OnForceAllEnemySpotlightsOff -= HandleForceAllEnemySpotlightsOff;
        }
    }

    /// <summary>
    /// Creates BattleEntity instances for all alive party members and instantiates their visual representations.
    /// </summary>
    private void CreatePartyEntities()
    {
        List<PartyMember> currentParty = partyManager.GetAliveParty();

        for (int i = 0; i < currentParty.Count; i++)
        {
            BattleEntities tempEntity = new BattleEntities();
            tempEntity.SetEntityValues(currentParty[i].memberName, currentParty[i].currentHealth, currentParty[i].maxHealth, currentParty[i].strength, currentParty[i].initiative, currentParty[i].level, true, currentParty[i].sprite);

            var pooler = ServiceLocator.GetService<IObjectPooler>();
            BattleVisuals tempBattleVisual;
            if (pooler != null)
            {
                tempBattleVisual = pooler.SpawnFromPool(currentParty[i].memberBattleVisualPrefab, partySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();
            }
            else
            {
                tempBattleVisual = Instantiate(currentParty[i].memberBattleVisualPrefab, partySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();
            }
            tempBattleVisual.SetStartingValues(currentParty[i].currentHealth, currentParty[i].maxHealth, currentParty[i].level);
            tempEntity.BattleVisuals = tempBattleVisual;

            allBattlers.Add(tempEntity);
            playerBattlers.Add(tempEntity);
        }
    }

    /// <summary>
    /// Creates BattleEntity instances for all enemies in the current encounter.
    /// </summary>
    private void CreateEnemyEntities()
    {
        List<Enemy> currentEnemies = enemyManager.GetCurrentEnemies();
        for (int i = 0; i < currentEnemies.Count; i++)
        {
            BattleEntities tempEntity = new BattleEntities();
            tempEntity.SetEntityValues(currentEnemies[i].enemyName, currentEnemies[i].currentHealth, currentEnemies[i].maxHealth, currentEnemies[i].strength, currentEnemies[i].initiative, currentEnemies[i].level, false);

            var pooler = ServiceLocator.GetService<IObjectPooler>();
            BattleVisuals tempBattleVisual;
            if (pooler != null)
            {
                tempBattleVisual = pooler.SpawnFromPool(currentEnemies[i].enemyBattleVisuals, enemySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();
            }
            else
            {
                tempBattleVisual = Instantiate(currentEnemies[i].enemyBattleVisuals, enemySpawnPoints[i].position, Quaternion.identity).GetComponent<BattleVisuals>();
            }
            tempBattleVisual.SetStartingValues(currentEnemies[i].maxHealth, currentEnemies[i].maxHealth, currentEnemies[i].level);
            tempEntity.BattleVisuals = tempBattleVisual;

            allBattlers.Add(tempEntity);
            enemyBattlers.Add(tempEntity);
        }
    }

    /// <summary>
    /// Sorts all battlers by their initiative stat (highest first) to determine turn order.
    /// </summary>
    private void DetermineBattleOrder()
    {
        allBattlers.Sort((bi1, bi2) => -bi1.Initiative.CompareTo(bi2.Initiative));
    }

    // --- User Input Handlers ---

    private void HandleAttackSelection()
    {
        uiManager.ShowEnemySelectionMenu(enemyBattlers);
    }

    private void HandleRunSelection()
    {
        playerBattlers[currentPartyHero].BattleVisuals.ToggleSpotlight(false);
        State = BattleState.Selection;
        BattleEntities currentPlayerEntity = playerBattlers[currentPartyHero];
        currentPlayerEntity.BattleAction = BattleEntities.Action.Run;

        uiManager.HideBattleMenu();
        currentPartyHero++;

        if (currentPartyHero >= playerBattlers.Count)
        {
            StartCoroutine(BattleRoutine());
        }
        else
        {
            playerBattlers[currentPartyHero].BattleVisuals.ToggleSpotlight(true);
            uiManager.ShowBattleMenu(playerBattlers[currentPartyHero].Name, playerBattlers[currentPartyHero].Portrait);
        }
    }

    private void HandleEnemySelection(int currentEnemyIndex)
    {
        playerBattlers[currentPartyHero].BattleVisuals.ToggleSpotlight(false);
        BattleEntities currentPlayerEntity = playerBattlers[currentPartyHero];
        currentPlayerEntity.SetTarget(allBattlers.IndexOf(enemyBattlers[currentEnemyIndex]));
        currentPlayerEntity.BattleAction = BattleEntities.Action.Attack;
        
        currentPartyHero++;

        if (currentPartyHero >= playerBattlers.Count)
        {
            uiManager.HideEnemySelectionMenu();
            StartCoroutine(BattleRoutine());
        }
        else
        {
            playerBattlers[currentPartyHero].BattleVisuals.ToggleSpotlight(true);
            uiManager.ShowBattleMenu(playerBattlers[currentPartyHero].Name, playerBattlers[currentPartyHero].Portrait);
        }
    }

    private void HandleEnemyHoverEntered(int index)
    {
        if (index >= 0 && index < enemyBattlers.Count)
        {
            enemyBattlers[index].BattleVisuals.ToggleSpotlight(true);
        }
    }

    private void HandleEnemyHoverExited(int index)
    {
        if (index >= 0 && index < enemyBattlers.Count)
        {
            enemyBattlers[index].BattleVisuals.ToggleSpotlight(false);
        }
    }

    /// <summary>
    /// Forces all enemy spotlights off — called when the selection menu opens/closes
    /// to prevent spotlights from staying stuck between hero turns or after clicking.
    /// </summary>
    private void HandleForceAllEnemySpotlightsOff()
    {
        for (int i = 0; i < enemyBattlers.Count; i++)
        {
            if (enemyBattlers[i].BattleVisuals != null)
            {
                enemyBattlers[i].BattleVisuals.ToggleSpotlight(false);
            }
        }
    }

    // --- Battle Logic ---

    /// <summary>
    /// Main battle coroutine that executes all battlers' actions in turn order.
    /// </summary>
    private IEnumerator BattleRoutine()
    {
        uiManager.HideEnemySelectionMenu();
        State = BattleState.Battle;
        uiManager.ShowBattleInfoPanel(true);
        enemyManager.HasWonBattle = false;

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
            uiManager.ShowBattleInfoPanel(false);
            currentPartyHero = 0;
            if (playerBattlers.Count > 0)
            {
                playerBattlers[currentPartyHero].BattleVisuals.ToggleSpotlight(true);
                uiManager.ShowBattleMenu(playerBattlers[currentPartyHero].Name, playerBattlers[currentPartyHero].Portrait);
            }
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

            ExecuteAttackAndLog(currAttacker, currTarget);
            yield return new WaitForSeconds(TURN_DURATION);

            if (currTarget.CurrentHealth <= 0)
            {
                uiManager.UpdateBattleInfo(string.Format("{0} defeated {1}", currAttacker.Name, currTarget.Name));
                yield return new WaitForSeconds(TURN_DURATION);
                enemyBattlers.Remove(currTarget);
                
                if (enemyBattlers.Count <= 0)
                {
                    State = BattleState.Won;
                    enemyManager.HasWonBattle = true;
                    uiManager.UpdateBattleInfo(WIN_MESSAGE);
                    GameEvents.ResolveBattle(BattleResult.Won);
                }
            }
        }
        else if (i < allBattlers.Count && allBattlers[i].IsPlayer == false)
        {
            BattleEntities currAttacker = allBattlers[i];
            currAttacker.SetTarget(GetRandomPartyMember());
            BattleEntities currTarget = allBattlers[currAttacker.Target];
            
            ExecuteAttackAndLog(currAttacker, currTarget);
            yield return new WaitForSeconds(TURN_DURATION);
            
            if (currTarget.CurrentHealth <= 0)
            {
                uiManager.UpdateBattleInfo(string.Format("{0} defeated {1}", currAttacker.Name, currTarget.Name));
                yield return new WaitForSeconds(TURN_DURATION);
                playerBattlers.Remove(currTarget);
                
                if (playerBattlers.Count <= 0)
                {
                    State = BattleState.Lost;
                    uiManager.UpdateBattleInfo(LOST_MESSAGE);
                    yield return new WaitForSeconds(TURN_DURATION);
                    uiManager.ShowGameOver();
                    GameEvents.ResolveBattle(BattleResult.Lost);
                }
            }
        }
    }

    private void ExecuteAttackAndLog(BattleEntities attacker, BattleEntities target)
    {
        int damage;
        CombatResolver.ResolveAttack(attacker, target, out damage);
        uiManager.UpdateBattleInfo(string.Format("{0} Attacks {1} for {2} damage", attacker.Name, target.Name, damage));
        SaveHealth();
    }

    private IEnumerator RunRoutine()
    {
        if (State == BattleState.Battle)
        {
            if (Random.Range(1, 101) >= RUN_CHANCE)
            {
                partyManager.SetPosition(partyManager.GetPlayerStartPosition());
                uiManager.UpdateBattleInfo(SUCCESSFULLY_RAN_AWAY_MESSAGE);
                State = BattleState.Run;
                allBattlers.Clear();
                yield return new WaitForSeconds(TURN_DURATION);
                GameEvents.ResolveBattle(BattleResult.Run);
                yield break;
            }
            else
            {
                uiManager.UpdateBattleInfo(FAIL_RAN_AWAY_MESSAGE);
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
}