// CameraRotationController.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CameraRotationController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("相机配置")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform cameraPivot;
    
    [Header("旋转配置")]
    [SerializeField] private float rotationSpeed = 0.5f;
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;
    
    private bool isRotating = false;
    private Vector2 lastPointerPosition;
    
    private float currentYaw = 0f;
    private float currentPitch = 0f;
    
    private void Awake()
    {
        Image targetImage = GetComponent<Image>();
        if (targetImage != null)
        {
            targetImage.raycastTarget = true;
        }
        
        if (cameraTransform != null)
        {
            Vector3 angles = cameraTransform.eulerAngles;
            currentYaw = angles.y;
            currentPitch = angles.x;
            
            if (currentPitch > 180f)
                currentPitch -= 360f;
        }
    }
    
    private void Update()
    {
        if (isRotating)
        {
            ApplyCameraRotation();
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        StartRotation(eventData.position);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (isRotating)
        {
            UpdateRotation(eventData.position);
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (isRotating)
        {
            StopRotation();
        }
    }
    
    private void StartRotation(Vector2 screenPosition)
    {
        isRotating = true;
        lastPointerPosition = screenPosition;
    }
    
    private void UpdateRotation(Vector2 screenPosition)
    {
        Vector2 delta = screenPosition - lastPointerPosition;
        
        currentYaw += delta.x * rotationSpeed;
        currentPitch -= delta.y * rotationSpeed;
        
        currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);
        
        lastPointerPosition = screenPosition;
    }
    
    private void StopRotation()
    {
        isRotating = false;
    }
    
    private void ApplyCameraRotation()
    {
        if (cameraTransform == null)
            return;
        
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        
        if (cameraPivot != null)
        {
            cameraPivot.rotation = rotation;
        }
        else
        {
            cameraTransform.rotation = rotation;
        }
    }
}