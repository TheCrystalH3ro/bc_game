using System;
using UnityEngine;

namespace Assets.Scripts.Requests
{
    [Serializable]
    public class CharacterCreateRequest : BaseRequest
    {
        public string name;
        public int characterClass;

        public CharacterCreateRequest(string name, int characterClass)
        {
            this.name = name;
            this.characterClass = characterClass;
        }
    }
}