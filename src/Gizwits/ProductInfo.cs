namespace Gizwits;

public class ProductInfo
{
    public readonly string App_ID;
    public readonly string App_Secret;
    public readonly IEnumerable<Tuple<string, string, string>> Product_Infos;
    public readonly string SoftAp_Prefix;
    public readonly string BLE_Prefix;

    public ProductInfo(
        string app_ID, string app_Secret,
        IEnumerable<Tuple<string, string, string>> product_Infos = null,
        string softAp_Prefix = null, string ble_Prefix = null)
    {
        App_ID = app_ID;
        App_Secret = app_Secret;
        Product_Infos = product_Infos;
        SoftAp_Prefix = !String.IsNullOrEmpty(softAp_Prefix) ? softAp_Prefix : "XPG-GAgent-";
        BLE_Prefix = !String.IsNullOrEmpty(ble_Prefix) ? ble_Prefix : this.SoftAp_Prefix;
    }
}
