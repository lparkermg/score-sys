using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScoreSys.Api
{
    public interface IQuery<T>
    {
        Task<IList<T>> Get(Guid gameId, int take, int skip);
    }
}
