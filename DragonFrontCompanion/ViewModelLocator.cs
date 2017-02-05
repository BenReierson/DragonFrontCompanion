using DragonFrontDeckBuilder.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonFrontDeckBuilder
{
    public static class ViewModelLocator
    {
        static MainViewModel mainVM;

        public static MainViewModel MainVM => mainVM ?? (mainVM = new MainViewModel());
    }
}
