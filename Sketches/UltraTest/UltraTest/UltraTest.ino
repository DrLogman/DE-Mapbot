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


void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  pinMode(UltraOne.trigPin, OUTPUT);
  pinMode(UltraOne.echoPin, INPUT);
  pinMode(UltraTwo.trigPin, OUTPUT);
  pinMode(UltraTwo.echoPin, INPUT);
  pinMode(UltraThree.trigPin, OUTPUT);
  pinMode(UltraThree.echoPin, INPUT);
  pinMode(UltraFour.trigPin, OUTPUT);
  pinMode(UltraFour.echoPin, INPUT);
}

void loop() {
  // put your main code here, to run repeatedly:
  Serial.print(getUltrasonic(UltraOne));
  Serial.print(" ");
  Serial.print(getUltrasonic(UltraTwo));
  Serial.print(" ");
  Serial.print(getUltrasonic(UltraThree));
  Serial.print(" ");
  Serial.println(getUltrasonic(UltraFour));
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

