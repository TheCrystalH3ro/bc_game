using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Responses;
using Assets.Scripts.Util;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Modules
{
    public class AuthModule
    {
        private static AuthModule _instance;

        public static AuthModule Singleton
        { 
            get
            {
                _instance ??=  new(ConfigModule.Get("API_URL"));

                return _instance;
            }
        }

        private readonly string apiUrl;

        private readonly Func<string, List<IValidationRule>, ValidationResult> _validate;
        private readonly IValidationProvider _validationProvider;
        public AuthModule(string apiUrl, Func<string, List<IValidationRule>, ValidationResult> validate = null, IValidationProvider validationProvider = null)
        {
            this.apiUrl = apiUrl;
        
            _validate = validate;
            _validationProvider = validationProvider;
        }

        public static AuthModule Initialize(string apiUrl, Func<string, List<IValidationRule>, ValidationResult> validate = null, IValidationProvider validationProvider = null)
        {
            _instance =  new(apiUrl, validate, validationProvider);

            return _instance;
        }

        public IEnumerator Login(string username, string password, Action<string> onLoginSuccess, Action<string> onLoginFail)
        {
            if (_validate != null)
            {
                var result = ValidateLogin(username, password);
                if (!result.IsValid) {
                    onLoginFail?.Invoke(result.ErrorMessage);
                    yield break;
                }
            }

            string loginUrl = apiUrl + "/login";

            JSONObject json = new();
            json.AddField("username", username);
            json.AddField("password", password);

            using (UnityWebRequest request = UnityWebRequest.Put(loginUrl, json.ToString())) {

                request.method = UnityWebRequest.kHttpVerbPOST;
                request.SetRequestHeader("Content-Type", "application/json"); 

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success) {
                    onLoginFail?.Invoke(request.error);
                    yield break;
                }

                string jwtToken = request.downloadHandler.text;
                onLoginSuccess?.Invoke(jwtToken);
            }
        }

        public IEnumerator Register(string username, string email, string password, string passwordConfirmation, Action onRegisterSuccess, Action<ApiErrors> onRegisterFail) {

            if (_validate != null)
            {
                var result = ValidateRegister(username, email, password, passwordConfirmation);
                if (!result.IsValid) {
                    onRegisterFail?.Invoke(new(result.ErrorMessage));
                    yield break;
                }
            }
        
            string registrationUrl = apiUrl + "/register";

            JSONObject json = new();
            json.AddField("username", username);
            json.AddField("email", email);
            json.AddField("password", password);
            json.AddField("password_confirm", passwordConfirmation);

            using (UnityWebRequest request = UnityWebRequest.Put(registrationUrl, json.ToString())) {

                request.method = UnityWebRequest.kHttpVerbPOST;
                request.SetRequestHeader("Content-Type", "application/json"); 

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success) {
                    Debug.LogError("Error registering user: " + request.error);
                    Debug.LogError(request.downloadHandler.text);
                    ApiErrors errors = ApiErrors.CreateFromJSON(request.downloadHandler.text);
                    onRegisterFail?.Invoke(errors);
                    yield break;
                }

                onRegisterSuccess?.Invoke();
            }
        
        }

        private ValidationResult ValidateField(string fieldKey, string value, object context = null)
        {
            List<IValidationRule> rules = _validationProvider.GetRules(fieldKey, context);
            ValidationResult result = _validate(value, rules);

            if (!result.IsValid)
            {
                result.ErrorMessage = _validationProvider.GetMessage(fieldKey, result.Rule);
            }

            return result;
        }

        private ValidationResult ValidateLogin(string username, string password)
        {
            ValidationResult result =  ValidateField("login.username", username);
            if (!result.IsValid) return result;

            result = ValidateField("login.password", password);
            if (!result.IsValid) return result;

            return result;
        }

        private ValidationResult ValidateRegister(string username, string email, string password, string passwordConfirmation)
        {
            ValidationResult result =  ValidateField("register.username", username);
            if (!result.IsValid) return result;

            result = ValidateField("register.email", email);
            if (!result.IsValid) return result;

            result = ValidateField("register.password", password);
            if (!result.IsValid) return result;

            result = ValidateField("register.passwordConfirmation", passwordConfirmation, password);
            if (!result.IsValid) return result;

            return result;
        }
    }
}
