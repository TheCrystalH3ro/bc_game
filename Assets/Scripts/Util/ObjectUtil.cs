using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

namespace Assets.Scripts.Util
{
    public static class ObjectUtil
    {
        public static T FindFirstByType<T>(HashSet<NetworkObject> networkObjects) where T : Component
        {
            foreach (var netObj in networkObjects)
            {
                if (netObj.TryGetComponent<T>(out var component))
                    return component;
            }
            
            return null;
        }
    }
}