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
        print("gravity: " + gravity + " jumpVelocity: " + jumpVelocity);
    }

    void Update()
    {
        if (controller.collision.above || controller.collision.below)
            velocity.y = 0;

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space) && controller.collision.below)
            velocity.y = jumpVelocity;

        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, controller.collision.below ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    
}
