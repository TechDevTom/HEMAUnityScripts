#define AK8963_ADDRESS 0x0C
#define AK8963_ST1 0x02
#define AK8963_XOUT_L 0x03
#define AK8963_XOUT_H 0x04
#define AK8963_YOUT_L 0x05
#define AK8963_YOUT_H 0x06
#define AK8963_ZOUT_L 0x07
#define AK8963_ZOUT_H 0x08
#define AK8963_CNTL 0x0A
#define AK8963_ASAX 0x10
#define SMPLRT_DIV 0x19
#define CONFIG 0x1A
#define GYRO_CONFIG 0x1B
#define ACCEL_CONFIG 0x1C
#define ACCEL_CONFIG2 0x1D
#define MOT_DUR 0x20
#define ZMOT_THR 0x21
#define ZRMOT_DUR 0x22
#define INT_PIN_CFG 0x37
#define INT_ENABLE 0x38
#define ACCEL_XOUT_H 0x3B
#define ACCEL_XOUT_L 0x3C
#define ACCEL_YOUT_H 0x3D
#define ACCEL_YOUT_L 0x3E
#define ACCEL_ZOUT_H 0x3F
#define ACCEL_ZOUT_L 0x40
#define TEMP_OUT_H 0x41
#define TEMP_OUT_L 0x42
#define GYRO_XOUT_H 0x43
#define GYRO_XOUT_L 0x44
#define GYRO_YOUT_H 0x45
#define GYRO_YOUT_L 0x46
#define GYRO_ZOUT_H 0x47
#define GYRO_ZOUT_L 0x48
#define PWR_MGMT_1 0x6B
#define WHO_AM_I_MPU9250 0x75
#define XA_OFFSET_H 0x77
#define XA_OFFSET_L 0x78
#define YA_OFFSET_H 0x7A
#define YA_OFFSET_L 0x7B
#define ZA_OFFSET_H 0x7D
#define ZA_OFFSET_L 0x7E


#define ADO 0
#if ADO
#define MPU9250_ADDRESS 0x69
#else
#define MPU9250_ADDRESS 0x68
#define AK8963_ADDRESS 0x0C
#endif

#include <SPI.h>
#include <SoftwareSerial.h>// import the serial library
#include <Wire.h>//I2C library

enum Ascale {
  AFS_2G = 0,
  AFS_4G,
  AFS_8G,
  AFS_16G
};

enum Gscale {
  GFS_250DPS,
  GFS_500DPS,
  GFS_1000DPS=0,
  GFS_2000DPS
};

enum Mscale {
  MFS_14BITS = 0,
  MFS_16BITS
};


uint8_t Gscale = GFS_1000DPS;
uint8_t Ascale = AFS_2G;
uint8_t Mscale = MFS_16BITS;
uint8_t Mmode = 0x02;
float aRes, gRes, mRes;

int intPin = 12;
int myLed = 13;

int16_t accelCount[3];
int16_t gyroCount[3];
int16_t magCount[3];

int16_t tempCount; 
float temperature; 
float SelfTest[6];


SoftwareSerial ser(10, 11); // RX, TX
int BluetoothData; // the data given from Computer
float AccelResult;
float AccelScale=16384;
float GyroResult;
float GyroScale=32.8;
float AltResult;
bool isInit=false;

void writeTo(byte device, byte toAddress, byte val) {
  Wire.beginTransmission(device);
  Wire.write(toAddress);
  Wire.write(val);
  Wire.endTransmission();
}

void readFrom(byte device, byte fromAddress, int num, byte result[]) {
  Wire.beginTransmission(device);
  Wire.write(fromAddress);
  Wire.endTransmission();
  Wire.requestFrom((int)device, num);
  int i = 0;
  while (Wire.available()) {
    result[i] = Wire.read();
    i++;
  }
}

void setup() {
  Wire.begin();
  initMPU9250();
  float magCalibration[3];
  initAK8963(magCalibration);
  ser.begin(9600);
}

void loop() {
  if (ser.available()){
    if(ser.read()=='b'){
    //ser.println("hello");
    getRawData();
    }
}
delay(50);// prepare for next data ...
}

void getRawData(){
  readAccelData(accelCount);
  readGyroData(gyroCount);
  readMagData(magCount);

  ser.print("b");ser.print(";");
  ser.print(accelCount[0]);ser.print("/");ser.print(accelCount[1]);ser.print("/");ser.print(accelCount[2]);ser.print(";");
  ser.print(gyroCount[0]);ser.print("/");ser.print(gyroCount[1]);ser.print("/");ser.print(gyroCount[2]);ser.print(";");
  ser.print(magCount[0]);ser.print("/");ser.print(magCount[1]);ser.print("/");ser.print(magCount[2]);ser.println(";");
  }

void getData(){
  
    readAccelData(accelCount);
    ser.println("ACCEL DATA------------------");
    ser.print("  X-Axis Accel "); ser.println(accelCount[0]);
    ser.print("  Y-Axis Accel "); ser.println(accelCount[1]);
    ser.print("  Z-Axis Accel "); ser.println(accelCount[2]);

    readGyroData(gyroCount);
    ser.println("GYRO DATA-------------------");
    ser.print("  X-Axis Gyro "); ser.println(gyroCount[0]);
    ser.print("  Y-Axis Gyro "); ser.println(gyroCount[1]);
    ser.print("  Z-Axis Gyro "); ser.println(gyroCount[2]);

    readMagData(magCount);
    ser.println("MAG DATA--------------------");
    ser.print("  X-Axis Mag "); ser.println(magCount[0]);
    ser.print("  Y-Axis Mag "); ser.println(magCount[1]);
    ser.print("  Z-Axis Mag "); ser.println(magCount[2]);
    ser.println("TEMP DATA-------------------");
    tempCount=readTempData();
    ser.print("  Temp "); ser.println(tempCount);
    ser.println("");
}

void writeByte(uint8_t address, uint8_t subAddress, uint8_t data)
{

  Wire.beginTransmission(address); 
  Wire.write(subAddress); 
  Wire.write(data); 
  Wire.endTransmission(); 
}

uint8_t readByte(uint8_t address, uint8_t subAddress)
{

  uint8_t data;
  Wire.beginTransmission(address); 
  Wire.write(subAddress); 
  Wire.endTransmission(false); 
  Wire.requestFrom(address, (uint8_t) 1);
  data = Wire.read(); 
  return data;
}

void readBytesN(uint8_t address, uint8_t subAddress, uint8_t count, uint8_t * dest)
{
  Wire.beginTransmission(address); 
  Wire.write(subAddress); 
  Wire.endTransmission(false); 
  uint8_t i = 0;
  Wire.requestFrom(address, count); 
  while (Wire.available()) {
    dest[i++] = Wire.read();
  
  } 
}



void readAccelData(int16_t * destination)
{
  uint8_t rawData[6]; 
  readBytesN(MPU9250_ADDRESS, ACCEL_XOUT_H, 6, &rawData[0]);
  destination[0] = ((int16_t)rawData[0] << 8) | rawData[1] ; 
  destination[1] = ((int16_t)rawData[2] << 8) | rawData[3] ;
  destination[2] = ((int16_t)rawData[4] << 8) | rawData[5] ;
}


void readGyroData(int16_t * destination)
{
  uint8_t rawData[6];
  readBytesN(MPU9250_ADDRESS, GYRO_XOUT_H, 6, &rawData[0]);
  destination[0] = ((int16_t)rawData[0] << 8) | rawData[1] ; 
  destination[1] = ((int16_t)rawData[2] << 8) | rawData[3] ;
  destination[2] = ((int16_t)rawData[4] << 8) | rawData[5] ;
}

void readMagData(int16_t * destination)
{
  uint8_t rawData[7]; 
  if (readByte(AK8963_ADDRESS, AK8963_ST1) & 0x01) { 
    readBytesN(AK8963_ADDRESS, AK8963_XOUT_L, 7, &rawData[0]); 
    uint8_t c = rawData[6]; 
    if (!(c & 0x08)) { 
      destination[0] = ((int16_t)rawData[1] << 8) | rawData[0] ;
      destination[1] = ((int16_t)rawData[3] << 8) | rawData[2] ; 
      destination[2] = ((int16_t)rawData[5] << 8) | rawData[4] ;
    }
  }
}

int16_t readTempData()
{
  uint8_t rawData[2];
  readBytesN(MPU9250_ADDRESS, TEMP_OUT_H, 2, &rawData[0]); 
  return ((int16_t)rawData[0] << 8) | rawData[1] ; 
}

void initAK8963(float * destination)
{
  uint8_t rawData[3]; 
  writeByte(AK8963_ADDRESS, AK8963_CNTL, 0x00); 
  delay(10);
  writeByte(AK8963_ADDRESS, AK8963_CNTL, 0x0F);
  delay(10);
  readBytesN(AK8963_ADDRESS, AK8963_ASAX, 3, &rawData[0]); 
  destination[0] = (float)(rawData[0] - 128) / 256. + 1.; 
  destination[1] = (float)(rawData[1] - 128) / 256. + 1.;
  destination[2] = (float)(rawData[2] - 128) / 256. + 1.;
  writeByte(AK8963_ADDRESS, AK8963_CNTL, 0x00);
  delay(10);
  writeByte(AK8963_ADDRESS, AK8963_CNTL, Mscale << 4 | Mmode); 
  delay(10);
}
void initMPU9250()
{ 
  delay(100); 
  writeByte(MPU9250_ADDRESS, PWR_MGMT_1, 0b11111111); 
  delay(200);
  writeByte(MPU9250_ADDRESS, CONFIG, 0x03); 
  writeByte(MPU9250_ADDRESS, SMPLRT_DIV, 0x04); 
  uint8_t c = readByte(MPU9250_ADDRESS, GYRO_CONFIG); 
  writeByte(MPU9250_ADDRESS, GYRO_CONFIG, c & ~0x02);
  writeByte(MPU9250_ADDRESS, GYRO_CONFIG, c & ~0x18);
  writeByte(MPU9250_ADDRESS, GYRO_CONFIG, c | Gscale << 3);
  c = readByte(MPU9250_ADDRESS, ACCEL_CONFIG); 
  writeByte(MPU9250_ADDRESS, ACCEL_CONFIG, c & ~0x18); 
  writeByte(MPU9250_ADDRESS, ACCEL_CONFIG, c | Ascale << 3);
  c = readByte(MPU9250_ADDRESS, ACCEL_CONFIG2);
  writeByte(MPU9250_ADDRESS, ACCEL_CONFIG2, c & ~0x0F); 
  writeByte(MPU9250_ADDRESS, ACCEL_CONFIG2, c | 0x03); 
  writeByte(MPU9250_ADDRESS, INT_PIN_CFG, 0x22);
  writeByte(MPU9250_ADDRESS, INT_ENABLE, 0x01); 
  delay(100);
}

