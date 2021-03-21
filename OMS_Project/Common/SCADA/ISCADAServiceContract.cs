using Common.DataModel;
using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.SCADA
{
    [ServiceContract]
    public interface ISCADAServiceContract
    {
        [OperationContract]
        UpdateResult ApplyUpdate();
    }
}
