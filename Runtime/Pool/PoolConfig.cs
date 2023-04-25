using System;
using UnityEngine;

using Edger.Unity;

namespace Edger.Unity.Pool {
    public class PoolConfig : ScriptableObject {
        public static PoolConfig DEFAULT = new PoolConfig {};

        public const int Default_MaxSize = 100;
        public const bool Default_UpdateParent = true;
        public const bool Default_SetActive = true;
        public const bool Default_UpdateInPoolData = true;

        public int MaxSize = Default_MaxSize;
        public bool UpdateParent = Default_UpdateParent;
        public bool SetActive = Default_SetActive;
        public bool UpdateInPoolData = Default_UpdateInPoolData;

        public override string ToString() {
            return $"{{ MaxSize = {MaxSize}, UpdateParent = {UpdateParent}, SetActive = {SetActive}, UpdateInPoolData = {UpdateInPoolData} }}";
        }
    }
}
