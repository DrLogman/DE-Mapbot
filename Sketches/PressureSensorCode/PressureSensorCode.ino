const int pins[] = {A0, A1, A2, A3, A4, A5, A6, A7, A8, A9, A10, A11, A12, A13, A14, A15};

void setup() {
  Serial.begin(9600);
  for(int i = 0; i < 16; i++) {
    pinMode(pins[i], INPUT);
  }
}

void loop() {
  for(int i = 0; i < 16; i++) {
    Serial.print(analogRead(pins[i]));
    Serial.print(" ");
  }
  Serial.println("");
}
