namespace CraigStars
{
    public class CargoTransferOrder : FleetOrder
    {
        public ICargoHolder Dest { get; set; }
        public Cargo Transfer { get; set; }

    }
}