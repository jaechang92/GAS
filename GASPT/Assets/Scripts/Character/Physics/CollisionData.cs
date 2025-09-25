using UnityEngine;

namespace Character.Physics
{
    [System.Serializable]
    public class CollisionData
    {
        // Ground Detection
        public bool isGrounded;
        public bool wasGroundedLastFrame;
        public float groundDistance;
        public Vector2 groundNormal;
        public RaycastHit2D groundHit;

        // Wall Detection
        public bool isWallLeft;
        public bool isWallRight;
        public float wallDistanceLeft;
        public float wallDistanceRight;
        public Vector2 wallNormalLeft;
        public Vector2 wallNormalRight;

        // Ceiling Detection
        public bool isCeiling;
        public float ceilingDistance;
        public Vector2 ceilingNormal;

        // Slope Detection
        public bool isOnSlope;
        public float slopeAngle;
        public Vector2 slopeNormal;
        public bool canWalkOnSlope;

        // Platform Detection
        public bool isOnMovingPlatform;
        public Transform platformTransform;
        public Vector2 platformVelocity;
        public bool isOnOneWayPlatform;

        // Corner Detection (for ledge grab, etc.)
        public bool hasCornerLeft;
        public bool hasCornerRight;
        public Vector2 cornerPositionLeft;
        public Vector2 cornerPositionRight;

        // Collision Flags
        public CollisionFlags collisionFlags;

        // Helper Properties
        public bool IsAgainstWall => isWallLeft || isWallRight;
        public bool JustLanded => isGrounded && !wasGroundedLastFrame;
        public bool JustLeftGround => !isGrounded && wasGroundedLastFrame;

        public void UpdateGroundedState()
        {
            wasGroundedLastFrame = isGrounded;
        }
    }

    [System.Flags]
    public enum CollisionFlags
    {
        None = 0,
        Below = 1,
        Above = 2,
        Left = 4,
        Right = 8,
        Slope = 16,
        Platform = 32
    }
}