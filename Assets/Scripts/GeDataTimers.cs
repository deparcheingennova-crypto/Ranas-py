using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GeDataTimers : MonoBehaviour
{
    public enum typeTimer
    {
         currentTime,
         tiemOfGame
    }

    public typeTimer type;

    private TMP_Text textTime;

    private void Start()
    {
        textTime = GetComponent<TMP_Text>();    
    }

    public void Update()
    {
        textTime.text = type == typeTimer.currentTime ? HandlerTimer.instance.timeCurrentGame : HandlerTimer.instance.timeOfGame;

        if (type == typeTimer.currentTime && HandlerTimer.instance.dataGameTime.timeCurrent >= (HandlerTimer.instance.dataGameTime.timeOfGame - HandlerTimer.instance.timeWaitOverGame))
            textTime.color = Color.red;
        else 
            if(type == typeTimer.currentTime)
                textTime.color = Color.green;
    }
}
