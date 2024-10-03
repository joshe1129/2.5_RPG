using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] private GameObject joinPopUp;
    [SerializeField] private GameObject AvatarsHUD;
    [SerializeField] private TextMeshProUGUI joinPopUpText;
    private PartyManager partyManager;

    private bool infrontOfPartyMember;
    private GameObject joinableMember;
    private PlayerControls playerControls;
    private List<GameObject> overWorldCharacters = new List<GameObject>();

    private const string NPC_JOINABLE_TAG = "NPCJoinable";
    private const string PARTY_JOINED_MESSAGE = " Joined The Party!";

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void Start()
    {
        playerControls.Player.Interact.performed += _ => Interact();
        partyManager = GameObject.FindFirstObjectByType<PartyManager>();
        if (partyManager.GetPosition() != Vector3.zero)
        {
            transform.position = partyManager.GetPosition();
        }
        SpawnOverworldMembers();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Interact()
    {
        if (infrontOfPartyMember && joinableMember != null)
        {
            JoinMember(joinableMember.GetComponent<JoinableCharacterScript>().membertoJoin);
            infrontOfPartyMember = false;
            joinableMember = null;
        }
    }

    private void JoinMember(PartyMemberInfo partyMember)
    {
        GameObject.FindFirstObjectByType<PartyManager>().AddMembertoPartyByName(partyMember.memberName);
        joinableMember.GetComponent<JoinableCharacterScript>().CheckIfJoined();
        joinPopUp.SetActive(true);
        joinPopUpText.text = partyMember.memberName + PARTY_JOINED_MESSAGE;
        SpawnOverworldMembers();
    }

    private void SpawnOverworldMembers()
    {
        for (int i = 0; i < overWorldCharacters.Count; i++)
        {
            Destroy(overWorldCharacters[i]);
        }
        overWorldCharacters.Clear();
        List<PartyMember> currentParty = GameObject.FindFirstObjectByType<PartyManager>().GetCurrentParty();
        AvatarsHUD.GetComponent<OverworldVisuals>().UpdateOverworldVisuals();
        for (int i = 0; i < currentParty.Count; i++)
        {
            if (i == 0)
            {
                GameObject player = gameObject;
                GameObject playerVisual = Instantiate(currentParty[i].memberOverworldVisualPrefab, player.transform.position, Quaternion.identity);
                playerVisual.transform.SetParent(player.transform);
                player.GetComponent<PlayerController>().SetOverworldVisuals(playerVisual.GetComponent<Animator>(), playerVisual.GetComponent<SpriteRenderer>());
                playerVisual.GetComponent<MemberFollowAI>().enabled = false;
                overWorldCharacters.Add(playerVisual);
            }
            else
            {
                Vector3 positionToSpawn = transform.position;
                positionToSpawn.x -= i;
                GameObject tempFollower = Instantiate(currentParty[i].memberOverworldVisualPrefab, positionToSpawn, Quaternion.identity);
                tempFollower.GetComponent<MemberFollowAI>().SetFollowDistance(i + 1.5f);
                overWorldCharacters.Add(tempFollower);
            }
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == NPC_JOINABLE_TAG)
        {
            infrontOfPartyMember = true;
            joinableMember = other.gameObject;
            joinableMember.GetComponent<JoinableCharacterScript>().ShowInteractPrompt(infrontOfPartyMember);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == NPC_JOINABLE_TAG)
        {
            infrontOfPartyMember = false;
            joinableMember.GetComponent<JoinableCharacterScript>().ShowInteractPrompt(infrontOfPartyMember);
            joinableMember = null;
        }
    }
}
