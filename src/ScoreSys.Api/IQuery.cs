using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScoreSys.Api
{
    public interface IQuery<T>
    {
        Task<T> Get(Guid id, int take, int skip);
    }
}
