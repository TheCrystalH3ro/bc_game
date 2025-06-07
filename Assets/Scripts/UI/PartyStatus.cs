using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Controllers;
using Assets.Scripts.Models;
using Assets.Scripts.UI.Controllers;
using UnityEngine;

public class PartyStatus : MonoBehaviour
{
    [SerializeField] private GameObject playerStatusPrefab;

    private Dictionary<uint, GameObject> players;

    void Awake()
    {

        players = new();
    }

    public void ClearPlayers()
    {
        players = new();

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void Init(List<PlayerCharacter> party)
    {
        ClearPlayers();

        PlayerCharacter player = PlayerController.Singleton.GetPlayerCharacter();

        foreach (PlayerCharacter character in party)
        {
            if (player.Equals(character)) continue;

            AddPlayer(character);
        }
    }

    public void AddPlayer(PlayerCharacter character)
    {
        GameObject playerStatus = Instantiate(playerStatusPrefab);

        PlayerStatusController playerStatusController = playerStatus.GetComponentInChildren<PlayerStatusController>();

        bool isInSameZone = character.GetCurrentScene() == PlayerController.Singleton.gameObject.scene.name;

        playerStatusController.Init(character, character.GetSprite(), 100, 100, isInSameZone);

        playerStatusController.UpdateHealth(character.GetHealth());
        playerStatusController.UpdateExp(character.GetExp());

        playerStatus.transform.SetParent(transform, false);

        players.Add(character.GetId(), playerStatus);
    }

    public void RemovePlayer(uint characterId)
    {
        if (!players.ContainsKey(characterId)) return;

        GameObject playerStatus = players[characterId];
        Destroy(playerStatus);

        players.Remove(characterId);
    }

    public void EnteredZone(uint characterId)
    {
        if (!players.ContainsKey(characterId)) return;

        GameObject playerStatus = players[characterId];

        PlayerStatusController playerStatusController = playerStatus.GetComponentInChildren<PlayerStatusController>();

        playerStatusController.SetTransparency(1);
    }

    public void LeftZone(uint characterId)
    {
        if (!players.ContainsKey(characterId)) return;

        GameObject playerStatus = players[characterId];

        PlayerStatusController playerStatusController = playerStatus.GetComponentInChildren<PlayerStatusController>();

        playerStatusController.SetTransparency(0.5f);
    }
}
