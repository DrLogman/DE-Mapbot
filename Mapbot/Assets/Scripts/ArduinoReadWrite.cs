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

    float currentAngle = 0;
    public int oldState = 0;
    public int newState = 0;

    public string[] splitData;

    public float distanceMoved, ultraDistanceOne, ultraAngleOne, ultraDistanceTwo, ultraAngleTwo, ultraDistanceThree, ultraAngleThree, ultraDistanceFour, ultraAngleFour;

    // Start is called before the first frame update
    void Start()
    {
        arduinoSerial = new SerialPort(portName, 9600);
        arduinoSerial.Open();
        arduinoSerial.ReadTimeout = 500;
        arduinoSerial.DtrEnable = true;
        arduinoSerial.BaseStream.Flush();

        StartCoroutine(CheckInputs());
    }

    IEnumerator CheckInputs()
    {
        while(true)
        {
            if (Input.GetKey(KeyCode.W)) //make ordered structure with else stop
            {
                arduinoSerial.WriteLine("forward");
            }
            if (Input.GetKey(KeyCode.S))
            {
                arduinoSerial.WriteLine("back");
            }
            if (Input.GetKey(KeyCode.A))
            {
                arduinoSerial.WriteLine("left");
            }
            if (Input.GetKey(KeyCode.D))
            {
                arduinoSerial.WriteLine("right");
            }
            yield return new WaitForSeconds(0.3f);
        }
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

                if(data.Length > 1)
                {
                    if (data.Substring(0, 1).Equals("A"))
                    {
                        InterpretMovementData(data.Substring(0));
                    }
                }
            }
            catch (System.TimeoutException)
            {
                Debug.LogWarning("Read timeout");
            }
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

        splitData = data.Split(delimiterChars);

        distanceMoved = float.Parse(splitData[1]);
        ultraDistanceOne = float.Parse(splitData[2]);
        ultraAngleOne = float.Parse(splitData[3]);
        ultraDistanceTwo = float.Parse(splitData[4]);
        ultraAngleTwo = float.Parse(splitData[5]);
        ultraDistanceThree = float.Parse(splitData[6]);
        ultraAngleThree = float.Parse(splitData[7]);
        ultraDistanceFour = float.Parse(splitData[8]);
        ultraAngleFour = float.Parse(splitData[9]);

        robot.transform.position += new Vector3(Mathf.Cos(currentAngle * Mathf.Deg2Rad) * distanceMoved, 0, Mathf.Sin(currentAngle * Mathf.Deg2Rad) * distanceMoved);

        robot.transform.rotation = Quaternion.Euler(0, currentAngle, 0);

        float noiseThreshold = 1000;
        float minValue = 0;

        if(ultraDistanceOne < noiseThreshold && ultraDistanceOne > minValue)
        {
            PlaceMarker(ultraDistanceOne, ultraAngleOne);
        }
        if (ultraDistanceTwo < noiseThreshold && ultraDistanceTwo > minValue)
        {
            PlaceMarker(ultraDistanceTwo, ultraAngleTwo);
        }
        if (ultraDistanceThree < noiseThreshold && ultraDistanceThree > minValue)
        {
            PlaceMarker(ultraDistanceThree, ultraAngleThree);
        }
        if (ultraDistanceFour < noiseThreshold && ultraDistanceFour > minValue)
        {
            PlaceMarker(ultraDistanceFour, ultraAngleFour);
        }


        currentAngle = accelerometerManager.newRot.y; //check to make sure axis is right
    }

    void PlaceMarker(float ultraDistance, float ultraAngle)
    {
        Vector3 markerLocation = robot.transform.position + new Vector3(Mathf.Cos((currentAngle + ultraAngle) * Mathf.Deg2Rad) * ultraDistance, 0, Mathf.Sin((currentAngle + ultraAngle) * Mathf.Deg2Rad) * ultraDistance);

        GameObject newMarker = Instantiate(markerPrefab, markerLocation, Quaternion.identity);

        markers.Add(newMarker);
    }
}
