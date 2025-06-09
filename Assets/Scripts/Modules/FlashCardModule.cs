using System.Collections.Generic;
using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.Modules
{
    public class FlashCardModule
    {
        private static FlashCardModule _instance;

        public static FlashCardModule Singleton
        {
            get
            {
                _instance ??= new();

                return _instance;
            }
        }

        public List<FlashCard> flashCards;

        public FlashCardModule()
        {
            LoadFlashCards();
        }

        private void LoadFlashCards()
        {
            flashCards = new()
            {
                new FlashCard
                (
                    "TEST 1",
                    new()
                    {
                        { 0, "Odpoved 1" },
                        { 1, "Odpoved 2" },
                        { 2, "Odpoved 3" },
                        { 3, "Odpoved 4" }
                    },
                    0,
                    5f
                ),
            };
        }

        public FlashCard GetFlashCard()
        {
            int index = Random.Range(0, flashCards.Count);
            return flashCards[index];
        }
    }
}