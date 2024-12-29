void setup() {
 pinMode(3, OUTPUT);
 Serial.begin(9600);
}

void loop() {
  int dutyCycle = (35)*2.55;
  analogWrite(3, dutyCycle);
}
