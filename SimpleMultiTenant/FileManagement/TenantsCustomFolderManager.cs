using SimpleMultiTenant.Extensions;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;

namespace SimpleMultiTenant.FileManagement
{
    public class TenantsCustomFolderManager
    {
        public static bool DoesTenantHaveCustomFolder(string tenantsDirectory, string tenantName)
        {
            return Directory.Exists(tenantsDirectory + tenantName);
        }

        public static void CreateTenantCustomFolder(string tenantsDirectory, string tenantName)
        {
            var templateFiles = Directory.EnumerateFiles(tenantsDirectory + "!", "*", SearchOption.AllDirectories);
            var directories = Directory.GetDirectories(tenantsDirectory + "!", "*", SearchOption.AllDirectories);
            directories.ToList().ForEach(directory => Directory.CreateDirectory(directory.Replace("!", tenantName)));

            foreach (var file in templateFiles)
            {
                var destFileName = file.Replace("!", tenantName);
                File.Copy(file, destFileName);
            }
        }

        /// <summary>
        /// Creates a custom folder for the tenant.
        /// </summary>
        /// <param name="tenantsDirectory"></param>
        /// <param name="tenantName"></param>
        /// <returns>Returns True if the Content Directory was created</returns>
        public static bool CreateContentDirectoryIfItDoesNotExist(string tenantsDirectory, string tenantName)
        {
            if (DoesTenantHaveCustomFolder(tenantsDirectory, tenantName) == false)
            {
                CreateTenantCustomFolder(tenantsDirectory, tenantName);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void DeleteTenantsCustomFolder(string tenantsDirectory, string tenantName)
        {
            var files = Directory.EnumerateFiles(tenantsDirectory + tenantName, "*", SearchOption.AllDirectories);
            files.ToList().ForEach(file => File.Delete(file));
            var directories = Directory.GetDirectories(tenantsDirectory + tenantName, "*", SearchOption.AllDirectories).ToList();
            directories.Sort((x, y) => y.NumberOfSlashes().CompareTo(x.NumberOfSlashes()));
            directories.ForEach(directory => Directory.Delete(directory));
            Directory.Delete(tenantsDirectory + tenantName);
        }

        public static void DeleteTenantFromConfiguration(string tenantName)
        {
            var appSettingsPath = Directory.GetCurrentDirectory() + "/appsettings.json";
            var appSettingsJObject = JObject.Parse(File.ReadAllText(appSettingsPath));
            var connectionStringToken = appSettingsJObject["ConnectionStrings"][tenantName];

            if (connectionStringToken != null)
            {
                connectionStringToken.Parent.Remove();
                appSettingsJObject.ToString();
                File.WriteAllText(appSettingsPath, appSettingsJObject.ToString());
            }
        }

        public static void CreateTenantInConfiguration(string tenantName, string connectionString)
        {
            var appSettingsPath = Directory.GetCurrentDirectory() + "/appsettings.json";
            var appSettingsJObject = JObject.Parse(File.ReadAllText(appSettingsPath));
            appSettingsJObject["ConnectionStrings"].Children().Last().AddAfterSelf(new JProperty(tenantName, connectionString));
            File.WriteAllText(appSettingsPath, appSettingsJObject.ToString());
        }
    }
}
