using System;
using UnityEngine;

namespace Assets.Scripts.Requests
{
    [Serializable]
    public class LoginRequest : BaseRequest
    {
        public string username;
        public string password;

        public LoginRequest(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
    }
}