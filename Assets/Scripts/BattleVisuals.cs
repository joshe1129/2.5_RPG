using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class BattleVisuals : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI lvlText;

    private int currentHealth;
    private int maxHealth;
    private int level;
    private Animator animator;

    private const string LEVEL_PREV_TEXT = "Lvl: ";
    private const string IS_ATTACKING_PARAM = "isAttacking";
    private const string IS_DEATH_PARAM = "isDeath";
    private const string IS_HIT_PARAM = "isHit";
    // Start is called before the first frame update
    void Awake()
    {
        animator = gameObject.GetComponentInChildren<Animator>();
    }

    public void SetStartingValues(int currentHealth, int maxHealth, int level)
    {
        this.currentHealth = currentHealth;
        this.maxHealth = maxHealth;
        this.level = level;
        lvlText.text = LEVEL_PREV_TEXT + this.level.ToString();
        UpdateHealthBar();
    }    

    public void UpdateHealthBar()
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }

    public void ChangeHealth(int currentHealth)
    {
        this.currentHealth = currentHealth;
        if (currentHealth <= 0)
        {
            PlayDeathAnimation();
            Destroy(gameObject, 1.5f);
        }
        UpdateHealthBar();
    }

    public void PlayAttackAnimation()
    {
        animator.SetTrigger(IS_ATTACKING_PARAM);
    }

    public void PlayHitAnimation()
    {
        animator.SetTrigger(IS_HIT_PARAM);
    }
    
    public void PlayDeathAnimation()
    {
        animator.SetTrigger(IS_DEATH_PARAM);
    }
}
