using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OverworldVisuals : MonoBehaviour
{
    private PartyManager partyManager;
    [SerializeField] private GameObject avatarProfile;

    private void Start()
    {
        partyManager = GameObject.FindFirstObjectByType<PartyManager>();
        UpdateOverworldVisuals();
    }

    public void UpdateOverworldVisuals()
    {
        // Limpia los hijos anteriores si es necesario
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < partyManager.GetCurrentParty().Count; i++)
        {
            var partyMember = partyManager.GetCurrentParty()[i];  

            // Instancia el prefab y establece su padre
            GameObject avatarInstance = Instantiate(avatarProfile, transform);
            avatarInstance.name = partyMember.memberName; // Opcional: cambiar el nombre para identificación
            // Si necesitas configurar el sprite del avatar, puedes hacerlo aquí
            Image avatarImage = avatarInstance.GetComponentInChildren<Image>();
            if (avatarImage != null)
            {
                avatarImage.sprite = partyMember.sprite;
            }
            Slider avatarHealthbar = avatarInstance.GetComponentInChildren<Slider>();
            if (avatarHealthbar != null)
            {
                avatarHealthbar.maxValue = partyMember.maxHealth;
                avatarHealthbar.value = partyMember.currentHealth;
            }

        }
    }
}
