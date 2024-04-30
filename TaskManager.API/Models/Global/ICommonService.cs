using Common.Models;

namespace TaskManager.API.Models.Global
{
    public interface ICommonService<T>
    {
        ResultModel Create(T model);
        ResultModel Patch(int id, T model);
        ResultModel Delete(int id);
        ResultModel Get(int id);
    }
}
    