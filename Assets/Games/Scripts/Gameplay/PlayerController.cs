// using UnityEngine;
//
// public class PlayerController : MonoBehaviour
// {
//     private Vector2 moveInput { get; set; }
//     private bool isJumping { get; set; }
//     private bool isRunning { get; set; }
//     private bool isGrounded { get; set; }
//
//
//     public float velocity = 5f;
//     public float moveSpeed = 5f;
//     public float jumpForce = 18f;
//     public float jumpTime = 0.85f;
//     public float gravity = 9.8f;
//
//     private float jumpElapsedTime = 0;
//     private CharacterController characterController;
//     private Animator animator;
//     private InputSystem_Actions playerInputActions;
//
//     // public override void Spawned()
//     // {
//     //     GetPlayerComponents();
//     //     SetupPlayerInputAction();
//     // }
//
//     private void GetPlayerComponents()
//     {
//         animator = GetComponent<Animator>();
//         characterController = GetComponent<CharacterController>();
//     }
//
//     private void SetupPlayerInputAction()
//     {
//         playerInputActions = new InputSystem_Actions();
//
//         playerInputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
//         playerInputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
//
//         playerInputActions.Player.Sprint.performed += ctx => isRunning = true;
//         playerInputActions.Player.Sprint.canceled += ctx => isRunning = false;
//
//         playerInputActions.Player.Jump.performed += ctx => Jump();
//     }
//
//     private void OnEnable()
//     {
//         playerInputActions.Enable();
//     }
//
//     private void OnDisable()
//     {
//         playerInputActions.Disable();
//     }
//
//     public override void FixedUpdateNetwork()
//     {
//
//         CheckGrounded();
//         Move();
//
//
//         if (animator != null)
//         {
//             float minimumSpeed = 0.9f;
//             animator.SetBool("IsWalking", characterController.velocity.magnitude > minimumSpeed);
//             animator.SetBool("IsRunning", isRunning);
//             animator.SetBool("IsJumping", !isGrounded);
//         }
//     }
//
//     private void CheckGrounded()
//     {
//         float raycastDistance = 0.2f;
//         Vector3 raycastOrigin = transform.position + Vector3.down * 0.1f;
//         RaycastHit hit;
//
//         if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, raycastDistance))
//         {
//             isGrounded = true;
//         }
//         else
//         {
//             isGrounded = false;
//         }
//     }
//
//     private void Move()
//     {
//         float velocityAddition = 0;
//         float directionX = moveInput.x * (velocity + velocityAddition) * Runner.DeltaTime;
//         float directionZ = moveInput.y * (velocity + velocityAddition) * Runner.DeltaTime;
//         float directionY = 0;
//
//         if (isJumping)
//         {
//             directionY = Mathf.SmoothStep(jumpForce, jumpForce * 0.30f, jumpElapsedTime / jumpTime) * Runner.DeltaTime;
//
//             jumpElapsedTime += Runner.DeltaTime;
//             if (jumpElapsedTime >= jumpTime)
//             {
//                 isJumping = false;
//                 jumpElapsedTime = 0;
//             }
//         }
//
//         directionY = directionY - gravity * Runner.DeltaTime;
//
//         Vector3 forward = Camera.main.transform.forward;
//         Vector3 right = Camera.main.transform.right;
//
//         forward.y = 0;
//         right.y = 0;
//
//         forward.Normalize();
//         right.Normalize();
//
//         forward = forward * directionZ;
//         right = right * directionX;
//
//         if (directionX != 0 || directionZ != 0)
//         {
//             float angle = Mathf.Atan2(forward.x + right.x, forward.z + right.z) * Mathf.Rad2Deg;
//             Quaternion rotation = Quaternion.Euler(0, angle, 0);
//             transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
//         }
//
//         Vector3 verticalDirection = Vector3.up * directionY;
//         Vector3 horizontalDirection = forward + right;
//
//         Vector3 movement = verticalDirection + horizontalDirection;
//         characterController.Move(movement);
//     }
//
//     private void Jump()
//     {
//         if (isGrounded)
//         {
//             isJumping = true;
//         }
//     }
// }