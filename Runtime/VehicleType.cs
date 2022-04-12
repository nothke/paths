
public enum VehicleType
{
    Car,        // 0
    Bus,        // 1
    Coach,      // 2..
    LightTruck,
    HeavyTruck,
    Trolleybus,
    Tram,
    Taxi
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
    Trolleybus =    1 << 5,
    Tram =          1 << 6,
    Taxi =          1 << 7,

    All = Car | Bus | Coach | LightTruck | HeavyTruck | Trolleybus | Tram | Taxi,
    PublicTransport = Bus | Trolleybus | Tram | Taxi,
}

public static class VehicleMaskUtils
{
    public static bool BelongsToMask(this VehicleType type, VehicleMask mask)
    {
        int vt = (int)type;
        int bm = (int)mask;
        bool result = ((1 << vt) & bm) != 0;

        //Debug.Log($"{System.Convert.ToString(bm, 2)} {vt} == {result}");

        return result;
    } 
}