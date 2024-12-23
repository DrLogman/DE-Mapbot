using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;

public class ArduinoSerial : MonoBehaviour
{
    private SerialPort arduinoSerial, accelerometerSerial;
    [SerializeField] string arduinoPortName, accelerometerPortName;
    [SerializeField] string inputString;
    [SerializeField] bool sendPing = false;
    [SerializeField] string serialMessage;
    [SerializeField] GameObject cube;
    float pingTime = 0;
    bool pingWaiting = false;
    [SerializeField] bool sendBool = false;
    int count = 0;
    float timePassed = 0;

    // Start is called before the first frame update
    void Start()
    {
        /*
        arduinoSerial = new SerialPort(arduinoPortName, 9600);
        arduinoSerial.Open();
        arduinoSerial.ReadTimeout = 10;
        arduinoSerial.DtrEnable = true;
        arduinoSerial.BaseStream.Flush();
        */
        accelerometerSerial = new SerialPort(accelerometerPortName, 9600);
        accelerometerSerial.Open();
        accelerometerSerial.ReadTimeout = 10;
        accelerometerSerial.DtrEnable = true;
        accelerometerSerial.BaseStream.Flush();

        //make it callibrate and set settings here thru serial
    }

    // Update is called once per frame
    void Update()
    {
        //arduinoSerial.ReadTimeout = 500;
        accelerometerSerial.ReadTimeout = 500;
        timePassed += Time.deltaTime;
        if(accelerometerSerial.BytesToRead > 0)
        {
            try
            {
                byte[] dataReceived = new byte[40]; //longer to account for anything silly...
                accelerometerSerial.Read(dataReceived, 0, 40); //again...silly....
                string data = "";
                for(int i = 0; i < 25; i++)
                {
                    data = data + " " + dataReceived[i];
                }
                Debug.Log("Received from Acc: " + data);
                Debug.Log(((short)dataReceived[3] << 8) | dataReceived[2]);

                Vector3 displacementVector = new Vector3((short)(((short)dataReceived[3] << 8) | dataReceived[2]), (short)(((short)dataReceived[5] << 8) | dataReceived[4]), /*(short)(((short)dataReceived[7] << 8) | dataReceived[6])*/ 0);

                //get time passed from serial, figure out negative wrapping


                cube.transform.position = (displacementVector / 10);
                timePassed = 0;

                Vector3 rotationVector = new Vector3((((short)dataReceived[15] << 8) | dataReceived[14]), (((short)dataReceived[17] << 8) | dataReceived[16]), (((short)dataReceived[19] << 8) | dataReceived[18]));
                cube.transform.rotation = Quaternion.Euler((rotationVector / 32768) * 180);
            }
            catch (System.TimeoutException)
            {
                Debug.LogWarning("Read timeout");
            }
        }
        if(sendBool)
        {
            sendBool = false;
            accelerometerSerial.WriteLine(serialMessage);
            serialMessage = "";
        }
        /*
        if (arduinoSerial.BytesToRead > 0)
        {
            try
            {
                string data = arduinoSerial.ReadLine();

                Debug.Log("Received from Arduino: " + data);

                if (data == "ping")
                {
                    Debug.Log("Ping: " + (int)(pingTime) + " ms");
                    arduinoSerial.WriteLine("Ping: " + (int)(pingTime) + " ms");
                }
            }
            catch (System.TimeoutException)
            {
                Debug.LogWarning("Read timeout");
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            arduinoSerial.WriteLine(inputString);
            inputString = null;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            arduinoSerial.WriteLine("Forward");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            arduinoSerial.WriteLine("Backward");
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            arduinoSerial.WriteLine("Left");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            arduinoSerial.WriteLine("Right");
        }
        if (sendPing)
        {
            arduinoSerial.Write("ping");
            pingTime = 0;
            pingWaiting = true;
            sendPing = false;
        }
        if (pingWaiting)
        {
            pingTime += (Time.deltaTime * 1000);
        }*/
        //arduinoSerial.BaseStream.Flush();
        accelerometerSerial.BaseStream.Flush();
    }

    void OnDestroy()
    {
        /*
        if (arduinoSerial != null && arduinoSerial.IsOpen)
        {
            arduinoSerial.Close();
        }
        */
        if (accelerometerSerial != null && accelerometerSerial.IsOpen)
        {
            accelerometerSerial.Close();
        }
    }
}