namespace MyStore.Application.ModelView
{
    public class VNPay
    {
        public string vnp_Amount { get; set; }
        public string vnp_Command { get; set; }
        public string vnp_CreateDate { get; set; }
        public string vnp_CurrCode { get; set; }
        public string vnp_ExpireDate { get; set; }
        public string vnp_IpAddr { get; set; }
        public string vnp_Locale { get; set; }
        public string vnp_OrderInfo { get; set; }
        public string vnp_OrderType { get; set; }
        public string vnp_ReturnUrl { get; set; }
        public string vnp_TmnCode { get; set; }
        public string vnp_TxnRef { get; set; }
        public string vnp_Version { get; set; }
    }

    public class VNPayRequest
    {
        public string vnp_TmnCode { get; set; }
        public string vnp_Amount { get; set; }
        public string vnp_BankCode { get; set; }
        public string? vnp_BankTranNo { get; set; }
        public string? vnp_CardType { get; set; }
        public string? vnp_PayDate { get; set; }
        public string vnp_OrderInfo { get; set; }
        public string vnp_TransactionNo { get; set; }
        public string vnp_ResponseCode { get; set; }
        public string vnp_TransactionStatus { get; set; }
        public string vnp_TxnRef { get; set; }
        public string vnp_SecureHash { get; set; }
    }

    public class VNPayQueryDr
    {
        public string vnp_RequestId { get; set; }
        public string vnp_Version { get; set; }
        public string vnp_Command { get; set; }
        public string vnp_TmnCode { get; set; }
        public string vnp_TxnRef { get; set; }
        public string vnp_TransactionDate { get; set; }
        public string vnp_CreateDate { get; set; }
        public string vnp_IpAddr { get; set; }
        public string vnp_OrderInfo { get; set; }
    }
    public class VNPayQueryDrResponse
    {
        public string vnp_ResponseId { get; set; }
        public string vnp_Command { get; set; }
        public string vnp_ResponseCode { get; set; }
        public string vnp_Message { get; set; }
        public string vnp_TmnCode { get; set; }
        public string vnp_TxnRef { get; set; }
        public string vnp_Amount { get; set; }
        public string vnp_BankCode { get; set; }
        public string vnp_PayDate { get; set; }
        public string vnp_TransactionNo { get; set; }
        public string vnp_TransactionType { get; set; }
        public string vnp_TransactionStatus { get; set; }
        public string vnp_OrderInfo { get; set; }
        public string? vnp_PromotionCode { get; set; }
        public string? vnp_PromotionAmount { get; set; }
        public string vnp_SecureHash { get; set; }
    }
}
