using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Tapex_Project.Models;

namespace Tapex_Project.ViewModels
{
    /// <summary>
    /// ResultView의 ViewModel: 검사 결과 리스트, 정렬, 선택 관리 및 타입별 개수
    /// </summary>
    public class ResultViewModel : ViewModelBase
    {
        // 원본 리스트 보관용
        private List<DefectResult> _rawResults = new();

        // 1) 필터용 옵션 (“All” + enum 이름들)
        public IReadOnlyList<string> FilterOptions { get; }
            = new[] { "All" }
              .Concat(Enum.GetNames(typeof(DefectType)))
              .ToList();

        private string _selectedFilter = "All";
        /// <summary>현재 선택된 불량 타입 필터 ("All" 이면 전체)</summary>
        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (SetProperty(ref _selectedFilter, value))
                    ApplySorting();    // 필터 변경 시에도 정렬+필터 재적용
            }
        }


        /// <summary>정렬 기준</summary>
        public enum SortField
        {
            WidthMm,     // 가로 길이 (mm) 기준
            Brightness,  // 밝기
            Sharpness    // 선명도
        }

        /// <summary>콤보박스 바인딩용 정렬 옵션</summary>
        public IReadOnlyList<SortField> SortOptions { get; }
            = Enum.GetValues(typeof(SortField))
                  .Cast<SortField>()
                  .ToList();

        private SortField _selectedSort = SortField.WidthMm;
        /// <summary>현재 선택된 정렬 기준</summary>
        public SortField SelectedSort
        {
            get => _selectedSort;
            set
            {
                if (SetProperty(ref _selectedSort, value))
                    ApplySorting();
            }
        }

        /// <summary>화면에 표시되는 결과 리스트</summary>
        public ObservableCollection<DefectResult> Results { get; }
            = new ObservableCollection<DefectResult>();

        private DefectResult? _selectedResult;
        /// <summary>현재 선택된 항목</summary>
        public DefectResult? SelectedResult
        {
            get => _selectedResult;
            set => SetProperty(ref _selectedResult, value);
        }

        /// <summary>타입별 검출 개수 요약</summary>
        public ObservableCollection<TypeCount> TypeCounts { get; }
            = new ObservableCollection<TypeCount>();

        /// <summary>
        /// 새로운 검사 결과가 들어올 때 호출.
        /// 내부에서 정렬, 개수 갱신, 선택 처리.
        /// </summary>
        public void Update(IEnumerable<DefectResult> items)
        {
            _rawResults = items.ToList();
            ApplySorting();
            RefreshTypeCounts();
            SelectedResult = null;
        }

        // 선택된 정렬 기준에 따라 Results 컬렉션 갱신
        private void ApplySorting()
        {
            var filtered = _selectedFilter == "All"
            ? _rawResults
            : _rawResults.Where(r => r.Type.ToString() == _selectedFilter);



            IOrderedEnumerable<DefectResult> sorted = _selectedSort switch
            {
                SortField.WidthMm => filtered.OrderByDescending(r => r.WidthMm),
                SortField.Brightness => filtered.OrderByDescending(r => r.Brightness),
                SortField.Sharpness => filtered.OrderByDescending(r => r.Sharpness),
                _ => filtered.OrderByDescending(r => r.WidthMm),
            };

            Results.Clear();
            foreach (var r in sorted)
                Results.Add(r);
        }

        // 원본 리스트 기준으로 타입별 개수 갱신
        private void RefreshTypeCounts()
        {
            TypeCounts.Clear();
            foreach (var g in _rawResults.GroupBy(r => r.Type))
            {
                TypeCounts.Add(new TypeCount
                {
                    Type = g.Key,
                    Count = g.Count()
                });
            }
        }

        /// <summary>타입별 개수 모델</summary>
        public class TypeCount
        {
            public DefectType Type { get; set; }
            public int Count { get; set; }
        }
    }
}
