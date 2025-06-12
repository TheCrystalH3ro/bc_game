using System;
using UnityEngine;

namespace Assets.Scripts.Requests
{
    [Serializable]
    public class RegisterRequest : BaseRequest
    {
        public string username;
        public string email;
        public string password;
        public string password_confirm;

        public RegisterRequest(string username, string email, string password, string password_confirm)
        {
            this.username = username;
            this.email = email;
            this.password = password;
            this.password_confirm = password_confirm;
        }
    }
}