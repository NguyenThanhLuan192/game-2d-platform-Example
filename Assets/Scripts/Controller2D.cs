using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : RaycastController
{

    public float maxSlopeAngle = 80;

    public CollisionInfo collision;
    [HideInInspector]
    public Vector2 playerInput;

    public override void Start()
    {
        base.Start();
        collision.faceDir = 1;
    }

    public void Move(Vector2 moveAmout, bool standingOnPlatform)
    {
        Move(moveAmout, Vector2.zero, standingOnPlatform);
    }

    public void Move(Vector2 moveAmout, Vector2 input, bool standingOnPlatform = false)
    {
        UpdateRaycastOrigin();

        collision.Reset();
        collision.moveAmoutOld = moveAmout;

        playerInput = input;

        // kiem tra huong di chuyen la trai hoac phai
        if (moveAmout.x != 0)
            collision.faceDir = (int)Mathf.Sign(moveAmout.x);

        if (moveAmout.x < 0)
            DescendSlope(ref moveAmout);

        HorizontalCollisions(ref moveAmout);

        if (moveAmout.y != 0)
            VerticalCo1llisions(ref moveAmout);
        
        // tinh toan vi tri cua nhan vat theo van toc
        transform.Translate(moveAmout);

        // kiem tra player co dung tren platform ko
        if (standingOnPlatform)
        {
            collision.below = true;
        }
    }

    /// <summary>
    /// Horizontals the collisions.
    /// va chạm ngang
    /// </summary>
    /// <param name="velocity">Velocity.</param>
    void HorizontalCollisions(ref Vector2 velocity)
    {
        // huong theo truc X. mathf.sign tra ve -1 hoac 1
        float directionX = collision.faceDir;
        // độ dài của tia bằng vận tốc của vật thể + skinWidth
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        // gioi han tia rayLength
        if (Mathf.Abs(velocity.x) < skinWidth)
        {
            rayLength = 2 * skinWidth;
        }

        for (int i = 0; i < horizontalRayCount; i++)
        {
            // gốc của tia raycast
            Vector2 rayOrigin = directionX == -1 ? raycastOrigin.bottomLeft : raycastOrigin.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            // tạo tia raycast 
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);

            if (hit)
            {

                if (hit.distance == 0)
                {
                    continue;
                }

                // tính góc của con dốc 
                // hit.normal là vector pháp tuyến từ mặt phẳng
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up); 

                if (i == 0 && slopeAngle <= maxSlopeAngle)
                {
                    if (collision.descendingSlope)
                    {
                        collision.descendingSlope = false;
                        velocity = collision.moveAmoutOld;
                    }


                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collision.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    ClimpSlope(ref velocity, slopeAngle, hit.normal); 
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
    void VerticalCo1llisions(ref Vector2 velocity)
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
            Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);

            if (hit)
            {
                // khi player nhay len va gap platform co the nhay qua thi se ko tinh toan phan van toc
                if (hit.collider.CompareTag("Throught"))
                {
                    if (directionY == 1 && hit.distance == 0)
                    {
                        // nhay ra khoi vong lap va tiep tuc vong lap
                        continue;
                    }

                    if (collision.fallingThroughPlatform)
                    {
                        continue;
                    }

                    if (playerInput.y == -1)
                    {
                        collision.fallingThroughPlatform = true;
                        Invoke("ResetFallingThroughPlatform", 0.5f);
                        continue;
                    }
                }


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

        // kiểm tra khi đi dang di chuyển trên dốc có gặp 1 con dốc khác
        if (collision.climbingSlope)
        {
            // hướng di chuyển
            float directionX = Mathf.Sign(velocity.x);
            // độ dài của tia
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            // gốc các tia
            Vector2 originRay = (directionX == -1) ? raycastOrigin.bottomLeft : raycastOrigin.bottomRight + Vector2.up * directionX;
            RaycastHit2D hit = Physics2D.Raycast(originRay, directionX * Vector2.right, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collision.slopeAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    collision.slopeAngle = slopeAngle;
                    collision.slopeNormal = hit.normal;
                }
            }
        }
    }

    /// <summary>'
    /// xử lý khi di chuyển lên dốc
    /// tính vận tốc của nhân vật khi di chuyển trên dốc
    /// khi di chuyển lên dốc vận tốc sẽ chậm hơn khi đi trên mặt bằng
    /// </summary>
    /// <param name="velocity">Velocity.</param>
    /// <param name="slopeAngle">Slope angle.</param>
    void ClimpSlope(ref Vector2 velocity, float slopeAngle, Vector2 slopeNormal)
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
            collision.slopeNormal = slopeNormal;
        }
    }

    /// <summary>
    /// Descends the slope.
    /// xử lý khi di chuyển xuống dốc
    /// góc xuống dốc
    /// </summary>
    /// <param name="velocity">Velocity.</param>
    void DescendSlope(ref Vector2 moveAmount)
    {
        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigin.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, collisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigin.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, collisionMask);

        if (maxSlopeHitLeft ^ maxSlopeHitRight)
        {
            SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
            SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);
        }

        if (!collision.slidingDownMaxSlope)
        {
            // hướng di chuyển 
            float directionX = Mathf.Sign(moveAmount.x);
            // gốc của tia
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigin.bottomLeft : raycastOrigin.bottomRight;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
                {
                    if (Mathf.Sign(hit.normal.x) == directionX)
                    {
                        if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x))
                        {
                            float moveDistance = Mathf.Abs(moveAmount.x);
                            float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

                            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                            moveAmount.y -= descendVelocityY;

                            collision.slopeAngle = slopeAngle;
                            collision.descendingSlope = false;
                            collision.below = true;
                        }
                    }
                }
            }
        }
    }

    void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount)
    {
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle > maxSlopeAngle)
            {
                moveAmount.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs(moveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

                collision.slopeAngle = slopeAngle;
                collision.slidingDownMaxSlope = true; 
                collision.slopeNormal = hit.normal;
            }
        }
    }

    void ResetFallingThroughPlatform()
    {
        collision.fallingThroughPlatform = false;
    }

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;
        public bool climbingSlope;
        public bool descendingSlope;
        public bool slidingDownMaxSlope;


        public float slopeAngle, slopeAngleOld;
        public Vector2 moveAmoutOld;
        public Vector2 slopeNormal;
        public float faceDir;
        public bool fallingThroughPlatform;

        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;
            slidingDownMaxSlope = false;

            slopeNormal = Vector2.zero;
            slopeAngle = slopeAngleOld;
            slopeAngleOld = 0;
        }
    }
}
