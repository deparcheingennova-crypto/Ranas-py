using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Text;
using TMPro;
using UnityEngine;

public class HandlerTimer : MonoBehaviour
{
    public DataGameTime dataGameTime;
    public int timeWaitOverGame = 10; // normalmente debe estar en 10
    public string timeOfGame;
    public string timeCurrentGame;

    public static HandlerTimer instance;

    //variables para lectura shared memory

    [System.Serializable]
    public class ObjectTime
    {
        public int min;
        public int seg;
        public bool timeOver;
    }

    public ObjectTime objectTime = new ObjectTime();
    private MemoryMappedFile mmf;
    private MemoryMappedViewAccessor accessor;
    private const float StartupDelaySeconds = 60f;
    private float timerStartedAtRealtime;

    public void Awake()
    {

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(WaitOfData());
        timeCurrentGame = $"Tiempo: {dataGameTime.timeCurrent} min";

        mmf = MemoryMappedFile.CreateOrOpen("GameTime", 1024);
        accessor = mmf.CreateViewAccessor();

        objectTime.min = 0;
        objectTime.seg = 0;
        objectTime.timeOver = false;

        WriteSharedMemory(objectTime);
        // Time.realtimeSinceStartup pertenece a toda la ejecucion de Unity.
        // Usar el segundo 60 como origen evita reinicios al cambiar de partida/escena.
        timerStartedAtRealtime = StartupDelaySeconds;
    }

    private void WriteSharedMemory(ObjectTime timeData)
    {
        string json = JsonUtility.ToJson(timeData);

        byte[] bytes = Encoding.UTF8.GetBytes(json);

        if (bytes.Length > 1020)
        {
            Debug.LogError("JSON demasiado grande.");
            return;
        }

        accessor.Write(0, bytes.Length);
        accessor.WriteArray(4, bytes, 0, bytes.Length);
    }

    public void OnApplicationQuit()
    {
        accessor?.Dispose();
        mmf?.Dispose();
    }

    IEnumerator WaitOfData()
    {
        yield return new WaitUntil(() =>
    GetDataPlayers.instance != null &&
    GetDataPlayers.instance.isPlayerLoading
);

        if (!dataGameTime.isDataReady)
        {
            dataGameTime.timeOfGame = GetDataPlayers.instance.sesiones * 30;
            dataGameTime.isDataReady = true;
        }

        timeOfGame = $"Tiempo de juego: {dataGameTime.timeOfGame} min";
        StartCoroutine(StartTimer());
    }

    IEnumerator StartTimer()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(0.25f);

            int elapsedSeconds = Mathf.Max(
                0,
                Mathf.FloorToInt(Time.realtimeSinceStartup - timerStartedAtRealtime)
            );
            dataGameTime.timeCurrent = elapsedSeconds / 60;
            int contSeg = elapsedSeconds % 60;

            timeCurrentGame = $"Tiempo: {dataGameTime.timeCurrent:00} : {contSeg:00} min";

            if (dataGameTime.timeCurrent >= (dataGameTime.timeOfGame - 2))
                dataGameTime.isTimeOver = true;

            objectTime.min = dataGameTime.timeCurrent;
            objectTime.seg = contSeg;
            objectTime.timeOver = dataGameTime.isTimeOver;

            WriteSharedMemory(objectTime);
        }

    }
}
