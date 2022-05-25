using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Api.Exceptions;

public class ExceptionList : Exception
{
    private readonly List<Exception> _list;
    public bool Any => _list.Any();

    public override string Message
    {
        get
        {
            StringBuilder sb = new StringBuilder();

            foreach (Exception exception in _list)
            {
                sb.Append(exception.Message);
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }
    }

    public ExceptionList()
    {
        _list = new List<Exception>();
    }

    public void AddException(Exception exception)
    {
        _list.Add(exception);
    }

    public string Messages()
    {
        StringBuilder sb = new StringBuilder();

        foreach (Exception exception in _list)
        {
            sb.Append(exception.Message);
            sb.Append("\n\n");
        }

        return sb.ToString();
    }
}