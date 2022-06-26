using System.Collections.Generic;
using System.Linq;

namespace Api.Models.TestTaskMangerModels;

public class TestTaskStatus
{
    public Dictionary<string, string?> Dictionary { get; set; }
    public bool HasFreeGpu => this.Dictionary.Values.Any(value => value is null);
}