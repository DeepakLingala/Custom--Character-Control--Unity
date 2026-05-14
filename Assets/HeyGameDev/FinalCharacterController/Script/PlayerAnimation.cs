using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeyGameDev.FinalCharacterController {
    
    public class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private float locomotionBlendSpeed = 4f; // Increased for better response

        private PlayerLocomotionInput _playerLocomotionInput;
        private PlayerState _playerState;

        private static int inputXHash = Animator.StringToHash("inputX");
        private static int inputYHash = Animator.StringToHash("inputY");
        private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");

        private Vector3 _currentBlendInput = Vector3.zero;

        private void Awake()
        {
            _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            _playerState = GetComponent<PlayerState>();
        }

        private void Update()
        {
            UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            // FIX 1: Changed '=' to '==' to compare the state
            // Also added 'PlayerState.' prefix to find the Enum correctly
            bool isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;

            Vector2 inputTarget = isSprinting ? _playerLocomotionInput.MovementInput * 1.5f : _playerLocomotionInput.MovementInput;
            
            // FIX 2: Added Time.deltaTime to keep the smoothing consistent regardless of frame rate
            _currentBlendInput = Vector3.Lerp(_currentBlendInput, (Vector3)inputTarget, locomotionBlendSpeed * Time.deltaTime);

            // FIX 3: Defined 'inputMagnitude' by calculating it from our smoothed input
            float inputMagnitude = _currentBlendInput.magnitude;

            // Apply values to the Animator
            _animator.SetFloat(inputXHash, _currentBlendInput.x);
            _animator.SetFloat(inputYHash, _currentBlendInput.y);
            _animator.SetFloat(inputMagnitudeHash, inputMagnitude);
        }
    }
}