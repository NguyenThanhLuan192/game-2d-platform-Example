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
 * cong thuc tinh bien thien van toc v^2 = v0^2 + 2as
 * v : van toc
 * v0 : van toc ban dau
 * a : gia toc
 * s : quang duong
 * => minJumpVelocity = sqrt(2* gravity * minJumpHieght);
 * 
 */

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    public float maxJumpHeight = 4f;
    public float minJumpHeight = 1f;
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
    float maxJumpVelocity;
    float minJumpVelocity;

    float gravity;
    float velocityXSmoothing;

    Controller2D controller;
    Vector2 directionalInput;
    bool wallSliding;
    int wallDirX;


    void Start()
    {
        controller = GetComponent<Controller2D>();

        gravity = -2 * (maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;

        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);

    }

    void Update()
    {

        // wall dang cham vao ben trai cua player
        wallDirX = controller.collision.left ? -1 : 1;

        CalculatorVelocity();

        HandleWallSliding();

        controller.Move(velocity * Time.deltaTime, directionalInput);

        if (controller.collision.above || controller.collision.below)
        {
            if (controller.collision.slidingDownMaxSlope)
            {
                velocity.y += controller.collision.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0;
            }
        }
    }


    public void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }

    public void OnJumpInputDown()
    {
        // nhay khi dang truot tren mat 1 buc tuong thang dung
        if (wallSliding)
        {
            // nhay khi ma dang tac dung 1 luc huong vao mat dang bam vao tuong(nhay tren cung 1 buc tuong)
            if (wallDirX == directionalInput.x)
            {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            // nhay ma ko co tac dung luc trai hay phai
            else if (directionalInput.x == 0)
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
            if (controller.collision.slidingDownMaxSlope)
            {
                if (directionalInput.x != -Mathf.Sign(controller.collision.slopeNormal.x)) // not jump against max slope
                {
                    velocity.y = maxJumpVelocity * controller.collision.slopeNormal.y;
                    velocity.x = maxJumpVelocity * controller.collision.slopeNormal.x;
                }
            }
            else
            {
                velocity.y = maxJumpVelocity;
            }
        }
    }

    public void OnJumpInputUp()
    {
        // neu giua phim space cang lau thi cang nhay cao. neu thao tac nhanh thi nhay se thap
        if (velocity.y > minJumpVelocity)
        {
            velocity.y = minJumpVelocity;
        }
    }

  
    void CalculatorVelocity()
    {
        velocity.y += gravity * Time.deltaTime;
        float targetVelocityX = directionalInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, controller.collision.below ? accelerationTimeGrounded : accelerationTimeAirborne);
    }

    void HandleWallSliding()
    {
        wallSliding = false;
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

                if (directionalInput.x != wallDirX && directionalInput.x != 0)
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
    }
}
