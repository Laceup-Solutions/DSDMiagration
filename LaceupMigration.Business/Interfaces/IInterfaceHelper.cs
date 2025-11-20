namespace LaceupMigration;

public interface IInterfaceHelper
{
    string GetDeviceModel();

    string GetOsVersion();

    string GetCarrier();

    string GetBrand();

    string GetDeviceId();

    string GetLocale();

    string GetSystemName();

    string GetIdiom();

    string GetSSID();

    void CheckForIcon(string value);

    void SyncronizeDefaults();

    void StoreDefaults(string value, string key);

    void StoreDefaults(float value, string key);

    void StoreDefaults(bool value, string key);

    void StoreDefaults(int value, string key);

    void StoreDefaults(double value, string key);

    string GetDefaultString(string key, string defaultValue);

    bool GetDefaultBool(string key, bool defaultValue);

    float GetDefaultFloat(string key, float defaultValue);

    int GetDefaultInt(string key, int defaultValue);

    double GetDefaultDouble(string key, double defaultValue);

    void ViewPdf(string filepath);
    void PrintPdf(string filepath);
    void SendReportByEmail(string pdfFile);

    void SubscribeToTopic(string topic);

    void UnsubscribeToTopic(string topic);

    int PrintProcess1(List<Order> orders);

    void PrintedSelected(string printer);

    bool Print(List<Order> orders);
    List<string> GetAllPrinters();

    void ExitApplication();

    void PrintIt(string printit);

    IList<PrinterDescription> AvailableDevices();
    IList<PrinterDescription> AvailablePrinters();

    string GetEmptyPdf();

    void HideKeyboard();
}