using UnityEngine;

namespace Character.Physics
{
    [System.Serializable]
    public class PhysicsData
    {
        public Vector2 velocity;
        public Vector2 externalForce;
        public float gravityScale = 1f;
        public float mass = 1f;
        public bool useGravity = true;

        public static PhysicsData Default => new PhysicsData
        {
            velocity = Vector2.zero,
            externalForce = Vector2.zero,
            gravityScale = 1f,
            mass = 1f,
            useGravity = true
        };

        public void AddForce(Vector2 force)
        {
            externalForce += force;
        }

        public void SetVelocity(Vector2 newVelocity)
        {
            velocity = newVelocity;
        }

        public void ResetForces()
        {
            externalForce = Vector2.zero;
        }
    }
}