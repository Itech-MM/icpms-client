using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using icpms_client.Common.UI;
using icpms_client.Network.DTO;
using icpms_client.Network.DTO.ParkingSession;
using icpms_client.Network.Response;
using icpms_client.Network.Services.Visitor;
using log4net;

namespace icpms_client.Pages.Screens.Sections.ViewModels;

public class RecentVisitorsSectionViewModel : ScreenViewModelBase
{
    private readonly ILog _log = LogManager.GetLogger(nameof(RecentVisitorsSectionViewModel));
    private readonly VisitorService _recentVisitorService;

    private readonly SemaphoreSlim _loadLock = new(1, 1);

    private int _currentPage = 1;
    private bool _hasNextPage = true;
    private bool _hasInitialized;
    
    public IEnumerable<int> SkeletonRows { get; } = Enumerable.Range(0, 6);

    public ObservableCollection<RecentVisitorItemViewModel> Visitors { get; } = new();

    private bool _isInitialLoading = true;
    public bool IsInitialLoading
    {
        get => _isInitialLoading;
        private set
        {
            _isInitialLoading = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasNoData));
        }
    }

    private bool _isLoadingMore;
    public bool IsLoadingMore
    {
        get => _isLoadingMore;
        private set { _isLoadingMore = value; OnPropertyChanged(); }
    }

    public bool HasNoData => !IsInitialLoading && Visitors.Count == 0;

    public RecentVisitorsSectionViewModel(VisitorService recentVisitorService)
    {
        _recentVisitorService = recentVisitorService;
    }

    public async Task InitializeAsync()
    {
        if (_hasInitialized) return;
        _hasInitialized = true;

        await LoadAsync(initialize: true);
    }

    private async Task LoadAsync(int pageNo = 1, bool initialize = false)
    {
        await _loadLock.WaitAsync();
        try
        {
            if (initialize)
            {
                IsInitialLoading = true;
                pageNo = 1;
            }

            var response = await _recentVisitorService.SearchRecentVisitors(pageNo);

            if (response is BaseResponse<SearchResultDto<RecentVisitorDto>> { Success: true } success)
            {
                var page = success.Data;
                var results = page?.Results ?? new List<RecentVisitorDto>();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (initialize) Visitors.Clear();

                    foreach (var dto in results)
                        Visitors.Add(new RecentVisitorItemViewModel(dto));

                    OnPropertyChanged(nameof(HasNoData));
                });

                _currentPage = page?.PageNo ?? pageNo;
                _hasNextPage = page?.HasNextPage ?? false;
            }
            else if (response is BaseErrorResponse<string> error)
            {
                _log.Error($"Failed to load recent visitors: {error.Message}");
            }
        }
        catch (Exception ex)
        {
            _log.Error($"LoadAsync error: {ex.Message}");
        }
        finally
        {
            if (initialize) IsInitialLoading = false;
            _loadLock.Release();
        }
    }

    public async Task LoadMoreAsync()
    {
        if (IsLoadingMore || IsInitialLoading || !_hasNextPage) return;

        IsLoadingMore = true;
        try
        {
            await LoadAsync(_currentPage + 1);
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    public async Task RefreshPreservingPositionAsync()
    {
        await _loadLock.WaitAsync();
        try
        {
            var pagesToReload = Math.Max(_currentPage, 1);
            var merged = new List<RecentVisitorDto>();
            var hasNext = false;
            var lastPageNo = 1;

            for (var page = 1; page <= pagesToReload; page++)
            {
                var response = await _recentVisitorService.SearchRecentVisitors(page);

                if (response is BaseResponse<SearchResultDto<RecentVisitorDto>> { Success: true } success)
                {
                    var pageData = success.Data;
                    merged.AddRange(pageData?.Results ?? new List<RecentVisitorDto>());
                    hasNext = pageData?.HasNextPage ?? false;
                    lastPageNo = pageData?.PageNo ?? page;
                }
                else if (response is BaseErrorResponse<string> error)
                {
                    _log.Error($"Refresh failed on page {page}: {error.Message}");
                    return;
                }
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                MergeVisitors(merged);
                OnPropertyChanged(nameof(HasNoData));
            });

            _currentPage = lastPageNo;
            _hasNextPage = hasNext;
        }
        finally
        {
            _loadLock.Release();
        }
    }

    private void MergeVisitors(List<RecentVisitorDto> merged)
    {
        var incomingIds = new HashSet<long>(merged.Select(d => d.Id));

        for (var i = Visitors.Count - 1; i >= 0; i--)
        {
            if (!incomingIds.Contains(Visitors[i].Id))
                Visitors.RemoveAt(i);
        }

        for (var targetIndex = 0; targetIndex < merged.Count; targetIndex++)
        {
            var dto = merged[targetIndex];
            var currentIndex = FindIndexById(dto.Id);

            if (currentIndex == -1)
            {
                Visitors.Insert(Math.Min(targetIndex, Visitors.Count), new RecentVisitorItemViewModel(dto));
            }
            else
            {
                if (currentIndex != targetIndex)
                    Visitors.Move(currentIndex, Math.Min(targetIndex, Visitors.Count - 1));

                Visitors[targetIndex].UpdateFrom(dto);
            }
        }
    }

    private int FindIndexById(long id)
    {
        for (var i = 0; i < Visitors.Count; i++)
        {
            if (Visitors[i].Id == id)
                return i;
        }
        return -1;
    }

}