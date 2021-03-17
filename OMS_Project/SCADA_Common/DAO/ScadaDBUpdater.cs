using Common.DataModel;
using SCADA_Common.DB_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Common.DAO
{
    public class ScadaDBUpdater : IDisposable
    {
        private RepoAccess<AnalogPointItemDB> analogRA;
        private RepoAccess<DiscretePointItemDB> discreteRA;

        public ScadaDBUpdater()
        {
            analogRA = new RepoAccess<AnalogPointItemDB>();
            discreteRA = new RepoAccess<DiscretePointItemDB>();
        }


        public void PersistUpdate(List<IdentifiedObject> inserted, List<IdentifiedObject> updated, List<IdentifiedObject> deleted)
        {
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
