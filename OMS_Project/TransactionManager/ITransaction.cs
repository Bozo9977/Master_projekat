using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager
{
    [ServiceContract(CallbackContract = typeof(ITransactionCallback))]
    public interface ITransaction
    {
        [OperationContract]
        void Enlist();

        [OperationContract]
        void Prepare(Delta delta);

        [OperationContract]
        void Commit();

        [OperationContract]
        void Rollback();
    }
}
