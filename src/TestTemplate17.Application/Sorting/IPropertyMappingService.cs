using System.Collections.Generic;
using TestTemplate17.Application.Sorting.Models;

namespace TestTemplate17.Application.Sorting;

public interface IPropertyMappingService
{
    IEnumerable<SortCriteria> Resolve(BaseSortable sortableSource, BaseSortable sortableTarget);
}
