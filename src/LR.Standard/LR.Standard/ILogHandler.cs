using System;

namespace LR.Standard;

public interface ILogHandler
{
    void HandleMessage(string level, string? message, string? member_name, string? file_path, int line_number);
}
