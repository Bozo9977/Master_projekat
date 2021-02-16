using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TransactionManager;

namespace NMS
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class NetworkModelTransactionService : ITransaction
    {
        static GenericDataAccess gda = new GenericDataAccess();


        public void Enlist()
        {
            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            Console.WriteLine("Enlist called on NMS.");

            try
            {
                gda.GetCopyOfNetworkModel();
                callback.CallbackEnlist(true);
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                callback.CallbackEnlist(false);
            }
        }

        public void Prepare(Delta delta)
        {
            Console.WriteLine("Prepare called on NMS.");
            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();

            try
            {
                UpdateResult updateRes = gda.ApplyUpdate(delta);
                if(updateRes.Result == ResultType.Success)
                {
                    callback.CallbackPrepare(true);
                }
                else
                {
                    callback.CallbackPrepare(false);
                }
            }catch(Exception e)
            {
                callback.CallbackPrepare(false);
                Console.WriteLine(e.Message);
            }
        }

        public void Commit()
        {
            Console.WriteLine("Commit called on NMS.");

            if(GenericDataAccess.NewNetworkModel != null)
            {
                GenericDataAccess.Model = GenericDataAccess.NewNetworkModel;
            }

            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            callback.CallbackCommit("Commit on NMS successful.");
        }

       

        public void Rollback()
        {
            Console.WriteLine("Rollback called on NMS.");
            GenericDataAccess.NewNetworkModel = null;
            GenericDataAccess.Model = GenericDataAccess.OldNetworkModel;
            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            callback.CallbackRollback("Error on NMS.");
        }

    }
}
