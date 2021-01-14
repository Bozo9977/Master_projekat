using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager
{
    
    public interface ITransactionCallback
    {
        [OperationContract(IsOneWay = true)]
        void CallbackEnlist(bool prepare);

        [OperationContract(IsOneWay = true)]
        void CallbackPrepare(bool prepare);

        [OperationContract(IsOneWay = true)]
        void CallbackCommit(string commit);

        [OperationContract(IsOneWay = true)]
        void CallbackRollback(string rollback);
    }
}
