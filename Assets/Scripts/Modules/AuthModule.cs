using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Requests;
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
                _instance ??=  new();

                return _instance;
            }
        }

        private readonly Func<string, List<IValidationRule>, ValidationResult> _validate;
        private readonly IValidationProvider _validationProvider;
        public AuthModule(Func<string, List<IValidationRule>, ValidationResult> validate = null, IValidationProvider validationProvider = null)
        {
            _validate = validate;
            _validationProvider = validationProvider;
        }

        public static AuthModule Initialize(Func<string, List<IValidationRule>, ValidationResult> validate = null, IValidationProvider validationProvider = null)
        {
            _instance =  new(validate, validationProvider);

            return _instance;
        }

        public void Login(string username, string password, Action<string> onLoginSuccess, Action<string> onLoginFail)
        {
            if (_validate != null)
            {
                var result = ValidateLogin(username, password);
                if (!result.IsValid) {
                    onLoginFail?.Invoke(result.ErrorMessage);
                    return;
                }
            }

            string loginEndPoint = "login";
            LoginRequest loginRequestBody = new(username, password);

            RequestModule.Singleton.PostRequest(loginEndPoint, null, loginRequestBody, onLoginSuccess, onLoginFail);
        }

        public void Register(string username, string email, string password, string passwordConfirmation, Action<string> onRegisterSuccess, Action<string> onRegisterFail) {

            if (_validate != null)
            {
                var result = ValidateRegister(username, email, password, passwordConfirmation);
                if (!result.IsValid) {
                    onRegisterFail?.Invoke(result.ErrorMessage);
                    return;
                }
            }

            string registerEndPoint = "register";
            RegisterRequest registerRequestBody = new(username, email, password, passwordConfirmation);
            
            RequestModule.Singleton.PostRequest(registerEndPoint, null, registerRequestBody, onRegisterSuccess, onRegisterFail);
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
