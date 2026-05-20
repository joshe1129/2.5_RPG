using System.Collections.Generic;
using TMPro;
using UnityEngine;
using RPGInterfaces;
using RPG.Core;

/// <summary>
/// Manages NPC recruitment and party member visuals in the overworld.
/// Handles interaction detection with joinable NPCs, spawns party member visuals following the player, and updates the HUD.
/// </summary>
public class CharacterManager : MonoBehaviour
{
    [SerializeField] private GameObject joinPopUp;
    [SerializeField] private GameObject avatarsHUD;
    [SerializeField] private TextMeshProUGUI joinPopUpText;
    private IPartyManager partyManager;

    private bool inFrontOfPartyMember;
    private GameObject joinableMember;
    private PlayerControls playerControls;
    private List<GameObject> overWorldCharacters = new List<GameObject>();

    private const string PARTY_JOINED_MESSAGE = " Joined The Party!";

    /// <summary>
    /// Initializes the player controls input system on awake.
    /// </summary>
    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    /// <summary>
    /// Initializes the character manager by setting up input listeners, restoring player position, and spawning party member visuals.
    /// </summary>
    private void Start()
    {
        playerControls.Player.Interact.performed += _ => Interact();
        partyManager = ServiceLocator.GetService<IPartyManager>();
        if (partyManager.GetPosition() != Vector3.zero)
        {
            transform.position = partyManager.GetPosition();
        }
        SpawnOverworldMembers();
    }

    /// <summary>
    /// Enables the player input controls when the script is enabled.
    /// </summary>
    private void OnEnable()
    {
        playerControls.Enable();
    }

    /// <summary>
    /// Disables the player input controls when the script is disabled.
    /// </summary>
    private void OnDisable()
    {
        playerControls.Disable();
    }

    /// <summary>
    /// Handles player interaction with nearby joinable NPCs.
    /// Recruits the NPC to the party and updates visuals if the player is in front of them.
    /// </summary>
    private void Interact()
    {
        if (joinableMember == null)
        {
            return;
        }
        if (inFrontOfPartyMember && joinableMember != null)
        {
            JoinMember(joinableMember.GetComponent<JoinableCharacterScript>().membertoJoin);
            inFrontOfPartyMember = false;
            joinableMember = null;
        }
    }

    /// <summary>
    /// Adds a new party member to the active party and updates the party visuals in the overworld.
    /// Displays a message confirming the party member has joined.
    /// </summary>
    /// <param name="partyMember">The PartyMemberInfo of the character to join the party.</param>
    private void JoinMember(PartyMemberInfo partyMember)
    {
        partyManager.AddMembertoPartyByName(partyMember.memberName);
        joinableMember.GetComponent<JoinableCharacterScript>().CheckIfJoined();
        joinPopUp.SetActive(true);
        joinPopUpText.text = partyMember.memberName + PARTY_JOINED_MESSAGE;
        SpawnOverworldMembers();
    }

    /// <summary>
    /// Instantiates and positions all current party members in the overworld.
    /// The first member is the player themselves, and subsequent members follow behind using MemberFollowAI.
    /// Updates the HUD avatar display as well.
    /// </summary>
    private void SpawnOverworldMembers()
    {
        var pooler = ServiceLocator.GetService<IObjectPooler>();
        for (int i = 0; i < overWorldCharacters.Count; i++)
        {
            if (pooler != null) pooler.ReturnToPool(overWorldCharacters[i]);
            else Destroy(overWorldCharacters[i]);
        }
        overWorldCharacters.Clear();
        List<PartyMember> currentParty = partyManager.GetCurrentParty();
        if (avatarsHUD != null)
        {
            var overworldVisuals = avatarsHUD.GetComponent<OverworldVisuals>();
            if (overworldVisuals != null)
            {
                overworldVisuals.UpdateOverworldVisuals();
            }
        }
        for (int i = 0; i < currentParty.Count; i++)
        {
            if (i == 0)
            {
                GameObject player = gameObject;
                GameObject playerVisual;
                if (pooler != null)
                {
                    playerVisual = pooler.SpawnFromPool(currentParty[i].memberOverworldVisualPrefab, player.transform.position, Quaternion.identity, player.transform);
                }
                else
                {
                    playerVisual = Instantiate(currentParty[i].memberOverworldVisualPrefab, player.transform);
                }
                playerVisual.transform.localPosition = Vector3.zero;
                playerVisual.transform.SetParent(player.transform);
                player.GetComponent<PlayerController>().SetOverworldVisuals(playerVisual.GetComponent<Animator>(), playerVisual.GetComponent<SpriteRenderer>());
                playerVisual.GetComponent<MemberFollowAI>().enabled = false;
                overWorldCharacters.Add(playerVisual);
            }
            else
            {
                Vector3 positionToSpawn = transform.position;
                positionToSpawn.x -= i;
                GameObject tempFollower;
                if (pooler != null)
                {
                    tempFollower = pooler.SpawnFromPool(currentParty[i].memberOverworldVisualPrefab, positionToSpawn, Quaternion.identity);
                }
                else
                {
                    tempFollower = Instantiate(currentParty[i].memberOverworldVisualPrefab, positionToSpawn, Quaternion.identity);
                }
                tempFollower.GetComponent<MemberFollowAI>().SetFollowDistance(i + 1.5f);
                overWorldCharacters.Add(tempFollower);
            }
        }
    }

    /// <summary>
    /// Detects when the player enters a trigger collider with a joinable NPC.
    /// Shows an interaction prompt for the NPC.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == GameConstants.TAG_NPC_JOINABLE)
        {
            inFrontOfPartyMember = true;
            joinableMember = other.gameObject;
            joinableMember.GetComponent<JoinableCharacterScript>().ShowInteractPrompt(inFrontOfPartyMember);
        }
    }

    /// <summary>
    /// Detects when the player exits a trigger collider with a joinable NPC.
    /// Hides the interaction prompt for the NPC.
    /// </summary>
    /// <param name="other">The collider that exited the trigger.</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == GameConstants.TAG_NPC_JOINABLE)
        {
            inFrontOfPartyMember = false;
            if (joinableMember != null)
            {
                var joinScript = joinableMember.GetComponent<JoinableCharacterScript>();
                if (joinScript != null)
                {
                    joinScript.ShowInteractPrompt(false);
                }
            }
            joinableMember = null;
        }
    }
}
