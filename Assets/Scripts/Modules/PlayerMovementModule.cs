using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Models;
using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Modules
{
    public class PlayerMovementModule : MovementModule
    {
        void Update()
        {
            if(!base.IsOwner) return;

            HandleMovementInput();
        }

        private void HandleMovementInput()
        {
            if(_movement.Value.x == Input.GetAxisRaw("Horizontal") && _movement.Value.y == Input.GetAxisRaw("Vertical"))
            {
                return;
            }

            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");

            SetDirection(x, y);
        }
    }
}