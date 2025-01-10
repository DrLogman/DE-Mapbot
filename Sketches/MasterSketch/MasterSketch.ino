#include <Wire.h>

void setup() {
  Wire.begin(9);
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