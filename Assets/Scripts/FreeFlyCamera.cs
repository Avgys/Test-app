using UnityEngine;
using Cursor = UnityEngine.Cursor;

[RequireComponent(typeof(Camera))]
public class FreeFlyCamera : MonoBehaviour
{
    #region UI

    [SerializeField] [Tooltip("The script is currently active")]
    private bool _active = true;

    private bool _drag = false;

    [SerializeField] [Tooltip("Camera rotation by mouse movement is active")]
    private bool _enableRotation = true;

    [SerializeField] [Tooltip("Sensitivity of mouse rotation")]
    private float _mouseSense = 1.8f;

    [SerializeField] [Tooltip("Camera zooming in/out by 'Mouse Scroll Wheel' is active")]
    private bool _enableTranslation = true;

    [SerializeField] [Tooltip("Velocity of camera zooming in/out")]
    private float _translationSpeed = 55f;

    [Space] [SerializeField] private bool _enableMovement = true;

    [SerializeField] private float _movementSpeed = 10f;

    [SerializeField] [Tooltip("Speed of the quick camera movement when holding the 'Left Shift' key")]
    private float _boostedSpeed = 50f;

    [SerializeField] private KeyCode _boostSpeed = KeyCode.LeftShift;

    [SerializeField] private KeyCode _moveUp = KeyCode.E;
    [SerializeField] private KeyCode _moveDown = KeyCode.Q;

    [SerializeField] private KeyCode _moveForward = KeyCode.W;

    [SerializeField] private KeyCode _moveBackward = KeyCode.S;
    [SerializeField] private KeyCode _moveLeft = KeyCode.A;
    [SerializeField] private KeyCode _moveRight = KeyCode.D;

    [Space] [SerializeField] [Tooltip("Acceleration at camera movement is active")]
    private bool _enableSpeedAcceleration = true;

    [SerializeField] [Tooltip("Rate which is applied during camera movement")]
    private float _speedAccelerationFactor = 1.5f;

    [Space] [SerializeField] [Tooltip("This keypress will move the camera to initialization position")]
    private KeyCode _initPositonButton = KeyCode.R;

    [SerializeField] [Tooltip("This keypress will change camera mode")]
    private KeyCode _cameraModeButton = KeyCode.F;

    #endregion UI

    private CursorLockMode _wantedMode;

    private float _currentIncrease = 1;
    private float _currentIncreaseMem = 0;

    private Vector3 _initPosition;
    private Vector3 _initRotation;

    private Vector3 _startPosition;
    private Vector3 _difference;
    private Camera _cameraMain;
    private int _floorLayer;
    private bool _rotate;

    public bool FlyMode { get; private set; }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_boostedSpeed < _movementSpeed)
            _boostedSpeed = _movementSpeed;
    }
#endif


    private void Start()
    {
        _initPosition = transform.position;
        _initRotation = transform.eulerAngles;
        _cameraMain = GetComponent<Camera>();
        _floorLayer = LayerMask.NameToLayer("Floor");
    }

    private void OnEnable()
    {
        // if (_active)
        //     _wantedMode = CursorLockMode.Locked;
    }

    // Apply requested cursor state
    public void ChangeCameraMode()
    {
        _wantedMode = _wantedMode == CursorLockMode.None
            ? CursorLockMode.Locked
            : CursorLockMode.None;
        FlyMode = CursorLockMode.Locked == _wantedMode;
        // Apply cursor state
        Cursor.lockState = _wantedMode;
        // Hide cursor when locking
        Cursor.visible = !FlyMode;
        
    }

    private void CalculateCurrentIncrease(bool moving)
    {
        _currentIncrease = Time.deltaTime;

        if (!_enableSpeedAcceleration || _enableSpeedAcceleration && !moving)
        {
            _currentIncreaseMem = 0;
            return;
        }

        _currentIncreaseMem += Time.deltaTime * (_speedAccelerationFactor - 1);
        _currentIncrease = Time.deltaTime * (1 + Mathf.Pow(_currentIncreaseMem, 3));
    }

    private void Update()
    {
        // var overMainZone = MousePosition.TryGet(_cameraMain, _floorLayer, out _);
        // if (!overMainZone) return;

        DragCamera();

        // Scroll movement 
        if (_enableTranslation)
        {
            transform.Translate(Vector3.forward * Input.mouseScrollDelta.y * Time.deltaTime * _translationSpeed);
        }

        // Return to init position
        if (Input.GetKeyDown(_initPositonButton))
        {
            transform.position = _initPosition;
            transform.eulerAngles = _initRotation;
        }

        // Return to init position
        if (Input.GetKeyDown(_cameraModeButton))
        {
           ChangeCameraMode();
        }
       
        RotateCamera();

        if (!_active || Cursor.visible)
            return;

        // Movement
        if (_enableMovement)
        {
            MoveCamera();
        }

        // Rotation
        
    }

    private void DragCamera()
    {
        if (Input.GetMouseButtonDown(2))
        {
            _drag = true;
            _startPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(2))
        {
            _drag = false;
            _startPosition = Vector3.zero;
        }

        if (_drag)
        {
            var difference = _translationSpeed * (Input.mousePosition - _startPosition).normalized;

            var pos = -Vector3.forward * difference.y + -Vector3.right * difference.x;

            transform.position += pos * Time.deltaTime * transform.position.y / 10;

            _startPosition = Input.mousePosition;
        }
    }

    private void MoveCamera()
    {
        Vector3 deltaPosition = Vector3.zero;
        float currentSpeed = _movementSpeed;

        if (Input.GetKey(_boostSpeed))
            currentSpeed = _boostedSpeed;

        if (Input.GetKey(_moveForward))
            deltaPosition += transform.forward;

        if (Input.GetKey(_moveBackward))
            deltaPosition -= transform.forward;

        if (Input.GetKey(_moveLeft))
            deltaPosition -= transform.right;

        if (Input.GetKey(_moveRight))
            deltaPosition += transform.right;

        if (Input.GetKey(_moveUp))
            deltaPosition += transform.up;

        if (Input.GetKey(_moveDown))
            deltaPosition -= transform.up;

        // Calc acceleration
        CalculateCurrentIncrease(deltaPosition != Vector3.zero);

        transform.position += deltaPosition * currentSpeed * _currentIncrease;
    }

    private void RotateCamera()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _rotate = true;
        }

        if (Input.GetMouseButtonUp(1))
        {
            _rotate = false;
        }

        if (!_enableRotation || (!FlyMode && !_rotate)) return;
        
        // Pitch
        transform.rotation *= Quaternion.AngleAxis(
            -Input.GetAxis("Mouse Y") * _mouseSense,
            Vector3.right
        );

        // Paw
        transform.rotation = Quaternion.Euler(
            transform.eulerAngles.x,
            transform.eulerAngles.y + Input.GetAxis("Mouse X") * _mouseSense,
            transform.eulerAngles.z
        );
    }
}