
public enum VehicleType
{
    Car,        // 0
    Bus,        // 1
    Coach,      // 2..
    LightTruck,
    HeavyTruck,
}

[System.Flags]
public enum VehicleMask
{
    None = 0,

    Car =           1 << 0,
    Bus =           1 << 1,
    Coach =         1 << 2,
    LightTruck =    1 << 3,
    HeavyTruck =    1 << 4,

    All = Car | Bus | Coach | LightTruck | HeavyTruck,
}

