using System;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Requests;
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
                _instance ??= new(ConfigModule.Get("SERVER_API_KEY"));

                return _instance;
            }
        }

        private readonly string apiKey;

        public GameDataModule(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public void LoadPlayerData(uint characterId, Action<PlayerDataResponse> onLoadSuccess, Action<UnityWebRequest> onLoadFail)
        {
            string loadEndPoint = $"character/load/{characterId}";

            RequestModule.Singleton.GetRequest(loadEndPoint, apiKey, onLoadSuccess, onLoadFail);
        }

        public void SavePlayerData(uint characterId, IPlayerData playerData, Action<string> onSaveSuccess, Action<UnityWebRequest> onSaveFail)
        {
            string saveUrl = $"character/save/{characterId}";

            PlayerDataRequest playerDataRequest = new(characterId, playerData.GetLevel(), playerData.GetExperience(), playerData.GetWorldData());

            RequestModule.Singleton.PatchRequest(saveUrl, apiKey, playerDataRequest, onSaveSuccess, onSaveFail);
        }
    }
}