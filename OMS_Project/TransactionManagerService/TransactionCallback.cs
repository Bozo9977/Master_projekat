using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionManager;

namespace TransactionManagerService
{
    public class TransactionCallback : ITransactionCallback
    {
        public TransactionCallback()
        {

        }
              

        public void CallbackEnlist(bool prepare)
        {
            Console.WriteLine("Return for enlist.");
        }

        public void CallbackPrepare(bool prepare)
        {
            Console.WriteLine("Return for prepare");
        }

        public void CallbackCommit(string commit)
        {
            Console.WriteLine(commit);
        }

        public void CallbackRollback(string rollback)
        {
            Console.WriteLine(rollback);
        }
    }
}
