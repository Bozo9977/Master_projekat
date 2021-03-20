using Common.DataModel;
using SCADA_Common.Data;
using SCADA_Common.DB_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Common.DAO
{
    public class ScadaDB : IDisposable, IScadaDB
    {
        private RepoAccess<AnalogPointItemDB> analogRA;
        private RepoAccess<DiscretePointItemDB> discreteRA;

        public ScadaDB()
        {
            analogRA = new RepoAccess<AnalogPointItemDB>();
            discreteRA = new RepoAccess<DiscretePointItemDB>();
        }


        public void Dispose()
        {
            
        }

        public Dictionary<long, ISCADAModelPointItem> GetModel()
        {
            Dictionary<long, ISCADAModelPointItem> dict = new Dictionary<long, ISCADAModelPointItem>();

            List<AnalogPointItemDB> analogs = analogRA.GetAll();
            foreach (var analog in analogs)
            {
                AnalogSCADAModelPointItem tempAnalog = new AnalogSCADAModelPointItem(analog);
                dict.Add(tempAnalog.Gid, tempAnalog);
            }

            List<DiscretePointItemDB> discretes = discreteRA.GetAll();
            foreach(var discrete in discretes)
            {
                DiscreteSCADAModelPointItem tempDiscrete = new DiscreteSCADAModelPointItem(discrete);
                dict.Add(tempDiscrete.Gid, tempDiscrete);
            }

            return dict;
        }
    }
}
