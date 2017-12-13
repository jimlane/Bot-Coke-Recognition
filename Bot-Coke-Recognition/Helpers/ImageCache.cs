using Microsoft.WindowsAzure.Storage.Table;

namespace Bot_Coke_Recognition.Helpers
{
    public class ImageCache : TableEntity
    {
        public ImageCache() { }
        public ImageCache(string Channel, string Id)
        {
            this.PartitionKey = Channel;
            this.RowKey = Id;
        }
        public string location { get; set; }
    }

}