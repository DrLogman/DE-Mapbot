using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;

public class ArduinoReadWrite : MonoBehaviour
{
    private SerialPort arduinoSerial;
    [SerializeField] string portName;
    [SerializeField] GameObject robot;
    [SerializeField] AccelerometerManager accelerometerManager;

    float pingTime = 0;
    bool pingWaiting = false;
    float currentAngle = 0;

    // Start is called before the first frame update
    void Start()
    {
        arduinoSerial = new SerialPort(portName, 9600);
        arduinoSerial.Open();
        arduinoSerial.ReadTimeout = 500;
        arduinoSerial.DtrEnable = true;
        arduinoSerial.BaseStream.Flush();
    }

    // Update is called once per frame
    void Update()
    {
        arduinoSerial.ReadTimeout = 500;
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

                if (data.Substring(0, 1) == "A")
                {
                    InterpretMovementData(data.Substring(1));
                }
            }
            catch (System.TimeoutException)
            {
                Debug.LogWarning("Read timeout");
            }
        }

        if (Input.GetKeyDown(KeyCode.W)) //make ordered structure with else stop
        {
            arduinoSerial.Write("forward");
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            arduinoSerial.Write("back");
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            arduinoSerial.Write("left");
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            arduinoSerial.Write("right");
        }
        else if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.D))
        {
            arduinoSerial.Write("stop");
        }

        arduinoSerial.BaseStream.Flush();
    }

    void OnDestroy()
    {
        if (arduinoSerial != null && arduinoSerial.IsOpen)
        {
            arduinoSerial.Close();
        }
    }

    void InterpretMovementData(string data)
    {
        char[] delimiterChars = { ' ', ',', '\t' };

        string[] splitData = data.Split(delimiterChars);

        float distanceMoved = float.Parse(splitData[0]);
        float ultraDistanceOne = float.Parse(splitData[1]);
        float ultraAngleOne = float.Parse(splitData[2]);
        float ultraDistanceTwo = float.Parse(splitData[3]);
        float ultraAngleTwo = float.Parse(splitData[4]);
        float ultraDistanceThree = float.Parse(splitData[5]);
        float ultraAngleThree = float.Parse(splitData[6]);
        float ultraDistanceFour = float.Parse(splitData[7]);
        float ultraAngleFour = float.Parse(splitData[8]);

        robot.transform.position += new Vector3(Mathf.Cos(currentAngle) * distanceMoved, 0, Mathf.Sin(currentAngle) * distanceMoved);

        robot.transform.rotation = Quaternion.Euler(0, currentAngle, 0);


        //organize how data is sent and recieved, also save rotation when sending move signal and then determine polar using that angle and the subsequent disp, then set new angle and repeat.

        currentAngle = accelerometerManager.newRot.y; //check to make sure axis is right
    }
}
