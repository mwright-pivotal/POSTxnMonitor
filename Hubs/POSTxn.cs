namespace POSTxns.Hubs

{
    public class POSTxn {
        public String? storeId { get; set; }
        public String? registerId { get; set; }
        public Decimal? total { get; set; }

        public POSTxn(String _storeId, String _registerId, Decimal _total) {
            this.storeId=_storeId;
            this.registerId=_registerId;
            this.total=_total;
        }
    }
}