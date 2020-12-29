using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.ViewModels
{
    public class ShellViewModel : Screen
    {
        private int _number1 = 0;

        public int Number1
        {
            get { return _number1; }
            set 
            { 
                _number1 = value;
                NotifyOfPropertyChange(() => Number1);
                NotifyOfPropertyChange(() => Result);
            }
        }

        private int _number2 = 0;

        public int Number2
        {
            get { return _number2; }
            set 
            { 
                _number2 = value;
                NotifyOfPropertyChange(() => Number2);
                NotifyOfPropertyChange(() => Result);
            }
        }


        public string Result
        {
            get { return (Number1 + Number2).ToString(); }
        }


    }
}
