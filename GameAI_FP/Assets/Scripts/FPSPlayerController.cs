using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PlayerInput))]
public class FPSPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 10f;
    float originalSpeed;
    public float jumpHeight = 0.4f;
    public float gravity = 9.81f;
    public float airControl = 10f;

    [Header("Sprint Settings")]
    public Slider staminaSlider;
    public int sprintSpeed = 15;
    public float stamina = 100f;
    public float maxStamina = 100f;
    public float minimumSprintStamina = 20f;
    public float sprintStaminaCost = 40;
    public float sprintStaminaRegen = 20;
    bool isSprinting = false;

    [Header("Audio Settings")]
    private AudioSource SFXaudioSource;
    [SerializeField] private AudioClip sprintSFX;
    [SerializeField] private AudioClip walkSFX;
    [Range(0f, 1f)] [SerializeField] private float walkSFXVolume;
    [Range(0f, 1f)] [SerializeField] private float sprintSFXVolume;

    Vector3 input;
    Vector3 moveDirection;
    CharacterController controller;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool sprintHeld;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        SFXaudioSource = GetComponent<AudioSource>();
        SFXaudioSource.playOnAwake = false;
        SFXaudioSource.loop = true;
        SFXaudioSource.clip = walkSFX;

        originalSpeed = speed;

        if (stamina <= 0f)
        {
            Debug.LogWarning("Stamina is set to invalid value. Setting to default value of 100.");
            stamina = 100f;
        }

        maxStamina = stamina;
        if (maxStamina <= 0f)
        {
            Debug.LogWarning("Max stamina is set to invalid value. Setting to default value of 100.");
            maxStamina = 100f;
        }

        if (!sprintSFX)
        {
            Debug.LogWarning("Sprint SFX not assigned");
            return;
        }

        UpdateSlider();
    }
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        jumpPressed = value.isPressed;
    }

    public void OnSprint(InputValue value)
    {
        sprintHeld = value.isPressed;
    }

    void Update()
    {
        input = transform.right * moveInput.x + transform.forward * moveInput.y;
        input.Normalize();

        if (controller.isGrounded)
        {
            moveDirection = input;

            if (jumpPressed)
            {
                moveDirection.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
            }
            else
            {
                moveDirection.y = 0f;
            }

            if (sprintHeld && stamina > minimumSprintStamina && !isSprinting)
            {
                StartSprint();
            }
            else if (!sprintHeld && isSprinting)
            {
                StopSprint();
            }
        }
        else
        {
            input.y = moveDirection.y;
            moveDirection = Vector3.Lerp(moveDirection, input, airControl * Time.deltaTime);
        }

        if (isSprinting)
        {
            Debug.Log("Sprinting with stamina: " + stamina);
            ReduceStamina();
            if (stamina <= 0)
            {
                StopSprint();
            }
        }
        else
        {
            RegenerateStamina();
        }

        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(speed * Time.deltaTime * moveDirection);

        if (input.magnitude > 0 && !SFXaudioSource.isPlaying)
        {
            SFXaudioSource.clip = walkSFX;
            SFXaudioSource.volume = walkSFXVolume;
            SFXaudioSource.Play();
        }
        else if (input.magnitude == 0 && SFXaudioSource.isPlaying)
        {
            SFXaudioSource.Stop();
        }
    }

    public void ApplyBounce(float bounceForce)
    {
        moveDirection.y = bounceForce;
        controller.Move(speed * Time.deltaTime * moveDirection);
    }

    void StartSprint()
    {
        if (isSprinting) return;

        Debug.Log("Sprinting started");
        isSprinting = true;
        speed = sprintSpeed;

        if (SFXaudioSource.isPlaying) SFXaudioSource.Stop();
        if (!SFXaudioSource.isPlaying)
        {
            SFXaudioSource.clip = sprintSFX;
            SFXaudioSource.volume = sprintSFXVolume;
            SFXaudioSource.Play();
        }
    }

    void StopSprint()
    {
        if (!isSprinting) return;

        Debug.Log("Sprinting stopped");
        isSprinting = false;
        speed = originalSpeed;

        if (SFXaudioSource.isPlaying) SFXaudioSource.Stop();
        if (!SFXaudioSource.isPlaying)
        {
            SFXaudioSource.clip = walkSFX;
            SFXaudioSource.volume = walkSFXVolume;
            SFXaudioSource.Play();
        }
    }

    void RegenerateStamina()
    {
        if (stamina < maxStamina)
        {
            stamina += sprintStaminaRegen * Time.deltaTime;
            if (stamina > maxStamina) stamina = maxStamina;
        }
        UpdateSlider();
    }

    void ReduceStamina()
    {
        if (stamina > 0f)
        {
            stamina -= sprintStaminaCost * Time.deltaTime;
            Debug.Log("Stamina: " + stamina);
            if (stamina <= 0f) stamina = 0f;
        }
        UpdateSlider();
    }

    void UpdateSlider()
    {
        if (staminaSlider)
            staminaSlider.value = stamina;
    }
}