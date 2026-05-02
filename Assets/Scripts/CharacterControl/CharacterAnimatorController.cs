using UnityEngine;

namespace CharacterControl
{
    /// <summary>
    /// Drives an Animator based on the state exposed by CustomCharacterController.
    /// 
    /// Expected Animator parameters (create these in the Animator window):
    ///   Float  "Speed"      — horizontal movement speed
    ///   Float  "VerticalVelocity" — vertical velocity (positive up)
    ///   Bool   "IsGrounded" — whether the character is on the ground
    ///   Bool   "IsCrouching"— whether the character is crouching
    ///   Bool   "IsSprinting"— whether the character is sprinting
    /// </summary>
    [RequireComponent(typeof(CustomCharacterController))]
    public class CharacterAnimatorController : MonoBehaviour
    {
        [Header("Animator Reference")]
        [Tooltip("Animator component to drive. Leave empty to auto-detect on this GameObject.")]
        [SerializeField] private Animator animator;

        [Header("Animator Parameter Names")]
        [SerializeField] private string speedParam           = "Speed";
        [SerializeField] private string verticalVelocityParam= "VerticalVelocity";
        [SerializeField] private string isGroundedParam      = "IsGrounded";
        [SerializeField] private string isCrouchingParam     = "IsCrouching";
        [SerializeField] private string isSprintingParam     = "IsSprinting";

        [Tooltip("Smoothing applied to the Speed animator parameter (lower = smoother).")]
        [SerializeField] private float speedDampTime = 0.1f;

        // ── Cached IDs ────────────────────────────────────────────────────

        private int _speedId;
        private int _verticalVelocityId;
        private int _isGroundedId;
        private int _isCrouchingId;
        private int _isSprintingId;

        private CustomCharacterController _controller;

        // ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            _controller = GetComponent<CustomCharacterController>();

            if (animator == null)
                animator = GetComponent<Animator>();

            // Cache parameter hashes for performance.
            _speedId            = Animator.StringToHash(speedParam);
            _verticalVelocityId = Animator.StringToHash(verticalVelocityParam);
            _isGroundedId       = Animator.StringToHash(isGroundedParam);
            _isCrouchingId      = Animator.StringToHash(isCrouchingParam);
            _isSprintingId      = Animator.StringToHash(isSprintingParam);
        }

        private void Update()
        {
            if (animator == null) return;

            animator.SetFloat(_speedId,            _controller.HorizontalSpeed,
                              speedDampTime, Time.deltaTime);
            animator.SetFloat(_verticalVelocityId, _controller.VerticalVelocity);
            animator.SetBool(_isGroundedId,        _controller.IsGrounded);
            animator.SetBool(_isCrouchingId,       _controller.IsCrouching);
            animator.SetBool(_isSprintingId,       _controller.IsSprinting);
        }
    }
}
