// ================================
// File: Assets/Scripts/GAS/AbilitySystem/Data/AbilityInputData.cs
// ================================
using System;
using UnityEngine;

namespace GAS.AbilitySystem
{
    /// <summary>
    /// Contains input data for ability activation
    /// </summary>
    [Serializable]
    public struct AbilityInputData
    {
        /// <summary>
        /// Input ID that maps to abilities
        /// </summary>
        public int inputId;

        /// <summary>
        /// Was the input pressed this frame?
        /// </summary>
        public bool isPressed;

        /// <summary>
        /// Was the input released this frame?
        /// </summary>
        public bool isReleased;

        /// <summary>
        /// Is the input currently held?
        /// </summary>
        public bool isHeld;

        /// <summary>
        /// How long has the input been held?
        /// </summary>
        public float holdDuration;

        /// <summary>
        /// Screen position of the input (for mouse/touch)
        /// </summary>
        public Vector2 screenPosition;

        /// <summary>
        /// World position of the input
        /// </summary>
        public Vector3 worldPosition;

        /// <summary>
        /// Target object under the input (if any)
        /// </summary>
        public GameObject targetObject;

        public AbilityInputData(int inputId)
        {
            this.inputId = inputId;
            this.isPressed = false;
            this.isReleased = false;
            this.isHeld = false;
            this.holdDuration = 0;
            this.screenPosition = Vector2.zero;
            this.worldPosition = Vector3.zero;
            this.targetObject = null;
        }
    }

    /// <summary>
    /// Configuration for ability input bindings
    /// </summary>
    [Serializable]
    public class AbilityInputBinding
    {
        /// <summary>
        /// Unique ID for this input
        /// </summary>
        public int inputId;

        /// <summary>
        /// Display name for the input
        /// </summary>
        public string inputName;

        /// <summary>
        /// Key code for keyboard input
        /// </summary>
        public KeyCode keyCode;

        /// <summary>
        /// Alternative key code
        /// </summary>
        public KeyCode alternativeKey;

        /// <summary>
        /// Gamepad button mapping
        /// </summary>
        public string gamepadButton;

        /// <summary>
        /// Should this input be triggered on press or release?
        /// </summary>
        public bool triggerOnPress;

        /// <summary>
        /// Can this input be held for continuous activation?
        /// </summary>
        public bool allowHold;

        /// <summary>
        /// Minimum hold time before activation (for charge abilities)
        /// </summary>
        public float minHoldTime;

        public AbilityInputBinding(int inputId, string inputName, KeyCode keyCode)
        {
            this.inputId = inputId;
            this.inputName = inputName;
            this.keyCode = keyCode;
            this.alternativeKey = KeyCode.None;
            this.gamepadButton = "";
            this.triggerOnPress = true;
            this.allowHold = false;
            this.minHoldTime = 0;
        }
    }
}