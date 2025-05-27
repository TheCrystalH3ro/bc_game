using System;
using System.Collections;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Responses;
using UnityEngine.Networking;

namespace Assets.Scripts.Modules
{
    public class GameDataModule
    {
        private static GameDataModule _instance;

        public static GameDataModule Singleton
        {
            get
            {
                _instance ??= new(ConfigModule.Get("API_URL"), ConfigModule.Get("SERVER_API_KEY"));

                return _instance;
            }
        }

        private readonly string apiUrl;
        private readonly string apiKey;

        public GameDataModule(string apiUrl, string apiKey)
        {
            this.apiUrl = apiUrl;
            this.apiKey = apiKey;
        }

        public IEnumerator LoadPlayerData(uint characterId, Action<PlayerDataResponse> onLoadSuccess, Action<string> onLoadFail)
        {
            string loadUrl = $"{apiUrl}/character/load/{characterId}";

            using (UnityWebRequest request = UnityWebRequest.Get(loadUrl))
            {
                request.SetRequestHeader("Authorization", apiKey);

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onLoadFail?.Invoke(request.error);
                    yield break;
                }

                PlayerDataResponse playerDataResponse = PlayerDataResponse.CreateFromJSON(request.downloadHandler.text);

                onLoadSuccess?.Invoke(playerDataResponse);
            }
        }

        public IEnumerator SavePlayerData(uint characterId, IPlayerData playerData, Action onSaveSuccess, Action<string> onSaveFail)
        {
            string saveUrl = $"{apiUrl}/character/save/{characterId}";

            PlayerDataResponse playerDataResponse = new(characterId, playerData.GetLevel(), playerData.GetExperience(), playerData.GetWorldData());

            using (UnityWebRequest request = UnityWebRequest.Put(saveUrl, playerDataResponse.ToJson()))
            {
                request.method = "PATCH";
                request.SetRequestHeader("Authorization", apiKey);
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onSaveFail?.Invoke(request.error);
                    yield break;
                }

                onSaveSuccess?.Invoke();
            }
        }
    }
}