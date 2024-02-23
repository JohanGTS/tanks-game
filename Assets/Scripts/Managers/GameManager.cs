using UnityEngine;
using System.Collections;
//using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public int m_NumRoundsToWin = 5;        
    public float m_StartDelay = 1f;         
    public float m_EndDelay = 1f;   

    public float tiempoRestante=60;        

    private float tiempoTotal=0f;

    public int m_NumRoundsToLose=10;
    public CameraControl m_CameraControl;   
    public Text m_MessageText;              
    public GameObject m_TankPrefab;         
    public TankManager[] m_Tanks;           

    public TMP_Text timer;

    private int m_RoundNumber;              
    private WaitForSeconds m_StartWait;     
    private WaitForSeconds m_EndWait;       
    private TankManager m_RoundWinner;
    private TankManager m_GameWinner;       

    private bool fin=false;
    public AudioSource audioSource;


    private void Start()
    {
    //Creamos los delays para que solo se apliquen una vez
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);
        tiempoTotal=0;
        SpawnAllTanks(); //Generar tanques
        SetCameraTargets(); //Ajustar cámara
        StartCoroutine(GameLoop()); //Iniciar juego
        
        //SceneManager.LoadScene(0);
    }

   void DisplayTime(float tiempo)
    {
        tiempo++;
        float minutos = Mathf.FloorToInt(tiempo / 60);
        float segundos = Mathf.FloorToInt(tiempo % 60);
        timer.text = string.Format("{0:00} : {1:00}", minutos, segundos);
    }

 private void SpawnAllTanks()
 {
 //Recorro los tanques...
    for (int i = 0; i < m_Tanks.Length; i++)
    {
        m_Tanks[i].m_Instance =
        Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
        m_Tanks[i].m_PlayerNumber = i + 1;
        m_Tanks[i].Setup();
    }
 }

void Update()
{
    if (tiempoRestante>=0)  
    {
        if(!fin)
        {
            
            tiempoRestante -= Time.deltaTime;
            DisplayTime(tiempoRestante);
        }
    }
    else
    {
        if (!fin)
            tiempoRestante=-1;
        //StartCoroutine(RoundEnding());
        //StartCoroutine(RoundEnding());
    }
    
}

// IEnumerator StartTimer()
//     {
//         while(timeRemaining>0)
//         { 
//             yield return new WaitForSeconds(1);
//             timeRemaining--;
//             updateTimerText();
//             Debug.Log("paso");
//         }
//         //ResetTimer();
//     }

// void updateTimerText()
// {
//     timer.text = string.Format("{0:00} : {1:00}", 0, tiempoRestante);
// }

    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[m_Tanks.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = m_Tanks[i].m_Instance.transform;
        }

        m_CameraControl.m_Targets = targets;
    }


    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());
        //yield return StartCoroutine(StartTimer());

        if (m_GameWinner != null)
        {
           // SceneManager.LoadScene(0);
        }
        else
        {
            StartCoroutine(GameLoop());
        }
   }


    private IEnumerator RoundStarting()
    {
        ResetAllTanks ();
        DisableTankControl ();
        //audioSource = GetComponent<AudioSource>();
        //audioSource.Play();
        //timeRemaining=5;
        //updateTimerText();
        fin=false;
        tiempoRestante=60;
        // Ajusto la cámara a los tanques resteteados.
        m_CameraControl.SetStartPositionAndSize ();
        // Incremento la ronda y muestro el texto informativo.
        m_RoundNumber++;
        m_MessageText.text = "ROUND " + m_RoundNumber;
        
        // Espero a que pase el tiempo de espera antes de volver al bucle.
        yield return m_StartWait;

    }


    private IEnumerator RoundPlaying()
    {
        EnableTankControl ();
        // Borro el texto de la pantalla.
        m_MessageText.text = string.Empty;
        // Mientras haya más de un tanque...
        while (!OneTankLeft())
        {
        // ... vuelvo al frame siguiente.
        yield return null;
        }

    }

    private IEnumerator RoundEnding()
    {
         DisableTankControl ();
        // Borro al ganador de la ronda anterior.
        m_RoundWinner = null;
        // Miro si hay un ganador de la ronda.
        string message ;
        if (true) //
        {
            m_RoundWinner = GetRoundWinner ();
            // Si lo hay, incremento su puntuación.
            if (m_RoundWinner != null)
            {
                m_RoundWinner.m_Wins++;
                IncrementaPerdedores(m_RoundWinner);
            }

            m_GameWinner = GetGameWinner ();
            // Genero el mensaje según si hay un gaandor del juego o no.
            message = EndMessage ();
        }
        else
        {
            IncrementaPerdedores(new TankManager());
            message="¡EL TIEMPO ACABÓ! TODOS LOS JUGADORES PIERDEN";
        }
        
        // Compruebo si alguien ha ganado el juego.
        //audioSource.Stop();
        m_MessageText.text = message;
        tiempoTotal+= 60-tiempoRestante;
        fin=true;
        // Espero a que pase el tiempo de espera antes de volver al bucle.
        yield return m_EndWait;
    }

    private bool OneTankLeft()
    {
        int numTanksLeft = 0;

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                numTanksLeft++;
        }

        return numTanksLeft <= 1;
    }

    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }

        return null;
    }

    private void IncrementaPerdedores(TankManager roundWinner)
{       
    for (int i = 0; i < m_Tanks.Length; i++)
    {
        if (m_Tanks[i] != roundWinner )
        {
            m_Tanks[i].m_Losses++;
        }
    }
}


    // private TankManager GetGameWinner()
    // {
    //     for (int i = 0; i < m_Tanks.Length; i++)
    //     {
    //         if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
    //             return m_Tanks[i];
    //     }

    //     return null;
    // }

private TankManager GetGameWinner()
{
    TankManager mostWinsPlayer = m_Tanks[0];
    TankManager mostLossesPlayer = m_Tanks[0];

    for (int i = 1; i < m_Tanks.Length; i++)
    {
        if (m_Tanks[i].m_Wins > mostWinsPlayer.m_Wins)
        {
            mostWinsPlayer = m_Tanks[i];
        }

        if (m_Tanks[i].m_Losses > mostLossesPlayer.m_Losses)
        {
            mostLossesPlayer = m_Tanks[i];
        }
    }



    if (mostLossesPlayer.m_Losses >= m_NumRoundsToLose)
    {

        return mostWinsPlayer;
    }

    return null;
}


    private string EndMessage()
    {
        string message = "EMPATE!";
        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " GANA LA RONDA!";
        
        message += "\n\n\n\n";
        for (int i = 0; i < m_Tanks.Length; i++)
        {
        message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " GANA\n";
        }

        if (m_GameWinner != null)
            {
                StopCoroutine(RoundStarting());
                StopCoroutine(RoundPlaying());
                StopCoroutine(RoundEnding());
                StopCoroutine(GameLoop());
                message = m_GameWinner.m_ColoredPlayerText + " GANA EL JUEGO!";
                message += "\n\n";

                float minutos = Mathf.FloorToInt(tiempoTotal / 60);
                float segundos = Mathf.FloorToInt(tiempoTotal % 60);
                message+= "TIEMPO USADO: ";
                
                message+= string.Format("{0:00}m {1:00}s", minutos, segundos);

                
            }
        return message;
    }

    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].Reset();
        }
    }


    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].EnableControl();
        }
    }


    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].DisableControl();
        }
    }
}