using Core.DTOs.Request;

namespace Core.Repositories;

public interface IHierarchyProviderConsumerRepo
{
    Task InsertHierarchyProviderConsumers(List<OrganisationHierarchyProvider> providers);
}