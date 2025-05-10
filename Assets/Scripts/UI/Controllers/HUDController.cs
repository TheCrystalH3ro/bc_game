using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI.Controllers
{
    public class HUDController : MonoBehaviour
    {
        public static HUDController Singleton { get; private set; }

        public PauseMenu PauseMenu;
        public PlayerCardController PlayerCard;

        void OnEnable()
        {
            Singleton = FindFirstObjectByType<HUDController>();
        }
    }
}