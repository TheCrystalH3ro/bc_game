using Assets.Scripts.Interfaces;

namespace Assets.Scripts.Models
{
    public class Enemy : IEnemy
    {
        private string name;

        public Enemy(string name)
        {
            this.name = name;
        }

        public string GetName()
        {
            return name;
        }
    }
}