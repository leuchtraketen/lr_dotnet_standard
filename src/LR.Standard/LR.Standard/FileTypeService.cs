using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace LR.Standard;

public sealed class FileTypeService
{
    private readonly ILogger _logger;

    public FileTypeService(ILogger<FileTypeService> logger)
    {
        _logger = logger;
    }

    private (string mime_type, string file_extension) _determineFileType(byte[] signature)
    {
        if (signature[0] == 0x25 && signature[1] == 0x50 && signature[2] == 0x44 && signature[3] == 0x46)
        {
            return ( "application/pdf", "pdf" );
        }

        if (signature[0] == 0x50 && signature[1] == 0x4B && signature[2] == 0x03 && signature[3] == 0x04)
        {
            return ( "application/zip", "zip" );
        }

        if (signature[4] == 0x66 && signature[5] == 0x74 && signature[6] == 0x79 && signature[7] == 0x70)
        {
            return ( "video/mp4", "mp4" );
        }

        if (signature[0] == 0xFF && signature[1] == 0xD8 && signature[2] == 0xFF)
        {
            return ( "image/jpeg", "jpg" );
        }

        if (signature[0] == 0x89 && signature[1] == 0x50 && signature[2] == 0x4E)
        {
            return ( "image/png", "png" );
        }

        if (signature[0] == 0x89 && signature[1] == 0x50 && signature[2] == 0x4E)
        {
            return ( "image/png", "png" );
        }

        return ( "", "" );
    }

    public string DetermineFileExtension(string? mime)
    {
        if (mime is null) return string.Empty;

        switch (mime)
        {
            case "application/pdf":
                return "pdf";
            case "application/zip":
                return "zip";
            case "video/mp4":
                return "mp4";
            case "image/jpeg":
                return "jpg";
            case "image/png":
                return "png";
            case "text/markdown":
                return "md";
            case "text/plain":
                return "txt";
            case "text/html":
                return "html";
        }

        return string.Empty;
    }

    public (string mime_type, string file_extension) DetermineFileType(string full_path)
    {
        using (FileStream stream = System.IO.File.OpenRead(full_path))
        {
            byte[] signature = new byte [10];
            stream.Read(signature, 0, signature.Length);
            ( string mime_type, string file_extension ) = _determineFileType(signature);

            // detect file types that are zip archives
            if (mime_type == "application/zip")
            {
                try
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    using (ZipArchive zip = new(stream, ZipArchiveMode.Read, true))
                    {
                        ReadOnlyCollection<ZipArchiveEntry> entries = zip.Entries;
                        if (entries.Any(e => e.FullName.EndsWith("workbook.xml")))
                        {
                            mime_type = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            file_extension = "xlsx";
                        }
                        else if (entries.Any(e => e.FullName.EndsWith("document.xml")))
                        {
                            mime_type = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                            file_extension = "docx";
                        }
                        else if (entries.Any(e => e.FullName.EndsWith("presentation.xml")))
                        {
                            mime_type = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                            file_extension = "pptx";
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to determine file type.");
                }
            }

            return ( mime_type, file_extension );
        }
    }
}
