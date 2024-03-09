namespace TaskManager.API.Models
{
    public enum TokenResults
    {
        Success,
        NeedsRefresh,
        Expired,
        UnableToRead
    }
}
