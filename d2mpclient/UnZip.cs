// 
// UnZip.cs
// Created by ilian000 on 2014-07-02
// Licenced under the Apache License, Version 2.0
//

using System;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
namespace d2mp
{
    public class UnZip
    {
        public static void unzipFromStream(Stream fileStream, string outFolder)
        {
            ZipConstants.DefaultCodePage = 850;//force the use of this codepage to unzip. Multilingual (Latin-1) (Western European languages)
            ZipFile zipFile = new ZipFile(fileStream);
            if (zipFile.TestArchive(true))
            {
                try
                {
                    foreach (ZipEntry e in zipFile)
                    {

                        String entryFileName = e.Name;
                        byte[] buffer = new byte[4096];
                        Stream zipStream = zipFile.GetInputStream(e);

                        String fullZipToPath = Path.Combine(outFolder, entryFileName);
                        string directoryName = Path.GetDirectoryName(fullZipToPath);
                        if (directoryName.Length > 0)
                            Directory.CreateDirectory(directoryName);

                        if (Path.GetFileName(fullZipToPath) != String.Empty)
                        {
                            using (FileStream streamWriter = File.Create(fullZipToPath))
                            {
                                StreamUtils.Copy(zipStream, streamWriter, buffer);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    try
                    {
                        if (Directory.Exists(outFolder))
                        {
                            Directory.Delete(outFolder);
                        }
                    }
                    catch { }
                    throw e;
                }
            }
            else
            {
                throw new Exception("Zip file not valid.");
            }
        }
    }
}
