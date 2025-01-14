using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;

public class ArduinoReadWrite : MonoBehaviour
{
    private SerialPort arduinoSerial;
    [SerializeField] string portName;
    [SerializeField] GameObject robot, markerPrefab;
    [SerializeField] AccelerometerManager accelerometerManager;

    List<GameObject> markers = new List<GameObject>();

    float pingTime = 0;
    bool pingWaiting = false;
    float currentAngle = 0;
    bool moving = false;

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
                /*
                if (string.Equals(data.Substring(0, 1), "A"))
                {
                    InterpretMovementData(data);
                }*/
            }
            catch (System.TimeoutException)
            {
                Debug.LogWarning("Read timeout");
            }
        }

        if (Input.GetKey(KeyCode.W)) //make ordered structure with else stop
        {
            if(!moving)
            {
                arduinoSerial.WriteLine("forward");
                moving = true;
                arduinoSerial.BaseStream.Flush();
            }
        }
        else if (Input.GetKey(KeyCode.S))
        {
            if (!moving)
            {
                arduinoSerial.WriteLine("back");
                moving = true;
                arduinoSerial.BaseStream.Flush();
            }
        }
        else if (Input.GetKey(KeyCode.A))
        {
            if (!moving)
            {
                arduinoSerial.WriteLine("left");
                moving = true;
                arduinoSerial.BaseStream.Flush();
            }
        }
        else if (Input.GetKey(KeyCode.D))
        {
            if (!moving)
            {
                arduinoSerial.WriteLine("left");
                moving = true;
                arduinoSerial.BaseStream.Flush();
            }
        }
         else if(moving)
        {
            arduinoSerial.WriteLine("stop");
            moving = false;
            arduinoSerial.BaseStream.Flush();
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
        char[] delimiterChars = { ' ', ',', '\t', 'A' };

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

        robot.transform.position += new Vector3(Mathf.Cos(currentAngle * Mathf.Deg2Rad) * distanceMoved, 0, Mathf.Sin(currentAngle * Mathf.Deg2Rad) * distanceMoved);

        robot.transform.rotation = Quaternion.Euler(0, currentAngle, 0);

        float noiseThreshold = 1000;

        if(ultraDistanceOne < noiseThreshold)
        {
            PlaceMarker(ultraDistanceOne, ultraAngleOne);
        }
        if (ultraDistanceTwo < noiseThreshold)
        {
            PlaceMarker(ultraDistanceTwo, ultraAngleTwo);
        }
        if (ultraDistanceThree < noiseThreshold)
        {
            PlaceMarker(ultraDistanceThree, ultraAngleThree);
        }
        if (ultraDistanceFour < noiseThreshold)
        {
            PlaceMarker(ultraDistanceFour, ultraAngleFour);
        }


        currentAngle = accelerometerManager.newRot.y; //check to make sure axis is right
    }

    void PlaceMarker(float ultraDistance, float ultraAngle)
    {
        Vector3 markerLocation = robot.transform.position + new Vector3(Mathf.Cos((currentAngle + ultraAngle) * Mathf.Deg2Rad) * ultraDistance, 0, Mathf.Sin((currentAngle + ultraAngle) * Mathf.Deg2Rad) * ultraDistance);

        GameObject newMarker = Instantiate(markerPrefab, transform.position, Quaternion.identity);

        markers.Add(newMarker);
    }
}
