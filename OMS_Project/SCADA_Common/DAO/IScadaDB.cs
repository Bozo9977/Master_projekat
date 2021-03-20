using SCADA_Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Common.DAO
{
    public interface IScadaDB
    {
        Dictionary<long, ISCADAModelPointItem> GetModel();
    }
}
