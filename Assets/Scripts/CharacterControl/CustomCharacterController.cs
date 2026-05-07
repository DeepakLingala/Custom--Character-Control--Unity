using UnityEngine;

namespace CharacterControl
{
    /// <summary>
    /// Custom character controller built on top of Unity's CharacterController component.
    /// Supports walking, sprinting, crouching, jumping, and simulated gravity.
    ///
    /// Required components on the same GameObject:
    ///   - UnityEngine.CharacterController
    ///   - PlayerInputHandler
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public class CustomCharacterController : MonoBehaviour
    {
        // ── Movement ──────────────────────────────────────────────────────

        [Header("Movement")]
        [Tooltip("Walking speed in metres per second.")]
        [SerializeField] private float walkSpeed = 5f;

        [Tooltip("Sprinting speed in metres per second.")]
        [SerializeField] private float sprintSpeed = 10f;

        [Tooltip("Crouching speed in metres per second.")]
        [SerializeField] private float crouchSpeed = 2.5f;

        [Tooltip("How quickly the character accelerates / decelerates on the ground (higher = snappier).")]
        [SerializeField] private float groundAcceleration = 15f;

        // ── Jumping ───────────────────────────────────────────────────────

        [Header("Jumping")]
        [Tooltip("Initial vertical velocity when jumping.")]
        [SerializeField] private float jumpForce = 7f;

        [Tooltip("Number of extra jumps allowed (0 = single jump, 1 = double jump, etc.).")]
        [SerializeField] private int extraJumps = 0;

        // ── Gravity ───────────────────────────────────────────────────────

        [Header("Gravity")]
        [Tooltip("Custom gravity magnitude (positive value; applied downward).")]
        [SerializeField] private float gravity = 20f;

        [Tooltip("Downward velocity applied when grounded so the character stays on slopes.")]
        [SerializeField] private float groundedGravity = 2f;

        // ── Crouching ─────────────────────────────────────────────────────

        [Header("Crouching")]
        [Tooltip("CharacterController height while standing.")]
        [SerializeField] private float standHeight = 1.8f;

        [Tooltip("CharacterController height while crouching.")]
        [SerializeField] private float crouchHeight = 0.9f;

        [Tooltip("How fast the height changes when crouching / standing up.")]
        [SerializeField] private float crouchTransitionSpeed = 8f;

        // ── Ground Check ──────────────────────────────────────────────────

        [Header("Ground Check")]
        [Tooltip("LayerMask that defines which layers are considered 'ground'.")]
        [SerializeField] private LayerMask groundMask = ~0;

        [Tooltip("Radius of the sphere used for the ground check.")]
        [SerializeField] private float groundCheckRadius = 0.25f;

        [Tooltip("Downward offset from the character's origin for the ground-check sphere.")]
        [SerializeField] private float groundCheckOffset = 0.05f;

        // ── Public state (read by CharacterAnimatorController) ────────────

        /// <summary>True when the character is standing on ground.</summary>
        public bool IsGrounded { get; private set; }

        /// <summary>True while the character is crouching.</summary>
        public bool IsCrouching { get; private set; }

        /// <summary>True while the character is sprinting.</summary>
        public bool IsSprinting { get; private set; }

        /// <summary>Horizontal speed this frame (useful for blend trees).</summary>
        public float HorizontalSpeed { get; private set; }

        /// <summary>Current vertical velocity (positive = up).</summary>
        public float VerticalVelocity => _verticalVelocity;

        // ── Private fields ────────────────────────────────────────────────

        private CharacterController _cc;
        private PlayerInputHandler  _input;

        private Vector3 _currentVelocity;   // smoothed horizontal velocity
        private float   _verticalVelocity;  // vertical velocity (gravity + jump)
        private int     _jumpsRemaining;
        private float   _targetHeight;

        // ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            _cc    = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInputHandler>();

            _cc.height = standHeight;
            _targetHeight = standHeight;
        }

        private void Update()
        {
            PerformGroundCheck();
            HandleCrouch();
            HandleGravityAndJump();
            HandleHorizontalMovement();
            ApplyMovement();
        }

        // ── Ground check ─────────────────────────────────────────────────

        private void PerformGroundCheck()
        {
            Vector3 sphereCenter = transform.position + Vector3.down *
                (_cc.height * 0.5f - _cc.radius + groundCheckOffset);

            IsGrounded = Physics.CheckSphere(sphereCenter, groundCheckRadius, groundMask,
                QueryTriggerInteraction.Ignore);

            if (IsGrounded)
                _jumpsRemaining = extraJumps;
        }

        // ── Crouch ────────────────────────────────────────────────────────

        private void HandleCrouch()
        {
            bool wantsCrouch = _input.CrouchHeld;

            // Cannot stand up if something is overhead.
            if (!wantsCrouch && IsCrouching)
            {
                Vector3 top = transform.position + Vector3.up * (standHeight - _cc.radius);
                bool blocked = Physics.CheckSphere(top, _cc.radius, groundMask,
                    QueryTriggerInteraction.Ignore);
                if (blocked)
                    wantsCrouch = true;
            }

            IsCrouching   = wantsCrouch;
            _targetHeight = IsCrouching ? crouchHeight : standHeight;

            // Smoothly adjust the CharacterController height.
            float newHeight = Mathf.Lerp(_cc.height, _targetHeight,
                Time.deltaTime * crouchTransitionSpeed);

            // Reposition the controller so the feet stay on the ground.
            float delta = newHeight - _cc.height;
            _cc.height = newHeight;
            _cc.center = new Vector3(0f, newHeight * 0.5f, 0f);

            // Move the transform to compensate for the height change.
            if (!IsGrounded && delta < 0f)
                transform.position += Vector3.up * (delta * 0.5f);
        }

        // ── Gravity and jumping ───────────────────────────────────────────

        private void HandleGravityAndJump()
        {
            if (IsGrounded && _verticalVelocity < 0f)
            {
                // Keep a small downward velocity so the character stays grounded on slopes.
                _verticalVelocity = -groundedGravity;
            }

            // Jump
            if (_input.JumpPressed)
            {
                if (IsGrounded || _jumpsRemaining > 0)
                {
                    _verticalVelocity = jumpForce;

                    if (!IsGrounded)
                        _jumpsRemaining--;
                }
            }

            // Apply gravity every frame.
            _verticalVelocity -= gravity * Time.deltaTime;
        }

        // ── Horizontal movement ───────────────────────────────────────────

        private void HandleHorizontalMovement()
        {
            // Sprint only while standing (not crouching) and on the ground.
            IsSprinting = _input.SprintHeld && !IsCrouching && IsGrounded;

            float targetSpeed = IsCrouching  ? crouchSpeed
                              : IsSprinting  ? sprintSpeed
                              : walkSpeed;

            // Convert 2-D input to 3-D world-space relative to the character's facing direction.
            Vector3 inputDir = transform.right   * _input.MoveInput.x
                             + transform.forward * _input.MoveInput.y;

            Vector3 targetVelocity = inputDir * targetSpeed;

            // Smooth the horizontal velocity.
            _currentVelocity = Vector3.Lerp(_currentVelocity, targetVelocity,
                Time.deltaTime * groundAcceleration);

            HorizontalSpeed = _currentVelocity.magnitude;
        }

        // ── Final move ────────────────────────────────────────────────────

        private void ApplyMovement()
        {
            Vector3 motion = _currentVelocity + Vector3.up * _verticalVelocity;
            _cc.Move(motion * Time.deltaTime);
        }

        // ── Gizmos ───────────────────────────────────────────────────────

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            CharacterController cc = GetComponent<CharacterController>();
            if (cc == null) return;

            Vector3 sphereCenter = transform.position + Vector3.down *
                (cc.height * 0.5f - cc.radius + groundCheckOffset);

            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(sphereCenter, groundCheckRadius);
        }
#endif
    }
}
