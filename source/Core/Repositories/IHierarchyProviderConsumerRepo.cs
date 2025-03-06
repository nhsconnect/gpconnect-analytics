using Core.DTOs.Request;

namespace Core.Repositories;

public interface IHierarchyProviderConsumerRepo
{
    Task<int> InsertHierarchyProviderConsumers(List<OrganisationHierarchyProvider> providers);
}