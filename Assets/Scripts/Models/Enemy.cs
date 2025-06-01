using Assets.Scripts.Interfaces;

namespace Assets.Scripts.Models
{
    public class Enemy : IEnemy
    {
        private string name;
        private int level;

        public Enemy(string name, int level)
        {
            this.name = name;
            this.level = level;
        }

        public int GetLevel()
        {
            return level;
        }

        public string GetName()
        {
            return name;
        }
    }
}