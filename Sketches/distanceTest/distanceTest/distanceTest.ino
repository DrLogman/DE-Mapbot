const int rightForward = 7;
const int rightBackward = 6;
const int leftForward = 5;
const int leftBackward = 4;

void setup() {
  pinMode(rightForward, OUTPUT);
  pinMode(rightBackward, OUTPUT);
  pinMode(leftForward, OUTPUT);
  pinMode(leftBackward, OUTPUT);

  delay(3000);
  MoveForward(1000);
}

void loop() {

}

void MoveForward(int ms) {
  digitalWrite(rightForward, HIGH);
  digitalWrite(rightBackward, LOW);
  digitalWrite(leftForward, HIGH);
  digitalWrite(leftBackward, LOW);
  delay(ms);
  digitalWrite(rightForward, LOW);
  digitalWrite(leftForward, LOW);
}