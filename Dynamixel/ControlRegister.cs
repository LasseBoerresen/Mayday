namespace Dynamixel;

public record ControlRegister(
    string Area,
    ushort Address,
    int SizeInBytes,
    string Access,
    int? InitialValue)
{
    public static ControlRegister ModelNumber =>           new("EEPROM",0,2,"R",1060);
    public static ControlRegister ModelInformation =>      new("EEPROM",2,4,"R",null);
    public static ControlRegister VersionOfFirmware =>     new("EEPROM",6,1,"R",null);
    public static ControlRegister Id =>                    new("EEPROM",7,1,"RW",1);
    public static ControlRegister BaudRate =>              new("EEPROM",8,1,"RW",1);
    public static ControlRegister ReturnDelayTime =>       new("EEPROM",9,1,"RW",250);
    public static ControlRegister DriveMode =>             new("EEPROM",10,1,"RW",0);
    public static ControlRegister OperatingMode =>         new("EEPROM",11,1,"RW",3);
    public static ControlRegister SecondaryId =>           new("EEPROM",12,1,"RW",255);
    public static ControlRegister ProtocolVersion =>       new("EEPROM",13,1,"RW",2);
    public static ControlRegister HomingOffset =>          new("EEPROM",20,4,"RW",0);
    public static ControlRegister MovingThreshold =>       new("EEPROM",24,4,"RW",10);
    public static ControlRegister TemperatureLimit =>      new("EEPROM",31,1,"RW",72);
    public static ControlRegister MaxVoltageLimit =>       new("EEPROM",32,2,"RW",140);
    public static ControlRegister MinVoltageLimit =>       new("EEPROM",34,2,"RW",60);
    public static ControlRegister PwmLimit =>              new("EEPROM",36,2,"RW",885);
    public static ControlRegister AccelerationLimit=>      new("EEPROM",40,4,"RW",32767);
    public static ControlRegister VelocityLimit=>          new("EEPROM",44,4,"RW",415);
    public static ControlRegister MaxPositionLimit =>      new("EEPROM",48,4,"RW",4095);
    public static ControlRegister MinPositionLimit =>      new("EEPROM",52,4,"RW",0);
    public static ControlRegister Shutdown =>              new("EEPROM",63,1,"RW",52);
    public static ControlRegister TorqueEnable =>          new("RAM",64,1,"RW",0);
    public static ControlRegister Led =>                   new("RAM",65,1,"RW",0);
    public static ControlRegister StatusReturnLevel =>     new("RAM",68,1,"RW",2);
    public static ControlRegister RegisteredInstruction => new("RAM",69,1,"R",0);
    public static ControlRegister HardwareErrorStatus =>   new("RAM",70,1,"R",0);
    public static ControlRegister VelocityIGain =>         new("RAM",76,2,"RW",1000);
    public static ControlRegister VelocityPGain =>         new("RAM",78,2,"RW",100);
    public static ControlRegister PositionDGain =>         new("RAM",80,2,"RW",4000);
    public static ControlRegister PositionIGain =>         new("RAM",82,2,"RW",0);
    public static ControlRegister PositionPGain =>         new("RAM",84,2,"RW",640);
    public static ControlRegister Feedforward2ndGain =>    new("RAM",88,2,"RW",0);
    public static ControlRegister Feedforward1stGain =>    new("RAM",90,2,"RW",0);
    public static ControlRegister BusWatchdog =>           new("RAM",98,1,"RW",0);
    public static ControlRegister GoalPwm =>               new("RAM",100,2,"RW",null);
    public static ControlRegister GoalVelocity =>          new("RAM",104,4,"RW",null);
    public static ControlRegister ProfileAcceleration =>   new("RAM",108,4,"RW",0);
    public static ControlRegister ProfileVelocity =>       new("RAM",112,4,"RW",0);
    public static ControlRegister GoalPosition =>          new("RAM",116,4,"RW",null);
    public static ControlRegister RealtimeTick=>           new("RAM",120,2,"R",null);
    public static ControlRegister Moving =>                new("RAM",122,1,"R",0);
    public static ControlRegister MovingStatus=>           new("RAM",123,1,"R",0);
    public static ControlRegister PresentPwm =>            new("RAM",124,2,"R",null);
    public static ControlRegister PresentLoad =>           new("RAM",126,2,"R",null);
    public static ControlRegister PresentVelocity =>       new("RAM",128,4,"R",null);
    public static ControlRegister PresentPosition =>       new("RAM",132,4,"R",null);
    public static ControlRegister VelocityTrajectory =>    new("RAM",136,4,"R",null);
    public static ControlRegister PositionTrajectory =>    new("RAM",140,4,"R",null);
    public static ControlRegister PresentInputVoltage =>   new("RAM",144,2,"R",null);
    public static ControlRegister PresentTemperature =>    new("RAM",146,1,"R",null);
    public static ControlRegister IndirectAddress1 =>      new("RAM",168,2,"RW",224);
    public static ControlRegister IndirectAddress2 =>      new("RAM",170,2,"RW",225);
    public static ControlRegister IndirectAddress3 =>      new("RAM",172,2,"RW",226);
    public static ControlRegister IndirectAddress26 =>     new("RAM",218,2,"RW",249);
    public static ControlRegister IndirectAddress27 =>     new("RAM",220,2,"RW",250);
    public static ControlRegister IndirectAddress28 =>     new("RAM",222,2,"RW",251);
    public static ControlRegister IndirectData1 =>         new("RAM",224,1,"RW",0);
    public static ControlRegister IndirectData2 =>         new("RAM",225,1,"RW",0);
    public static ControlRegister IndirectData3 =>         new("RAM",226,1,"RW",0);
    public static ControlRegister IndirectData26 =>        new("RAM",249,1,"RW",0);
    public static ControlRegister IndirectData27 =>        new("RAM",250,1,"RW",0);
    public static ControlRegister IndirectData28 =>        new("RAM",251,1,"RW",0);
    public static ControlRegister IndirectAddress29 =>     new("RAM",578,2,"RW",634);
    public static ControlRegister IndirectAddress30 =>     new("RAM",580,2,"RW",635);
    public static ControlRegister IndirectAddress31 =>     new("RAM",582,2,"RW",636);
    public static ControlRegister IndirectAddress54 =>     new("RAM",628,2,"RW",659);
    public static ControlRegister IndirectAddress55 =>     new("RAM",630,2,"RW",660);
    public static ControlRegister IndirectAddress56 =>     new("RAM",632,2,"RW",661);
    public static ControlRegister IndirectData29 =>        new("RAM",634,1,"RW",0);
    public static ControlRegister IndirectData30 =>        new("RAM",635,1,"RW",0);
    public static ControlRegister IndirectData31 =>        new("RAM",636,1,"RW",0);
    public static ControlRegister IndirectData54 =>        new("RAM",659,1,"RW",0);
    public static ControlRegister IndirectData55 =>        new("RAM",660,1,"RW",0);
    public static ControlRegister IndirectData56 =>        new("RAM",661,1,"RW",0);
};
