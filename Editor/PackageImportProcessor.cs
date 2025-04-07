#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.PackageManager;

namespace RBG.Test2.Editor
{
    public class PackageImportProcessor : AssetPostprocessor
    {
        private const string PACKAGE_NAME = "com.rbg.test2";
        private const string SOURCE_PATH = "Assets/Test2 Assets";
        
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            Debug.Log($"Initialize methods");
            UnityEditor.PackageManager.Events.registeredPackages -= OnPackagesRegistered;
            UnityEditor.PackageManager.Events.registeredPackages += OnPackagesRegistered;
        }

        private static void OnPackagesRegistered(PackageRegistrationEventArgs args)
        {
            foreach (var package in args.added)
            {
                Debug.Log($"Adding package: Display name {package.displayName}, package.version: {package.version},  Package name : {package.name}");
                if (!package.name.ToLower().Equals(PACKAGE_NAME.ToLower())) continue;
                OrganizeAssets();
            }
        }

        private static void OrganizeAssets()
        {
            Debug.Log($"Organizing assets for PACKAGE ::{PACKAGE_NAME}");
            GetPackageFolders(PACKAGE_NAME);
            AssetDatabase.Refresh();
        }
        
        private static void GetPackageFolders(string packageName)
        {
            List<string> folderPaths = new List<string>();
            string packagePath = $"Packages/{packageName}";
            
            if (Directory.Exists(packagePath))
            {
                string[] subFolders = AssetDatabase.GetSubFolders(packagePath);
                foreach (string subFolder in subFolders)
                {
                    string folder = Path.GetFileName(subFolder.TrimEnd('/', '\\'));
                    if(folder.ToLower().Equals("editor") || folder.ToLower().Equals("scripts")|| folder.ToLower().Equals("script"))
                        continue;
                    
                    string newPath = $"{SOURCE_PATH}/{folder}"; 
                    MoveFolder(subFolder, newPath);
                }
            }
        }
        private static void MoveFolder(string sourceFolderPath, string destinationFolderPath)
        {
            try
            {
                // Ensure source folder exists
                if (!AssetDatabase.IsValidFolder(sourceFolderPath))
                {
                    Debug.LogError($"Source folder not found: {sourceFolderPath}");
                    return;
                }

                // Ensure destination path starts with "Assets/"
                if (!destinationFolderPath.StartsWith("Assets/"))
                {
                    destinationFolderPath = "Assets/" + destinationFolderPath;
                }

                // Create destination folder structure
                string[] folderStructure = destinationFolderPath.Split('/');
                string currentPath = folderStructure[0]; // "Assets"
                for (int i = 1; i < folderStructure.Length; i++)
                {
                    string parentPath = currentPath;
                    string folderName = folderStructure[i];
                    string newPath = Path.Combine(parentPath, folderName).Replace("\\", "/");
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(parentPath, folderName);
                    }
                    currentPath = newPath;
                }

                // Get all assets in the source folder
                string[] guids = AssetDatabase.FindAssets("", new[] { sourceFolderPath });

                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                   
                    // Calculate the relative path from source to create the same structure in destination
                    string relativePath = assetPath.Substring(sourceFolderPath.Length);
                    string newPath = destinationFolderPath + relativePath;
                    // Ensure the directory exists
                    string directory = Path.GetDirectoryName(newPath);
                   // Debug.Log($"relative path : {relativePath}, newPath :{newPath}");
                    if (!string.IsNullOrEmpty(directory))
                    {
                        string unityDirectory = directory.Replace("\\", "/");
                        if (!AssetDatabase.IsValidFolder(unityDirectory))
                        {
                            string parentPath = Path.GetDirectoryName(unityDirectory).Replace("\\", "/");
                            string folderName = Path.GetFileName(unityDirectory);
                            AssetDatabase.CreateFolder(parentPath, folderName);
                        }
                    }

                    // Copy the asset
                    if (File.Exists(assetPath))
                    {
                        bool issuccess = AssetDatabase.CopyAsset(assetPath, newPath);
                        if (!issuccess)
                        {
                            Debug.LogError($"Error copying asset {assetPath}: ");
                        }
                        else
                        {
                            //Debug.Log($"Successfully copied: {assetPath} to {newPath}");
                        }
                    }
                }

                AssetDatabase.Refresh();
                Debug.Log($"Successfully moved folder from {sourceFolderPath} to {destinationFolderPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error moving folder: {e.Message}\n{e.StackTrace}");
            }
        }

        
    }
}
#endif

