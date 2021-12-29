
using CraigStars;

public record CargoTransferResult(Cargo cargo, int fuel)
{
    public static CargoTransferResult operator -(CargoTransferResult a, CargoTransferResult b)
    {
        return new CargoTransferResult(
            a.cargo - b.cargo,
            a.fuel - b.fuel
        );
    }

    public static CargoTransferResult operator +(CargoTransferResult a, CargoTransferResult b)
    {
        return new CargoTransferResult(
            a.cargo + b.cargo,
            a.fuel + b.fuel
        );
    }

}

