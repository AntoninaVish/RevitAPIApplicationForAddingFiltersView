using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Prism.Commands;
using RevitAPIApplicationFiltersViewLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPIApplicationForAddingFiltersView
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;

        private Document _doc;

        public List<ViewPlan> Views { get; }

        public List<ParameterFilterElement> Filters { get; }

        public DelegateCommand AddFilterCommand { get; }

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;

            _doc = _commandData.Application.ActiveUIDocument.Document;

            Views = ViewsUtils.GetFloorPlanViews(_doc);

            Filters = FilterUtils.GetFilters(_doc);

            //создаем новую команду 
            AddFilterCommand = new DelegateCommand(OnAddFilterCommand);

        }

        public ParameterFilterElement SelectedFilter { get; set; } //фильтр это параметр, а все остальные это список

        private void OnAddFilterCommand() //метод, который будет добавлять фильтр на текущий выбраный вид в плане
        {
            if (SelectedViewPlan == null || SelectedFilter == null) //проверяем выбранный фильтр что он не пустой
                return;

            using ( var ts = new Transaction(_doc, "Set filter"))
            {
                ts.Start();
                SelectedViewPlan.AddFilter(SelectedFilter.Id); //к выбраному плану добавляем фильтр

                //настраиваем фильтр, помним что у Revit есть настройки цвета, настройки линий
                //создаем переменную OverrideGraphicSettings
                //т.е выбираем уже добавленый фильтр из плана этажа и все настройки будем проводить через переменную overrideGraphicSettings
                OverrideGraphicSettings overrideGraphicSettings = SelectedViewPlan.GetFilterOverrides(SelectedFilter.Id);
                overrideGraphicSettings.SetProjectionLineColor(new Color(255, 0, 0)); //задаем цвет линии
                SelectedViewPlan.SetFilterOverrides(SelectedFilter.Id, overrideGraphicSettings); //устанавливаем эти настройки для фильтра SelectedFilter к выбраному виду

                ts.Commit();
            }
            RaiseCloseRequest();


        }
                        
        public ViewPlan SelectedViewPlan { get; set; }
              
        public event EventHandler CloseRequest;

        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }

    }
}
