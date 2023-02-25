#if !DEBUG
using System.Collections.Generic;
using System.IO;
using System.Linq;
#endif
using System;
using Teuria;

namespace Archie.DesktopGL;

internal class Program 
{
    [STAThread]
    internal static void Main() 
    {
        SkyLog.Log("Pixxa is starting", SkyLog.LogLevel.Info);
        using var game = new Pixxa.Pixxa(1024, 640, 1024, 640, "Pixxa", false);
#if !DEBUG
        try 
        {

            game.Run();
        }
        catch (Exception e) 
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (CheckIfLogsBloat(logPath, 5, out List<string> files)) 
            {
                foreach (var f in files) 
                {
                    File.Delete(f);
                }
            }
            var dateTime = DateTime.Now.ToString("HH-mm-ss");
            var file = $"Logs/ARCHIEERROR-{dateTime}.txt";
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
            SkyLog.Error(e.ToString());
            SkyLog.WriteToFile(filePath);
            SkyLog.OpenLog(filePath);
        }
#else
        game.Run();
#endif
    }
#if !DEBUG
    private static bool CheckIfLogsBloat(string path, int numOfBloat, out List<string> files) 
    {
        var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        if (!Directory.Exists(logPath)) 
        {
            files = null;
            return false;
        }

        files = Directory.GetFiles(logPath).Where(
            x => Path.GetFileName(x).StartsWith("ARCHIEERROR")).ToList();
        return files.Count >= 5;
    }
#endif
}
