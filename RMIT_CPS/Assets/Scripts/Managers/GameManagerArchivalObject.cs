using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerArchivalObject : MonoBehaviour
{
    #region Serialized Variables

    #region Datas
    [Space, Header("Datas")]
    [SerializeField]
    [Tooltip("GameManager Scriptable Object")]
    private GameMangerData gmData = default;

    [SerializeField]
    [Tooltip("Do you want to disable the curosr?")]
    private bool isCursorDisabled = default;
    #endregion

    #region Cinemachine Cams
    [Space, Header("Cinemachine Cameras")]

    [SerializeField]
    [Tooltip("LayerMask to switch to")]
    private LayerMask switchLayer = default;

    [SerializeField]
    [Tooltip("Default LayerMask")]
    private LayerMask defaultLayer = default;
    #endregion

    #region UI
    [Space, Header("UI")]
    [SerializeField]
    [Tooltip("Pause Buttons")]
    private Button[] pauseButtons = default;
    #endregion

    #region Animators
    [Space, Header("Animators")]
    [SerializeField]
    [Tooltip("Fade Panel")]
    private Animator fadeBG = default;

    [SerializeField]
    [Tooltip("Stickman Flicker Anim")]
    private Animator stickmanFlickerAnim = default;
    #endregion

    #region GameObjects
    [Space, Header("GameObjects")]
    [SerializeField]
    [Tooltip("Player Root")]
    private GameObject playerRoot = default;

    [SerializeField]
    [Tooltip("Player HUD Panel")]
    private GameObject hudPanel = default;

    [SerializeField]
    [Tooltip("Player Pause Panel")]
    private GameObject pausePanel = default;

    [SerializeField]
    [Tooltip("Crane GameObject")]
    private GameObject craneObj = default;

    [SerializeField]
    [Tooltip("Room 2 GameObject")]
    private GameObject room2Obj = default;

    [SerializeField]
    [Tooltip("Room 2 Door GameObject")]
    private GameObject room2DoorObj = default;
    #endregion

    #region Transforms
    [Space, Header("Transforms")]
    [SerializeField]
    [Tooltip("Room 2 Teleport Position")]
    private Transform room2TeleportPos = default;
    #endregion

    #region Floats
    [Space, Header("Floats")]
    [SerializeField]
    [Tooltip("Crane Rotation Speed")]
    private float craneRotationSpeed = default;

    [SerializeField]
    [Tooltip("Room 2 shrink size change Speed")]
    [Range(0f, 0.5f)] private float room2ShrinkChangeSpeed = default;

    [SerializeField]
    [Tooltip("Room 2 expand size change Speed")]
    [Range(0f, 3f)] private float room2ExpandChangeSpeed = default;

    [SerializeField]
    [Tooltip("How long will the 2nd room keep shrinking?")]
    private float room2ShrinkDelay = default;

    [SerializeField]
    [Tooltip("When will the 2nd room door appear?")]
    private float room2DoorDelay = default;
    #endregion

    #region Events Float
    public delegate void SendEventsFloat(float floatIndex);
    /// <summary>
    /// Event sent from GameManagerArchivalObject to FPSController Scripts;
    /// Changes the Player's speed;
    /// </summary>
    public static event SendEventsFloat OnSpeedToggle;
    #endregion

    #endregion

    #region Private Variables
    private Camera _cam = default;
    private bool _isCraneRotating = default;
    private Vector3 _intialRoom2Size = default;
    private bool _isRoom2GettingSmall = default;
    private CharacterController _charControl = default;
    #endregion

    #region Unity Callbacks

    #region Events
    void OnEnable()
    {

    }

    void OnDisable()
    {

    }

    void OnDestroy()
    {

    }
    #endregion

    void Start()
    {
        _cam = Camera.main;
        StartCoroutine(StartDelay());
        _intialRoom2Size = room2Obj.transform.localScale;
        _charControl = playerRoot.GetComponent<CharacterController>();

        if (isCursorDisabled)
            gmData.DisableCursor();
    }

    void Update()
    {
        RotatingCrane();
        Room2Size();

        if (gmData.currState == GameMangerData.GameState.Game &&
            Input.GetKeyDown(KeyCode.Escape))
            PauseGame();
    }
    #endregion

    #region My Functions

    #region UI

    #region Buttons
    /// <summary>
    /// Function tied with Resume_Button Button;
    /// Resumes the Game;
    /// </summary>
    public void OnClick_Resume()
    {
        gmData.TogglePause(false);
        pausePanel.SetActive(false);
        hudPanel.SetActive(true);
        gmData.DisableCursor();
    }

    /// <summary>
    /// Function tied with Restart_Button Button;
    /// Restarts the game with a delay;
    /// </summary>
    public void OnClick_Restart() => StartCoroutine(RestartGameDelay());

    /// <summary>
    /// Button tied with Menu_Button;
    /// Goes to the Menu with a delay;
    /// </summary>
    public void OnClick_Menu() => StartCoroutine(MenuDelay());

    /// <summary>
    /// Function tied with Quit_Button Buttons;
    /// Quits the game with a delay;
    /// </summary>
    public void OnClick_Quit() => StartCoroutine(QuitGameDelay());

    /// <summary>
    /// Function tied with Restart_Button, Menu_Button and Quit_Button Buttons;
    /// Disables the buttons so the Player can't interact with them when the panel is fading out;
    /// </summary>
    public void OnClick_DisableButtons()
    {
        for (int i = 0; i < pauseButtons.Length; i++)
            pauseButtons[i].interactable = false;
    }
    #endregion

    void PauseGame()
    {
        gmData.TogglePause(true);
        gmData.EnableCursor();
        pausePanel.SetActive(true);
        hudPanel.SetActive(false);
    }
    #endregion

    #region Room 1
    /// <summary>
    /// Switches the layer of the Camera's CullingMask;
    /// Also toggles the main Virutal Camera GameObject;
    /// </summary>
    /// <param name="isSwitchLayer"> If true, change to custom LayerMask, if false, change to default; </param>
    public void OnSwitchCameraLayer(bool isSwitchLayer)
    {
        if (isSwitchLayer)
        {
            hudPanel.SetActive(false);
            _cam.cullingMask = switchLayer;
        }
        else
        {
            hudPanel.SetActive(true);
            _cam.cullingMask = defaultLayer;
        }
    }

    /// <summary>
    /// Makes the crane rotate with the bool from the Unity Event;
    /// </summary>
    /// <param name="isRotatingCrane"> If true, rotates the crane, else it stops rotation; </param>
    public void OnCraneRotate(bool isRotatingCrane)
    {
        if (isRotatingCrane)
            _isCraneRotating = true;
        else
            _isCraneRotating = false;
    }

    /// <summary>
    /// Subbed to level trigger for room 1;
    /// Changes the speed of the player;
    /// </summary>
    /// <param name="speed"> Speed variable for the player; </param>
    public void OnIsPlayerSpeedy(float speed) => OnSpeedToggle?.Invoke(speed);

    /// <summary>
    /// Crane rotation;
    /// </summary>
    void RotatingCrane()
    {
        if (_isCraneRotating)
            craneObj.transform.Rotate(craneRotationSpeed * Time.deltaTime * Vector3.up);
    }
    #endregion

    #region Room 2
    public void OnRoom2Event() => StartCoroutine(Room2Sequence());

    void Room2Size()
    {
        if (_isRoom2GettingSmall)
            room2Obj.transform.localScale = Vector3.Lerp(room2Obj.transform.localScale, Vector3.zero, room2ShrinkChangeSpeed * Time.deltaTime);
        else
            room2Obj.transform.localScale = Vector3.Lerp(room2Obj.transform.localScale, _intialRoom2Size, room2ExpandChangeSpeed * Time.deltaTime);
    }
    #endregion

    #endregion

    #region Coroutines

    #region UI
    /// <summary>
    /// Starts Game with delay
    /// </summary>
    /// <returns> Float delay; </returns>
    IEnumerator StartDelay()
    {
        fadeBG.Play("Fade_In");
        gmData.ChangeGameState("Intro");
        yield return new WaitForSeconds(0.5f);
        gmData.ChangeGameState("Game");
    }

    /// <summary>
    /// Restarts the game with a Delay;
    /// </summary>
    /// <returns> Float Delay; </returns>
    IEnumerator RestartGameDelay()
    {
        gmData.TogglePause(false);
        fadeBG.Play("Fade_Out");
        gmData.ChangeGameState("Exit");
        yield return new WaitForSeconds(0.5f);
        gmData.ChangeLevel(Application.loadedLevel);
    }

    /// <summary>
    /// Goes to Menu with a Delay;
    /// </summary>
    /// <returns> Float Delay; </returns>
    IEnumerator MenuDelay()
    {
        gmData.TogglePause(false);
        gmData.ChangeGameState("Exit");
        fadeBG.Play("Fade_Out");
        yield return new WaitForSeconds(0.5f);
        gmData.ChangeLevel(0);
    }

    /// <summary>
    /// Quits with a Delay;
    /// </summary>
    /// <returns> Float Delay; </returns>
    IEnumerator QuitGameDelay()
    {
        gmData.TogglePause(false);
        fadeBG.Play("Fade_Out");
        gmData.ChangeGameState("Exit");
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }
    #endregion

    IEnumerator Room2Sequence()
    {
        stickmanFlickerAnim.Play("Stickman_Flicker_Anim");
        _charControl.enabled = false;
        playerRoot.transform.position = room2TeleportPos.position;
        _charControl.enabled = true;
        room2DoorObj.SetActive(true);
        _isRoom2GettingSmall = true;
        yield return new WaitForSeconds(room2ShrinkDelay);
        _isRoom2GettingSmall = false;
        stickmanFlickerAnim.Play("Empty");
        yield return new WaitForSeconds(room2DoorDelay);
        room2DoorObj.SetActive(false);
    }
    #endregion

    #region Events

    #endregion
}