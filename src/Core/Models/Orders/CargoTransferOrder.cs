namespace CraigStars
{
    public class CargoTransferOrder
    {
        public ICargoHolder Source { get; set; }
        public ICargoHolder Dest { get; set; }
        public Cargo Transfer { get; set; }

    }
}