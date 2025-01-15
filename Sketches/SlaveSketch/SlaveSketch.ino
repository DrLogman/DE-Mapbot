#include <Servo.h>
#include <SPI.h>
#include <WiFi.h>
#include <vector>
#include <string>
#include <iostream>
#include <algorithm>
#include <cstring>

unsigned long last_time = 0;

const char ssid[] = "GL-AXT1800-514";          //  your network SSID (name)
const char pass[] = "HASY2D98PT";   // your network password
int status = WL_IDLE_STATUS;

WiFiServer server(23);

WiFiClient client;

const float wheelRadius = 3.3125; //cm
const float calculatedSpeed = 16.4; //cm/s

Servo ultraServo;

class UltrasonicSensor{ //finish this
  public:
  float initialAngle;

  public:
  int trigPin;

  public:
  int echoPin;

  public:
  float averageDist = 0;

  public:
  UltrasonicSensor(float inAngle, int trig, int echo) {
    initialAngle = inAngle;
    trigPin = trig;
    echoPin = echo;
  }
};

UltrasonicSensor UltraOne(180, 4, 5);
UltrasonicSensor UltraTwo(90, 6, 7);
UltrasonicSensor UltraThree(0, 8, 9);
UltrasonicSensor UltraFour(270, 2, 3);

Servo frontLeft;
Servo frontRight;
Servo backLeft;
Servo backRight;

int state = 0; //0 is waiting, 1 is moving forward, 2 is moving backward, 3 is turning left, 4 is turning right
const char forwardString[] = "f";
const char backString[] = "b";
const char leftString[] = "l";
const char rightString[] = "r";
const char stopString[] = "s";

const char forwardChar = 'f';
const char backChar = 'b';
const char leftChar = 'l';
const char rightChar = 'r';
const char stopChar = 's';

float totalDistance = 0;

void setup() {
  delay(2000);

  pinMode(UltraOne.trigPin, OUTPUT);
  pinMode(UltraOne.echoPin, INPUT);
  pinMode(UltraTwo.trigPin, OUTPUT);
  pinMode(UltraTwo.echoPin, INPUT);
  pinMode(UltraThree.trigPin, OUTPUT);
  pinMode(UltraThree.echoPin, INPUT);
  pinMode(UltraFour.trigPin, OUTPUT);
  pinMode(UltraFour.echoPin, INPUT);

  ultraServo.attach(1);

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
}


void loop()
{
  if (status != WL_CONNECTED) {
    status = WiFi.begin(ssid, pass);
    while(true);
  }
  client = server.available();
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
      char* movementInput = new char[1];
      movementInput[0] = serialInput[0];
      //Serial.print(state);
      Serial.println(movementInput);
      client.println(serialInput);     
      
      /*
      if(strcmp(movementInput, forwardString) == 0) 
      {
        state = 1;
      }
      else if(strcmp(movementInput, backString) == 0) 
      {
        state = 2;
      }
      else if(strcmp(movementInput, leftString) == 0) 
      {
        state = 3;
      }
      else if(strcmp(movementInput, rightString) == 0) 
      {
        state = 4;
      } else if(strcmp(movementInput, stopString) == 0)
      {
        state = 0;
      }
      */
      if(containsChar(serialInput, forwardChar)) 
      {
        state = 1;
      }
      else if(containsChar(serialInput, backChar)) 
      {
        state = 2;
      }
      else if(containsChar(serialInput, leftChar)) 
      {
        state = 3;
      }
      else if(containsChar(serialInput, rightChar)) 
      {
        state = 4;
      } else if(containsChar(serialInput, stopChar))
      {
        state = 0;
      }
      
      if(state == 1) {
        totalDistance += moveDistance(30);
      }
      if(state == 2) {
        totalDistance -= moveDistanceBackward(30);
      }
      if(state == 3) {
        turnLeft();
      }
      if(state == 4) {
        turnRight();
      }
    } else {
      if(state == 0) {
        if(millis() > last_time + 1500) {
          backLeft.detach();
          backRight.detach();
          frontLeft.detach();
          frontRight.detach();

          sendData(totalDistance);

          totalDistance = 0;

          last_time = millis();
        }
      }
    }

    
    client.flush();
  }
  if(millis() > last_time + 1000) {
    last_time = millis();

    Serial.println("Arduino Alive");
  }
}

float getUltrasonic(UltrasonicSensor ultra) { //consider adding filter here
  float distance = 0;

  digitalWrite(ultra.trigPin, HIGH);
  delayMicroseconds(10);
  digitalWrite(ultra.trigPin, LOW);

  float echoTime = pulseIn(ultra.echoPin, HIGH);

  distance = echoTime / 58.0;

  return distance;
}

float moveDistance(float targetDistance) {
  backLeft.attach(11);
  backRight.attach(10);
  frontLeft.attach(13);
  frontRight.attach(12);

  float realDistance = 0.0;
  float timeIncrement = 100.0; //ms
  float incrementsCounted = 0.0;

  Serial.println("Moving");

  frontLeft.write(180);
  backLeft.write(180);
  frontRight.write(0);
  backRight.write(0);

  float startTime = millis();

  float errorConstant = 1.0;

  while(realDistance < (targetDistance - errorConstant)) {
    delay(timeIncrement);

    float deltaTime = millis() - startTime;

    realDistance += (0.001 * timeIncrement * calculatedSpeed);

    //Serial.println(realDistance);

    incrementsCounted++;

    startTime = millis();
  }

  realDistance += errorConstant;


  Serial.print("Speed: ");
  Serial.println(realDistance / (incrementsCounted * timeIncrement * 0.001 )); //cm/s
  Serial.print("Distance: ");
  Serial.println(realDistance);

  frontLeft.write(90);
  backLeft.write(90);
  frontRight.write(90);
  backRight.write(90);

  backLeft.detach();
  backRight.detach();
  frontLeft.detach();
  frontRight.detach();

  state = 0;

  return realDistance;
}

float moveDistanceBackward(float targetDistance) { //MAKE SURE TO DELAY BEFORE MOVING BACKWARD
  backLeft.attach(11);
  backRight.attach(10);
  frontLeft.attach(13);
  frontRight.attach(12);

  float realDistance = 0.0;
  float timeIncrement = 100.0; //ms
  float incrementsCounted = 0.0;

  //Serial.println("Moving");

  frontLeft.write(0);
  backLeft.write(0);
  frontRight.write(180);
  backRight.write(180);

  float startTime = millis();

  float errorConstant = 1.0;

  while(realDistance < (targetDistance - errorConstant)) {
    delay(timeIncrement);

    float deltaTime = millis() - startTime;

    realDistance += (0.001 * timeIncrement * calculatedSpeed);

    Serial.println(realDistance);

    incrementsCounted++;

    startTime = millis();
  }

  realDistance += errorConstant;


  Serial.print("Speed: ");
  Serial.println(realDistance / (incrementsCounted * timeIncrement * 0.001 )); //cm/s
  Serial.print("Distance: ");
  Serial.println(realDistance);

  frontLeft.write(90);
  backLeft.write(90);
  frontRight.write(90);
  backRight.write(90);

  backLeft.detach();
  backRight.detach();
  frontLeft.detach();
  frontRight.detach();

  state = 0;

  return realDistance;
}

void turnLeft() {
  backLeft.attach(11);
  backRight.attach(10);
  frontLeft.attach(13);
  frontRight.attach(12);

  float timeIncrement = 1000.0; //ms

  //Serial.println("Moving");

  frontLeft.write(0);
  backLeft.write(0);
  frontRight.write(0);
  backRight.write(0);

  delay(timeIncrement);

  frontLeft.write(90);
  backLeft.write(90);
  frontRight.write(90);
  backRight.write(90);

  backLeft.detach();
  backRight.detach();
  frontLeft.detach();
  frontRight.detach();

  state = 0;
}

void turnRight() { //MAKE SURE TO DELAY BEFORE MOVING BACKWARD
  backLeft.attach(11);
  backRight.attach(10);
  frontLeft.attach(13);
  frontRight.attach(12);

  float timeIncrement = 1000.0; //ms

  //Serial.println("Moving");

  frontLeft.write(180);
  backLeft.write(180);
  frontRight.write(180);
  backRight.write(180);

  delay(timeIncrement);

  frontLeft.write(90);
  backLeft.write(90);
  frontRight.write(90);
  backRight.write(90);

  backLeft.detach();
  backRight.detach();
  frontLeft.detach();
  frontRight.detach();

  state = 0;
}

void sendData(float distanceMoved) {
  float* dataToSend = new float[9]; //fix
 
  dataToSend[0] = distanceMoved;

  float ultraServoAngle = ultraServo.read();

  dataToSend[1] = getUltrasonic(UltraOne);

  dataToSend[2] = (UltraOne.initialAngle + ultraServoAngle);

  dataToSend[3] = getUltrasonic(UltraTwo);

  dataToSend[4] = (UltraTwo.initialAngle + ultraServoAngle);

  dataToSend[5] = getUltrasonic(UltraThree);

  dataToSend[6] = (UltraThree.initialAngle + ultraServoAngle);

  dataToSend[7] = getUltrasonic(UltraFour);

  dataToSend[8] = (UltraFour.initialAngle + ultraServoAngle);


  client.print("A");
  for(int i = 0; i < 9; i++) {
    client.print(dataToSend[i]);
    client.print(" ");
  }
  client.println("");
  client.flush();
}

bool containsChar(const char* arr, char target) {
    return std::find(arr, arr + std::strlen(arr), target) != arr + std::strlen(arr);
}