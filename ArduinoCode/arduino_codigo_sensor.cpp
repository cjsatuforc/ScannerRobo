/*
 * arduino_codigo_sensor.cpp
 *
 *  Created on: Aug 4, 2016
 *      Author: jesseh
 */


class SerialCommunication
{

private:
  String str = "";
  char c;
public:

  void init(int baud = 9600)
  {
    Serial.begin(baud);
    Serial.println("Starting Connection...");
    for(;;)
    {
      if(getString() == "finatec")
        break;
    }
    Serial.println("Connection Done!");
  }

  bool status()
  {
    return Serial.available();
  }

  String getString()
  {
    str = "";
    c = ' ';

    for(;Serial.available() > 0;)
    {
        c = Serial.read();
        if(c != '\n')
          str.concat(c);
        else
          break;
        delay(5);
    }
    //Serial.print("Received: ");
    //Serial.println(str);
    return str;
  }

  void writeString(String str)
  {
    Serial.println(str);
  }

  void writeStringwn(String str)
  {
    Serial.print(str);
  }

  float getFloat()
  {
    getString();
    return str.toFloat();
  }

  void writeFloat(float f)
  {
    Serial.print(f,3);
  }

  int getInt()
  {
    getString();
    return str.toInt();
  }
};

//PARA O NANO
class DCMotor
{
private:
  //Pinos da ponte H
  int _pinVCC, _pinMOT_IN1, _pinMOT_IN2, _pinMOT_PWM;

  //Carcateristicas de giro
  bool _sent_gir; //Sentido de giro (anti-horario)
  bool _is_running;
  int _speedPWM; //Largura do pulso do PWM
  short _defaultSpeedPWM;

public:
  DCMotor(int pinVCC, int pinMOTIN1, int pinMOTIN2, int pinMOTPWM, short defaultPWMSpeed=6, int pwmFrequencyScaler=1024)
  {
    setPin(pinVCC, pinMOTIN1, pinMOTIN2, pinMOTPWM);
    _sent_gir = true;
    _speedPWM = 0;
    _is_running = 0;
    stopMotor();
    setDefaultSpeedPWM(defaultPWMSpeed);
    setPWMfrequency(pwmFrequencyScaler);
  }

  void setPin(int pinVCC, int pinMOTIN1, int pinMOTIN2, int pinMOTPWM)
  {
    _pinVCC = pinVCC;
    _pinMOT_IN1 = pinMOTIN1;
    _pinMOT_IN2 = pinMOTIN2;
    _pinMOT_PWM = pinMOTPWM;
    pinMode(_pinVCC, OUTPUT);
    pinMode(_pinMOT_IN1, OUTPUT);
    pinMode(_pinMOT_IN2, OUTPUT);
    pinMode(_pinMOT_PWM, OUTPUT);
  }

  void setSentidoGiro(bool is_antihorario)
  {
    analogWrite(_pinMOT_PWM, 0);
    _sent_gir = is_antihorario;
    digitalWrite(_pinMOT_IN1, (!_sent_gir)&&(_is_running));
    digitalWrite(_pinMOT_IN2, (_sent_gir)&&(_is_running));
    analogWrite(_pinMOT_PWM, _speedPWM*(_is_running? 1 : 0));
  }

  void setPWMfrequency(int prescaler)
  {
    //Seta o prescalling do clock para o timer que é definido pelo pino
    byte scaler;
    switch(prescaler)
    {
    case 1:
      scaler = 0x01;
      break;
    case 8:
      scaler = 0x02;
      break;
    case 64:
      scaler = 0x03;
      break;
    case 256:
      scaler = 0x04;
      break;
    case 1024:
      scaler = 0x05;
      break;
    default:
      scaler = 0x03;
      break;
    }


    TCCR1B = (TCCR1B & 0b11111000) | scaler;

    //MICRO PRO
    //Evitar usar os pinos 3,9, 10
    //Porque a mudanca do prescaler do clock pode alterar outras funcoes
    //que usam os registradores de time comparison do ATMEGA32U4
//    if(pin==3) //Pino OC0B
//    {
//      TCCR0B = (TCCR0B & 0b11111000) | scaler;
//    }
//    else if(pin==5)//Pino OC3A
//    {
//      TCCR3B = (TCCR3B & 0b11111000) | scaler;
//    }
//    else if(pin==6)//Pino OC4D
//    {
//      TCCR4B = (TCCR4B & 0b11111000) | scaler;
//    }
//    else if(pin==9||pin==10)//Pino OC1A e OC1B
//    {
//      TCCR1B = (TCCR1B & 0b11111000) | scaler;
//    }
  }

  void setDefaultSpeedPWM(short newDefaultSpeed)
  {
    _defaultSpeedPWM = newDefaultSpeed;
  }

  void setSpeedPWM()
  {
    _speedPWM = _defaultSpeedPWM;
    analogWrite(_pinMOT_PWM, _speedPWM*(_is_running? 1 : 0));
  }

  void setSpeedPWM(short speedPWM)
  {
    _speedPWM = speedPWM;
    analogWrite(_pinMOT_PWM, _speedPWM*(_is_running? 1 : 0));
  }

  void turnOnHBridge()
  {
    digitalWrite(_pinVCC, HIGH);
  }

  void turnOffHBridge()
  {
    _is_running = false;
    analogWrite(_pinMOT_PWM, 0);
    digitalWrite(_pinMOT_IN1, LOW);
    digitalWrite(_pinMOT_IN2, LOW);
    digitalWrite(_pinVCC, LOW);
  }

  void runMotor()
  {
    turnOnHBridge();
    _is_running = true;
    analogWrite(_pinMOT_PWM, _speedPWM*(_is_running? 1 : 0));
    digitalWrite(_pinMOT_IN1, (_sent_gir)&&(_is_running));
    digitalWrite(_pinMOT_IN2, (!_sent_gir)&&(_is_running));
  }

  void stopMotor()
  {
    _is_running = false;
    analogWrite(_pinMOT_PWM, 0);
    digitalWrite(_pinMOT_IN1, LOW);
    digitalWrite(_pinMOT_IN2, LOW);
  }

};

void runCommand(DCMotor motor_, SerialCommunication usb, String cmd, int las_a, int las_b)
{
    if (cmd=="ligar laser fixo") {
      digitalWrite(las_b, HIGH);
      usb.writeString("OK!");
    }
    else if (cmd == "ligar laser movel") {
      digitalWrite(las_a, HIGH);
      usb.writeString("OK!");
    }
    else if (cmd== "desligar laser fixo") {
      digitalWrite(las_b, LOW);
      usb.writeString("OK!");
    }
    else if (cmd== "desligar laser movel") {
      digitalWrite(las_a, LOW);
      usb.writeString("OK!");
    }
    else if (cmd== "parar motor") {
      motor_.stopMotor();
      motor_.turnOffHBridge();
      usb.writeString("OK");
    }
    else if (cmd== "ligar motor horario") {
      motor_.turnOnHBridge();
      motor_.setSentidoGiro(false);
      motor_.setSpeedPWM();
      motor_.runMotor();
      usb.writeString("OK!");
    }
    else if ("ligar motor antihorario") {
      motor_.turnOnHBridge();
      motor_.setSentidoGiro(true);
      motor_.setSpeedPWM();
      motor_.runMotor();
      usb.writeString("OK!");
    }
    else if (cmd== "ligar motor horario rapido") {
      motor_.turnOnHBridge();
      motor_.setSentidoGiro(false);
      motor_.setSpeedPWM(25);
      motor_.runMotor();
      usb.writeString("OK!");
    }
    else if (cmd== "ligar motor antihorario rapido") {
      motor_.turnOnHBridge();
      motor_.setSentidoGiro(true);
      motor_.setSpeedPWM(25);
      motor_.runMotor();
      usb.writeString("OK!");
    }
  }


//pinos
//lasers
int las_a = 4;
int las_b = 5;
int mot_in1 = 18;
int mot_in2 = 17;
int mot_pwm = 10; //SEMPRE USAR O 10 no nano
int mot_vcc = 19;
int enc_a = 16;
int enc_b = 15;
DCMotor motor(mot_vcc, mot_in1, mot_in2, mot_pwm);
SerialCommunication usb_com;

void setup()
{
  analogWrite(mot_pwm, 0);
  Serial.begin(9600);

  motor.turnOffHBridge();
  motor.turnOnHBridge();


  usb_com.init();
  digitalWrite(mot_vcc, HIGH);

  //Setup dos Lasers
  pinMode(las_a, OUTPUT);
  pinMode(las_b, OUTPUT);
  digitalWrite(las_a, LOW);
  digitalWrite(las_b, LOW);
}

String command;
void loop()
{

  //usb_com.writeString("Waiting Command!");
  if(usb_com.status() > 0)
     command = usb_com.getString();
  else
    command = "";

  if(command .length() == 0)
    return;

  usb_com.writeStringwn("Command: ");
  usb_com.writeString(command);

  runCommand(motor, usb_com, command, las_a, las_b);
}
