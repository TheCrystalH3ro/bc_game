using System;
using UnityEngine;

namespace Assets.Scripts.Requests
{
    [Serializable]
    public abstract class BaseRequest
    {
        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }
    }
}