using LegacyOrderService.Common;

namespace LegacyOrderService.Infrastructure;

public static class DatabaseInitializer
{
    public static string DbPath => GetDbPath();
    
    public static string EnsureDatabase()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        Console.WriteLine($"Serve the database at: {appData}");
        
        var appDir = Path.Combine(appData, AppConstants.AppName);
        // create dir if it's not existed
        Directory.CreateDirectory(appDir);
        
        var dbPath = Path.Combine(appDir, AppConstants.DatabaseFileName);

        if (!File.Exists(dbPath))
        {
            var templatePath = Path.Combine(
                AppContext.BaseDirectory,
                AppConstants.DatabaseFileName
            );
            
            if (!File.Exists(templatePath))
                throw new FileNotFoundException(
                    "Database template not found",
                    templatePath
                );
            
            File.Copy(templatePath, dbPath);
        }
        
        return dbPath;
    }

    private static string GetDbPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            $"{AppConstants.AppName}/{AppConstants.DatabaseFileName}"
        );
    }
}