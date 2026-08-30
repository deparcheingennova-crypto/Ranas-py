using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using TotalCreations.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class MenuSection
{
    public string name;
    public GameObject secondScreenSection;
    public GameObject[] sectionObjects;
    public UnityEngine.Events.UnityEvent onSectionEnter;
}
public class GameManager : MonoBehaviour
{
    [Header("Game Logic")]
    public List<Player> players;
    public List<Player> team1 = new List<Player>();
    public List<Player> team2 = new List<Player>();
    public int playersPerTeam = 1;  // Start with 1 player per team
    private int currentPlayerIndex = 0;
    private bool gameOver = false;

    [Header("Attract Mode UI")]
    public bool isOnAttractMode = true;
    public GameObject attractModeUI1, mainMenuUI1, attractModeUI2, mainMenuUI2;
    public AudioSource backgroundMusicAudioSource;
    [Range(0f, 1f)] public float backgroundMusicVolume = 0.6f;

    [Header("Main menu UI Elements")]
    public TextMeshProUGUI versionTxt;
    public SecondScreen secondScreenScript;
    public bool isOnMainMenu = true;
    [SerializeField] private MenuSection[] menuSections;
    [SerializeField] private int actualSection = 0;
    [SerializeField] private Transform monitorACanvas, monitorBCanvas;
    [SerializeField] private GameObject popupPrefab;
    [SerializeField] private Transform contentTransform, draggableContentTransform, team1SectionTransform, team2SectionTransform;
    [SerializeField] private GameObject playerCardPrefab, draggablePlayerCardPrefab, playerTeamEmptyPrefab;
    [SerializeField] private GameObject waitingForPlayersText, continueTeamAssignButton, draggableCardsScrollView;
    [SerializeField] private TextMeshProUGUI timerText, playerCountText, playersPerTeamText;
    [SerializeField] private Sprite defaultPlayerPhoto;
    // Team 1 color is #7DEFAF, text is #007333
    // Team 2 color is #C191E0, text is #51236F
    public static readonly Color team1Color = new Color(0.4901961f, 0.9294118f, 0.6862745f);
    public static readonly Color team2Color = new Color(0.7568628f, 0.5686275f, 0.8784314f);
    // Team 1 text color is #7DEFAF, text is #C84B00
    // Team 2 text color is #C191E0, text is #4B4B4B
    public static readonly Color team1TextColor = new Color(1f, 1f, 1f);
    public static readonly Color team2TextColor = new Color(1f, 1f, 1f);

    private float timeCount;
    private string timerString;

    [Header("Audio")]
    public AudioSource mainAudioSource;
    [SerializeField] private AudioClip buttonClickSound;

    // This is a singleton pattern, which ensures there's only one instance of GameManager
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Esto mantiene a este singleton en todas las Escenas, haciendo facil entregar
                                       // la información necesaria al empezar el juego
    
        if (!mainAudioSource)
            mainAudioSource = GetComponent<AudioSource>();

        if (!mainAudioSource)
            Debug.LogError("No audiosource in the GameManager object, fix");

        isOnAttractMode = true;
    }
    private void Start()
    {
        if (isOnMainMenu)
        {
            if (players.Count > 0)
            {
                //
                //AssignCurrentPlayer(players[currentPlayerIndex]);
            }

            UpdatePlayerCountText();
            AddButtonSounds();

            // Disable all sections objects and enable the first one
            foreach (MenuSection section in menuSections)
            {
                // Also disable second screen section
                if (section.secondScreenSection)
                    section.secondScreenSection.SetActive(false);
                foreach (GameObject sectionObject in section.sectionObjects)
                {
                    sectionObject.SetActive(false);
                }
            }
            // Enable first section objects
            if (menuSections[actualSection].secondScreenSection)
                menuSections[actualSection].secondScreenSection.SetActive(true);
            foreach (GameObject sectionObject in menuSections[actualSection].sectionObjects)
            {
                sectionObject.SetActive(true);
            }

            if (versionTxt != null)
            {
                versionTxt.text = "Version: " + Application.version;
            }
        }

        StartCoroutine(UpdatePlayers());
    }

    public void UpdatePlayersManually()
    {
        FindAnyObjectByType<GetDataPlayers>().ManuallyGetData();
    }

    IEnumerator UpdatePlayers()
    {
        yield return new WaitForSeconds(2.5f);
        CreateDummyPlayers();
    }

    private void Update()
    {
        HandleMainMenu();
        timeCount += Time.deltaTime;

        int hours = Mathf.FloorToInt(timeCount / 3600);
        int minutes = Mathf.FloorToInt((timeCount % 3600) / 60);
        int seconds = Mathf.FloorToInt(timeCount % 60);

        timerString = string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);

        if (Input.GetKeyDown(KeyCode.F2))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            Destroy(GameManager.Instance);
        }
    }
    public string GetTimerString()
    {
        return timerString;
    }

    #region Main Menu

    // Set all players list gameobject to children of this script
    public void SetPlayersAsChildren()
    {
        foreach (Player player in players)
        {
            player.transform.SetParent(transform);
        }
    }
    public void CheckForPlayersCount()
    {
        if (players.Count > 0)
        {
            waitingForPlayersText.SetActive(false);
        }
        else
        {
            waitingForPlayersText.SetActive(true);
        }
    }
    private void AddButtonSounds()
    {
        // search for all buttons in scene and add a button click sound to them
        Button[] buttons = FindObjectsOfType<Button>(true);
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => mainAudioSource.PlayOneShot(buttonClickSound));
        }
    }
    public void GoToNextSection()
    {
        if (actualSection + 1 < menuSections.Length)
        {
            // Disable second screen section
            if (menuSections[actualSection].secondScreenSection)
                menuSections[actualSection].secondScreenSection.SetActive(false);
            // Enabble next second screen section
            if (menuSections[actualSection + 1].secondScreenSection)
                menuSections[actualSection + 1].secondScreenSection.SetActive(true);
            // Disable current section objects
            foreach (GameObject sectionObject in menuSections[actualSection].sectionObjects)
            {
                sectionObject.SetActive(false);
            }
            // Enable next section objects
            foreach (GameObject sectionObject in menuSections[actualSection + 1].sectionObjects)
            {
                sectionObject.SetActive(true);
            }

            actualSection++;
            menuSections[actualSection].onSectionEnter.Invoke();
        }
    }

    public void GoToLastSection()
    {
        if (actualSection - 1 >= 0)
        {
            // Disable second screen section
            if (menuSections[actualSection].secondScreenSection)
                menuSections[actualSection].secondScreenSection.SetActive(false);
            // Enabble last second screen section
            if (menuSections[actualSection - 1].secondScreenSection)
                menuSections[actualSection - 1].secondScreenSection.SetActive(true);
            // Disable current section objects
            foreach (GameObject sectionObject in menuSections[actualSection].sectionObjects)
            {
                sectionObject.SetActive(false);
            }
            // Enable last section objects
            foreach (GameObject sectionObject in menuSections[actualSection - 1].sectionObjects)
            {
                sectionObject.SetActive(true);
            }

            actualSection--;
            menuSections[actualSection].onSectionEnter.Invoke();
        }
    }

    // This function exists to handle the main menu and only the main menu. It's cleaner and I only check once per update frame
    private void HandleMainMenu()
    {
        if (!isOnMainMenu)
            return;

        //HandleTimer();
        // Dummy Test
        //if (Input.GetKeyDown(KeyCode.Alpha1)) CreateDummyPlayers(1);
     
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UpdateDraggablePlayerCards();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            StartPlayerTeamImages();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            GoToNextSection();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            GoToLastSection();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerAttractMode();
        }
    }

 

    public void TriggerAttractMode()
    {
        if (isOnAttractMode)
        {
            attractModeUI1.SetActive(false); attractModeUI2.SetActive(false);
            mainMenuUI1.SetActive(true); mainMenuUI2.SetActive(true);
            backgroundMusicAudioSource.volume = backgroundMusicVolume;
        }
        else
        {
            attractModeUI1.SetActive(true); attractModeUI2.SetActive(true);
            mainMenuUI1.SetActive(false); mainMenuUI2.SetActive(false);
            backgroundMusicAudioSource.volume = 0f;
        }
        isOnAttractMode = !isOnAttractMode;
    }

    private void HandleTimer()
    {
        timerText.text = timerString;
    }

    public void AddPlayer(Player player)
    {
        players.Add(player);

        PlayerCard playerCard = Instantiate(playerCardPrefab, contentTransform).GetComponent<PlayerCard>();
        playerCard.SetPlayerData(player);

        UpdatePlayerCountText();
        UpdatePlayersPerTeam();
        CheckForPlayersCount();
    }

    private void UpdatePlayerCountText()
    {
        if (playerCountText)
        {
            playerCountText.text = "Hay " + players.Count + " jugadores en la sala";
        }
    }

    public void CreateDummyPlayers()
    {
        //Debug.LogWarning("Create dummy players is disabled at the moment. Add new players using the daatabase and reset the game!");
        //return;

        //string[] names = {
        //    "Alejandra", "Gabriel", "Valentina", "Sebastián", "Alejandro",
        //    "Mariana", "Maximiliano", "Isabella", "Emiliano", "Sofía",
        //    "Estefanía", "Federico", "Leonardo", "Luciana", "Cristóbal",
        //    "Martín", "Juliana", "Nicolás", "Juanita", "Camilo",
        //    "Matías", "Alejandro", "Karina", "Rodrigo", "Tatiana",
        //    "Andrés", "Gonzalo", "Violeta", "Salomé", "Fabián",
        //    "Renata"
        //};

        for (int i = 0; i < GetDataPlayers.instance.jugadores.Count; i++)
        {
            //int randomIndex = Random.Range(0, names.Length);
            GameObject playerObject = new GameObject(GetDataPlayers.instance.jugadores[i].nombres);
            Player newPlayer = playerObject.AddComponent<Player>();
            // Initialize the player
            newPlayer.Initialize(GetDataPlayers.instance.jugadores[i].nombres, defaultPlayerPhoto, players.Count);
            AddPlayer(newPlayer);
        }

        Debug.Log(GetDataPlayers.instance.jugadores.Count + " dummy players have been created.");
    }

    public void RemoveLastPlayer()
    {
        if (players.Count > 0)
        {
            // Remove the last player from the list
            Player lastPlayer = players[players.Count - 1];
            players.RemoveAt(players.Count - 1);

            // Destroy the corresponding player card in the UI
            if (contentTransform.childCount > 0)
            {
                Transform lastPlayerCard = contentTransform.GetChild(contentTransform.childCount - 1);
                Destroy(lastPlayerCard.gameObject);
            }

            // Update UI and game logic
            UpdatePlayerCountText();
            UpdatePlayersPerTeam();
            CheckForPlayersCount();
        }
        else
        {
            Debug.LogWarning("No players to remove.");
        }
    }


    public void UpdatePlayersPerTeam()
    {
        if (players.Count == 2)
        {
            playersPerTeam = 1;  // 1 jugador por equipo
        }
        else if (players.Count == 4)
        {
            playersPerTeam = 2;  // 2 jugadores por equipo
        }
        else if (players.Count == 6)
        {
            playersPerTeam = 3;  // 3 jugadores por equipo
        }
        else if (players.Count == 12)
        {
            playersPerTeam = 6; // 12 jugadores por equipo
        }
        else
        {
            playersPerTeamText.text = $"Error: Numero de jugadores invalido ({players.Count}). Debe haber 2, 4, 6, o 12 jugadores.";
            playersPerTeamText.color = Color.red;
            Debug.LogWarning($"Error: Número de jugadores inválido ({players.Count}). Debe haber 2, 4, 6, o 12 jugadores.");
            return;
        }

        playersPerTeamText.text = $"Los jugadores se distribuiran en <b>2</b> equipos.\nExistiran <b>{playersPerTeam}</b> jugador/es por equipo";
        playersPerTeamText.color = Color.white;
    }

    public void UpdateDraggablePlayerCards()
    {
        // Clear existing draggable cards before adding new ones
        DestroyChildren(draggableContentTransform);

        // Create a draggable player card for each player
        foreach (Player player in players)
        {
            // Instantiate the draggable player card prefab
            DraggableUIElement draggableCard = Instantiate(draggablePlayerCardPrefab, draggableContentTransform).GetComponent<DraggableUIElement>();
            PlayerCard playerCard = draggableCard.GetComponent<PlayerCard>();
            playerCard.SetPlayerData(player);

            // Assuming DraggableUIElement has a method to set player data
            draggableCard.SetPlayerData(player);
        }
    }

    public void StartPlayerTeamImages()
    {
        team1.Clear(); team2.Clear(); // Clear the teams
        DestroyChildren(team1SectionTransform); DestroyChildren(team2SectionTransform);
        for (int i = 0; i < playersPerTeam; i++)
        {
            PlayerTeamImageCard team1Card = Instantiate(playerTeamEmptyPrefab, team1SectionTransform).GetComponent<PlayerTeamImageCard>();
            PlayerTeamImageCard team2Card = Instantiate(playerTeamEmptyPrefab, team2SectionTransform).GetComponent<PlayerTeamImageCard>();

            team1Card.team = PlayerTeamImageCard.Team.Team1;
            team2Card.team = PlayerTeamImageCard.Team.Team2;
        }

        GridLayoutGroup team1LayoutGroup = team1SectionTransform.GetComponent<GridLayoutGroup>();
        GridLayoutGroup team2LayoutGroup = team2SectionTransform.GetComponent<GridLayoutGroup>();

        // Set the cell size and constraint based on the number of players per team
        switch (playersPerTeam)
        {
            case 1:
                team1LayoutGroup.cellSize = new Vector2(240, 240);
                team2LayoutGroup.cellSize = new Vector2(240, 240);
                team1LayoutGroup.constraint = GridLayoutGroup.Constraint.Flexible;
                team2LayoutGroup.constraint = GridLayoutGroup.Constraint.Flexible;
                break;
            case 2:
                team1LayoutGroup.cellSize = new Vector2(200, 200);
                team2LayoutGroup.cellSize = new Vector2(200, 200);
                team1LayoutGroup.constraint = GridLayoutGroup.Constraint.Flexible;
                team2LayoutGroup.constraint = GridLayoutGroup.Constraint.Flexible;
                break;
            case 3:
                team1LayoutGroup.cellSize = new Vector2(120, 120);
                team2LayoutGroup.cellSize = new Vector2(120, 120);
                team1LayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                team2LayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                team1LayoutGroup.constraintCount = 3;
                team2LayoutGroup.constraintCount = 3;
                break;
            case 6:
                team1LayoutGroup.cellSize = new Vector2(120, 120);
                team2LayoutGroup.cellSize = new Vector2(120, 120);
                team1LayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                team2LayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                team1LayoutGroup.constraintCount = 3;
                team2LayoutGroup.constraintCount = 3;
                break;
        }

    }

    public PlayerTeamImageCard GetNextEmptyPlayerTeamImageCard()
    {
        // Search inside the team1 and team2 sections for the next empty player team image card
        foreach (Transform child in team1SectionTransform)
        {
            PlayerTeamImageCard playerTeamImageCard = child.GetComponent<PlayerTeamImageCard>();
            if (playerTeamImageCard.player == null)
            {
                return playerTeamImageCard;
            }
        }
        // Same for team2
        foreach (Transform child in team2SectionTransform)
        {
            PlayerTeamImageCard playerTeamImageCard = child.GetComponent<PlayerTeamImageCard>();
            if (playerTeamImageCard.player == null)
            {
                return playerTeamImageCard;
            }
        }

        return null;
    }

    public List<Player> GetTeam1Players()
    {
        return team1;
    }
    public List<Player> GetTeam2Players()
    {
        return team2;
    }

    public void AddPlayerToTeam1(Player player)
    {
        if (!team1.Contains(player))
        {
            team1.Add(player);
            Debug.Log($"Player {player.playerName} added to Team 1");
        }
    }

    public void AddPlayerToTeam2(Player player)
    {
        if (!team2.Contains(player))
        {
            team2.Add(player);
            Debug.Log($"Player {player.playerName} added to Team 2");
        }
    }

    public void CheckIfAllPlayersInATeam()
    {
        if (team1.Count == playersPerTeam && team2.Count == playersPerTeam)
        {
            continueTeamAssignButton.SetActive(true);
            draggableCardsScrollView.SetActive(false);
        }
        else
        {
            continueTeamAssignButton.SetActive(false);
            draggableCardsScrollView.SetActive(true);
        }
    }

    private GameObject popupHolder;
    public void SpawnAnimatedPopUp(Transform parent, Vector2 size, string message, float duration)
    {
        StartCoroutine(SpawnAnimatedPopUpCoroutine(parent, size, message, duration));
    }

    public void SpawnGameModeInfoPopup(string info)
    {
        Transform canvasParent = monitorACanvas;
        // Instead of hardcoding the size, use the canvas size value
        Vector2 size = new Vector2(canvasParent.GetComponent<RectTransform>().rect.width / 1.45f, canvasParent.GetComponent<RectTransform>().rect.height / 1.45f);
        SpawnPopUpInstant(canvasParent, size, info);

        //float duration = .185f;
        //SpawnAnimatedPopUp(canvasParent, size, info, duration);
    }

    private void SpawnPopUpInstant(Transform parent, Vector2 size, string message)
    {
        GameObject popUp = Instantiate(popupPrefab, parent);

        RectTransform rectTransform = popUp.transform.GetChild(0).GetComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        if (popupHolder)
            Destroy(popupHolder);

        popupHolder = popUp;

        JumpInJumpOut jumpInJumpOut = popUp.transform.GetChild(0).GetComponent<JumpInJumpOut>();
        if (jumpInJumpOut)
            jumpInJumpOut.JumpIn();
    }
    // Starts a coroutine where the pop up rect size starts at 0 and grows to the desired size in a given duration
    private IEnumerator SpawnAnimatedPopUpCoroutine(Transform parent, Vector2 size, string message, float duration)
    {
        // Instantiate the popup prefab
        GameObject popUp = Instantiate(popupPrefab, parent);

        RectTransform rectTransform = popUp.GetComponent<RectTransform>();
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        if (popupHolder)
            Destroy(popupHolder);

        popupHolder = popUp;

        float timeCount = 0;
        while (timeCount < duration && popUp)
        {
            timeCount += Time.deltaTime;
            rectTransform.sizeDelta = Vector2.Lerp(Vector2.zero, size, timeCount / duration);
            yield return null;
        }     
    }

    #endregion

    // Funny name
    public static void DestroyChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }
}
