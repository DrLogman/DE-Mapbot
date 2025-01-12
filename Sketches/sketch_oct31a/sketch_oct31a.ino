unsigned long last_time = 0;

#include "Arduino_GigaDisplay_GFX.h"

GigaDisplay_GFX display; // create the object

#define BLACK 0x0000
#define WHITE 0xffff
#define CYAN 0x07f7

#include <SPI.h>
#include <WiFi.h>

#include <vector>


char ssid[] = "LogOS";          //  your network SSID (name)
char pass[] = "00000000";   // your network password
int status = WL_IDLE_STATUS;

WiFiServer server(23);

void setup()
{
  Serial.begin(9600);

  Serial.println("Attempting to connect to WPA network...");
  Serial.print("SSID: ");
  Serial.println(ssid);

  display.setRotation(1);  
  display.fillScreen(BLACK);
  display.cp437(true);
  display.setCursor(10,10); //x,y
  display.setTextSize(8);
  display.setTextColor(CYAN);
  display.println("LogOS");
  delay(500);
  display.setTextSize(5);
  display.println("Oh yeah! That's what's up!");
  delay(500);
  display.setTextColor(WHITE);
  display.setTextSize(3); //adjust text size
  display.println("Attempting to connect to network..."); //print
  display.print("SSID: ");
  display.println(ssid);

  status = WiFi.begin(ssid, pass);
  if ( status != WL_CONNECTED) {
    Serial.println("Couldn't get a wifi connection");
    display.println("Couldn't get a wifi connection"); //print
    while(true);
  }
  else {
    server.begin();
    Serial.print("Connected to wifi. My address:");
    IPAddress myAddress = WiFi.localIP();
    Serial.println(myAddress);
    display.print("Connected to wifi. My address: "); //print
    display.println(myAddress);
  }

  delay(1500);
  display.println("Circulating Gabelian Fluids...");
  delay(1000);
  display.println("Counting Haste Racist Moments...");
  delay(1000);
  display.println("Resurrecting George Michael...");
  delay(1000);
  Serial.setTimeout(10);
  display.begin();
  display.fillScreen(BLACK);
  display.setCursor(10,10); //x,y
  display.setTextSize(5); //adjust text size
  display.print("Hello Huzz!"); //print
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
      display.fillScreen(BLACK);
      display.setCursor(10,10); //x,y
      display.setTextSize(5); //adjust text size
      display.println(serialInput); //print
      client.println(serialInput);
      Serial.println(serialInput);
      char pingString[] = "ping";
      if(strcmp(serialInput, pingString) == 0) 
      {
        client.println("ping");
        display.println("PINGING");
      }
      client.flush();
    }
  }


    if (millis() > last_time + 2000)
    {
      Serial.println("Arduino is alive!!");
      last_time = millis();
    }
}