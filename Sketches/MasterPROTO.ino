// Chapter 7 - Communications
// I2C Master
// By Cornel Amariei for Packt Publishing

// Include the required Wire library for I2C
#include <Wire.h>

int x = 0;

void setup() {
  // Start the I2C Bus as Master
  Wire.begin(); 
}

void loop() {
  Wire.beginTransmission(9); // transmit to device #9
  Wire.write(x);              // sends x 
  Wire.endTransmission();    // stop transmitting
 
  x++; // Increment x
  if (x > 5) x = 0; // `reset x once it gets 6
  
  delay(500);
}



\\ultrasonic code



void setup(){

  pinMode(RightMotorForward, OUTPUT);
  pinMode(LeftMotorForward, OUTPUT);
  pinMode(LeftMotorBackward, OUTPUT);
  pinMode(RightMotorBackward, OUTPUT);
  pinMode(ForwardLED, OUTPUT);//Robot Lk
  pinMode(BackwardLED, OUTPUT);//Robot Lk
  pinMode(LeftLED, OUTPUT);//Robot Lk
  pinMode( RightLED, OUTPUT);
  pinMode(LeftSensorLED, OUTPUT);
  pinMode( RightSensorLED, OUTPUT);
  
  servo_motor.attach(10); //our servo pin

  servo_motor.write(115);
  delay(2000);
  distance = readPing();
  delay(100);
  distance = readPing();
  delay(100);
  distance = readPing();
  delay(100);
  distance = readPing();
  delay(100);
}

void loop(){

  int distanceRight = 0;
  int distanceLeft = 0;
  delay(50);

  if (distance <= 45){
    moveStop();
    delay(300);
    moveBackward();
    delay(400);
    moveStop();
    delay(300);
    distanceRight = lookRight();
    delay(300);
    distanceLeft = lookLeft();
    delay(300);

    if (distance >= distanceLeft){
      turnRight();
      moveStop();
    }
    else{
      turnLeft();
      moveStop();
    }
  }
  else{
    moveForward(); 
  }
    distance = readPing();
}

int lookRight(){  

   
  digitalWrite(RightSensorLED, HIGH);
 delay(200);
 digitalWrite(RightSensorLED, LOW);
  servo_motor.write(50);
  delay(500);
  int distance = readPing();
  delay(100);
  servo_motor.write(115);
  return distance;
}

int lookLeft(){

   digitalWrite(LeftSensorLED, HIGH);//Robot Lk
delay(500);
digitalWrite(LeftSensorLED, LOW);//Robot Lk
 
  servo_motor.write(170);
  delay(500);
  int distance = readPing();
  delay(100);
  servo_motor.write(115);
  return distance;
  delay(100);
}

int readPing(){
  delay(70);
  int cm = sonar.ping_cm();
  if (cm==0){
    cm=250;
  }
  return cm;
}