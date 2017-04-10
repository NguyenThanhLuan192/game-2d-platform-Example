using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * cong thuc tinh quang duong S = So + v*t + a*(t^2)/2
 * cong thức tính vận tốc V = Vo + a*t
 * trong đó s là quãng đường
 * v là vận tốc
 * a là gia tốc
 * t thời gian
 */

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    public float jumpHeight = 4f;
    public float timeToJumpApex = 0.4f;
    float accelerationTimeAirborne = 0.2f;
    float accelerationTimeGrounded = 0.1f;
    float moveSpeed = 6;

    // nhay khi dang truot tren tuong
    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;

    public float wallSlideSpeedMax = 3;
    public float wallStickTime = 0.25f;
    float timeToWallUnstick;

    Vector3 velocity;
    float jumpVelocity;
    float gravity;
    float velocityXSmoothing;

    Controller2D controller;

    void Start()
    {
        controller = GetComponent<Controller2D>();

        gravity = -2 * (jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
    }

    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        // wall dang cham vao ben trai cua player
        int wallDirX = controller.collision.left ? -1 : 1;

        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, controller.collision.below ? accelerationTimeGrounded : accelerationTimeAirborne);

        bool wallSliding = false;
        if ((controller.collision.right || controller.collision.left) && !controller.collision.below && velocity.y < 0)
        {
            wallSliding = true;
            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }
            // thoi gian delay khi nhay ra khoi tuong
            if (timeToWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if (input.x != wallDirX && input.x != 0)
                {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }
        }


        if (controller.collision.above || controller.collision.below)
            velocity.y = 0;

        // khi click keycode space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // nhay khi dang truot tren mat 1 buc tuong thang dung
            if (wallSliding)
            {
                // nhay khi ma dang tac dung 1 luc huong vao mat dang bam vao tuong(nhay tren cung 1 buc tuong)
                if (wallDirX == input.x)
                {
                    velocity.x = -wallDirX * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }
                // nhay ma ko co tac dung luc trai hay phai
                else if (input.x == 0)
                {   
                    velocity.x = -wallDirX * wallJumpOff.x;
                    velocity.y = wallJumpOff.y;
                }
                // nhay ma ra khoi mat dang bam vao tuong(nhay ra khoi buc)
                else
                {
                    velocity.x = -wallDirX * wallLeap.x;   
                    velocity.y = wallLeap.y;
                }

            }
            // neu player dang dung tren 1 mat phang hoac mat nghieng
            if (controller.collision.below)
            {
                velocity.y = jumpVelocity;
            }
        }

     
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }


}
