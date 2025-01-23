using Core.DTOs.Request;

namespace gpconnect_analytics.Test.Helpers;

public class OrganisationHierarchyProviderComparer : IEqualityComparer<OrganisationHierarchyProvider>
{
    public bool Equals(OrganisationHierarchyProvider x, OrganisationHierarchyProvider y)
    {
        if (x == null || y == null) return false;

        return x.OdsCode == y.OdsCode &&
               x.PracticeName == y.PracticeName &&
               x.RegisteredPatientCount == y.RegisteredPatientCount &&
               x.RegionCode == y.RegionCode &&
               x.RegionName == y.RegionName &&
               x.Icb22Name == y.Icb22Name &&
               x.PcnName == y.PcnName &&
               x.Appointments13000 == y.Appointments13000;
    }

    public int GetHashCode(OrganisationHierarchyProvider obj)
    {
        return HashCode.Combine(obj.OdsCode, obj.PracticeName, obj.RegisteredPatientCount, obj.RegionCode,
            obj.RegionName, obj.Icb22Name, obj.PcnName, obj.Appointments13000);
    }
}