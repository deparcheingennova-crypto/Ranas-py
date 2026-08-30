using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GetDataPlayers : MonoBehaviour
{
    public int codJuego;
    public string versionGame;
    public string urlGetData;
    public string apiKey;
    public string nameGame;
    public string localName;

    [Header("Data configGame")]
    public TMP_Text infoGame;
    public GameObject windowConfig;
    public TMP_InputField inputFieldCod;

    [Serializable]
    public class Jugador
    {
        public int id_jugador;
        public bool leader;
        public string nombres;
        public string apellidos;
        public string email;
        public int estado;
    }

    [Serializable]
    public class DataGrupo
    {
        public int id_grupo;
        public int cant_jugadores;
        public string nombre_grupo;
        public List<Jugador> jugadores;
    }

    [Serializable]
    public class Root
    {
        public int id_partida;
        public int estado;
        public int id_api;
        public int lineas;
        public int sesiones;
        public DataGrupo data_grupo;
    }

    [Space(20)]
    [Header("Datos de partida")]
    public int idPartida;
    public int estadoPartida;
    public int idApi;
    public int lineas;
    public int sesiones;

    [Header("Grupo")]
    public int idGrupo;
    public int cantJugadores;
    public string nameEquipo;
    public List<Jugador> jugadores = new List<Jugador>();

    private string json;
    private string baseUrlGetData;
    public static GetDataPlayers instance;
    private Jugador demoData;
    public bool isPlayerLoading = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        baseUrlGetData = urlGetData;
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        UpdateInfoGame();
        StartCoroutine(GetData());
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshInfoGame();
        UpdateInfoGame();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            windowConfig.SetActive(true);
            int codGame = PlayerPrefs.GetInt("cod_juego");
            inputFieldCod.text = codGame.ToString();
        }
    }


    public void UpdateInfoGame()
    {
        if (!PlayerPrefs.HasKey("cod_juego"))
        {
            PlayerPrefs.SetInt("cod_juego", 5001);
            PlayerPrefs.Save();
        }
        else
            codJuego = PlayerPrefs.GetInt("cod_juego");

        infoGame.text = $"Cod Game: {codJuego} Versión {versionGame}";
    }

    private void RefreshInfoGame()
    {
        Debug.Log("Actualizando referencias UI");

        // Buscar TMP_Text aunque esté desactivado
        TMP_Text[] texts = FindObjectsOfType<TMP_Text>(true);

        foreach (TMP_Text txt in texts)
        {
            if (txt.name == "InfoGame")
            {
                infoGame = txt;
                break;
            }
        }

        // Buscar InputField aunque esté desactivado
        TMP_InputField[] inputs = FindObjectsOfType<TMP_InputField>(true);

        foreach (TMP_InputField input in inputs)
        {
            if (input.name == "inputCod")
            {
                inputFieldCod = input;
                break;
            }
        }

        // Buscar WindowConfig aunque esté desactivado
        GameObject[] gos = FindObjectsOfType<GameObject>(true);

        foreach (GameObject go in gos)
        {
            if (go.name == "WindowConfig")
            {
                windowConfig = go;
                break;
            }
        }

        Debug.Log(infoGame != null
            ? "infoGame encontrado"
            : "infoGame NO encontrado");
    }

    public void SaveCodGame()
    {
        string codNew = inputFieldCod.text;
        PlayerPrefs.SetInt("cod_juego", int.Parse(codNew));
        windowConfig.SetActive(false);
        UpdateInfoGame();

    }


    IEnumerator GetData()
    {
        Debug.Log("Trayendo Jugadores");
        string requestUrl = $"{urlGetData}?cod_juego={codJuego}";

        UnityWebRequest www = UnityWebRequest.Get(requestUrl);
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("x-api-ingennova", apiKey);

        www.timeout = 10;
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            try
            {
                json = www.downloadHandler.text;
                Root datos = JsonUtility.FromJson<Root>(json);

                idPartida = datos.id_partida;
                idApi = datos.id_api;
                lineas = datos.lineas;
                sesiones = datos.sesiones;
                estadoPartida = datos.estado;
                idGrupo = datos.data_grupo.id_grupo;
                cantJugadores = datos.data_grupo.cant_jugadores;
                nameEquipo = datos.data_grupo.nombre_grupo;
                jugadores = datos.data_grupo.jugadores ?? new List<Jugador>();

                isPlayerLoading = true;
            }
            catch (Exception e)
            {
                Debug.LogError("Error parseando respuesta: " + e.Message);
            }

            //if (idApi != gameDataLocal.idApi)
            //    SendDataWinner.instance.CloseBeforeGame();

        }

        if (jugadores.Count % 2 != 0 && idPartida != 0)
        {
            demoData.id_jugador = jugadores.Count;
            demoData.leader = false;
            demoData.nombres = "GamePC";
            demoData.apellidos = "";
            demoData.email = "demo@gmail.com";
            demoData.estado = 1;
            jugadores.Add(demoData);
        }

        if (jugadores.Count == 0 || idPartida == 0)
        {
            jugadores.Clear();
            demoData = new Jugador();

            idPartida = 9999;
            idApi = 9999;
            lineas = 8;
            sesiones = 1;  //modificar si es necesario más tiempo
            estadoPartida = 1;
            idGrupo = 9999;
            cantJugadores = 8;
            nameEquipo = "demo";

            for (int i = 0; i < 2; i++)
            {
                demoData.id_jugador = i;
                demoData.leader = false;
                demoData.nombres = "jugador " + (i + 1);
                demoData.apellidos = "";
                demoData.email = "demo@gmail.com";
                demoData.estado = 1;
                jugadores.Add(demoData);
                demoData = new Jugador();
            }

            isPlayerLoading = true;
        }
    }

    public void ManuallyGetData()
    {
        StartCoroutine(GetData());
    }
}
