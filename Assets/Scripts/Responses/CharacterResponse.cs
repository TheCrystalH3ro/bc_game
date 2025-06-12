using System;
using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Responses
{
    [Serializable]
    public class CharacterResponse : BaseResponse
    {
        public uint id;
        public string name;
        public PlayerClass characterTypeId;
        public uint playerId;
        public ushort level;
        public float experience;
    }

    [Serializable]
    public class CharactersResponse : BaseResponse
    {
        public CharacterResponse[] characters;
    }
}
