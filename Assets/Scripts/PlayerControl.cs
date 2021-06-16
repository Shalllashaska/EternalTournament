using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Variables

     //Spawn
     
        public Transform spawn;
        public Transform weapon;
        public Transform playerBody;
        public Transform cameraTransform;
        public Transform raycastTransform;
        public LayerMask forRaycast;
        public Camera playerCamera;
        public GameObject flashLight;
        public GameObject flashLightCorp;
        public GameObject flashLightLight;
        public GameObject cameraParent;
        public GameObject pauseMenu;
        public TextMeshPro nicknameText;
        public AudioSource jtsfx;

        public Renderer[] teamIndicators;

        private Text teamUI;
        
        //public player vars
        public float groundDrag = 6f;
        public float airDrag = 0f;
        public float walkSpeed = 10;
        public float spaceSpeed = 10;
        public float angleSpeedRotation = 1f;
        public float speedRotation = 40f;
        public float speedStop = 40f;
        public float airMultiplier = 0.4f;
        public float jumpForce = 220;
        public float maxFuel = 200f;
        public float jetRecovery = 2f;
        public float jetWait = 0.5f; 
        public Transform fulebar;
        public float jatForce = 500;
        public float sprintSpeed = 20f;
        public float crouchSpeed = 5f;

        [HideInInspector] public bool awayTeam;
        
        // public vars
        public float mouseSensitivityX = 500;
        public float mouseSensitivityY = 500;

        public LayerMask groundedMask;
    	public float rotationSpeed = 1;
    
        // System vars
        private Vector3 originVector = new Vector3(0, 1, 0);
        private Vector3 sendGunD;
        private Vector3 recieveGunD;
        private Vector3 moveAmount;
        private Vector3 smoothMoveVelocity;
        private float verticalLookRotation;
        private Rigidbody rb;
        private GravityBody pl;
        private int currentHealth;
        private float curentFuel;
        private float currentRecovery;

        //crouch
        private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
        private Vector3 playerScale;
        private Vector3 weaponScale;
    
        //input 
        private KeyCode crouchKey = KeyCode.LeftControl;
        private KeyCode sprintKey = KeyCode.LeftShift;
        private KeyCode jumpKey = KeyCode.Space;
        private KeyCode spaceUpKey;
        private KeyCode spaceDownKey;
        private KeyCode spaceForwardKey = KeyCode.W;
        private KeyCode spaceBackwardKey = KeyCode.S;
        private KeyCode spaceLeft = KeyCode.A;
        private KeyCode spaceRight = KeyCode.D;
        private KeyCode spaceRightRotate = KeyCode.E;
        private KeyCode spaceLeftRotate = KeyCode.Q;
        private KeyCode spaceStopVelocity = KeyCode.C;
        private float inputX, inputY, cam;
        
        //Headbob
        private float movementCounter;
        private float idleCounter;
        private Vector3 weaponOrigin;
        private Vector3 moveDirection;
        private Vector3 targetWeaponBobPosition;

        
        //moving
        private float speedWalk, maxWalk, maxSprint, maxCrouch, maxSpace;
        private float aimAngle;
        private float playerAngleX, playerAngleZ;
        
        //Bools
        
        private bool grounded, spaceCheck, spaced;
        private bool crouching, sprinting, redyToJat = false;
        private bool flashLightInput = false;
        private bool jatMus;



        private WeaponMenadger weaponMenadger;
        private Text ammoUI;
        #endregion


        #region PhotonCallbacks

        public void OnPhotonSerializeView(PhotonStream _stream, PhotonMessageInfo _message)
        {
            if (_stream.IsWriting)
            {
                _stream.SendNext(flashLightInput);
                _stream.SendNext((int) (weapon.transform.localEulerAngles.x * 100f));
                //_stream.SendNext((int) (playerBody.transform.localEulerAngles.x * 100f));
                //_stream.SendNext((int) (playerBody.transform.localEulerAngles.z * 100f));
                _stream.SendNext((int) (sendGunD.x * 100f));
                _stream.SendNext((int) (sendGunD.y * 100f));
                _stream.SendNext((int) (sendGunD.z * 100f));
                _stream.SendNext(spaceCheck);
            }
            else
            {
                flashLightInput = (bool) _stream.ReceiveNext();
                aimAngle = (int) _stream.ReceiveNext() / 100f;
                //playerAngleX = (int) _stream.ReceiveNext() / 100f;
                //playerAngleZ = (int) _stream.ReceiveNext() / 100f;
                recieveGunD.x = (int) _stream.ReceiveNext() / 100f;
                recieveGunD.y = (int) _stream.ReceiveNext() / 100f;
                recieveGunD.z = (int) _stream.ReceiveNext() / 100f;
                spaced = (bool) _stream.ReceiveNext();
            }
        }

        
        
        #endregion


    #region SystemFunctions

    
    void Awake() {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            speedWalk = walkSpeed;
            rb = GetComponent<Rigidbody> ();
            maxWalk = walkSpeed;
            maxSprint = sprintSpeed;
            maxCrouch = crouchSpeed;
            maxSpace = spaceSpeed;
            flashLight.SetActive(false);
        }
        // Start is called before the first frame update
        void Start()
        {
            nicknameText.SetText(photonView.Owner.NickName);
            cameraParent.SetActive(photonView.IsMine);
            if (!photonView.IsMine)
            {
                gameObject.layer = 11;
                flashLightCorp.layer = 0;
                flashLightLight.layer = 0;
                nicknameText.gameObject.layer = 0;
            }
            else
            {
                teamUI = GameObject.Find("HUD/Team/Text").GetComponent<Text>();
                ammoUI = GameObject.Find("HUD/Ammo/AmmoText").GetComponent<Text>();
                curentFuel = maxFuel;
                cam = playerCamera.fieldOfView;
                playerScale = transform.localScale;
                weaponOrigin = weapon.localPosition;
                weaponScale = weapon.localScale;
                pl = gameObject.GetComponent<GravityBody>();
                mouseSensitivityX = GameSettings.XmouseSenivity;
                mouseSensitivityY = GameSettings.YmouseSenivity;
                weaponMenadger = GetComponent<WeaponMenadger>();
                if (GameSettings.gameMode == GameMode.TEAMDEATHMATCH)
                {
                    photonView.RPC("SyncTeam", RpcTarget.All, GameSettings.isAwayTeam);

                    if (GameSettings.isAwayTeam)
                    {
                        teamUI.text = "RED TEAM";
                        teamUI.color = Color.red;
                    }
                    else
                    {
                        teamUI.text = "BLUE TEAM";
                        teamUI.color = Color.blue;
                    }
                }
                else
                {
                    teamUI.gameObject.SetActive(false);
                }
            }
        }
    
        // Update is called once per frame
        void Update()
        {
            if (photonView.IsMine)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ToggleCursorMode();
                    pauseMethods();
                }
                
                weaponMenadger.RefreshAmmo(ammoUI);
                
                
                // Grounded check
                Ray ray = new Ray(transform.position, -transform.up);
                RaycastHit hit;
                
                
                if (Physics.Raycast(ray, out hit, 1 + .1f, groundedMask))
                {
                    grounded = true;
                    redyToJat = false;
                }
                else
                {
                    grounded = false;
                    redyToJat = true;
                }
                
                //Look and input
                if (!spaceCheck)
                {
                    if (Cursor.lockState == CursorLockMode.Locked)
                    {
                        LookOnPlanet();
                    }
                }
                else
                {
                    if (Cursor.lockState == CursorLockMode.Locked)
                    {
                        LookInSpace();
                    }
                }

                ControlDrag();
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    MyInput();
                }

                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    // Jump
                    if (Input.GetKeyDown(jumpKey) && jumpKey != KeyCode.None && grounded)
                    {
                        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
                        currentRecovery = 0f;
                    }
                }
                /*// Jump
                if (Input.GetKeyDown(jumpKey) && jumpKey != KeyCode.None)
                {
                    if (grounded)
                    {
                        jump = true;
                        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
                    }
                    if (redyToJat && !grounded)
                    {
                        if (Input.GetKey(jumpKey))
                        {
                            rb.AddForce(transform.up * jatForce, ForceMode.Impulse); //надо исправить потом на что-то другое
                            jump = false;
                        }
                    }
                }*/
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    //Flashlight
                    if (flashLightInput)
                    {
                        flashLight.SetActive(true);
                    }
                    else
                    {
                        flashLight.SetActive(false);
                    }
                }
            }
            else
            {
                //Flashlight
                if (flashLightInput)
                {
                    flashLight.SetActive(true);
                }
                else
                {
                    flashLight.SetActive(false);
                }
                RefershMultiplayerState();
            }
        }

        void FixedUpdate()
        {
            if (!photonView.IsMine)
            {
                return;
            }

            if (!spaceCheck)
            {
                
                RaycastHit hit;
                if (Physics.Raycast(raycastTransform.position, Vector3.down, out hit, Mathf.Infinity, forRaycast))
                {
                    sendGunD = hit.normal;
                }
                Movement();
               
            }

            //space check
            if (pl.planet == null)
            {
                spaceCheck = true;
            }
            else
            {
                spaceCheck = false;
            }

            //jet
            if (Input.GetKey(jumpKey) && redyToJat && curentFuel > 0)
            {
                if(!jatMus)
                {
                    jtsfx.Play();
                    jatMus = true;
                }
                
                rb.AddForce(transform.up * jatForce * Time.fixedDeltaTime,
                    ForceMode.Acceleration);
                curentFuel = Mathf.Max(0, curentFuel - Time.fixedDeltaTime);
            }
            else
            {
                if (jatMus)
                {
                    jtsfx.Stop();
                    jatMus = false;
                }
                
            }

            if (grounded)
            {
                if (currentRecovery < jetWait)
                    currentRecovery = Mathf.Min(jetWait, currentRecovery + Time.deltaTime);
                else
                    curentFuel = Mathf.Min(maxFuel, curentFuel + Time.deltaTime * jetRecovery);
            }

            fulebar.localScale = new Vector3(curentFuel / maxFuel, 1, 1);
        }

        #endregion

    #region LookSystem

     void LookOnPlanet()
        {
            // Look rotation:
            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * mouseSensitivityX);
            verticalLookRotation += Input.GetAxis("Mouse Y") * mouseSensitivityY;
            verticalLookRotation = Mathf.Clamp(verticalLookRotation,-90,90);
            cameraTransform.localEulerAngles = Vector3.left * verticalLookRotation;
            weapon.localEulerAngles = cameraTransform.localEulerAngles;
        }
    
        void LookInSpace()
        {
            // Look rotation:
            cameraTransform.rotation =
                Quaternion.Lerp(cameraTransform.rotation, transform.rotation, rotationSpeed * Time.deltaTime);
            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * mouseSensitivityX);
            transform.Rotate(Vector3.left * Input.GetAxis("Mouse Y") * mouseSensitivityY);
            weapon.localEulerAngles = cameraTransform.localEulerAngles;
        }

    #endregion


    #region PrivateMethods

    public void TrySync()
    {
        if(!photonView.IsMine) return;

        if (GameSettings.gameMode == GameMode.TEAMDEATHMATCH)
        {
            photonView.RPC("SyncTeam", RpcTarget.All, GameSettings.isAwayTeam);
        }
    }
    private void ColorTeamIndicators(Color _color)
    {
        foreach (Renderer a in teamIndicators)
        {
            a.material.color = _color;
        }
    }
    
    private void pauseMethods()
    {
        if (pauseMenu.activeSelf)
        {
            pauseMenu.SetActive(false);
        }
        else
        {
            pauseMenu.SetActive(true);
        }
    }
    private void ToggleCursorMode()
    {
        Cursor.visible = !Cursor.visible;

        if (Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
    void RefershMultiplayerState()
    {
        if (!spaced)
        {
            float cacheEulY = weapon.localEulerAngles.y;
            float minusAngle = Mathf.Acos(
                (originVector.x * recieveGunD.x + originVector.y * recieveGunD.y + originVector.z * recieveGunD.z) /
                (Mathf.Sqrt(originVector.x * originVector.x + originVector.y * originVector.y +
                            originVector.z * originVector.z) *
                 Mathf.Sqrt(recieveGunD.x * recieveGunD.x + recieveGunD.y * recieveGunD.y +
                            recieveGunD.z * recieveGunD.z)));
            Quaternion targetRotationZ =
                Quaternion.identity * Quaternion.AngleAxis(playerBody.localEulerAngles.z, Vector3.forward);
            Quaternion targetRotationX = Quaternion.identity *
                                         Quaternion.AngleAxis(aimAngle - minusAngle, Vector3.right);
            
            weapon.rotation = Quaternion.Slerp(weapon.rotation, targetRotationX * targetRotationZ, Time.deltaTime * 8f);

            Vector3 finalRotation = weapon.localEulerAngles;
            finalRotation.y = cacheEulY;
            weapon.localEulerAngles = finalRotation;
        }
        else
        {
            weapon.localEulerAngles = Vector3.zero;
        }
    }

    void ControlDrag()
    {
        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else if (!grounded && !spaceCheck)
        {
            rb.drag = 0;
        }
        else if (spaceCheck && !grounded)
        {
            rb.drag = 0;
        }

    }

    void MyInput()
    {
        // Calculate movement:
        inputY = Input.GetAxis("Horizontal");
        inputX = Input.GetAxis("Vertical");
        moveDirection = inputX * transform.forward + inputY * transform.right;


        //Headbob
        if (grounded)
        {
            if (gameObject.GetComponent<WeaponMenadger>().aiming)
            {
                targetWeaponBobPosition = weaponOrigin;
                weapon.localPosition = Vector3.Lerp(weapon.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
            }
            else if (inputX == 0 && inputY == 0)
            {
                Headbob(idleCounter, 0.025f, 0.025f);
                idleCounter += Time.deltaTime;
                weapon.localPosition = Vector3.Lerp(weapon.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
            }
            else if (!sprinting)
            {
                Headbob(movementCounter, 0.035f, 0.035f);
                movementCounter += Time.deltaTime * walkSpeed / (1.5f * 10);
                weapon.localPosition = Vector3.Lerp(weapon.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
            }
            else
            {
                Headbob(movementCounter, 0.045f, 0.075f);
                movementCounter += Time.deltaTime * walkSpeed / (1.5f * 10);
                weapon.localPosition =
                    Vector3.Lerp(weapon.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
            }
        }
        else
        {
            targetWeaponBobPosition = weaponOrigin;
            weapon.localPosition = Vector3.Lerp(weapon.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
        }


        crouching = Input.GetKey(crouchKey);
        sprinting = Input.GetKey(sprintKey);

        /*Vector3 moveDir = new Vector3(inputX, 0, inputY).normalized;
        Vector3 targetMoveAmount = moveDir * walkSpeed;
        moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, .15f);*/


        movementInSpace();

        if (Input.GetKeyDown(crouchKey) && crouchKey != KeyCode.None)
        {
            StartCrouching();
        }

        if (Input.GetKeyUp(crouchKey))
        {
            StopCrouch();
        }

        if (Input.GetKeyDown(sprintKey) && sprintKey != KeyCode.None)
        {
            StartSprint();
        }

        if (Input.GetKeyUp(sprintKey))
        {
            StopSprint();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            FlashLight();
        }

    }

    [PunRPC]
    private void SyncTeam(bool _awayTeam)
    {
        awayTeam = _awayTeam;
        if (awayTeam)
        {
            ColorTeamIndicators(Color.red);
        }
        else
        {
            ColorTeamIndicators(Color.blue);
        }
    }
    
    #endregion
    

    #region SpaceMovement

    void movementInSpace()
        {
            if (!spaceCheck)
            {
                if (grounded && sprintKey == KeyCode.None)
                {
                    setMovementOnPlanet();
                }
                return;
            }
            else if(spaceCheck)
            {
                playerCamera.fieldOfView = cam;
                setMovementInSpace();
                if (Input.GetKey(spaceUpKey))
                {
                    spaceUp();
                }
                if (Input.GetKey(spaceDownKey))
                {
                    spaceDown();
                }
                if (Input.GetKey(spaceLeft))
                {
                    spaceLeftA();
                }
                if (Input.GetKey(spaceRight))
                {
                    spaceRightD();
                }
                if (Input.GetKey(spaceForwardKey))
                {
                    spaceForW();
                }
                if (Input.GetKey(spaceBackwardKey))
                {
                    spaceBackS();
                }
    
                if (Input.GetKey(spaceRightRotate))
                {
                    rotateE();
                }
    
                if (Input.GetKey(spaceLeftRotate))
                {
                    rotateQ();
                }
    
                if (Input.GetKey(spaceStopVelocity))
                {
                    StopVelocity();
                }
            }
        }
    
        void setMovementInSpace()
        {
            if (sprintKey != KeyCode.None)
            {
                spaceUpKey = KeyCode.Space;
                spaceDownKey = KeyCode.LeftShift;
                sprintKey = KeyCode.None;
                jumpKey = KeyCode.None;
                crouchKey = KeyCode.None;
    
            }
        }
        void setMovementOnPlanet()
        {
            if (sprintKey == KeyCode.None)
            {
                spaceUpKey = KeyCode.None;
                spaceDownKey = KeyCode.None;
                sprintKey = KeyCode.LeftShift;
                jumpKey = KeyCode.Space;
                crouchKey = KeyCode.LeftControl;
                
                if (walkSpeed > maxWalk)
                {
                    walkSpeed = maxWalk;
                }
                if (crouchSpeed > maxCrouch)
                {
                    crouchSpeed = maxCrouch;
                }
                if (spaceSpeed > maxSpace)
                {
                    spaceSpeed = maxSpace;
                }
                if (sprintSpeed > maxSprint)
                {
                    sprintSpeed = maxSprint;
                }
            }
        }
    
        void spaceUp()
        {
            rb.AddForce(transform.up * spaceSpeed * Time.deltaTime);
        }
    
        void spaceDown()
        {
            rb.AddForce(-transform.up * spaceSpeed * Time.deltaTime);
        }
    
        void spaceForW()
        {
            rb.AddForce(transform.forward * spaceSpeed * Time.deltaTime);
        }
        void spaceBackS()
        {
            rb.AddForce(-transform.forward * spaceSpeed * Time.deltaTime);
        }
    
        void spaceLeftA()
        {
            rb.AddForce(-transform.right * spaceSpeed * Time.deltaTime);
        }
        void spaceRightD()
        {
            rb.AddForce(transform.right * spaceSpeed * Time.deltaTime);
        }
        void rotateQ()
        {
            transform.Rotate(0, 0, angleSpeedRotation * Time.deltaTime * speedRotation, Space.Self);
        }
        void rotateE()
        {
            transform.Rotate(0, 0, -angleSpeedRotation * Time.deltaTime * speedRotation, Space.Self);
        }
        void StopVelocity()
        {
            rb.AddForce(-rb.velocity * Time.deltaTime * speedStop);
        }

    #endregion

    #region PlanetMovement

     private void StartCrouching()
     {
         weapon.localScale = weapon.localScale;
            walkSpeed = crouchSpeed;
            transform.localScale = crouchScale;
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
    
        }
        private void StopCrouch()
        {
            weapon.localScale = weaponScale;
            walkSpeed = speedWalk;
            transform.localScale = playerScale;
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
    
        }
        private void StartSprint()
        {
            
            playerCamera.fieldOfView = cam + 10;
            walkSpeed = sprintSpeed;
        }
        private void StopSprint()
        {
            playerCamera.fieldOfView = cam;
            walkSpeed = speedWalk;
        }

        private void FlashLight()
        {
            if (flashLight.activeSelf)
                {
                    flashLightInput = false;
                }
                else
                {
                    flashLightInput = true;
                }
        }
        
    
        void Movement()
        {
            if (grounded)
            {
                rb.AddForce(moveDirection * walkSpeed, ForceMode.Acceleration);
            }
            else if (!grounded)
            {
                rb.AddForce(moveDirection.normalized * walkSpeed * airMultiplier,
                    ForceMode.Acceleration);
            }

            /*// Apply movement to rigidbody
            Vector3 localMove = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
            
            rb.MovePosition(rb.position + localMove);*/
        }

    #endregion

    #region HeadBob

    private void Headbob(float parZ, float parX_intens, float parY_intens)
    {
        
        targetWeaponBobPosition = weaponOrigin + new Vector3(Mathf.Cos(parZ)*parX_intens, Mathf.Sin(parZ * 2) * parY_intens, 0);
    }

    #endregion
    
    
}