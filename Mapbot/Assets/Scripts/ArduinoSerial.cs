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
    [SerializeField] string serialMessage;
    [SerializeField] bool sendPing = false;
    [SerializeField] GameObject cube;
    float pingTime = 0;
    bool pingWaiting = false;
    [SerializeField] bool sendBool = false;
    int count = 0;
    float timePassed = 0;
    Vector3 basePos;
    bool doneCal = false; //prevents stuff from moving during calibration
    Vector3 priorAcceleration;

    // Start is called before the first frame update
    void Start()
    {
        //SetUpArduinoSerial();

        SetUpAccelerometerSerial();

        basePos = new Vector3(0, 0, 0);

        priorAcceleration = new Vector3(0, 0, 0);

        SendSerialHex("FFAA030700"); //set frequency
        //SendSerialHex("FFAA960100"); //set displacement + rot
        SendSerialHex("FFAA960000"); //set acc + rot
        StartCoroutine(CalibrateAccelerometer(5));
    }
    void Update()
    {
        timePassed += Time.deltaTime;

        AccelerometerUpdate();

        
        arduinoSerial.BaseStream.Flush();
        accelerometerSerial.BaseStream.Flush();
    }

    void AccelerometerUpdate()
    {
        if (accelerometerSerial.BytesToRead > 0)
        {
            try
            {
                byte[] dataReceived = ReceiveData();

                if (doneCal)
                {
                    Vector3 filteredAcceleration = GetFilteredAcceleration(dataReceived, 0.95f);

                    float[,] rotationMatrix = GetRotationMatrix(dataReceived);

                    Vector3 dynamicAcceleration = GetDynamicAcceleration(filteredAcceleration, rotationMatrix);

                    Vector3 integratedVelocity = timePassed * dynamicAcceleration;

                    Vector3 integratedDisplacement = timePassed * dynamicAcceleration / 2;

                    timePassed = 0;

                    cube.transform.position += integratedDisplacement;

                    cube.transform.rotation = GetRotationQuaternion(dataReceived);
                }

            }
            catch (System.TimeoutException)
            {
                Debug.LogWarning("Read timeout");
            }
        }
    }

    void ArduinoUpdate()
    {
        if (arduinoSerial.BytesToRead > 0)
        {
            try
            {
                string data = arduinoSerial.ReadLine();

                Debug.Log("Received from Arduino: " + data);

            }
            catch (System.TimeoutException)
            {
                Debug.LogWarning("Read timeout");
            }
        }
    }

    void SetUpArduinoSerial()
    {
        arduinoSerial = new SerialPort(arduinoPortName, 9600);
        arduinoSerial.Open();
        arduinoSerial.ReadTimeout = 500;
        arduinoSerial.DtrEnable = true;
        arduinoSerial.BaseStream.Flush();
    }

    void SetUpAccelerometerSerial()
    {
        accelerometerSerial = new SerialPort(accelerometerPortName, 9600, Parity.None, 8, StopBits.One);
        accelerometerSerial.Open();
        accelerometerSerial.ReadTimeout = 500;
        accelerometerSerial.DtrEnable = true;
        accelerometerSerial.BaseStream.Flush();
    }

    Vector3 GetFilteredAcceleration(byte[] dataReceived, float alpha) //test diff alpha values, must be < 1
    {
        Vector3 accelerationVector = (16 * 9.8f / 32768) * new Vector3((short)(((short)dataReceived[3] << 8) | dataReceived[2]), (short)(((short)dataReceived[5] << 8) | dataReceived[4]), (short)(((short)dataReceived[7] << 8) | dataReceived[6]));

        Vector3 filteredAcceleration = (alpha * accelerationVector) + ((1 - alpha) * priorAcceleration);

        priorAcceleration = accelerationVector;

        return filteredAcceleration;
    }

    Vector3 GetDynamicAcceleration(Vector3 filteredAcceleration, float[,] rotationMatrix)
    {
        Vector3 dynamicAcceleration = filteredAcceleration - GetGravityVector(rotationMatrix);

        return dynamicAcceleration;
    }
    
    Quaternion GetRotationQuaternion(byte[] dataReceived)
    {
        Vector3 rotationVector = new Vector3((((short)dataReceived[15] << 8) | dataReceived[14]), (((short)dataReceived[17] << 8) | dataReceived[16]), (((short)dataReceived[19] << 8) | dataReceived[18]));
        Quaternion rotationQuaternion = Quaternion.Euler((rotationVector / 32768) * 180);
        return rotationQuaternion;
    }

    float[,] GetRotationMatrix(byte[] dataReceived)
    {
        Vector3 rotationVector = new Vector3((((short)dataReceived[15] << 8) | dataReceived[14]), (((short)dataReceived[17] << 8) | dataReceived[16]), (((short)dataReceived[19] << 8) | dataReceived[18])) * (Mathf.PI / 32768);

        float alpha = rotationVector.z;
        float beta = rotationVector.y;
        float gamma = rotationVector.x;

        float[,] rotationMatrix = new float[3, 3];

        rotationMatrix[0, 0] = Mathf.Cos(beta) * Mathf.Cos(gamma);
        rotationMatrix[1, 0] = Mathf.Cos(beta) * Mathf.Sin(gamma);
        rotationMatrix[2, 0] = -Mathf.Sin(beta);

        rotationMatrix[0, 1] = (Mathf.Sin(alpha) * Mathf.Sin(beta) * Mathf.Cos(gamma)) - (Mathf.Cos(alpha) * Mathf.Sin(gamma));
        rotationMatrix[1, 1] = (Mathf.Sin(alpha) * Mathf.Sin(beta) * Mathf.Sin(gamma)) + (Mathf.Cos(alpha) * Mathf.Cos(gamma));
        rotationMatrix[2, 1] = Mathf.Sin(alpha) * Mathf.Cos(beta);

        rotationMatrix[0, 2] = (Mathf.Cos(alpha) * Mathf.Sin(beta) * Mathf.Cos(gamma)) + (Mathf.Sin(alpha) * Mathf.Sin(gamma));
        rotationMatrix[1, 2] = (Mathf.Cos(alpha) * Mathf.Sin(beta) * Mathf.Sin(gamma)) - (Mathf.Sin(alpha) * Mathf.Cos(gamma));
        rotationMatrix[2, 2] = Mathf.Cos(alpha) * Mathf.Cos(beta);

        return rotationMatrix;
    }

    Vector3 GetGravityVector(float[,] rotationMatrix)
    {
        float g = -9.81f;

        Vector3 gravityVector = new Vector3
        (
            (g * rotationMatrix[2, 0]),
            (g * rotationMatrix[2, 1]),
            (g * rotationMatrix[2, 2])
        );
        
        return gravityVector;
    }

    void SendSerialHex(string hexString) //no spaces
    {
        byte[] messageToSend = new byte[5];
        for (int i = 0; i < 5; i++)
        {
            string selectedHex = hexString.Substring((2 * i), 2);
            messageToSend[i] = Convert.ToByte(selectedHex, 16);
        }

        accelerometerSerial.DiscardOutBuffer();
        accelerometerSerial.DiscardInBuffer();
        //Array.Reverse(messageToSend);
        // Send each byte with a small delay
        for (int i = 0; i < messageToSend.Length; i++)
        {
            accelerometerSerial.Write(new byte[] { messageToSend[i] }, 0, 1);
            System.Threading.Thread.Sleep(5); // Small delay between bytes (adjust as necessary)
        }

        //accelerometerSerial.WriteLine("");
        Debug.Log("Bytes Sent: " + BitConverter.ToString(messageToSend));
    }

    IEnumerator CalibrateAccelerometer(float waitTime)
    {
        SendSerialHex("FFAA010100");
        yield return new WaitForSeconds(waitTime);
        SendSerialHex("FFAA000000");
        SendSerialHex("FFAA010800");
        yield return new WaitForSeconds(1);
        SendSerialHex("FFAA000000");
        SendSerialHex("FFAA010400");
        doneCal = true;
    }

    byte[] ReceiveData()
    {
        byte[] dataReceived = new byte[40]; //longer to account for anything silly...
        accelerometerSerial.Read(dataReceived, 0, 40); //again...silly....
        string data = "";
        for (int i = 0; i < 25; i++)
        {
            data = data + " " + dataReceived[i];
        }

        return dataReceived;
    }

    void OnDestroy()
    {
        if (arduinoSerial != null && arduinoSerial.IsOpen)
        {
            arduinoSerial.Close();
        }
        if (accelerometerSerial != null && accelerometerSerial.IsOpen)
        {
            accelerometerSerial.Close();
        }
    }
}