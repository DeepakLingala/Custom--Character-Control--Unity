using UnityEngine;

namespace CharacterControl
{
    /// <summary>
    /// Reads and exposes player input each frame.
    /// Attach this component to the same GameObject as CustomCharacterController.
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        // ── Raw input values (read-only properties) ────────────────────────

        /// <summary>Normalized horizontal / vertical movement direction (world-space is applied by the controller).</summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>True for the single frame the jump button is pressed.</summary>
        public bool JumpPressed { get; private set; }

        /// <summary>True while the sprint button is held.</summary>
        public bool SprintHeld { get; private set; }

        /// <summary>True while the crouch button is held.</summary>
        public bool CrouchHeld { get; private set; }

        // ── Input axis / button name constants ────────────────────────────

        [Header("Input Settings")]
        [Tooltip("Name of the Horizontal input axis (Edit > Project Settings > Input Manager).")]
        [SerializeField] private string horizontalAxis = "Horizontal";

        [Tooltip("Name of the Vertical input axis.")]
        [SerializeField] private string verticalAxis = "Vertical";

        [Tooltip("KeyCode used for jumping.")]
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;

        [Tooltip("KeyCode used for sprinting.")]
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

        [Tooltip("KeyCode used for crouching.")]
        [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;

        // ──────────────────────────────────────────────────────────────────

        private void Update()
        {
            float h = Input.GetAxisRaw(horizontalAxis);
            float v = Input.GetAxisRaw(verticalAxis);

            // Normalize so diagonal movement is not faster than axial movement.
            MoveInput = new Vector2(h, v).normalized;

            JumpPressed  = Input.GetKeyDown(jumpKey);
            SprintHeld   = Input.GetKey(sprintKey);
            CrouchHeld   = Input.GetKey(crouchKey);
        }
    }
}
