using System;
using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Responses
{
    [Serializable]
    public class CharacterResponse
    {
        public uint id;
        public string name;
        public PlayerClass characterTypeId;
        public uint playerId;
        public ushort level;
        public float experience;

        public static CharacterResponse CreateFromJSON(string jsonString) {
            return JsonUtility.FromJson<CharacterResponse>(jsonString);
        }

        public string ToJson() {
            return JsonUtility.ToJson(this);
        }
    }

    [Serializable]
    public class CharactersResponse
    {
        public CharacterResponse[] characters;
        public static CharactersResponse CreateFromJSON(string jsonString) {
            return JsonUtility.FromJson<CharactersResponse>(jsonString);
        }
    }
}
