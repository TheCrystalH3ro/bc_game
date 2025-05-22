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

        foreach (PlayerCharacter character in party)
        {
            if (PlayerController.Singleton.GetPlayerCharacter().Equals(character)) continue;

            AddPlayer(character);
        }
    }

    public void AddPlayer(PlayerCharacter character)
    {
        GameObject playerStatus = Instantiate(playerStatusPrefab);

        PlayerStatusController playerStatusController = playerStatus.GetComponentInChildren<PlayerStatusController>();

        playerStatusController.Init(character, character.GetSprite(), 100, 100);

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
}
