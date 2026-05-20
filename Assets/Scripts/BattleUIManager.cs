using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles all battle-related UI elements and passes user inputs as events to the BattleSystem.
/// </summary>
public class BattleUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject battleMenu;
    [SerializeField] private GameObject enemySelectionMenu;
    [SerializeField] private GameObject battleInfoPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private UnityEngine.UI.Image portraitImage;
    [SerializeField] private TextMeshProUGUI battleInfoText;
    [SerializeField] private GameObject[] enemySelectionButtons;

    // Events to notify the BattleSystem of player decisions
    public event Action OnAttackSelected;
    public event Action OnRunSelected;
    public event Action<int> OnEnemyTargetSelected;
    public event Action<int> OnEnemyHoverEntered;
    public event Action<int> OnEnemyHoverExited;
    public event Action OnForceAllEnemySpotlightsOff;

    private void Start()
    {
        for (int i = 0; i < enemySelectionButtons.Length; i++)
        {
            UIHoverHandler hoverHandler = enemySelectionButtons[i].gameObject.AddComponent<UIHoverHandler>();
            hoverHandler.index = i;
            hoverHandler.OnHoverEntered += (idx) => OnEnemyHoverEntered?.Invoke(idx);
            hoverHandler.OnHoverExited += (idx) => OnEnemyHoverExited?.Invoke(idx);
        }
    }

    /// <summary>
    /// Displays the main battle action menu for a specific battler.
    /// </summary>
    /// <param name="battlerName">The name of the battler taking the turn.</param>
    public void ShowBattleMenu(string battlerName, Sprite portraitSprite)
    {
        actionText.text = battlerName + "'s Action: ";
        if (portraitImage != null && portraitSprite != null)
        {
            portraitImage.sprite = portraitSprite;
            portraitImage.gameObject.SetActive(true);
        }
        else if (portraitImage != null)
        {
            portraitImage.gameObject.SetActive(false);
        }
        battleMenu.SetActive(true);
        enemySelectionMenu.SetActive(false);
    }

    /// <summary>
    /// Displays the enemy selection menu, activating a button for each alive enemy.
    /// </summary>
    /// <param name="enemyBattlers">The list of alive enemies.</param>
    public void ShowEnemySelectionMenu(List<BattleEntities> enemyBattlers)
    {
        // Force all spotlights off before reopening (prevents stuck lights between hero turns)
        OnForceAllEnemySpotlightsOff?.Invoke();
        battleMenu.SetActive(false);
        SetEnemySelectionButtons(enemyBattlers);
        enemySelectionMenu.SetActive(true);
    }

    private void SetEnemySelectionButtons(List<BattleEntities> enemyBattlers)
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

    /// <summary>
    /// Updates the central battle information text panel.
    /// </summary>
    /// <param name="message">The message to display.</param>
    public void UpdateBattleInfo(string message)
    {
        battleInfoText.text = message;
    }

    /// <summary>
    /// Toggles the visibility of the battle info panel.
    /// </summary>
    public void ShowBattleInfoPanel(bool show)
    {
        battleInfoPanel.SetActive(show);
    }

    /// <summary>
    /// Hides battle menus and shows the Game Over panel.
    /// </summary>
    public void ShowGameOver()
    {
        battleInfoPanel.SetActive(false);
        gameOverPanel.SetActive(true);
    }

    public void HideEnemySelectionMenu()
    {
        // Force all spotlights off when the selection menu closes
        OnForceAllEnemySpotlightsOff?.Invoke();
        enemySelectionMenu.SetActive(false);
    }

    public void HideBattleMenu()
    {
        battleMenu.SetActive(false);
    }

    // --- UI Event Handlers (Linked in the Unity Inspector) ---

    public void OnAttackButton()
    {
        OnAttackSelected?.Invoke();
    }

    public void OnRunButton()
    {
        OnRunSelected?.Invoke();
    }

    public void OnEnemyButton(int index)
    {
        OnEnemyTargetSelected?.Invoke(index);
    }
}
