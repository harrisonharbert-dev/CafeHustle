using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace PbwanskaasProductions.BasicThirdPersonController
{
    /// <summary>
    /// This class handles player input using Unity's Input System.
    /// It captures input for movement, looking, jumping, sprinting, and locking on targets.
    /// </summary>
    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;
        public bool locked;
        public bool dodge;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Called when the "Move" input action is performed.
        /// </summary>
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        /// <summary>
        /// Called when the "Look" input action is performed.
        /// </summary>
        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        /// <summary>
        /// Called when the "Jump" input action is performed.
        /// </summary>
        public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        /// <summary>
        /// Called when the "Sprint" input action is performed.
        /// </summary>
        public void OnSprint(InputValue value)
        {
            sprint = value.isPressed;
        }

        /// <summary>
        /// Called when the "Lock" input action is performed.
        /// </summary>
        public void OnLock(InputValue value)
        {
            locked = value.isPressed;
        }

        /// <summary>
        /// Called when the "Dodge" input action is performed.
        /// </summary>
        public void Dodge(InputValue value)
        {
            dodge = value.isPressed;
        }

#endif

        /// <summary>
        /// Sets the move input direction.
        /// </summary>
        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        /// <summary>
        /// Sets the look input direction.
        /// </summary>
        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        /// <summary>
        /// Sets the jump input state.
        /// </summary>
        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        /// <summary>
        /// Sets the sprint input state.
        /// </summary>
        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        /// <summary>
        /// Sets the lock input state.
        /// </summary>
        public void LockInput(bool newLockState)
        {
            locked = newLockState;
        }

        /// <summary>
        /// Sets the dodge input state.
        /// </summary>
        public void DodgeInput(bool newDodgeState)
        {
            dodge = newDodgeState;

        }


        /// <summary>
        /// Called when the application gains or loses focus.
        /// It sets the cursor lock state.
        /// </summary>
        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        /// <summary>
        /// Sets the cursor lock state.
        /// </summary>
        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}