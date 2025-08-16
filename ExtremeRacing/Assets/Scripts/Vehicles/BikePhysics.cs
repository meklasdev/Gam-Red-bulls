using UnityEngine;
using ExtremeRacing.Managers;

namespace ExtremeRacing.Vehicles
{
    [RequireComponent(typeof(Rigidbody))]
    public class BikePhysics : MonoBehaviour
    {
        [Header("Bike Configuration")]
        [SerializeField] private BikeType _bikeType = BikeType.Mountain;
        [SerializeField] private float _maxSpeed = 40f;
        [SerializeField] private float _acceleration = 8f;
        [SerializeField] private float _brakeForce = 15f;
        [SerializeField] private float _steerSensitivity = 2f;

        [Header("Balance System")]
        [SerializeField] private float _balanceForce = 500f;
        [SerializeField] private float _balanceSpeed = 3f;
        [SerializeField] private float _maxLeanAngle = 45f;
        [SerializeField] private bool _autoBalance = true;

        [Header("Stunt System")]
        [SerializeField] private float _jumpForce = 300f;
        [SerializeField] private float _flipTorque = 100f;
        [SerializeField] private bool _enableStunts = true;
        [SerializeField] private float _stuntPointMultiplier = 10f;

        [Header("Ground Detection")]
        [SerializeField] private LayerMask _groundMask = 1;
        [SerializeField] private float _groundCheckDistance = 1.5f;
        [SerializeField] private Transform _frontWheelCheck;
        [SerializeField] private Transform _rearWheelCheck;

        private Rigidbody _rigidbody;
        private bool _isGrounded;
        private bool _frontWheelGrounded;
        private bool _rearWheelGrounded;
        private float _currentBalance;
        private float _currentSpeed;
        private bool _isStunting;
        private int _currentStuntScore;
        private string _currentStunt = "";

        // Input
        private float _throttleInput;
        private float _steerInput;
        private float _brakeInput;
        private bool _jumpInput;
        private bool _stuntInput;

        // Events
        public System.Action<int> OnStuntPerformed;
        public System.Action OnCrash;

        public enum BikeType
        {
            Mountain,
            BMX,
            Road,
            Motocross
        }

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.centerOfMass = new Vector3(0, -0.3f, 0);
            
            // Ustaw właściwości na podstawie typu roweru
            ConfigureBikeType();
        }

        private void Update()
        {
            HandleInput();
            CheckGroundContact();
            UpdateBalance();
            UpdateStunts();
        }

        private void FixedUpdate()
        {
            ApplyMovement();
            ApplyBalance();
            ApplyGravity();
        }

        private void ConfigureBikeType()
        {
            switch (_bikeType)
            {
                case BikeType.Mountain:
                    _maxSpeed = 45f;
                    _acceleration = 8f;
                    _jumpForce = 250f;
                    break;
                case BikeType.BMX:
                    _maxSpeed = 30f;
                    _acceleration = 12f;
                    _jumpForce = 400f;
                    _enableStunts = true;
                    break;
                case BikeType.Road:
                    _maxSpeed = 60f;
                    _acceleration = 6f;
                    _jumpForce = 150f;
                    _enableStunts = false;
                    break;
                case BikeType.Motocross:
                    _maxSpeed = 80f;
                    _acceleration = 15f;
                    _jumpForce = 500f;
                    _flipTorque = 200f;
                    break;
            }
        }

        private void HandleInput()
        {
            if (InputManager.Instance != null)
            {
                _throttleInput = InputManager.Instance.Throttle;
                _steerInput = InputManager.Instance.Steer;
                _brakeInput = InputManager.Instance.Brake;
                _jumpInput = InputManager.Instance.Jump;
                _stuntInput = InputManager.Instance.Handbrake; // Używamy handbrake jako stunt button
            }
            else
            {
                // Fallback do Input.inputString
                _throttleInput = Input.GetAxis("Vertical");
                _steerInput = Input.GetAxis("Horizontal");
                _brakeInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;
                _jumpInput = Input.GetKeyDown(KeyCode.LeftShift);
                _stuntInput = Input.GetKey(KeyCode.LeftControl);
            }
        }

        private void CheckGroundContact()
        {
            // Sprawdź przednie koło
            _frontWheelGrounded = Physics.Raycast(_frontWheelCheck.position, Vector3.down, 
                _groundCheckDistance, _groundMask);
            
            // Sprawdź tylne koło
            _rearWheelGrounded = Physics.Raycast(_rearWheelCheck.position, Vector3.down, 
                _groundCheckDistance, _groundMask);
            
            _isGrounded = _frontWheelGrounded || _rearWheelGrounded;

            // Sprawdź czy to wheelie (tylko tylne koło na ziemi)
            bool isWheelie = _rearWheelGrounded && !_frontWheelGrounded;
            if (isWheelie && _currentSpeed > 5f)
            {
                StartStunt("Wheelie");
            }

            // Sprawdź czy to stoppie (tylko przednie koło na ziemi)
            bool isStoppie = _frontWheelGrounded && !_rearWheelGrounded && _brakeInput > 0.5f;
            if (isStoppie && _currentSpeed > 5f)
            {
                StartStunt("Stoppie");
            }
        }

        private void ApplyMovement()
        {
            if (!_isGrounded) return;

            _currentSpeed = _rigidbody.velocity.magnitude * 3.6f; // m/s to km/h

            // Forward/backward movement
            Vector3 forwardForce = transform.forward * _throttleInput * _acceleration * 100f;
            
            // Ogranicz prędkość maksymalną
            if (_currentSpeed < _maxSpeed)
            {
                _rigidbody.AddForce(forwardForce);
            }

            // Steering
            if (_currentSpeed > 1f) // Można skręcać tylko podczas ruchu
            {
                float steerTorque = _steerInput * _steerSensitivity * _currentSpeed * 0.5f;
                _rigidbody.AddTorque(0, steerTorque, 0);
            }

            // Braking
            if (_brakeInput > 0f)
            {
                Vector3 brakeForceVec = -_rigidbody.velocity.normalized * _brakeForce * _brakeInput;
                _rigidbody.AddForce(brakeForceVec);
            }

            // Jumping
            if (_jumpInput && _isGrounded)
            {
                PerformJump();
            }
        }

        private void ApplyBalance()
        {
            if (!_autoBalance && !_stuntInput) return;

            // Automatyczny balans gdy jedzie na ziemi
            if (_isGrounded && _currentSpeed > 2f)
            {
                Vector3 currentRotation = transform.eulerAngles;
                float zRotation = currentRotation.z;
                
                // Normalizuj do -180/180
                if (zRotation > 180f) zRotation -= 360f;

                // Zastosuj siłę balansującą
                float balanceTorque = -zRotation * _balanceForce * Time.fixedDeltaTime;
                _rigidbody.AddTorque(0, 0, balanceTorque);

                _currentBalance = Mathf.Abs(zRotation);
            }

            // Balans w powietrzu podczas stuntów
            if (!_isGrounded && _stuntInput)
            {
                float airBalanceTorque = _steerInput * _flipTorque * Time.fixedDeltaTime;
                _rigidbody.AddTorque(airBalanceTorque, 0, 0);
            }
        }

        private void ApplyGravity()
        {
            // Dodatkowa grawitacja dla lepszej kontroli
            if (!_isGrounded)
            {
                _rigidbody.AddForce(Vector3.down * 20f);
            }
        }

        private void UpdateBalance()
        {
            // Sprawdź czy się przewrócił
            float tiltAngle = Vector3.Angle(transform.up, Vector3.up);
            
            if (tiltAngle > 60f && _currentSpeed < 1f)
            {
                // Przewrócił się
                StartCoroutine(HandleCrash());
            }
        }

        private void PerformJump()
        {
            Vector3 jumpVector = Vector3.up * _jumpForce;
            
            // Dodaj forward momentum jeśli jedzie szybko
            if (_currentSpeed > 10f)
            {
                jumpVector += transform.forward * _jumpForce * 0.3f;
            }

            _rigidbody.AddForce(jumpVector);
            
            if (_enableStunts)
            {
                StartStunt("Jump");
            }

            Debug.Log($"[BikePhysics] Jump performed! Force: {_jumpForce}");
        }

        private void UpdateStunts()
        {
            if (!_enableStunts) return;

            // Sprawdź czy wykonuje backflip/frontflip w powietrzu
            if (!_isGrounded && _stuntInput)
            {
                float rotationSpeed = _rigidbody.angularVelocity.x;
                
                if (Mathf.Abs(rotationSpeed) > 2f)
                {
                    if (rotationSpeed > 0)
                        StartStunt("Backflip");
                    else
                        StartStunt("Frontflip");
                }
            }

            // Zakończ stunt gdy wyląduje
            if (_isGrounded && _isStunting)
            {
                CompleteStunt();
            }

            // Sprawdź czy jedzie na jednym kole (manual/wheelie)
            if (_isGrounded && _currentSpeed > 5f)
            {
                if (_rearWheelGrounded && !_frontWheelGrounded)
                {
                    ContinueStunt("Manual", 5);
                }
            }
        }

        private void StartStunt(string stuntName)
        {
            if (_currentStunt != stuntName)
            {
                _currentStunt = stuntName;
                _currentStuntScore = 0;
                _isStunting = true;
                
                Debug.Log($"[BikePhysics] Started stunt: {stuntName}");
            }
        }

        private void ContinueStunt(string stuntName, int pointsPerSecond)
        {
            if (_currentStunt == stuntName)
            {
                _currentStuntScore += Mathf.RoundToInt(pointsPerSecond * Time.deltaTime);
            }
            else
            {
                StartStunt(stuntName);
            }
        }

        private void CompleteStunt()
        {
            if (!_isStunting) return;

            int finalScore = Mathf.RoundToInt(_currentStuntScore * _stuntPointMultiplier);
            
            // Bonus za czyste lądowanie
            if (_currentBalance < 10f)
            {
                finalScore *= 2; // Double points za perfect landing
                Debug.Log($"[BikePhysics] PERFECT LANDING! Stunt: {_currentStunt} - Score: {finalScore}");
            }
            else
            {
                Debug.Log($"[BikePhysics] Completed stunt: {_currentStunt} - Score: {finalScore}");
            }

            OnStuntPerformed?.Invoke(finalScore);
            
            _isStunting = false;
            _currentStunt = "";
            _currentStuntScore = 0;
        }

        private System.Collections.IEnumerator HandleCrash()
        {
            Debug.Log("[BikePhysics] CRASH!");
            OnCrash?.Invoke();
            
            // Deactivate input na chwilę
            enabled = false;
            
            yield return new WaitForSeconds(2f);
            
            // Reset position i rotation
            transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            
            enabled = true;
        }

        // Public API
        public float GetSpeed() => _currentSpeed;
        public bool IsGrounded() => _isGrounded;
        public bool IsStunting() => _isStunting;
        public string GetCurrentStunt() => _currentStunt;
        public int GetStuntScore() => _currentStuntScore;
        public BikeType GetBikeType() => _bikeType;

        public void SetBikeType(BikeType newType)
        {
            _bikeType = newType;
            ConfigureBikeType();
        }

        public void ResetBike()
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
            _isStunting = false;
            _currentStunt = "";
            _currentStuntScore = 0;
        }

        // Debug
        private void OnDrawGizmosSelected()
        {
            if (_frontWheelCheck != null)
            {
                Gizmos.color = _frontWheelGrounded ? Color.green : Color.red;
                Gizmos.DrawRay(_frontWheelCheck.position, Vector3.down * _groundCheckDistance);
            }

            if (_rearWheelCheck != null)
            {
                Gizmos.color = _rearWheelGrounded ? Color.green : Color.red;
                Gizmos.DrawRay(_rearWheelCheck.position, Vector3.down * _groundCheckDistance);
            }

            // Pokaż center of mass
            Gizmos.color = Color.yellow;
            if (_rigidbody != null)
                Gizmos.DrawWireSphere(transform.TransformPoint(_rigidbody.centerOfMass), 0.1f);
        }
    }
}