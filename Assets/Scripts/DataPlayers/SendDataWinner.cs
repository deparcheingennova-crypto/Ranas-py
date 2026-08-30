using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using static GetDataPlayers;

public class SendDataWinner : MonoBehaviour
{
    [System.Serializable]
    public class HistorialData
    {
        public string gana_equipo;
        public string[] jugadores;
        public int id_partida;
        public string juego;
        public string fecha;
        public string winner;
        public string local;
        public int lineas;
        public int idApi;
    }

    public class GameData
    {
        public int action;
        public int id_api;
        public int cod_juego;
    }

    public class UnyrealsoftData
    {
        public string game_state_id;
        public string console_state_id;
        public string reserve_id;
    }

    public string urlSendData;
    [Header("Data Unyrealsoft")]
    public string urlUnyrealsoft;
    public string tokenApi;
    private List<string> nameJugadores = new List<string>();

    public static SendDataWinner instance;
    public DataGameTime gameTime;

    private void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(instance);
    }

    public void SendDataGame(string equipo)
    {

        StartCoroutine(SendData(equipo));

        if (gameTime.isTimeOver)
        {
            //StartCoroutine(SendDataUnyreal());
            StartCoroutine(PowerOffGame());
        }

    }

    public void OnApplicationQuit()
    {
        Debug.Log("La aplicación se cerro enviar datos de cierre");
        gameTime.timeCurrent = 0;
        gameTime.timeOfGame = 0;
    }

    IEnumerator PowerOffGame()
    {

        yield return new WaitForSeconds(2f);
        FindObjectOfType<KeyboardSimulator>().PressQKey();

        GameData dataGame = new GameData
        {
            action = 0,
            id_api = GetDataPlayers.instance.idApi,
            cod_juego = GetDataPlayers.instance.codJuego
        };

        string jsonData = JsonUtility.ToJson(dataGame);
        Debug.Log("PowerOffGame JSON enviado: " + jsonData);

        UnityWebRequest www = new UnityWebRequest("https://distrito-e.com.co/api/power_of_game", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("x-api-ingennova", GetDataPlayers.instance.apiKey);

        www.timeout = 10;
        yield return www.SendWebRequest();

        Debug.Log("PowerOffGame HTTP Code: " + www.responseCode);
        Debug.Log("PowerOffGame Response: " + www.downloadHandler.text);

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("PowerOffGame enviado correctamente");
            //Application.Quit();
        }
        else
        {
            Debug.LogError("Error PowerOffGame: " + www.error);
            Debug.LogError("Respuesta backend: " + www.downloadHandler.text);
        }

        gameTime.timeCurrent = 0;
        gameTime.timeOfGame = 0;

        Application.Quit();
    }

    IEnumerator SendData(string winEquipo)
    {
        nameJugadores.Clear();

        foreach (var jugadorName in GetDataPlayers.instance.jugadores)
        {
            nameJugadores.Add(jugadorName.id_jugador.ToString());
        }

        HistorialData data = new HistorialData
        {
            gana_equipo = winEquipo,
            jugadores = nameJugadores.ToArray(),
            id_partida = GetDataPlayers.instance.idPartida,
            juego = GetDataPlayers.instance.nameGame + " " + GetDataPlayers.instance.codJuego,
            fecha = System.DateTime.Now.ToString("yyyy-MM-dd"),
            winner = GetDataPlayers.instance.nameEquipo,
            local = GetDataPlayers.instance.localName,
            lineas = GetDataPlayers.instance.sesiones,
            idApi = GetDataPlayers.instance.idApi
        };

        string jsonData = JsonUtility.ToJson(data);

        UnityWebRequest www = new UnityWebRequest(urlSendData, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("x-api-ingennova", GetDataPlayers.instance.apiKey);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Datos enviados con éxito");
        }
        else
        {
            Debug.LogError("Error al enviar datos: " + www.error);
        }
    }
}
