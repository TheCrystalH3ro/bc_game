using Assets.Scripts.Enums;
using Assets.Scripts.Models;
using FishNet.Serializing;

namespace Assets.Scripts.Serializers
{
    public static class PlayerCharacterSerializer
    {
        public static void WritePlayerCharacter(this Writer writer, PlayerCharacter value)
        {
            writer.WriteUInt32(value.GetId());
            writer.WriteString(value.GetName());
            writer.WriteUInt16(value.GetLevel());
            writer.WriteUInt8Unpacked((byte)value.GetPlayerClass());
        }

        public static PlayerCharacter ReadPlayerCharacter(this Reader reader)
        {
            uint id = reader.ReadUInt32();
            string name = reader.ReadStringAllocated();
            ushort level = reader.ReadUInt16();
            PlayerClass playerClass = (PlayerClass) reader.ReadUInt8Unpacked();

            return new PlayerCharacter(id, name, level, playerClass);
        }
    }
}
