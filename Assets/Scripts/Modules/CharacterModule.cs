using System;
using System.Collections;
using Assets.Scripts.Requests;
using Assets.Scripts.Responses;
using Assets.Scripts.Util;
using UnityEngine.Networking;

namespace Assets.Scripts.Modules
{
    public class CharacterModule
    {
        private static CharacterModule _instance;

        public static CharacterModule Singleton
        { 
            get
            {
                if (_instance == null)
                {
                    _instance =  new(ConfigModule.Get("API_URL"));
                }

                return _instance;
            }
        }

        private readonly string apiUrl;

        public CharacterModule(string apiUrl)
        {
            this.apiUrl = apiUrl;
        }

        public void GetCharacters(string authToken, Action<CharactersResponse> onGetSuccess, Action<UnityWebRequest> onGetFail)
        {
            string characterEndPoint = "character";

            RequestModule.Singleton.GetRequest(characterEndPoint, authToken, onGetSuccess, onGetFail);
        }

        public void CreateCharacter(string name, int characterClass, string authToken, Action<CharacterResponse> onCreateSuccess, Action<UnityWebRequest> onCreateFail)
        {
            string createEndPoint = "character";

            CharacterCreateRequest createRequest = new(name, characterClass);

            RequestModule.Singleton.PostRequest(createEndPoint, authToken, createRequest, onCreateSuccess, onCreateFail);
        }

        public void DeleteCharacter(uint id, string authToken, Action<string> onDeleteSuccess, Action<UnityWebRequest> onDeleteFail)
        {
            string deleteEndPoint = $"character/{id}";

            RequestModule.Singleton.DeleteRequest(deleteEndPoint, authToken, onDeleteSuccess, onDeleteFail);
        }
    }
}