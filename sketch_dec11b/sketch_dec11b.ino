const int inPin = A2;

void setup() {
  pinMode(inPin, INPUT);
  Serial.begin(9600);
}

void loop() {
  int analogInputs[100];
  int analogMean = 0;

  for(int i = 0; i < 100; i++) {
  analogInputs[i] = analogRead(inPin);
  analogMean += (analogInputs[i])/100;  
  delay(15);    
  if(i == 99) {
    Serial.println(analogMean / 10);
  }
  }
}
