using System;
using System.Collections;
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

        public IEnumerator GetCharacters(string authToken, Action<CharactersResponse> onGetSuccess, Action<string> onGetFail)
        {
            string characterUrl = apiUrl + "/character";

            using (UnityWebRequest request = UnityWebRequest.Get(characterUrl)) {

                request.SetRequestHeader("Authorization", authToken);

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success) {
                    onGetFail?.Invoke(request.error);
                    yield break;
                }

                CharactersResponse characters = CharactersResponse.CreateFromJSON(request.downloadHandler.text);
                onGetSuccess?.Invoke(characters);
            }
        }

        public IEnumerator CreateCharacter(string name, int characterClass, string authToken, Action<CharacterResponse> onCreateSuccess, Action<string> onCreateFail)
        {
            string createUrl = apiUrl + "/character";

            JSONObject json = new();
            json.AddField("name", name);
            json.AddField("characterClass", characterClass);

            using (UnityWebRequest request = UnityWebRequest.Put(createUrl, json.ToString())) {

                request.method = UnityWebRequest.kHttpVerbPOST;
                request.SetRequestHeader("Content-Type", "application/json"); 
                request.SetRequestHeader("Authorization", authToken);

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success) {
                    onCreateFail?.Invoke(request.error);
                    yield break;
                }

                CharacterResponse character = CharacterResponse.CreateFromJSON(request.downloadHandler.text);
                onCreateSuccess?.Invoke(character);
            }
        }

        public IEnumerator DeleteCharacter(uint id, string authToken, Action onDeleteSuccess, Action<string> onDeleteFail)
        {
            string deleteUrl = apiUrl + "/character/" + id;

            using (UnityWebRequest request = UnityWebRequest.Delete(deleteUrl)) {

                request.SetRequestHeader("Authorization", authToken);

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success) {
                    onDeleteFail?.Invoke(request.error);
                    yield break;
                }

                onDeleteSuccess?.Invoke();
            }
        }
    }
}