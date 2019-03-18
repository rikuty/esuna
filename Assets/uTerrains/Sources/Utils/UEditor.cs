using System;
using UnityEngine;

namespace UltimateTerrains
{
    public static class UEditor
    {
        public static bool ValidateMonoScript<T>(Type scriptType)
        {
            if (scriptType == null ||
                scriptType.IsInterface ||
                !scriptType.IsSubclassOf(typeof(T))) {
                return false;
            }

            return true;
        }

        public static T GetSerializableInstance<T>(Type scriptType) where T : ScriptableObject
        {
            if (scriptType != null && ValidateMonoScript<T>(scriptType)) {
                var so = ScriptableObject.CreateInstance(scriptType);
                if (so is T) {
                    return (T) so;
                }
            }

            return null;
        }
    }
}