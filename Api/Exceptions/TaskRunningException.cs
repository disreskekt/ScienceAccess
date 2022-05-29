using System;

namespace Api.Exceptions;

public class TaskRunningException : Exception
{
    public TaskRunningException(string message)
        : base(message) { }
}