using System;
using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Models;
using Assets.Scripts.Requests;
using Assets.Scripts.Responses;
using UnityEngine.Networking;

namespace Assets.Scripts.Modules
{
    public class FlashCardModule
    {
        private static FlashCardModule _instance;

        public static FlashCardModule Singleton
        { 
            get
            {
                _instance ??=  new();

                return _instance;
            }
        }

        private readonly string apiKey = ConfigModule.Get("SERVER_API_KEY");

        public void GetFlashCard(BaseCharacterController character, Action<FlashCardResponse> onFlashCardGeneratedSuccess, Action<UnityWebRequest> onFlashCardGeneratedFail)
        {
            string flashCardEndPoint = "question";

            if (character is PlayerController)
                flashCardEndPoint += "/character/" + character.GetId();
            else
                flashCardEndPoint += "/enemy/" + ((int)(character as EnemyController).GetEnemyType());

            RequestModule.Singleton.GetRequest(flashCardEndPoint, apiKey, onFlashCardGeneratedSuccess, onFlashCardGeneratedFail);
        }

        public void SavePerformance(PlayerController player, FlashCard flashCard, bool isCorrect, float time)
        {
            SendPerformance(player, flashCard, isCorrect, time);
        }

        private void SendPerformance(PlayerController player, FlashCard flashCard, bool isCorrect, float time)
        {
            string flashCardEndPoint = $"question/character/{player.GetId()}";

            FlashCardPerformanceRequest performanceData = new(flashCard.GetId(), isCorrect, time);

            RequestModule.Singleton.PatchRequest(flashCardEndPoint, apiKey, performanceData);
        }
    }
}