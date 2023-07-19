using UnityEngine;

namespace MyProject.Struct
{
    public struct DamageParam
    {
        public HealthModifier healthModifier;
        public Vector3 point;
        public Vector3 direction;
        public float force;
        public float time;
    }
}