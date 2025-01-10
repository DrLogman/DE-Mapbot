#include <Servo.h>
#include <SPI.h>
#include <WiFi.h>
#include <vector>

unsigned long last_time = 0;

char ssid[] = "LogOS";          //  your network SSID (name)
char pass[] = "00000000";   // your network password
int status = WL_IDLE_STATUS;

WiFiServer server(23);

const float wheelRadius = 3.3125; //cm
const float motorFrequency = 0.7849; //Hz

Servo frontLeft;
Servo frontRight;
Servo backLeft;
Servo backRight;

void setup() {
  delay(2000);

  Serial.begin(9600);

  Serial.println("Attempting to connect to WPA network...");
  Serial.print("SSID: ");
  Serial.println(ssid);

  status = WiFi.begin(ssid, pass);
  if ( status != WL_CONNECTED) {
    Serial.println("Couldn't get a wifi connection");
    while(true);
  }
  else {
    server.begin();
    Serial.print("Connected to wifi. My address:");
    IPAddress myAddress = WiFi.localIP();
    Serial.println(myAddress);
  }

  //delay(2000);
  //moveDistance(100);
}

void loop()
{
  if ( status != WL_CONNECTED) {
    status = WiFi.begin(ssid, pass);
    while(true);
  }
  WiFiClient client = server.available();
  while (client) 
  {
    if(client.available() > 0) 
    {
      int maxSize = 10;  // Initial buffer size (can be adjusted)
      char* serialInput = new char[maxSize];  // Dynamically allocate memory
      int currentSize = 0;    
      while (client.available() > 0) 
      {
        // Check if we need more space
        if (currentSize >= maxSize) 
        {
          maxSize *= 2;  // Double the buffer size
          char* temp = new char[maxSize];  // Allocate new larger buffer

          // Copy old data to the new buffer
          for (int i = 0; i < currentSize; i++) 
          {
            temp[i] = serialInput[i];
          }

          delete[] serialInput;  // Free the old buffer
          serialInput = temp;  // Point to the new buffer
        }

        serialInput[currentSize++] = client.read();  // Add the byte and increment the size
      }

      serialInput[currentSize] = '\0';
      Serial.println(serialInput);
      char pingString[] = "ping";
      
/*      
      if(strcmp(serialInput, pingString) == 0) 
      {
        client.println("ping");
        display.println("PINGING");
      }
*/

      client.flush();
    }
  }


    if (millis() > last_time + 2000)
    {
      Serial.println("Arduino is alive!!");
      last_time = millis();
    }
}

float getUltrasonic(int trigPin, int echoPin) {
  float distance = 0;

  digitalWrite(trigPin, HIGH);
  delay(10);
  digitalWrite(trigPin, LOW);

  float echoTime = pulseIn(echoPin, HIGH);

  distance = echoTime / 148.0;

  return distance;
}

float moveDistance(float targetDistance) {
  backLeft.attach(11);
  backRight.attach(10);
  frontLeft.attach(13);
  frontRight.attach(12);
  
  delay(500);

  float realDistance = 0.0;
  float timeIncrement = 500.0; //ms
  float incrementsCounted = 0.0;

  Serial.println("Moving");

  frontLeft.write(180);
  backLeft.write(180);
  frontRight.write(0);
  backRight.write(0);

  while(realDistance < targetDistance) {
    delayMicroseconds(timeIncrement);

    realDistance += (0.001 * timeIncrement * motorFrequency * wheelRadius * 2.0 * 3.14159);

    Serial.println(realDistance);

    incrementsCounted++;
  }

  Serial.print("Speed: ");
  Serial.println(realDistance * (incrementsCounted * timeIncrement)); //cm/s

  backLeft.detach();
  backRight.detach();
  frontLeft.detach();
  frontRight.detach();

  return realDistance;
}