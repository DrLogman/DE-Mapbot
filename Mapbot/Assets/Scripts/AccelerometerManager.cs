using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;

public class AccelerometerManager : MonoBehaviour
{
    [SerializeField] string accelerometerPortName;
    SerialPort accelerometerSerial;
    public bool doneCal = false;
    bool initialRotFound = false;
    public Vector3 newRot;
    Vector3 initialRot;
    public Vector3 accelerationVector;
    void Start()
    {
        SetUpAccelerometerSerial();
        SendSerialHex("FFAA030700"); //set frequency
        SendSerialHex("FFAA960000"); //set acc + rot
        StartCoroutine(CalibrateAccelerometer(3, 3));
    }

    void Update()
    {
        AccelerometerUpdate();

        accelerometerSerial.BaseStream.Flush();
    }
    void SetUpAccelerometerSerial()
    {
        accelerometerSerial = new SerialPort(accelerometerPortName, 9600, Parity.None, 8, StopBits.One);
        accelerometerSerial.Open();
        accelerometerSerial.ReadTimeout = 500;
        accelerometerSerial.DtrEnable = true;
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
                    if (!initialRotFound)
                    {
                        initialRot = new Vector3((((short)dataReceived[17] << 8) | dataReceived[16]), -(((short)dataReceived[19] << 8) | dataReceived[18]), (((short)dataReceived[15] << 8) | dataReceived[14]));
                        initialRotFound = true;
                    }
                    newRot = GetRotationQuaternion(dataReceived).eulerAngles;
                    //accelerationVector = ((16f) / 32768f) * new Vector3((short)(((short)dataReceived[3] << 8) | dataReceived[2]), (short)(((short)dataReceived[5] << 8) | dataReceived[4]), (short)(((short)dataReceived[7] << 8) | dataReceived[6]));
                }
                //took out g const from calc
            }
            catch (System.TimeoutException)
            {
                Debug.LogWarning("Read timeout");
            }
        }
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
    IEnumerator CalibrateAccelerometer(float waitTime1, float waitTime2)
    {
        SendSerialHex("FFAA010100");
        yield return new WaitForSeconds(waitTime1);
        SendSerialHex("FFAA000000");
        SendSerialHex("FFAA000000");
        SendSerialHex("FFAA010400");
        SendSerialHex("FFAA010800");
        yield return new WaitForSeconds(waitTime2);
        SendSerialHex("FFAA000000");
        SendSerialHex("FFAA010400");
        SendSerialHex("FFAA000000");
        doneCal = true;
    }
    Quaternion GetRotationQuaternion(byte[] dataReceived)
    {
        Vector3 rotationVector = new Vector3((((short)dataReceived[17] << 8) | dataReceived[16]), -(((short)dataReceived[19] << 8) | dataReceived[18]), (((short)dataReceived[15] << 8) | dataReceived[14])) - initialRot;
        Quaternion rotationQuaternion = Quaternion.Euler((rotationVector / 32768) * 180);
        return rotationQuaternion;
    }
}
