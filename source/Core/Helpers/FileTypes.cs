namespace Core.Helpers
{
    public enum FileTypes
    {
        [FilePath("asid-lookup-data")] asidlookup,
        [FilePath("ssp-transactions")] ssptrans,
        [FilePath("mesh-transactions")] meshtrans
    }
}