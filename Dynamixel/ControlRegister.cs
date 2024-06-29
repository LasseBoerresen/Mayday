namespace Dynamixel;

public record ControlRegister(
    string Name,
    string Area,
    ushort Address,
    int SizeInBytes,
    string Access,
    int? InitialValue)
{
    public static ControlRegister ModelNumber =>           new("ModelNumber", "EEPROM", 0,2, "R", 1060);
    public static ControlRegister ModelInformation =>      new("ModelInformation", "EEPROM", 2,4, "R", null);
    public static ControlRegister VersionOfFirmware =>     new("VersionOfFirmware", "EEPROM", 6,1, "R", null);
    public static ControlRegister Id =>                    new("Id", "EEPROM", 7,1, "RW", 1);
    public static ControlRegister BaudRate =>              new("BaudRate", "EEPROM", 8,1, "RW", 1);
    public static ControlRegister ReturnDelayTime =>       new("ReturnDelayTime", "EEPROM", 9,1, "RW", 250);
    public static ControlRegister DriveMode =>             new("DriveMode", "EEPROM", 10,1, "RW", 0);
    public static ControlRegister OperatingMode =>         new("OperatingMode", "EEPROM", 11,1, "RW", 3);
    public static ControlRegister SecondaryId =>           new("SecondaryId", "EEPROM", 12,1, "RW", 255);
    public static ControlRegister ProtocolVersion =>       new("ProtocolVersion", "EEPROM", 13,1, "RW", 2);
    public static ControlRegister HomingOffset =>          new("HomingOffset", "EEPROM", 20,4, "RW", 0);
    public static ControlRegister MovingThreshold =>       new("MovingThreshold", "EEPROM", 24,4, "RW", 10);
    public static ControlRegister TemperatureLimit =>      new("TemperatureLimit", "EEPROM", 31,1, "RW", 72);
    public static ControlRegister MaxVoltageLimit =>       new("MaxVoltageLimit", "EEPROM", 32,2, "RW", 140);
    public static ControlRegister MinVoltageLimit =>       new("MinVoltageLimit", "EEPROM", 34,2, "RW", 60);
    public static ControlRegister PwmLimit =>              new("PwmLimit", "EEPROM", 36,2, "RW", 885);
    public static ControlRegister AccelerationLimit=>      new("AccelerationLimit", "EEPROM", 40,4, "RW", 32767);
    public static ControlRegister VelocityLimit=>          new("VelocityLimit", "EEPROM", 44,4, "RW", 415);
    public static ControlRegister MaxPositionLimit =>      new("MaxPositionLimit", "EEPROM", 48,4, "RW", 4095);
    public static ControlRegister MinPositionLimit =>      new("MinPositionLimit", "EEPROM", 52,4, "RW", 0);
    public static ControlRegister Shutdown =>              new("Shutdown", "EEPROM", 63,1, "RW", 52);
    public static ControlRegister TorqueEnable =>          new("TorqueEnable", "RAM", 64,1, "RW", 0);
    public static ControlRegister Led =>                   new("Led", "RAM", 65,1, "RW", 0);
    public static ControlRegister StatusReturnLevel =>     new("StatusReturnLevel", "RAM", 68,1, "RW", 2);
    public static ControlRegister RegisteredInstruction => new("RegisteredInstruction", "RAM", 69,1, "R", 0);
    public static ControlRegister HardwareErrorStatus =>   new("HardwareErrorStatus", "RAM", 70,1, "R", 0);
    public static ControlRegister VelocityIGain =>         new("VelocityIGain", "RAM", 76,2, "RW", 1000);
    public static ControlRegister VelocityPGain =>         new("VelocityPGain", "RAM", 78,2, "RW", 100);
    public static ControlRegister PositionDGain =>         new("PositionDGain", "RAM", 80,2, "RW", 4000);
    public static ControlRegister PositionIGain =>         new("PositionIGain", "RAM", 82,2, "RW", 0);
    public static ControlRegister PositionPGain =>         new("PositionPGain", "RAM", 84,2, "RW", 640);
    public static ControlRegister Feedforward2ndGain =>    new("Feedforward2ndGain", "RAM", 88,2, "RW", 0);
    public static ControlRegister Feedforward1stGain =>    new("Feedforward1stGain", "RAM", 90,2, "RW", 0);
    public static ControlRegister BusWatchdog =>           new("BusWatchdog", "RAM", 98,1, "RW", 0);
    public static ControlRegister GoalPwm =>               new("GoalPwm", "RAM", 100,2, "RW", null);
    public static ControlRegister GoalVelocity =>          new("GoalVelocity", "RAM", 104,4, "RW", null);
    public static ControlRegister ProfileAcceleration =>   new("ProfileAcceleration", "RAM", 108,4, "RW", 0);
    public static ControlRegister ProfileVelocity =>       new("ProfileVelocity", "RAM", 112,4, "RW", 0);
    public static ControlRegister GoalPosition =>          new("GoalPosition", "RAM", 116,4, "RW", null);
    public static ControlRegister RealtimeTick=>           new("RealtimeTick", "RAM", 120,2, "R", null);
    public static ControlRegister Moving =>                new("Moving", "RAM", 122,1, "R", 0);
    public static ControlRegister MovingStatus=>           new("MovingStatus", "RAM", 123,1, "R", 0);
    public static ControlRegister PresentPwm =>            new("PresentPwm", "RAM", 124,2, "R", null);
    public static ControlRegister PresentLoad =>           new("PresentLoad", "RAM", 126,2, "R", null);
    public static ControlRegister PresentVelocity =>       new("PresentVelocity", "RAM", 128,4, "R", null);
    public static ControlRegister PresentPosition =>       new("PresentPosition", "RAM", 132,4, "R", null);
    public static ControlRegister VelocityTrajectory =>    new("VelocityTrajectory", "RAM", 136,4, "R", null);
    public static ControlRegister PositionTrajectory =>    new("PositionTrajectory", "RAM", 140,4, "R", null);
    public static ControlRegister PresentInputVoltage =>   new("PresentInputVoltage", "RAM", 144,2, "R", null);
    public static ControlRegister PresentTemperature =>    new("PresentTemperature", "RAM", 146,1, "R", null);
    public static ControlRegister IndirectAddress1 =>      new("IndirectAddress1", "RAM", 168,2, "RW", 224);
    public static ControlRegister IndirectAddress2 =>      new("IndirectAddress2", "RAM", 170,2, "RW", 225);
    public static ControlRegister IndirectAddress3 =>      new("IndirectAddress3", "RAM", 172,2, "RW", 226);
    public static ControlRegister IndirectAddress26 =>     new("IndirectAddress26", "RAM", 218,2, "RW", 249);
    public static ControlRegister IndirectAddress27 =>     new("IndirectAddress27", "RAM", 220,2, "RW", 250);
    public static ControlRegister IndirectAddress28 =>     new("IndirectAddress28", "RAM", 222,2, "RW", 251);
    public static ControlRegister IndirectData1 =>         new("IndirectData1", "RAM", 224,1, "RW", 0);
    public static ControlRegister IndirectData2 =>         new("IndirectData2", "RAM", 225,1, "RW", 0);
    public static ControlRegister IndirectData3 =>         new("IndirectData3", "RAM", 226,1, "RW", 0);
    public static ControlRegister IndirectData26 =>        new("IndirectData26", "RAM", 249,1, "RW", 0);
    public static ControlRegister IndirectData27 =>        new("IndirectData27", "RAM", 250,1, "RW", 0);
    public static ControlRegister IndirectData28 =>        new("IndirectData28", "RAM", 251,1, "RW", 0);
    public static ControlRegister IndirectAddress29 =>     new("IndirectAddress29", "RAM", 578,2, "RW", 634);
    public static ControlRegister IndirectAddress30 =>     new("IndirectAddress30", "RAM", 580,2, "RW", 635);
    public static ControlRegister IndirectAddress31 =>     new("IndirectAddress31", "RAM", 582,2, "RW", 636);
    public static ControlRegister IndirectAddress54 =>     new("IndirectAddress54", "RAM", 628,2, "RW", 659);
    public static ControlRegister IndirectAddress55 =>     new("IndirectAddress55", "RAM", 630,2, "RW", 660);
    public static ControlRegister IndirectAddress56 =>     new("IndirectAddress56", "RAM", 632,2, "RW", 661);
    public static ControlRegister IndirectData29 =>        new("IndirectData29", "RAM", 634,1, "RW", 0);
    public static ControlRegister IndirectData30 =>        new("IndirectData30", "RAM", 635,1, "RW", 0);
    public static ControlRegister IndirectData31 =>        new("IndirectData31", "RAM", 636,1, "RW", 0);
    public static ControlRegister IndirectData54 =>        new("IndirectData54", "RAM", 659,1, "RW", 0);
    public static ControlRegister IndirectData55 =>        new("IndirectData55", "RAM", 660,1, "RW", 0);
    public static ControlRegister IndirectData56 =>        new("IndirectData56", "RAM", 661,1, "RW", 0);
};
