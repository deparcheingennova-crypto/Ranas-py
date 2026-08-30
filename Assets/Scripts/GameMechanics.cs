//if (playerTurnsLeft[actualTurnPlayer] <= 0)
//{
//    canThrow = false;
//    // CHANGED: Debug log with green color that now ytou cant throw because theres no turns left right now
//    Debug.Log("<color=green>CAHNGED: Player " + actualTurnPlayer.playerName + " has no turns left, switching teams if necessary</color>");
//
//}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TotalCreations.UI;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using DG.Tweening;

public class GameMechanics : MonoBehaviour
{
    public int turnsPerTeam = 6;

    private bool isPaused = false;
    private bool canPause = true;
    public bool canThrow = false;
    public bool isTimeout = false;
    public int jsonTurn = 0;

    [Header("Popup Message Queue System")]
    [SerializeField]
    private Queue<PopupMessageData> popupQueue = new Queue<PopupMessageData>();
    private bool isProcessingPopup = false;

    // Clase para almacenar los datos de un mensaje en la cola
    private class PopupMessageData
    {
        public string message;
        public Color? backgroundColor;
        public bool isBlocking;
        public KeyCode keyToClose;

        public PopupMessageData(string msg, Color? bgColor = null, bool blocking = false, KeyCode key = KeyCode.None)
        {
            message = msg;
            backgroundColor = bgColor;
            isBlocking = blocking;
            keyToClose = key;
        }
    }
    private Coroutine blockingPopupCoroutine;
    private GameObject blockingPopupInstance;

    [HideInInspector] public bool specialShot;

    public GameObject pauseMenuDisplay1, pauseMenuDisplay2;
    public GameObject gameDiscPrefab;
    public Transform discSpawnPosition;
    public Animator molinilloAnimator;
    public Animator gameIntroAnimator;

    public Camera gameCamera;
    public TextMeshPro scoreText3D, scoreText3DChild;
    public bool moveCameraOnThrow = true;
    public Transform cameraThrowPosition, cameraOriginalPosition;
    private IEnumerator moveCameraToThrowPosition, moveCameraToOriginalPosition;
    private float cameraMoveDuration = .2f;

    public GameObject[] sceneLights;
    public GameObject[] introLights;

    public Material team1Material, team2Material, disabledTeamMaterial;
    public GameObject[] team1NeonSign, team2NeonSign;

    public List<Player> team1 = new List<Player>();
    public List<Player> team2 = new List<Player>();
    [SerializeField] private int team1Score, team2Score, team1RoundScore, team2RoundScore;
    private bool teamHasChanged = false;
    private bool pendingTeamSwitchTurnStart = false; //nueva bandera dev-manuel
    public GameObject team1ScoreParticles, team2ScoreParticles;
    public GameObject explosionParticles;

    private Hole[] holes;

    [Header("Audio")]
    [SerializeField] public AudioSource musicAudioSource;
    [SerializeField] private AudioClip drumRoll;
    [SerializeField] private AudioClip peopleCheeringSound;
    [SerializeField] private AudioClip peopleCheeringIntenseSound;
    [SerializeField] private AudioClip peopleBooingSound;
    [SerializeField] private AudioClip spinningSound;
    [SerializeField] private AudioClip scoreSound;
    [SerializeField] private AudioClip turnLightSound;
    [SerializeField] private AudioClip throwSound;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip explosionSound;
    public AudioSource audioSource;

    private bool gameStarted = false;
    private int turnCount = 0;
    private int roundCount = 1;

    [Header("Rounds")]
    public int maxRounds = 6;

    private Player actualTurnPlayer;
    private int team1TurnsRemaining = 6; // Total turns for Team 1
    private int team2TurnsRemaining = 6; // Total turns for Team 2
    private Dictionary<Player, int> playerTurnsLeft = new Dictionary<Player, int>();
    private Dictionary<Player, int> playerTotalTurns = new Dictionary<Player, int>();

    [Header("UI Elements")]
    public Button continueGameButton;
    public Button nextTeamButton;
    public GameObject victoryPanelPrefab;
    public GameObject popupMessagePrefab;
    public Color team1ColorPopup, team2ColorPopup;
    public TextMeshProUGUI team1ScoreText, team2ScoreText, roundsText;
    public Transform pointsTableTransform;
    public GameObject pointsRoundPrefab;
    public TextMeshProUGUI totalPointsTeam1, totalPointsTeam2, gameTimer;
    private RoundContent actualRoundContent;
    [Header("Display Canvas")]
    public Canvas display1Canvas, display2Canvas;
    [Header("Teams change every player?")]
    public bool teamsChangeEveryPlayer = false;

    public TextMeshProUGUI turnsText;

    public int actualTurnPlayerTurns = 0;

    public static GameMechanics Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        //CreateDummyTeam();
    }
    void Start()
    {
        moveCameraToThrowPosition = MoveCameraToThrowPosition();
        moveCameraToOriginalPosition = MoveCameraToOriginalPosition();

        nextTeamButton.onClick.AddListener(() =>
        {
            ForceTeamChange();
        });

        if (!audioSource)
            audioSource = GetComponent<AudioSource>();

        holes = GameObject.FindObjectsOfType<Hole>();
        if (holes.Length == 0)
        {
            Debug.LogError("No point holes found in the scene.");
        }

        continueGameButton.interactable = false;

        gameStarted = false;
        GetTeams();
        StartGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            NextTurn();
            //NextTurnFullTeams();
        }
   

        if (GameManager.Instance)
        {
            gameTimer.text = GameManager.Instance.GetTimerString();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }


        if (actualTurnPlayer != null && playerTurnsLeft.ContainsKey(actualTurnPlayer))
            actualTurnPlayerTurns = playerTurnsLeft[actualTurnPlayer];
        else
            actualTurnPlayerTurns = 0;

        ProcessPopupQueue();

    }
    // Using dotween
    public void ShakeCameras()
    {
        Camera mainCamera = GameObject.FindGameObjectsWithTag("MainCamera")[0].GetComponent<Camera>();
        Camera secondScreenCamera = GameObject.FindGameObjectsWithTag("SecondScreenCamera")[0].GetComponent<Camera>();

        mainCamera.transform.DOShakePosition(0.5f, 0.5f, 10, 90, false, true);
        secondScreenCamera.transform.DOShakePosition(0.5f, 0.5f, 10, 90, false, true);

        mainCamera.transform.DOShakeRotation(0.5f, 0.5f, 10, 90, false);
        secondScreenCamera.transform.DOShakeRotation(0.5f, 0.5f, 10, 90, false);
    }
    public void Explosion(Vector3 pos, bool turnOffLights)
    {
        StartCoroutine(ExplosionCoroutine(pos, turnOffLights));
    }
    private IEnumerator ExplosionCoroutine(Vector3 pos, bool turnOffLights)
    {
        GameObject explosionInstance = Instantiate(explosionParticles, pos, Quaternion.identity);
        Light explosionLight = explosionInstance.GetComponentInChildren<Light>();
        explosionLight.intensity = 0;
        // Use dotween to make an animation to go from 0 to 1000 in 0.25 seconds
        explosionLight.DOIntensity(100, 0.35f);
        explosionLight.DOIntensity(0, 1f).SetDelay(0.35f);

        audioSource.PlayOneShot(explosionSound);
        Destroy(explosionInstance, 3f);

        if (turnOffLights)
        {
            foreach (GameObject light in sceneLights)
            {
                light.SetActive(false);
            }
            yield return new WaitForSeconds(2f);
            foreach (GameObject light in sceneLights)
            {
                yield return new WaitForSeconds(0.15f);
                audioSource.PlayOneShot(turnLightSound);
                light.SetActive(true);
            }
        }
    }
    public Hole[] GetHoles()
    {
        return holes;
    }
    public void TogglePause()
    {
        if (!canPause)
            return;

        if (isPaused)
        {
            Time.timeScale = 1;
            pauseMenuDisplay1.SetActive(false);
            pauseMenuDisplay2.SetActive(false);
            isPaused = false;
        }
        else
        {
            Time.timeScale = 0;
            pauseMenuDisplay1.SetActive(true);
            pauseMenuDisplay2.SetActive(true);
            isPaused = true;
        }
    }

    public bool GetIsPaused()
    {
        return isPaused;
    }
    public void CreateDummyTeam()
    {
        string[] names = {
            "Alejandra", "Gabriel", "Valentina", "Sebastián", "Alejandro",
            "Mariana", "Maximiliano", "Isabella", "Emiliano", "Sofía",
            "Estefanía", "Federico", "Leonardo", "Luciana", "Cristóbal",
            "Martín", "Juliana", "Nicolás", "Juanita", "Camilo",
            "Matías", "Alejandro", "Karina", "Rodrigo", "Tatiana",
            "Andrés", "Gonzalo", "Violeta", "Salomé", "Fabián",
            "Renata"
        };

        int playersCount = 4;
        int playersPerTeam = playersCount / 2;
        for (int i = 0; i < playersCount; i++)
        {
            GameObject playerObject = new GameObject("Player (" + i + ")");
            Player newPlayer = playerObject.AddComponent<Player>();
            // Initialize the player
            int randomIndex = UnityEngine.Random.Range(0, names.Length);
            newPlayer.Initialize(names[randomIndex], null, i);

            if (i < playersPerTeam)
            {
                team1.Add(newPlayer);
            }
            else
            {
                team2.Add(newPlayer);
            }
        }
    }
    private void StartGame()
    {
        StartCoroutine(StartGameCoroutine());
    }
    private IEnumerator StartGameCoroutine()
    {
        foreach (GameObject light in sceneLights)
        {
            light.SetActive(false);
        }
        display2Canvas.gameObject.SetActive(false);

        Camera mainCamera = GameObject.FindGameObjectsWithTag("MainCamera")[0].GetComponent<Camera>();
        Camera secondScreenCamera = GameObject.FindGameObjectsWithTag("SecondScreenCamera")[0].GetComponent<Camera>();
        Vector3 startPos1 = mainCamera.transform.position;
        Quaternion startRot1 = mainCamera.transform.rotation;
        Vector3 startPos2 = secondScreenCamera.transform.position;
        Quaternion startRot2 = secondScreenCamera.transform.rotation;

        gameIntroAnimator.SetTrigger("StartIntro");

        yield return new WaitForSeconds(6.8f);

        display2Canvas.gameObject.SetActive(true);
        Destroy(gameIntroAnimator);
        musicAudioSource.spatialBlend = 0;
        mainCamera.transform.position = startPos1;
        mainCamera.transform.rotation = startRot1;
        secondScreenCamera.transform.position = startPos2;
        secondScreenCamera.transform.rotation = startRot2;
        UpdateScores();

        yield return new WaitForSeconds(2f);

        foreach (GameObject light in introLights)
        {
            light.SetActive(false);
        }
        foreach (GameObject light in sceneLights)
        {
            yield return new WaitForSeconds(0.36f);
            audioSource.PlayOneShot(turnLightSound);
            light.SetActive(true);
        }

        yield return new WaitForSeconds(1f);
        // Run CreatePopUpMessage("¡El Juego ha Comenzado!") and wait for it to end
        CreatePopUpMessage("¡El Juego ha Comenzado!");
        yield return new WaitForSeconds(4f);
        NextTurn();
        //NextTurnFullTeams();
    }

    public void MakeThrowSound()
    {
        audioSource.PlayOneShot(throwSound);
    }
    public void StartMoveCameraToThrowPosition()
    {
        if (moveCameraToOriginalPosition != null)
            StopCoroutine(moveCameraToOriginalPosition);

        moveCameraToThrowPosition = MoveCameraToThrowPosition();
        StartCoroutine(moveCameraToThrowPosition);
    }

    public void StartMoveCameraToOriginalPosition()
    {
        if (moveCameraToThrowPosition != null)
            StopCoroutine(moveCameraToThrowPosition);

        moveCameraToOriginalPosition = MoveCameraToOriginalPosition();
        StartCoroutine(moveCameraToOriginalPosition);
    }
    private IEnumerator MoveCameraToThrowPosition()
    {
        if (!moveCameraOnThrow)
            yield break;

        Vector3 startPos = gameCamera.transform.position;
        Quaternion startRot = gameCamera.transform.rotation;
        Vector3 endPos = cameraThrowPosition.position;
        Quaternion endRot = cameraThrowPosition.rotation;

        for (float t = 0; t < 1; t += Time.deltaTime / cameraMoveDuration)
        {
            gameCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            gameCamera.transform.rotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }

        gameCamera.transform.position = endPos;
        gameCamera.transform.rotation = endRot;
    }

    private IEnumerator MoveCameraToOriginalPosition()
    {
        Vector3 startPos = gameCamera.transform.position;
        Quaternion startRot = gameCamera.transform.rotation;
        Vector3 endPos = cameraOriginalPosition.position;
        Quaternion endRot = cameraOriginalPosition.rotation;

        for (float t = 0; t < 1; t += Time.deltaTime / cameraMoveDuration)
        {
            gameCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            gameCamera.transform.rotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }

        gameCamera.transform.position = endPos;
        gameCamera.transform.rotation = endRot;
    }
    private void GetTeams()
    {
        if (GameManager.Instance)
        {
            team1 = GameManager.Instance.team1;
            team2 = GameManager.Instance.team2;
        }
        else
        {
            CreateDummyTeam();
        }
    }
    private void InitializePlayerTurns()
    {
        int totalPlayers = team1.Count + team2.Count;
        int turnsPerPlayer = turnsPerTeam / (totalPlayers / 2);

        playerTurnsLeft.Clear();
        playerTotalTurns.Clear();

        foreach (var player in team1)
        {
            player.team = Player.Team.Team1;
            playerTurnsLeft[player] = turnsPerPlayer;
            playerTotalTurns[player] = turnsPerPlayer;
        }

        foreach (var player in team2)
        {
            player.team = Player.Team.Team2;
            playerTurnsLeft[player] = turnsPerPlayer;
            playerTotalTurns[player] = turnsPerPlayer;
        }

        team1TurnsRemaining = team1.Count * turnsPerPlayer;
        team2TurnsRemaining = team2.Count * turnsPerPlayer;
        teamHasChanged = false;
        isTimeout = false;
    }

    private void UpdateTurnsText()
    {
        if (actualTurnPlayer == null) return;

        int remaining = actualTurnPlayer.team == Player.Team.Team1
            ? team1TurnsRemaining
            : team2TurnsRemaining;
        int current = Mathf.Clamp(turnsPerTeam - remaining, 0, turnsPerTeam);

        turnsText.text = $"Argolla {current}/{turnsPerTeam}";
    }


    private void EnableTeamsLights(bool enableTeam1, bool enableTeam2)
    {
        if (enableTeam1)
        {
            foreach (GameObject sign in team1NeonSign)
            {
                sign.GetComponent<Renderer>().material = team1Material;
            }
        }
        else
        {
            foreach (GameObject sign in team1NeonSign)
            {
                sign.GetComponent<Renderer>().material = disabledTeamMaterial;
            }
        }
        if (enableTeam2)
        {
            foreach (GameObject sign in team2NeonSign)
            {
                sign.GetComponent<Renderer>().material = team2Material;
            }
        }
        else
        {
            foreach (GameObject sign in team2NeonSign)
            {
                sign.GetComponent<Renderer>().material = disabledTeamMaterial;
            }
        }
    }
    public void NextTurn()
    {
        Debug.Log("Next turn called");
        StartCoroutine(WaitBeforeEnablingCanvas());
        turnCount += 1;
        roundsText.text = $"ARGOLLA {jsonTurn}/{turnsPerTeam}";

        // Start the game
        if (!gameStarted)
        {
            actualTurnPlayer = team1[0];  // Team 1 starts first
            InitializePlayerTurns();
            gameStarted = true;

            UpdateTurnsText();


            //CreatePopUpMessage($"Comienza {team1[0].playerName} del Equipo 1", new Color(0.901f, 1f, 0f));
            CreatePopUpMessage($"Comienza Equipo 1 con {team1TurnsRemaining} argollas", team1ColorPopup);
            EnableTeamsLights(true, false);

            // Add new round to screen 1
            team1RoundScore = 0; team2RoundScore = 0;

            // Destroy last round visual if there are more than 5 rounds in the table
            if (roundCount > 10)
            {
                Destroy(pointsTableTransform.GetChild(0).gameObject);
            }

            if (actualRoundContent)
                Destroy(actualRoundContent.roundParticles);

            actualRoundContent = Instantiate(pointsRoundPrefab, pointsTableTransform).GetComponent<RoundContent>();
            actualRoundContent.roundNumberTxt.text = roundCount.ToString();
            actualRoundContent.team1PointsTxt.text = team1RoundScore.ToString();
            actualRoundContent.team2PointsTxt.text = team2RoundScore.ToString();

            FindObjectOfType<KeyboardSimulator>().PressGKey();
        }
        else
        {
            bool endRoundInstantly = false;

            //StartMoveCameraToOriginalPosition();
            // Check for timeout
            if (isTimeout)
            {
                // Change teams
                if (team1TurnsRemaining > 0)
                {
                    team1TurnsRemaining = 0;
                    teamHasChanged = false;
                } // Else, end round
                else
                {
                    endRoundInstantly = true;
                }
            }

            // Check if there are any turns left for any player
            if ((team1TurnsRemaining <= 0 && team2TurnsRemaining <= 0) || endRoundInstantly)
            {
                EndRound();
                //actualRoundContent.roundNumberTxt.text = roundCount.ToString();
                actualRoundContent.team1PointsTxt.text = team1RoundScore.ToString();
                actualRoundContent.team2PointsTxt.text = team2RoundScore.ToString();

                totalPointsTeam1.text = team1Score.ToString();
                totalPointsTeam2.text = team2Score.ToString();

                FindObjectOfType<KeyboardSimulator>().PressTKey();

                canThrow = false;
                CreatePopUpMessageBlocked("Cambio de equipo. Presiona el botón de continuación");

                return;
            }

            // Check if the current player has turns left
            if (playerTurnsLeft[actualTurnPlayer] > 0 && isTimeout == false)
            {
                playerTurnsLeft[actualTurnPlayer]--;
                UpdateTeamTurnsRemaining();

                // NUEVO: si este lanzamiento fue el turno 6 y ya no quedan turnos del Equipo 1,
                // activar el cambio de equipo en ESTE MISMO lanzamiento, sin esperar al siguiente.
                if (!teamsChangeEveryPlayer && actualTurnPlayer.team == Player.Team.Team1 && team1TurnsRemaining <= 0 && !teamHasChanged)
                {
                    actualTurnPlayer = GetNextPlayer();
                    teamHasChanged = true;
                    isTimeout = false;

                    FindObjectOfType<KeyboardSimulator>().PressTKey();

                    canThrow = false;
                    pendingTeamSwitchTurnStart = true;
                    CreatePopUpMessageBlocked("Cambio de equipo. Presiona el botón de continuación");
                    return;
                }

                // Finalizar la ronda inmediatamente después de la sexta argolla de Equipo 2.
                if (!teamsChangeEveryPlayer && actualTurnPlayer.team == Player.Team.Team2 && team2TurnsRemaining <= 0)
                {
                    EndRound();
                    actualRoundContent.team1PointsTxt.text = team1RoundScore.ToString();
                    actualRoundContent.team2PointsTxt.text = team2RoundScore.ToString();

                    totalPointsTeam1.text = team1Score.ToString();
                    totalPointsTeam2.text = team2Score.ToString();

                    FindObjectOfType<KeyboardSimulator>().PressTKey();

                    canThrow = false;
                    CreatePopUpMessageBlocked("Fin de la ronda. Presiona el botón de continuación");
                    return;
                }

                // Notify
                if (playerTurnsLeft[actualTurnPlayer] > 0)
                {
                    //CreatePopUpMessage($"Continua {actualTurnPlayer.playerName}, {playerTurnsLeft[actualTurnPlayer] + 1} turnos restantes", actualTurnPlayer.team == Player.Team.Team1 ? new Color(0.901f, 1f, 0f) : new Color(0.117f, 0.216f, 1f));
                    string msg = actualTurnPlayer.team == Player.Team.Team1 ? "Equipo 1" : "Equipo 2";
                    //CreatePopUpMessage($"Continua el {msg}", actualTurnPlayer.team == Player.Team.Team1 ? team1ColorPopup : team2ColorPopup);
                }
                else
                {
                    //CreatePopUpMessage($"Continua {actualTurnPlayer.playerName} con su ultimo tiro", actualTurnPlayer.team == Player.Team.Team1 ? new Color(0.901f, 1f, 0f) : new Color(0.117f, 0.216f, 1f));
                    string msg = actualTurnPlayer.team == Player.Team.Team1 ? "Equipo 1" : "Equipo 2";
                    //CreatePopUpMessage($"Continua el {msg}", actualTurnPlayer.team == Player.Team.Team1 ? team1ColorPopup : team2ColorPopup);
                }

                if (actualTurnPlayer.team == Player.Team.Team1)
                {
                    EnableTeamsLights(true, false);
                }
                else
                {
                    EnableTeamsLights(false, true);
                }
            }
            else
            {
                if (teamsChangeEveryPlayer)
                {
                    // Player has no turns left, switch to the next player
                    actualTurnPlayer = GetNextPlayer();

                    playerTurnsLeft[actualTurnPlayer]--;
                    UpdateTeamTurnsRemaining();
                }
                else
                {
                    if (team1TurnsRemaining <= 0 && !teamHasChanged)
                    {
                        actualTurnPlayer = GetNextPlayer();
                        teamHasChanged = true;
                        isTimeout = false;

                        FindObjectOfType<KeyboardSimulator>().PressTKey();

                        canThrow = false;
                        pendingTeamSwitchTurnStart = true;
                        CreatePopUpMessageBlocked("Cambio de equipo. Presiona el botón de continuación");
                    }
                    else
                    {
                        actualTurnPlayer = GetNextPlayerSameTeam();
                    }


                    if (!pendingTeamSwitchTurnStart)
                    {
                        playerTurnsLeft[actualTurnPlayer]--;
                        UpdateTeamTurnsRemaining();
                    }
                }

                // Notify
                if (playerTurnsLeft[actualTurnPlayer] > 0)
                {
                    //CreatePopUpMessage($"Turno de {actualTurnPlayer.playerName} del Equipo {(actualTurnPlayer.team == Player.Team.Team1 ? "1" : "2")}", actualTurnPlayer.team == Player.Team.Team1 ? new Color(0.901f, 1f, 0f) : new Color(0.117f, 0.216f, 1f));
                    string msg = actualTurnPlayer.team == Player.Team.Team1 ? "Equipo 1" : "Equipo 2";
                    int shots = actualTurnPlayer.team == Player.Team.Team1 ? team1TurnsRemaining : team2TurnsRemaining;
                    CreatePopUpMessage($"Continua el {msg}", actualTurnPlayer.team == Player.Team.Team1 ? team1ColorPopup : team2ColorPopup);
                }
                else
                {
                    //CreatePopUpMessage($"Turno de {actualTurnPlayer.playerName} con su ultimo tiro", actualTurnPlayer.team == Player.Team.Team1 ? new Color(0.901f, 1f, 0f) : new Color(0.117f, 0.216f, 1f));
                    string msg = actualTurnPlayer.team == Player.Team.Team1 ? "Equipo 1" : "Equipo 2";
                    CreatePopUpMessage($"Continua el {msg}", actualTurnPlayer.team == Player.Team.Team1 ? team1ColorPopup : team2ColorPopup);
                }

                if (actualTurnPlayer.team == Player.Team.Team1)
                {
                    EnableTeamsLights(true, false);
                }
                else
                {
                    EnableTeamsLights(false, true);
                }
            }

            actualRoundContent.roundNumberTxt.text = roundCount.ToString();
            actualRoundContent.team1PointsTxt.text = team1RoundScore.ToString();
            actualRoundContent.team2PointsTxt.text = team2RoundScore.ToString();

            totalPointsTeam1.text = team1Score.ToString();
            totalPointsTeam2.text = team2Score.ToString();
        }
    }
    private IEnumerator WaitBeforeEnablingCanvas()
    {
        yield return new WaitForSeconds(2f);
        display1Canvas.gameObject.SetActive(true);
    }
    private IEnumerator WaitBeforeNextThrowCoroutine(float waitTime)
    {
        canThrow = false;
        yield return new WaitForSeconds(waitTime);
        canThrow = true;
    }
    public void SpinMolinillo()
    {
        audioSource.PlayOneShot(spinningSound);
        molinilloAnimator.SetTrigger("Rotate");
    }

    public void NextTurnAfterScore()
    {
        StartCoroutine(NextTurnAfterScoreCoroutine());
    }

    private IEnumerator NextTurnAfterScoreCoroutine()
    {
        NextTurn();
        yield return new WaitForSeconds(.5f);
        if (specialShot)
        {
            audioSource.PlayOneShot(peopleCheeringIntenseSound);
            specialShot = false;
        }
        else
        {
            audioSource.PlayOneShot(peopleCheeringSound);
        }
        yield return new WaitForSeconds(2f);
        //gameDisc.SetActive(true);
        //gameDisc.transform.position = discSpawnPosition.position;
        //NextTurn();
        //NextTurnFullTeams();
    }

    public void FailedShot()
    {
        StartCoroutine(FailedShotCoroutine());

    }
    public IEnumerator FailedShotCoroutine()
    {
        NextTurn();
        yield return new WaitForSeconds(0.55f);
        audioSource.PlayOneShot(peopleBooingSound);
        yield return new WaitForSeconds(1f);
        //gameDisc.SetActive(true);
        //gameDisc.transform.position = discSpawnPosition.position;
        //NextTurn();
        //NextTurnFullTeams();
    }

    private Player GetNextPlayer()
    {
        // Get the index of the current player
        //int currentIndex = (actualTurnPlayer.team == Player.Team.Team1) ? team1.IndexOf(actualTurnPlayer) : team2.IndexOf(actualTurnPlayer);

        // Alternate teams after a player finishes their turns
        if (actualTurnPlayer.team == Player.Team.Team1)
        {
            // Switch to Team 2's next available player
            return GetNextAvailablePlayerFromTeam(team2);
        }
        else
        {
            // Switch to Team 1's next available player
            return GetNextAvailablePlayerFromTeam(team1);
        }
    }
    private Player GetNextPlayerSameTeam()
    {
        if (actualTurnPlayer.team == Player.Team.Team1)
        {
            return GetNextAvailablePlayerFromTeam(team1);
        }
        else
        {
            return GetNextAvailablePlayerFromTeam(team2);
        }
    }

    private Player GetNextAvailablePlayerFromTeam(List<Player> team)
    {
        // Get the index of the current player in the team
        int currentIndex = team.IndexOf(actualTurnPlayer);

        // Check if the next player in the team has remaining turns
        for (int i = currentIndex + 1; i < team.Count; i++)
        {
            if (playerTurnsLeft[team[i]] > 0)
            {
                return team[i];  // Return the next available player with remaining turns
            }
        }

        // If all players in the team have finished their turns, return the first player in the team
        return team[0];
    }

    private void UpdateTeamTurnsRemaining()
    {
        if (actualTurnPlayer.team == Player.Team.Team1)
        {
            team1TurnsRemaining--;
        }
        else
        {
            team2TurnsRemaining--;
        }

        UpdateTurnsText();
    }

    public void EndRound()
    {
        StartCoroutine(EndRoundCoroutine());
    }
    private IEnumerator EndRoundCoroutine()
    {
        // Increment round count
        roundCount += 1;

        // Check if max rounds have been reached
        if (roundCount > maxRounds)
        {
            EndGame();
            yield break;
        }

        // Notify players of the new round
        CreatePopUpMessage($"¡La Ronda {roundCount} Comienza!");
        yield return new WaitForSeconds(2f);

        // Reset turns for each player
        gameStarted = false;
        NextTurn();
        //NextTurnFullTeams();

        // Reset the turn count for the next round
        turnCount = 1;
        roundsText.text = $"ARGOLLA {jsonTurn}/{turnsPerTeam}";
    }

    public void EndGame()
    {
        StartCoroutine(EndGameCoroutine());
    }
    private IEnumerator EndGameCoroutine()
    {
        FindObjectOfType<KeyboardSimulator>().PressEKey();

        canThrow = false;
        canPause = false;

        team1ScoreText.text = "????"; team2ScoreText.text = "????";
        totalPointsTeam1.text = "????"; totalPointsTeam2.text = "????";

        CreatePopUpMessage($"¡El juego ha terminado, los resultados han llegado!");

        float actualMusicVolume = musicAudioSource.volume;
        musicAudioSource.volume = 0.1f;
        audioSource.PlayOneShot(drumRoll);

        yield return new WaitForSeconds(drumRoll.length - 2.2f);

        audioSource.PlayOneShot(victorySound);
        musicAudioSource.volume = actualMusicVolume;

        string teamWin = "0";

        VictoryPanel victoryPanel1 = Instantiate(victoryPanelPrefab, display1Canvas.transform).GetComponent<VictoryPanel>();
        //victoryPanel1.gameObject.GetComponent<JumpInJumpOut>().JumpIn();
        VictoryPanel victoryPanel2 = Instantiate(victoryPanelPrefab, display2Canvas.transform).GetComponent<VictoryPanel>();
        //victoryPanel2.gameObject.GetComponent<JumpInJumpOut>().JumpIn();

        yield return new WaitForEndOfFrame();

        if (team1Score > team2Score)
        {
            victoryPanel1.team1VictoryPanel.SetActive(true);
            victoryPanel2.team1VictoryPanel.SetActive(true);
            teamWin = "Equipo 1";
        }
        else if (team2Score > team1Score)
        {
            victoryPanel1.team2VictoryPanel.SetActive(true);
            victoryPanel2.team2VictoryPanel.SetActive(true);
            teamWin = "Equipo 2";
        }
        else
        {
            victoryPanel1.tiePanel.SetActive(true);
            victoryPanel2.tiePanel.SetActive(true);
            teamWin = "Empate";
        }

        SendDataWinner.instance.SendDataGame(teamWin);

        if (GameManager.Instance)
        {
            victoryPanel1.timerText.text = "Tiempo Total: " + GameManager.Instance.GetTimerString();
            victoryPanel2.timerText.text = "Tiempo Total: " + GameManager.Instance.GetTimerString();
        }

        yield return new WaitForSeconds(15f);

        if (GameManager.Instance)
        {
            Destroy(GameManager.Instance.gameObject);
        }
        if (DataManager.instance)
        {
            Destroy(DataManager.instance.gameObject);
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public Player.Team GetCurrentTeam()
    {
        if (actualTurnPlayer != null)
        {
            return actualTurnPlayer.team;
        }
        else
        {
            Debug.LogWarning("No player has the current turn.");
            return default;
        }
    }

    public void UpdateScores()
    {
        team1ScoreText.text = team1Score.ToString();
        team2ScoreText.text = team2Score.ToString();
    }
    public void AddScore(int score)
    {
        audioSource.PlayOneShot(scoreSound);

        if (GetCurrentTeam() == Player.Team.Team1)
        {
            team1Score += score;
            team1RoundScore += score;
            JumpInJumpOut jumpInJumpOut = team1ScoreText.GetComponent<JumpInJumpOut>();
            jumpInJumpOut.JumpIn();
        }
        else
        {
            team2Score += score;
            team2RoundScore += score;
            JumpInJumpOut jumpInJumpOut = team2ScoreText.GetComponent<JumpInJumpOut>();
            jumpInJumpOut.JumpIn();
        }

        StartCoroutine(ShowScoreIn3DText(score));
        UpdateScores();
    }
    private IEnumerator ShowScoreIn3DText(int score)
    {
        string scoreDisplay = "+" + score.ToString();

        for (int i = 0; i < 5; i++)
        {
            scoreText3D.text = scoreDisplay;
            scoreText3DChild.text = scoreDisplay;
            yield return new WaitForSeconds(0.25f);

            scoreText3D.text = "";
            scoreText3DChild.text = "";
            yield return new WaitForSeconds(0.25f);
        }

        scoreText3D.text = "---";
        scoreText3DChild.text = "---";
    }


    public void ForceTeamChange()
    {
        // Safety checks
        if (!gameStarted || actualTurnPlayer == null)
        {
            Debug.LogWarning("[GameMechanics] ForceTeamChange called but game is not started or no current player.");
            return;
        }

        // Case where teams alternate every player -> just exhaust this player
        if (teamsChangeEveryPlayer)
        {
            if (playerTurnsLeft.TryGetValue(actualTurnPlayer, out _))
                playerTurnsLeft[actualTurnPlayer] = 0;

            Debug.Log("[GameMechanics] ForceTeamChange: exhausting only the current player (teamsChangeEveryPlayer = true).");
            NextTurn();
            return;
        }

        bool currentIsTeam1 = actualTurnPlayer.team == Player.Team.Team1;

        // Drenar los turnos del equipo actual
        if (currentIsTeam1)
        {
            Debug.Log($"[GameMechanics] ForceTeamChange: Drenando {team1TurnsRemaining} turnos del Equipo 1");
            team1TurnsRemaining = 0;
            // También drenar los turnos de todos los jugadores del equipo 1
            foreach (var player in team1)
            {
                if (playerTurnsLeft.ContainsKey(player))
                    playerTurnsLeft[player] = 0;
            }
        }
        else
        {
            Debug.Log($"[GameMechanics] ForceTeamChange: Drenando {team2TurnsRemaining} turnos del Equipo 2");
            team2TurnsRemaining = 0;
            // También drenar los turnos de todos los jugadores del equipo 2
            foreach (var player in team2)
            {
                if (playerTurnsLeft.ContainsKey(player))
                    playerTurnsLeft[player] = 0;
            }
        }

        // Llamar a NextTurn para que maneje el cambio de equipo automáticamente
        Debug.Log("[GameMechanics] ForceTeamChange: Llamando a NextTurn para cambiar de equipo");
        NextTurn();
    }

    private void ProcessPopupQueue()
    {
        // Si ya estamos procesando un mensaje, no hacer nada
        if (isProcessingPopup)
            return;

        // Si hay un mensaje bloqueante activo, esperar a que termine
        if (blockingPopupCoroutine != null)
            return;

        // Si hay mensajes en la cola, procesar el siguiente
        if (popupQueue.Count > 0)
        {
            PopupMessageData popupData = popupQueue.Dequeue();
            isProcessingPopup = true;

            if (popupData.isBlocking)
            {
                // Iniciar coroutine bloqueante
                blockingPopupCoroutine = StartCoroutine(BlockingPopUpMessageCoroutine(popupData.message, popupData.keyToClose));
            }
            else
            {
                // Iniciar coroutine normal
                if (popupData.backgroundColor.HasValue)
                {
                    StartCoroutine(CreatePopUpMessageCoroutine(popupData.message, popupData.backgroundColor.Value));
                }
                else
                {
                    StartCoroutine(CreatePopUpMessageCoroutine(popupData.message));
                }
            }
        }
    }

    public void CreatePopUpMessage(string message)
    {
        popupQueue.Enqueue(new PopupMessageData(message));
    }
    public void CreatePopUpMessage(string message, Color backgroundColor)
    {
        popupQueue.Enqueue(new PopupMessageData(message, backgroundColor));
    }
    public void CreatePopUpMessageBlocked(string message, KeyCode keyToClose = KeyCode.None)
    {
        popupQueue.Enqueue(new PopupMessageData(message, null, true, keyToClose));
    }

    private IEnumerator BlockingPopUpMessageCoroutine(string message, KeyCode keyToClose)
    {
        canThrow = false;
        yield return new WaitForSeconds(1.4f); // Small delay to ensure animations in game are visible before the popup starts


        float animationDuration = 0.15f;

        // Instanciar popup
        blockingPopupInstance = Instantiate(popupMessagePrefab, display2Canvas.transform);

        RectTransform popupRt = blockingPopupInstance.GetComponent<RectTransform>();
        popupRt.anchorMin = new Vector2(0f, 0.5f);  // estirado horizontal
        popupRt.anchorMax = new Vector2(1f, 0.5f);
        popupRt.pivot = new Vector2(0.5f, 0.5f);
        popupRt.anchoredPosition = Vector2.zero;

        RectTransform parentRt = (RectTransform)blockingPopupInstance.transform.parent;
        float desiredHeight = parentRt.rect.height * 0.5f;

        // Empezar colapsado
        popupRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);

        // Texto
        TextMeshProUGUI messageText = blockingPopupInstance.GetComponentInChildren<TextMeshProUGUI>(true);
        if (messageText) messageText.text = message;

        // Animación opcional
        JumpInJumpOut jump = blockingPopupInstance.GetComponent<JumpInJumpOut>();
        if (jump) jump.JumpOut();

        // Expandir
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            float h = Mathf.Lerp(0f, desiredHeight, t);
            popupRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
            elapsed += Time.unscaledDeltaTime; // para que funcione con Time.timeScale = 0
            yield return null;
        }
        popupRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, desiredHeight);

        // Esperar a que se presione la tecla (código comentado como estaba antes)
        //bool keyPressed = false;
        //while (!keyPressed)
        //{
        //    if (keyToClose == KeyCode.None)
        //    {
        //        if (Input.anyKeyDown) keyPressed = true;
        //    }
        //    else
        //    {
        //        if (Input.GetKeyDown(keyToClose)) keyPressed = true;
        //    }
        //
        //    yield return null;
        //}

        // Esperar a que se llame un botón para cerrar el popup
        bool buttonPressed = false;

        // Asume que tienes una referencia al botón en tu popup
        continueGameButton.onClick.AddListener(() => buttonPressed = true);
        continueGameButton.interactable = true;

        while (!buttonPressed)
        {
            yield return null;
        }

        continueGameButton.onClick.RemoveAllListeners();
        continueGameButton.interactable = false;

        // Animación de salida
        if (jump) jump.JumpOut();

        elapsed = 0f;
        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            float h = Mathf.Lerp(desiredHeight, 0f, t);
            popupRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        popupRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);

        if (jump)
            yield return new WaitForSecondsRealtime(jump.hideDuration + 0.1f);

        // Log al cerrarse
        Debug.Log("[GameMechanics] Blocking popup closed by key press.");

        // Limpiar
        if (blockingPopupInstance != null)
        {
            Destroy(blockingPopupInstance);
            blockingPopupInstance = null;
        }

        // <CHANGE> Marcar que terminamos de procesar este mensaje bloqueante
        blockingPopupCoroutine = null;
        isProcessingPopup = false;
        // </CHANGE>

        if (pendingTeamSwitchTurnStart)
        {
            pendingTeamSwitchTurnStart = false;
            UpdateTurnsText();
        }

        canThrow = true;

        FindObjectOfType<KeyboardSimulator>().PressRKey();
    }

    private IEnumerator CreatePopUpMessageCoroutine(string message)
    {
        yield return new WaitForSeconds(0.5f); // Small delay to ensure animations in game are visible before the popup starts
        // Set desired height based on screen size
        float animationDuration = 0.15f;

        GameObject popupMessage = Instantiate(popupMessagePrefab, display2Canvas.transform);

        RectTransform popupRt = popupMessage.GetComponent<RectTransform>();

        popupRt.anchorMin = new Vector2(0f, 0.5f);
        popupRt.anchorMax = new Vector2(1f, 0.5f);
        popupRt.pivot = new Vector2(0.5f, 0.5f);
        popupRt.anchoredPosition = Vector2.zero;

        RectTransform parentRt = (RectTransform)popupRt.parent;
        float desiredHeight = parentRt.rect.height * 0.5f;

        RectTransform popupRectTransform = popupMessage.GetComponent<RectTransform>();
        popupRectTransform.sizeDelta = new Vector2(popupRectTransform.sizeDelta.x, 0);
        popupRectTransform.anchoredPosition = new Vector2(0, 0);  // Center the popup on the canvas

        TextMeshProUGUI messageText = popupMessage.GetComponentInChildren<TextMeshProUGUI>();
        messageText.text = message;

        JumpInJumpOut jumpInJumpOutAnim = popupMessage.GetComponent<JumpInJumpOut>();

        jumpInJumpOutAnim.JumpOut();

        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            float newHeight = Mathf.Lerp(0, desiredHeight, elapsedTime / animationDuration);
            popupRectTransform.sizeDelta = new Vector2(popupRectTransform.sizeDelta.x, newHeight);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        popupRectTransform.sizeDelta = new Vector2(popupRectTransform.sizeDelta.x, desiredHeight);

        yield return new WaitForSeconds(1.3f);
        jumpInJumpOutAnim.JumpOut();

        elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            float newHeight = Mathf.Lerp(desiredHeight, 0, elapsedTime / animationDuration);
            popupRectTransform.sizeDelta = new Vector2(popupRectTransform.sizeDelta.x, newHeight);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        popupRectTransform.sizeDelta = new Vector2(popupRectTransform.sizeDelta.x, 0);

        yield return new WaitForSeconds(jumpInJumpOutAnim.hideDuration + 0.1f);

        if (blockingPopupCoroutine == null)
            canThrow = true;

        // <CHANGE> Marcar que terminamos de procesar este mensaje
        isProcessingPopup = false;
        // </CHANGE>

        Destroy(popupMessage);
    }

    private IEnumerator CreatePopUpMessageCoroutine(string message, Color? backgroundColor = null)
    {
        yield return new WaitForSeconds(0.5f); // Small delay to ensure animations in game are visible before the popup starts
        float animationDuration = 0.15f;

        GameObject popupMessage = Instantiate(popupMessagePrefab, display2Canvas.transform);

        // Optional background color
        var img = popupMessage.GetComponent<Image>();
        if (img && backgroundColor.HasValue) img.color = backgroundColor.Value;

        // Ensure predictable anchors/pivot
        RectTransform popupRt = popupMessage.GetComponent<RectTransform>();
        popupRt.anchorMin = new Vector2(0f, 0.5f);   // stretch horizontally
        popupRt.anchorMax = new Vector2(1f, 0.5f);
        popupRt.pivot = new Vector2(0.5f, 0.5f);
        popupRt.anchoredPosition = Vector2.zero;

        // Use parent rect to compute relative height
        RectTransform parentRt = (RectTransform)popupMessage.transform.parent;
        float desiredHeight = parentRt.rect.height * 0.5f;

        // Start collapsed
        popupRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);

        // Set text
        TextMeshProUGUI messageText = popupMessage.GetComponentInChildren<TextMeshProUGUI>(true);
        if (messageText) messageText.text = message;

        // Optional entrance/exit animation component
        JumpInJumpOut jump = popupMessage.GetComponent<JumpInJumpOut>();
        if (jump) jump.JumpOut();

        // Expand
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            float h = Mathf.Lerp(0f, desiredHeight, t);
            popupRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        popupRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, desiredHeight);

        // Display time
        yield return new WaitForSecondsRealtime(1.3f);

        if (jump) jump.JumpOut();

        // Collapse
        elapsed = 0f;
        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            float h = Mathf.Lerp(desiredHeight, 0f, t);
            popupRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        popupRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);

        // Wait for hide animation to finish if present
        if (jump) yield return new WaitForSecondsRealtime(jump.hideDuration + 0.1f);

        if (blockingPopupCoroutine == null)
            canThrow = true;

        // <CHANGE> Marcar que terminamos de procesar este mensaje
        isProcessingPopup = false;
        // </CHANGE>

        Destroy(popupMessage);
    }
}
