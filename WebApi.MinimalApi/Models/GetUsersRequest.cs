using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApi.MinimalApi.Models;

public class GetUsersRequest
{
    [DefaultValue(1)]
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [DefaultValue(10)]
    [Range(1, 20)]
    public int PageSize { get; set; } = 10;
}