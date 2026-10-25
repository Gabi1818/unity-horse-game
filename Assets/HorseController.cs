using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class HorseController : MonoBehaviour
{
    public Transform saddlePoint;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float acceleration = 2f;
    public float deceleration = 2f;

    private float currentSpeed = 0f;
    private bool isMounted = false;

    private float gravity = -9.81f;
    private Vector3 verticalVelocity = Vector3.zero;

    private Animator animator;
    private Transform rider;
    private CharacterController controller;


    [Header("Audio")]
    public AudioSource footstepSource;
    public AudioClip walkClip;
    public AudioClip runClip;

    private float stepTimer = 0f;
    private float stepInterval = 0.5f;

    private const float walkStepInterval = 1f; // make sure its the same length as your step clip 
    private const float runStepInterval = 0.5f;

    private const float idleToWalkThreshold = 3f;
    private const float walkToRunThreshold = 4f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!isMounted || rider == null)
            return;

        HandleMovement();
        UpdateAnimations();
    }

    private void HandleMovement()
    {
        float vertical = Input.GetAxis("Vertical");

        // Determine target speed
        float targetSpeed = 0f;
        if (vertical > 0.1f)
            targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // Accelerate / Decelerate
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed,
            (currentSpeed < targetSpeed ? acceleration : deceleration) * Time.deltaTime);

        // Turn
        float horizontal = Input.GetAxis("Horizontal");
        if (Mathf.Abs(horizontal) > 0.1f)
            transform.Rotate(Vector3.up * horizontal * 60f * Time.deltaTime);

        // Apply gravity
        if (controller.isGrounded)
            verticalVelocity.y = -1f; // Small push down to stay grounded
        else
            verticalVelocity.y += gravity * Time.deltaTime;

        // Final movement vector
        Vector3 move = transform.forward * currentSpeed + verticalVelocity;
        controller.Move(move * Time.deltaTime);
    }

    private void UpdateAnimations()
    {
        animator.SetFloat("Speed", currentSpeed);

        PlayHorseSounds();
    }

    private void PlayHorseSounds()
    {

        if (controller.isGrounded && currentSpeed > idleToWalkThreshold)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                // Set footstep interval based on walking or running
                stepInterval = currentSpeed > walkSpeed + walkToRunThreshold ? runStepInterval : walkStepInterval;

                // Play correct clip
                AudioClip clipToPlay = currentSpeed > walkSpeed + walkToRunThreshold ? runClip : walkClip;

                if (footstepSource && clipToPlay)
                    footstepSource.PlayOneShot(clipToPlay);

                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f; // Reset so we play immediately on next move
        }
    }

    /**
    public void Mount(Transform player)
    {
        if (isMounted)
            return;

        isMounted = true;
        rider = player;

        // Position the rider on the saddle
        rider.SetParent(saddlePoint);
        rider.localPosition = Vector3.zero;
        rider.localRotation = Quaternion.identity;

        // Disable player movement
        MovementManager.Instance.EnableMovement(false);
        MovementManager.Instance.EnableLook(false);
    }

    public void Dismount()
    {
        if (!isMounted || rider == null)
            return;

        // Detach and place nearby
        rider.SetParent(null);
        rider.position = transform.position - transform.right * 3f + Vector3.up * 1f;

        // Enable player movement
        MovementManager.Instance.EnableMovement(true);
        MovementManager.Instance.EnableLook(true);

        isMounted = false;
        rider = null;
        currentSpeed = 0f;
    }
    **/

    public void Mount(Transform player)
    {
        if (isMounted)
            return;

        if (player == null || saddlePoint == null)
        {
            Debug.LogError("HorseController.Mount: player or saddlePoint is not assigned.", this);
            return;
        }

        isMounted = true;
        rider = player;

        // Freeze the player BEFORE parenting
        var fpc = rider.GetComponent<FirstPersonController>();
        if (fpc) fpc.InputEnabled = false;

        var riderController = rider.GetComponent<CharacterController>();
        if (riderController) riderController.enabled = false;

        rider.SetParent(saddlePoint);
        rider.localPosition = Vector3.zero;
        rider.localRotation = Quaternion.identity;
    }

    public void Dismount()
    {
        if (!isMounted || rider == null)
            return;

        rider.SetParent(null);
        rider.position = transform.position - transform.right * 3f + Vector3.up * 1f;

        var riderController = rider.GetComponent<CharacterController>();
        if (riderController) riderController.enabled = true;

        var fpc = rider.GetComponent<FirstPersonController>();
        if (fpc) fpc.InputEnabled = true;

        isMounted = false;
        rider = null;
        currentSpeed = 0f;
    }
}
