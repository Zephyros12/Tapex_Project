using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tapex_Project.Services;

public interface IFilePickerService
{
    Task<IReadOnlyList<string>> PickImageFilesAsync();
}
