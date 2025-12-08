using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem; // 新增：Input System 命名空间

namespace BKPureNature
{
    public class BK_FreeCamera : MonoBehaviour
    {
        public float movementSpeed = 10f;
        public float fastMovementSpeed = 100f;
        public float freeLookSensitivity = 3f;
        public float zoomSensitivity = 10f;
        public float fastZoomSensitivity = 50f;
        private bool looking = false;

        void Update()
        {
            // 检查键盘是否存在
            if (Keyboard.current == null) return;

            // 检查是否按下 Shift 键（快速移动模式）
            var fastMode = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
            var currentMovementSpeed = fastMode ? this.fastMovementSpeed : this.movementSpeed;

            // 左右移动 (A/D 或 左右箭头)
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                transform.position += -transform.right * currentMovementSpeed * Time.deltaTime;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                transform.position += transform.right * currentMovementSpeed * Time.deltaTime;
            }

            // 前后移动 (W/S 或 上下箭头)
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                transform.position += transform.forward * currentMovementSpeed * Time.deltaTime;
            }

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                transform.position += -transform.forward * currentMovementSpeed * Time.deltaTime;
            }

            // 上下移动 (Q/E - 相对于相机)
            if (Keyboard.current.qKey.isPressed)
            {
                transform.position += transform.up * currentMovementSpeed * Time.deltaTime;
            }

            if (Keyboard.current.eKey.isPressed)
            {
                transform.position += -transform.up * currentMovementSpeed * Time.deltaTime;
            }

            // 世界坐标上下移动 (R/F 或 PageUp/PageDown)
            if (Keyboard.current.rKey.isPressed || Keyboard.current.pageUpKey.isPressed)
            {
                transform.position += Vector3.up * currentMovementSpeed * Time.deltaTime;
            }

            if (Keyboard.current.fKey.isPressed || Keyboard.current.pageDownKey.isPressed)
            {
                transform.position += -Vector3.up * currentMovementSpeed * Time.deltaTime;
            }

            // 鼠标自由视角
            if (looking && Mouse.current != null)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                
                // 注意：新 Input System 的 delta 值通常比旧系统大，可能需要调整灵敏度
                // 如果旋转太快，可以在 Inspector 中降低 freeLookSensitivity 的值
                float sensitivity = freeLookSensitivity * 0.1f; // 调整系数，根据实际效果微调
                
                float newRotationX = transform.localEulerAngles.y + mouseDelta.x * sensitivity;
                float newRotationY = transform.localEulerAngles.x - mouseDelta.y * sensitivity;
                transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
            }

            // 鼠标滚轮缩放 FOV
            if (Mouse.current != null)
            {
                float scrollValue = Mouse.current.scroll.ReadValue().y;
                
                if (scrollValue > 0)
                {
                    GetComponent<Camera>().fieldOfView--;
                }
                else if (scrollValue < 0)
                {
                    GetComponent<Camera>().fieldOfView++;
                }
            }

            // 鼠标右键控制视角
            if (Mouse.current != null)
            {
                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    StartLooking();
                }
                else if (Mouse.current.rightButton.wasReleasedThisFrame)
                {
                    StopLooking();
                }
            }
        }

        void OnDisable()
        {
            StopLooking();
        }

        public void StartLooking()
        {
            looking = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void StopLooking()
        {
            looking = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}