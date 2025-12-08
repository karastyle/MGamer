// VirtualJoystick.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("摇杆UI组件")]
    [SerializeField] private RectTransform joystickBackground;
    [SerializeField] private RectTransform joystickHandle;
    
    [Header("摇杆限制")]
    [SerializeField] private float joystickRadius = 80f;
    
    [Header("相机控制")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float moveSpeed = 5f;
    
    private Canvas canvas;
    private bool isJoystickActive = false;
    private Vector2 inputDirection = Vector2.zero;
    
    public Vector2 InputDirection => inputDirection;
    public bool IsActive => isJoystickActive;
    
    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        
        Image targetImage = GetComponent<Image>();
        if (targetImage != null)
        {
            targetImage.raycastTarget = true;
        }
        
        if (joystickBackground != null)
            joystickBackground.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        if (isJoystickActive && cameraTransform != null)
        {
            MoveCamera();
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        ActivateJoystick(eventData.position);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (isJoystickActive)
        {
            UpdateJoystick(eventData.position);
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (isJoystickActive)
        {
            DeactivateJoystick();
        }
    }
    
    private void ActivateJoystick(Vector2 screenPosition)
    {
        isJoystickActive = true;
        
        if (joystickBackground != null)
        {
            joystickBackground.gameObject.SetActive(true);
            
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPosition,
                canvas.worldCamera,
                out localPoint
            );
            
            joystickBackground.localPosition = localPoint;
        }
        
        if (joystickHandle != null)
        {
            joystickHandle.localPosition = Vector2.zero;
        }
        
        inputDirection = Vector2.zero;
    }
    
    private void UpdateJoystick(Vector2 screenPosition)
    {
        if (joystickBackground == null || joystickHandle == null)
            return;
        
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPosition,
            canvas.worldCamera,
            out localPoint
        );
        
        Vector2 offset = localPoint - (Vector2)joystickBackground.localPosition;
        
        if (offset.magnitude > joystickRadius)
        {
            offset = offset.normalized * joystickRadius;
        }
        
        joystickHandle.localPosition = offset;
        inputDirection = offset / joystickRadius;
    }
    
    private void DeactivateJoystick()
    {
        isJoystickActive = false;
        inputDirection = Vector2.zero;
        
        if (joystickBackground != null)
        {
            joystickBackground.gameObject.SetActive(false);
        }
        
        if (joystickHandle != null)
        {
            joystickHandle.localPosition = Vector2.zero;
        }
    }
    
    private void MoveCamera()
    {
        if (inputDirection.magnitude < 0.01f)
            return;
        
        // 获取相机的前方和右方向
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        
        // 摇杆上(y>0) = 相机向前, 摇杆右(x>0) = 相机向右
        Vector3 moveDirection = (right * inputDirection.x + forward * inputDirection.y).normalized;
        
        cameraTransform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}