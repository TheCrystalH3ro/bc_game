using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;
using Assets.Scripts.Models;
using Assets.Scripts.Requests;
using Assets.Scripts.Responses;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Modules
{
    public class FlashCardModule : MonoBehaviour
    {
        public static FlashCardModule Singleton { get; private set; }

        public List<FlashCard> flashCards;

        private readonly string apiUrl = ConfigModule.Get("API_URL");
        private readonly string apiKey = ConfigModule.Get("SERVER_API_KEY");

        void Awake()
        {
            Singleton = this;
        }

        public void GetFlashCard(BaseCharacterController character, Action<FlashCard> onFlashCardGenerated, Action<string> onFlashCardGeneratedFail)
        {
            StartCoroutine(FetchFlashCard(character, onFlashCardGenerated, onFlashCardGeneratedFail));
        }

        private IEnumerator FetchFlashCard(BaseCharacterController character, Action<FlashCard> onFlashCardGeneratedSuccess, Action<string> onFlashCardGeneratedFail)
        {
            string flashCardUrl = $"{apiUrl}/question";

            if (character is PlayerController)
                flashCardUrl += "/character/" + character.GetId();
            else
                flashCardUrl += "/enemy/" + ((int)(character as EnemyController).GetEnemyType());

            using (UnityWebRequest request = UnityWebRequest.Get(flashCardUrl))
            {
                request.SetRequestHeader("Authorization", apiKey);

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onFlashCardGeneratedFail?.Invoke(request.error);
                    yield break;
                }

                FlashCardResponse flashCardResponse = FlashCardResponse.CreateFromJSON(request.downloadHandler.text);
                FlashCard flashCard = new(flashCardResponse);

                onFlashCardGeneratedSuccess?.Invoke(flashCard);
            }
        }

        public void SavePerformance(PlayerController player, FlashCard flashCard, bool isCorrect, float time)
        {
            StartCoroutine(SendPerformance(player, flashCard, isCorrect, time));
        }

        private IEnumerator SendPerformance(PlayerController player, FlashCard flashCard, bool isCorrect, float time)
        {
            string flashCardUrl = $"{apiUrl}/question/character/{player.GetId()}";

            FlashCardPerformanceRequest performanceData = new(flashCard.GetId(), isCorrect, time);

            using UnityWebRequest request = UnityWebRequest.Put(flashCardUrl, performanceData.ToJson());

            request.method = "PATCH";
            request.SetRequestHeader("Authorization", apiKey);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
        }
    }
}