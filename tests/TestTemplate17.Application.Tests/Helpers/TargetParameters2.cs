using System.Collections.Generic;
using TestTemplate17.Application.Sorting.Models;

namespace TestTemplate17.Application.Tests.Helpers;

public class TargetParameters2
    : BaseSortable<MappingTargetModel2>
{
    public override IEnumerable<SortCriteria> SortBy { get; set; } = new List<SortCriteria>();
}
