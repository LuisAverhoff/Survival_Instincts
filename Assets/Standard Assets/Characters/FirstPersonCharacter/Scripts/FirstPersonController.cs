using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private bool m_IsWalking;
        [SerializeField] private bool m_IsCrouching;
        [SerializeField] private float m_WalkSpeed;
        private float m_RunSpeed;
        [SerializeField] private float m_RunSpeedNormal;
        [SerializeField] private float m_crouchSpeed;
        [SerializeField] private float crouchSmoothness;
        [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] private float m_GravityMultiplier;
        //[SerializeField] private MouseLook m_MouseLook; DO NOT UNCOMMENT THIS. Had to create my own mouse_look script.
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;
        [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] private AudioClip m_JumpSound;           // the sound is played when character leaves the ground.
        [SerializeField] private AudioClip m_LandSound;           // the sound is played when character touches back on ground.

        private Camera m_Camera;
        private bool m_Jump;
        private bool m_StaminaDepleted;
        private float m_YRotation;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_Jumping;
        private AudioSource m_PlayerMovementAudioSource;
        private AudioSource m_PlayerBreathingAudioSource;
        private float originalCharacterHeight;
        private float crouchCharacterHeight;

        // Use this for initialization
        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle/2f;
            m_Jumping = false;
            m_StaminaDepleted = false;
            AudioSource[] allPlayerAudioSources = GetComponents<AudioSource>();
            m_PlayerMovementAudioSource = allPlayerAudioSources[0];
            m_PlayerBreathingAudioSource = allPlayerAudioSources[1];
            originalCharacterHeight = m_CharacterController.height;
            crouchCharacterHeight = originalCharacterHeight * 0.5f;
			//m_MouseLook.Init(transform , m_Camera.transform);
        }


        // Update is called once per frame
        private void Update()
        {
            //RotateView();
            // the jump state needs to read here to make sure it is not missed
            if (!m_Jump && !m_Jumping)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }

            if(CrossPlatformInputManager.GetButtonDown("Crouching"))
            {
                m_IsCrouching = !m_IsCrouching;
            }

            float lastHeight = m_CharacterController.height;

            if (m_IsCrouching)
            {
                m_CharacterController.height = Mathf.Lerp(m_CharacterController.height, crouchCharacterHeight, crouchSmoothness * Time.deltaTime);
            }
            else
            {
                m_CharacterController.height = Mathf.Lerp(m_CharacterController.height, originalCharacterHeight, crouchSmoothness * Time.deltaTime);
            }

            Vector3 heightOffset = new Vector3(0, (m_CharacterController.height - lastHeight) * 0.5f, 0);
            transform.position += heightOffset;

            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }


        private void PlayLandingSound()
        {
            //m_AudioSource.clip = m_LandSound;
            m_PlayerMovementAudioSource.PlayOneShot(m_LandSound);
            m_NextStep = m_StepCycle + .5f;
        }


        private void FixedUpdate()
        {
            float speed;
            GetInput(out speed);
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward*m_Input.y + transform.right*m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                               crouchCharacterHeight, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x*speed;
            m_MoveDir.z = desiredMove.z*speed;


            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else
            {
                m_MoveDir += Physics.gravity*m_GravityMultiplier*Time.fixedDeltaTime;
            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir*Time.fixedDeltaTime);

            ProgressStepCycle(speed);
            UpdateCameraPosition(speed);

            //m_MouseLook.UpdateCursorLock();
        }


        private void PlayJumpSound()
        {
            //m_AudioSource.clip = m_JumpSound;
            m_PlayerMovementAudioSource.PlayOneShot(m_JumpSound);
        }

        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_PlayerMovementAudioSource.clip = m_FootstepSounds[n];
            m_PlayerMovementAudioSource.PlayOneShot(m_PlayerMovementAudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_PlayerMovementAudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }
            m_Camera.transform.localPosition = newCameraPosition;
        }


        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");

            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !CrossPlatformInputManager.GetButton("Running");
#endif
            // set the desired speed to be walking or running
            if (m_IsCrouching)
            {
                speed = m_crouchSpeed;
            }
            else if (m_IsWalking)
            {
                speed = m_WalkSpeed;
            }
            else
                speed = m_RunSpeed;

            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }


        private void RotateView()
        {
            //m_MouseLook.LookRotation (transform, m_Camera.transform);
        }

        public bool isPlayerRunning()
        {
            return m_CharacterController.velocity.magnitude > 0 && !m_IsWalking && !m_IsCrouching;
        }

        public void setRunningSpeed(float stamina)
        {
            // How stamina you have to recover before been able to run again.
            if (m_StaminaDepleted && stamina > 0.5f)
            {
                m_StaminaDepleted = false;
            }

            if(stamina <= 0.0f)
            {
                m_RunSpeed = m_WalkSpeed;
                m_StaminaDepleted = true;
                m_PlayerBreathingAudioSource.Play();
            }
            else if(!m_StaminaDepleted)
            {
                m_RunSpeed = m_RunSpeedNormal;
            }
        }

        public bool isStaminaDepleted()
        {
            return m_StaminaDepleted;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
        }
    }
}
