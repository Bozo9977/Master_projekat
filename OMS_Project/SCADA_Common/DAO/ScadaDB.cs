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

        public void PersistUpdate(Dictionary<long, ISCADAModelPointItem> newModel)
        {
            Dictionary<long, ISCADAModelPointItem> currentModel = GetModel();

            foreach (var point in newModel)
            {
                if (!currentModel.ContainsKey(point.Key))
                    InsertIntoDB(point.Value);
                if (currentModel.ContainsKey(point.Key))
                    UpdateIntoDB(point.Value);
            }

            foreach (var point in currentModel)
            {
                if (!newModel.ContainsKey(point.Key))
                    DeleteFromDB(point.Value);
            }

        }

        public void RollbackUpdate(Dictionary<long, ISCADAModelPointItem> oldModel)
        {
            Dictionary<long, ISCADAModelPointItem> currentModel = GetModel();

            foreach (var point in oldModel)
            {
                if (!currentModel.ContainsKey(point.Key))
                    InsertIntoDB(point.Value);
                if (currentModel.ContainsKey(point.Key))
                    UpdateIntoDB(point.Value);
            }

            foreach (var point in currentModel)
            {
                if (!oldModel.ContainsKey(point.Key))
                    DeleteFromDB(point.Value);
            }

        }

        private void InsertIntoDB(ISCADAModelPointItem point)
        {
            if (point.RegisterType == PointType.ANALOG_INPUT || point.RegisterType == PointType.ANALOG_OUTPUT)
            {
                analogRA.Insert(((AnalogSCADAModelPointItem)point).ToDBEntity());
            }
            else
            {
                discreteRA.Insert(((DiscreteSCADAModelPointItem)point).ToDBEntity());
            }
        }

        private void UpdateIntoDB(ISCADAModelPointItem point)
        {
            if (point.RegisterType == PointType.ANALOG_INPUT || point.RegisterType == PointType.ANALOG_OUTPUT)
            {
                analogRA.Update(((AnalogSCADAModelPointItem)point).ToDBEntity());
            }
            else
            {
                discreteRA.Update(((DiscreteSCADAModelPointItem)point).ToDBEntity());
            }
        }

        private void DeleteFromDB(ISCADAModelPointItem point)
        {
            if (point.RegisterType == PointType.ANALOG_INPUT || point.RegisterType == PointType.ANALOG_OUTPUT)
            {
                analogRA.Delete(point.Gid);
            }
            else
            {
                discreteRA.Delete(point.Gid);
            }
        }

    }
}
