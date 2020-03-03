namespace SimpleMultiTenant.Services
{
    public interface ISettingsCache
    {
        void ClearSetting<T>();
        T GetSetting<T>();
        void SetSetting<T>(object setting);
    }
}