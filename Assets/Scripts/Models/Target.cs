using Assets.Scripts.Controllers;
using Assets.Scripts.Controllers.Server;

namespace Assets.Scripts.Models
{
    public class Target
    {
        public uint Id { get; set; }
        public bool IsBot { get; set; }

        public Target()
        {
            Id = 0;
            IsBot = true;
        }

        public Target(uint id, bool isBot)
        {
            Id = id;
            IsBot = isBot;
        }

        public bool Equals(BaseCharacterController character)
        {
            if (IsBot)
            {
                EnemyController enemy = character as EnemyController;

                if (enemy == null)
                    return false;

                return enemy.Id == Id;
            }

            PlayerController player = character as PlayerController;

            if (player == null)
                return false;

            return player.GetId() == Id;
        }
    }
}