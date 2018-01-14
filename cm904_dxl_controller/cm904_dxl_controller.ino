/*SerialUSB_Echo_Interrupt
 
 Demonstrates sending data from the computer to the CM900,OpenCM9.04
 echoes it to the computer again.
 You can just type in terminal program, character you typed will be displayed

                 Compatibility
 CM900                  O
 OpenCM9.04             O

 created 16 Nov 2012
 by ROBOTIS CO,.LTD.
 */


#include <string.h> 
#define DXL_BUS_SERIAL1 1  //Dynamixel on Serial1(USART1)  <-OpenCM9.04

Dynamixel Dxl(DXL_BUS_SERIAL1);


void setup(){
  // Initialize the dynamixel bus:
  // Dynamixel 2.0 Baudrate -> 0: 9600, 1: 57600, 2: 115200, 3: 1Mbps  
  Dxl.begin(3);  
  Dxl.jointMode(0); //jointMode() is to use position mode
  Dxl.jointMode(1); //jointMode() is to use position mode
  Dxl.jointMode(2); //jointMode() is to use position mode
  
  //You can attach your serialUSB interrupt
  //or, also detach the interrupt by detachInterrupt(void) method
  SerialUSB.begin();
  SerialUSB.attachInterrupt(usbInterrupt);
  pinMode(BOARD_LED_PIN, OUTPUT);  //toggleLED_Pin_Out
}


void usbInterrupt(byte* buffr, byte nCount){
  SerialUSB.print("nCount =");
  SerialUSB.println(nCount);
  for(unsigned int i=0; i < nCount;i++)  //printf_SerialUSB_Buffer[N]_receive_Data
  {
    SerialUSB.print((char)buffr[i]);
  }
  SerialUSB.println("");
  
  char cmder[4];
  cmder[0] = (char)buffr[0];
  cmder[1] = (char)buffr[1];
  cmder[2] = (char)buffr[2];
  cmder[3] = '\0';
  
    SerialUSB.println("cmder: ");
    SerialUSB.println(cmder);
  
  if (strcmp(cmder, "gps") == 0)
  {
    char id[4];
    id[0] = (char)buffr[4];
    id[1] = (char)buffr[5];
    id[2] = (char)buffr[6];
    id[3] = '\0';
  
    char pos[5];
    pos[0] = (char)buffr[8];
    pos[1] = (char)buffr[9];
    pos[2] = (char)buffr[10];
    pos[3] = (char)buffr[11];
    pos[4] = '\0';

    SerialUSB.println("id: ");
    SerialUSB.println(id);
    SerialUSB.print(atoi(id));
    
    SerialUSB.println("pos: ");
    SerialUSB.println(pos);
    SerialUSB.print(atoi(pos));

    Dxl.goalPosition(atoi(id), atoi(pos));
  }
  else
  {
    SerialUSB.println("not if");
  }
 

}


void loop(){
  toggleLED();
  delay(100);

}

