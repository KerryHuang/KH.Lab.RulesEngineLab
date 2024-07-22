namespace KH.Lab.RulesEnglineService
{
    internal class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string ServiceType { get; set; } = null!;
        public string ParentId { get; set; } = null!;
        public int Amount { get; set; }
    }
}