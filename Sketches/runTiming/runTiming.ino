#include <Servo.h>

Servo frontLeft;
Servo frontRight;
Servo backLeft;
Servo backRight;

void setup() {
  // put your setup code here, to run once:
  delay(2000);

  backLeft.attach(11);
  backRight.attach(10);
  frontLeft.attach(13);
  frontRight.attach(12);

  frontLeft.write(180);
  backLeft.write(180);
  frontRight.write(0);
  backRight.write(0);
  
  for(int i = 0; i < 20; i++) {
    delay(100);
  }

  frontLeft.write(90);
  backLeft.write(90);
  frontRight.write(90);
  backRight.write(90);

  backLeft.detach();
  backRight.detach();
  frontLeft.detach();
  frontRight.detach();
}

void loop() {

}