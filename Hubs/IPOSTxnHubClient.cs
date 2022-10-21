using POSTxns.Hubs;
public interface IPOSTxnHubClient
{
    Task SendTxn(string storeId, string registerId, decimal total);
}