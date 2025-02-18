#nullable enable
namespace Core.DTOs.Request
{
    public class OrganisationHierarchyProvider
    {
        public required string OdsCode { get; set; }
        public string? PracticeName { get; set; }
        public int RegisteredPatientCount { get; set; }
        public string? RegionCode { get; set; }
        public string? RegionName { get; set; }
        public string? Icb22Name { get; set; }
        public string? PcnName { get; set; }
        public int Appointments13000 { get; set; }
    }
}