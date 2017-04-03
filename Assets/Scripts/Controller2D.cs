using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
    public LayerMask collisionMask;
    const float skinWidth = 0.015f;
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    float maxSlopeAngle = 80;

    float horizontalRaySpacing;
    float verticalRaySpacing;

    BoxCollider2D collider;
    RaycastOrigin raycastOrigin;
    public CollisionInfo collision;

    void Start()
    {
        this.collider = GetComponent<BoxCollider2D>();
        CalculatorRaySpacing();
    }

    public void Move(Vector3 velocity)
    {
        UpdateRaycastOrigin();

        collision.Reset();

        if (velocity.x != 0)
            HorizontalCollisions(ref velocity);

        if (velocity.y != 0)
            VerticalCo1llisions(ref velocity);
        
        // tinh toan vi tri cua nhan vat theo van toc
        transform.Translate(velocity);
    }

    /// <summary>
    /// Horizontals the collisions.
    /// va chạm ngang
    /// </summary>
    /// <param name="velocity">Velocity.</param>
    void HorizontalCollisions(ref Vector3 velocity)
    {
        // huong theo truc X. mathf.sign tra ve -1 hoac 1
        float directionX = Mathf.Sign(velocity.x);
        // độ dài của tia bằng vận tốc của vật thể + skinWidth
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;


        for (int i = 0; i < horizontalRayCount; i++)
        {
            // gốc của tia raycast
            Vector2 rayOrigin = directionX == -1 ? raycastOrigin.bottomLeft : raycastOrigin.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            // tạo tia raycast 
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit)
            {
                // tính góc của con dốc 
                // hit.normal là vector pháp tuyến từ mặt phẳng
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up); 

                if (i == 0 && slopeAngle <= maxSlopeAngle)
                {
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collision.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    ClimpSlope(ref velocity, slopeAngle); 
                    velocity.x += distanceToSlopeStart * directionX;
                }

                if (!collision.climbingSlope || slopeAngle > maxSlopeAngle)
                {
                    // tính toán lại vận tốc
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    // tính lại độ dài tia raycast
                    rayLength = hit.distance;

                    collision.left = directionX == -1;
                    collision.right = directionX == 1;
                }
            }
        }
    }

    /// <summary>
    /// Verticals the co1llisions.
    /// va chạm đứng
    /// </summary>
    /// <param name="velocity">Velocity.</param>
    void VerticalCo1llisions(ref Vector3 velocity)
    {
        // hướng di chuyển của nhân vật =1 đi lên; =-1 đi xuống
        float directionY = Mathf.Sign(velocity.y);
        // độ dài của tia bằng với vận tốc của vận tốc + skinWidth
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;


        for (int i = 0; i < verticalRayCount; i++)
        {
            // tinh toan gốc của tia raycast
            Vector2 rayOrigin = directionY == -1 ? raycastOrigin.bottomLeft : raycastOrigin.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            // tạo tia raycast 
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit)
            {
                // tính toán lại vận tốc
                velocity.y = (hit.distance - skinWidth) * directionY;
                // tính lại độ dài tia raycast
                rayLength = hit.distance;

                if (collision.climbingSlope)
                {
                    velocity.x = velocity.y / Mathf.Tan(collision.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }
                
                collision.above = directionY == 1;
                collision.below = directionY == -1;
            }
        }
    }

    /// <summary>
    /// tính vận tốc của nhân vật khi di chuyển trên dốc
    /// khi di chuyển lên dốc vận tốc sẽ chậm hơn khi đi trên mặt bằng
    /// </summary>
    /// <param name="velocity">Velocity.</param>
    /// <param name="slopeAngle">Slope angle.</param>
    void ClimpSlope(ref Vector3 velocity, float slopeAngle)
    {
        // vận tốc theo trục x
        float moveDistance = Mathf.Abs(velocity.x);
        // van toc theo truc y tren doc
        float climVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        if (velocity.y <= climVelocityY)
        {
            // tính lại vận tốc trục trục x
            velocity.y = climVelocityY;
            // tính lại vận tốc của trục y
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);

            collision.below = true;

            collision.climbingSlope = true;
            collision.slopeAngle = slopeAngle;
        }
    }

    /// <summary>
    /// Updates the raycast origin.
    /// các gốc tọa độ của tia ray cast
    /// được tính toán trong update
    /// </summary>
    void UpdateRaycastOrigin()
    {
        Bounds bound = this.collider.bounds;
        bound.Expand(skinWidth * -2);

        raycastOrigin.bottomLeft = new Vector2(bound.min.x, bound.min.y);
        raycastOrigin.bottomRight = new Vector2(bound.max.x, bound.min.y);
        raycastOrigin.topLeft = new Vector2(bound.min.x, bound.max.y);
        raycastOrigin.topRight = new Vector2(bound.max.x, bound.max.y);
    }

    /// <summary>
    /// Calculators the ray spacing.
    /// tính khoảng cách giữa các tia ray cast
    /// </summary>
    void CalculatorRaySpacing()
    {
        Bounds bound = this.collider.bounds;
        bound.Expand(skinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bound.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bound.size.x / (verticalRayCount - 1);
    }

    struct RaycastOrigin
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;
        public bool climbingSlope;

        public float slopeAngle, slopeAngleOld;

        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            slopeAngle = slopeAngleOld;
            slopeAngleOld = 0;
        }
    }
}
