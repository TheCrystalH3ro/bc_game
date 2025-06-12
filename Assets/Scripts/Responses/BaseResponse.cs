using System;
using UnityEngine;

namespace Assets.Scripts.Responses
{
    public abstract class BaseResponse
    {
        public static T CreateFromJSON<T>(string jsonString) where T : BaseResponse
        {
            return JsonUtility.FromJson<T>(jsonString);
        }
    }
}