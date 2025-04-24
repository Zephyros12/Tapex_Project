using System.Collections.Generic;
using System.Collections.ObjectModel;
using Tapex_Project.Models;

namespace Tapex_Project.ViewModels
{
    /// <summary>
    /// ResultView의 ViewModel: 검사 결과 리스트 및 선택 관리
    /// </summary>
    public class ResultViewModel : ViewModelBase
    {
        public ObservableCollection<DefectResult> Results { get; } = new();

        private DefectResult? _selectedResult;
        public DefectResult? SelectedResult
        {
            get => _selectedResult;
            set => SetProperty(ref _selectedResult, value);
        }

        public void Update(IEnumerable<DefectResult> items)
        {
            Results.Clear();
            foreach (var item in items)
                Results.Add(item);
        }
    }
}