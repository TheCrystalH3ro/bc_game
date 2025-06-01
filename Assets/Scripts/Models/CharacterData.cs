using UnityEngine;

namespace Assets.Scripts.Models
{
    public class CharacterData
    {
        private uint id;
        private string name;
        private int level;
        private int health;
        private int maxHealth;
        private Sprite sprite;

        public CharacterData(uint id, string name, int level, int health, int maxHealth, Sprite sprite)
        {
            this.id = id;
            this.name = name;
            this.level = level;
            this.health = health;
            this.maxHealth = maxHealth;
            this.sprite = sprite;
        }

        public uint GetId()
        {
            return id;
        }

        public void SetId(uint id)
        {
            this.id = id;
        }

        public string GetName()
        {
            return name;
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public int GetLevel()
        {
            return level;
        }

        public void SetLevel(int level)
        {
            this.level = level;
        }

        public int GetHealth()
        {
            return health;
        }

        public void SetHealth(int health)
        {
            this.health = health;
        }

        public int GetMaxHealth()
        {
            return maxHealth;
        }

        public void SetMaxHealth(int health)
        {
            this.maxHealth = health;
        }

        public Sprite GetSprite()
        {
            return sprite;
        }

        public void SetSprite(Sprite sprite)
        {
            this.sprite = sprite;
        }
    }
}