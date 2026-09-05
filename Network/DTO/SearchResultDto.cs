using System.Collections.Generic;
using icpms_client.Common.Constants;

namespace icpms_client.Network.DTO;

public class SearchResultDto<T>
{
    public int? PageNo { get; set; }
    
    public int? Limit { get; set; } = CommonConstants.RowPerPage; 
    
    public int? TotalPage { get; set; }
    
    public int? TotalRecords { get; set; }
    
    public int? PageCount { get; set; }
    
    public bool? HasNextPage { get; set; }
    
    public List<T> Results { get; set; } = new List<T>();
}