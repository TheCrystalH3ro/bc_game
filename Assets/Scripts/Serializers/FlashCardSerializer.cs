using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.Models;
using FishNet.Serializing;

namespace Assets.Scripts.Serializers
{
    public static class FlashCardSerializer
    {
        public static void WriteFlashCard(this Writer writer, FlashCard value)
        {
            writer.WriteString(value.GetQuestion());

            Dictionary<uint, string> answers = value.GetAnswers();
            writer.WriteUInt32((uint)answers.Count);

            foreach (var kvp in answers)
            {
                writer.WriteUInt32(kvp.Key);
                writer.WriteString(kvp.Value);
            }

            writer.WriteSingle(value.GetTime());
        }

        public static FlashCard ReadFlashCard(this Reader reader)
        {
            string question = reader.ReadStringAllocated();

            uint count = reader.ReadUInt32();
            Dictionary<uint, string> answers = new();

            for (int i = 0; i < count; i++)
            {
                uint key = reader.ReadUInt32();
                string value = reader.ReadStringAllocated();
                answers.Add(key, value);
            }

            float time = reader.ReadSingle();

            return new FlashCard(question, answers, 0, time);
        }
    }
}
