using System.Collections.ObjectModel;
using System.Windows;
using icpms_client.Common.Command;
using icpms_client.Common.UI;
using icpms_client.Network.DTO;
using icpms_client.Network.DTO.Member;
using icpms_client.Network.DTO.Vehicle;
using icpms_client.Network.Request.Member;
using icpms_client.Network.Response;
using icpms_client.Network.Services.Member;
using icpms_client.Network.Session;
using icpms_client.Services.UIServices;
using icpms_client.ViewModels.Wrappers;

namespace icpms_client.Pages.Screens.ViewModels;

public class MemberScreenViewModel : ScreenViewModelBase
{
    private readonly MemberService _memberService;

    private string? _searchKeyword;
    private bool _isSearching;
    private bool _isLookupTabActive = true;

    private MemberDto? _selectedMember;
    private MemberSubscriptionDto? _selectedSubscription;
    private ObservableCollection<MemberBalanceTransactionDto> _transactionHistory = new();

    private long _totalMembers;
    private long _regularMembers;
    private long _vipMembers;
    private long _expiredMembers;

    private string? _newMemberName;
    private string? _newMemberPhone;
    private string? _newMemberEmail;
    private bool _newMemberIsVip;

    private MemberPlanDto? _selectedPlanForSubscribe;
    private LabelValueWrapper? _selectedPlanOption;
    private decimal? _topUpAmount;
    private string? _topUpRemark;

    private bool _isSupervisorModalOpen;
    private string? _supervisorUsername;
    private string? _supervisorPassword;
    private Func<long?, Task>? _pendingAction;

    public ObservableCollection<MemberDto> SearchResults { get; } = new();

    public ObservableCollection<MemberPlanDto> ActivePlans { get; } = new();

    public ObservableCollection<LabelValueWrapper> ActivePlanOptions { get; } = new();
    
    private string? _newVehiclePlateNumber;
    private string? _newVehicleType;

    public ObservableCollection<VehicleDto> NewMemberVehicles { get; } = [];

    public MemberScreenViewModel(MemberService memberService)
    {
        _memberService = memberService;

        SwitchTabCommand = new RelayCommand(param => SwitchTab(param as string));
        SearchMembersCommand = new RelayCommand(async void (_) => await SearchMembersAsync());
        SelectMemberCommand = new RelayCommand(async void (param) => await SelectMemberAsync(param as MemberDto));

        SubscribeCommand = new RelayCommand(async void (_) => await ExecuteSupervisorGatedAsync(SubscribeAsync), _ => SelectedMember != null && SelectedPlanForSubscribe != null);
        RenewCommand = new RelayCommand(async void (_) => await ExecuteSupervisorGatedAsync(RenewAsync), _ => SelectedMember != null && SelectedSubscription != null);
        TopUpCommand = new RelayCommand(async void (_) => await ExecuteSupervisorGatedAsync(TopUpAsync), _ => SelectedMember != null && SelectedSubscription != null && TopUpAmount.HasValue);
        CancelSubscriptionCommand = new RelayCommand(async void (_) => await ExecuteSupervisorGatedAsync(CancelSubscriptionAsync), _ => SelectedMember != null && SelectedSubscription != null);

        ConfirmSupervisorApprovalCommand = new RelayCommand(async void (_) => await ConfirmSupervisorApprovalAsync());
        CancelSupervisorApprovalCommand = new RelayCommand(_ => CloseSupervisorModal());
        
        RegisterMemberCommand = new RelayCommand(
            async void (_) => await ExecuteSupervisorGatedAsync(RegisterMemberAsync),
            _ => NewMemberVehicles.Count > 0);
        
        NewMemberVehicles.CollectionChanged += (_, _) => RegisterMemberCommand.RaiseCanExecuteChanged();

        AddVehicleCommand = new RelayCommand(_ => AddVehicle(),
            _ => !string.IsNullOrWhiteSpace(NewVehiclePlateNumber));

        RemoveVehicleCommand = new RelayCommand(param =>
        {
            if (param is VehicleDto vehicle) NewMemberVehicles.Remove(vehicle);
        });
        
    }

    public bool IsCurrentOperatorSupervisor =>
        UserSession.CurrentUser?.CurrentAuth?.Roles?.Contains("ROLE_SUPERVISOR") == true;

    public long? CurrentOperatorId =>
        UserSession.CurrentUser.CurrentAuth?.OperatorId;

    public bool IsLookupTabActive
    {
        get => _isLookupTabActive;
        private set { _isLookupTabActive = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsManageTabActive)); }
    }
    
    private void AddVehicle()
    {
        var plate = NewVehiclePlateNumber?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(plate)) return;

        if (NewMemberVehicles.Any(v => string.Equals(v.PlateNumber, plate, StringComparison.OrdinalIgnoreCase)))
        {
            ToastService.ShowError("That plate number is already added.");
            return;
        }

        NewMemberVehicles.Add(new VehicleDto
        {
            PlateNumber = plate,
            VehicleType = string.IsNullOrWhiteSpace(NewVehicleType) ? "Car" : NewVehicleType.Trim()
        });

        NewVehiclePlateNumber = null;
        NewVehicleType = null;
    }

    private async Task RegisterMemberAsync(long? approvedBySupervisorId)
    {
        var request = new RegisterMemberRequest
        {
            Member = new MemberDto
            {
                Name = NewMemberName,
                PhoneNumber = NewMemberPhone,
                Email = NewMemberEmail,
                IsVip = NewMemberIsVip,
                MembershipType = NewMemberIsVip ? 2 : 1
            },
            Vehicles = NewMemberVehicles.ToList()
        };

        var response = await _memberService.RegisterMember(request, approvedBySupervisorId);
        if (response is BaseResponse<MemberDto> { Success: true })
        {
            ToastService.ShowSuccess("Member registered successfully.");
            NewMemberName = null;
            NewMemberPhone = null;
            NewMemberEmail = null;
            NewMemberIsVip = false;
            NewMemberVehicles.Clear();
            await LoadSummaryAsync();
        }
        else
        {
            var reason = (response as BaseErrorResponse<string>)?.Message ?? "Failed to register member.";
            ToastService.ShowError(reason);
        }
    }

    public bool IsManageTabActive => !IsLookupTabActive;

    public string? SearchKeyword
    {
        get => _searchKeyword;
        set { _searchKeyword = value; OnPropertyChanged(); }
    }

    public bool IsSearching
    {
        get => _isSearching;
        private set { _isSearching = value; OnPropertyChanged(); }
    }

    public MemberDto? SelectedMember
    {
        get => _selectedMember;
        private set
        {
            _selectedMember = value;
            OnPropertyChanged();
            SubscribeCommand.RaiseCanExecuteChanged();
            RenewCommand.RaiseCanExecuteChanged();
            TopUpCommand.RaiseCanExecuteChanged();
            CancelSubscriptionCommand.RaiseCanExecuteChanged();
        }
    }

    public MemberSubscriptionDto? SelectedSubscription
    {
        get => _selectedSubscription;
        private set
        {
            _selectedSubscription = value;
            OnPropertyChanged();
            RenewCommand.RaiseCanExecuteChanged();
            TopUpCommand.RaiseCanExecuteChanged();
            CancelSubscriptionCommand.RaiseCanExecuteChanged();
        }
    }

    public ObservableCollection<MemberBalanceTransactionDto> TransactionHistory
    {
        get => _transactionHistory;
        private set { _transactionHistory = value; OnPropertyChanged(); }
    }

    public long TotalMembers
    {
        get => _totalMembers;
        private set { _totalMembers = value; OnPropertyChanged(); }
    }

    public long RegularMembers
    {
        get => _regularMembers;
        private set { _regularMembers = value; OnPropertyChanged(); }
    }

    public long VipMembers
    {
        get => _vipMembers;
        private set { _vipMembers = value; OnPropertyChanged(); }
    }

    public long ExpiredMembers
    {
        get => _expiredMembers;
        private set { _expiredMembers = value; OnPropertyChanged(); }
    }

    public string? NewMemberName
    {
        get => _newMemberName;
        set { _newMemberName = value; OnPropertyChanged(); }
    }

    public string? NewMemberPhone
    {
        get => _newMemberPhone;
        set { _newMemberPhone = value; OnPropertyChanged(); }
    }

    public string? NewMemberEmail
    {
        get => _newMemberEmail;
        set { _newMemberEmail = value; OnPropertyChanged(); }
    }

    public bool NewMemberIsVip
    {
        get => _newMemberIsVip;
        set { _newMemberIsVip = value; OnPropertyChanged(); }
    }

    public MemberPlanDto? SelectedPlanForSubscribe
    {
        get => _selectedPlanForSubscribe;
        private set
        {
            _selectedPlanForSubscribe = value;
            OnPropertyChanged();
            SubscribeCommand.RaiseCanExecuteChanged();
        }
    }

    public LabelValueWrapper? SelectedPlanOption
    {
        get => _selectedPlanOption;
        set
        {
            _selectedPlanOption = value;
            OnPropertyChanged();
            SelectedPlanForSubscribe = value?.ReferenceObject as MemberPlanDto;
        }
    }

    public decimal? TopUpAmount
    {
        get => _topUpAmount;
        set
        {
            _topUpAmount = value;
            OnPropertyChanged();
            TopUpCommand.RaiseCanExecuteChanged();
        }
    }

    public string? TopUpRemark
    {
        get => _topUpRemark;
        set { _topUpRemark = value; OnPropertyChanged(); }
    }

    public bool IsSupervisorModalOpen
    {
        get => _isSupervisorModalOpen;
        private set { _isSupervisorModalOpen = value; OnPropertyChanged(); }
    }

    public string? SupervisorUsername
    {
        get => _supervisorUsername;
        set { _supervisorUsername = value; OnPropertyChanged(); }
    }

    public string? SupervisorPassword
    {
        get => _supervisorPassword;
        set { _supervisorPassword = value; OnPropertyChanged(); }
    }

    public string? NewVehiclePlateNumber
    {
        get => _newVehiclePlateNumber;
        set { _newVehiclePlateNumber = value; OnPropertyChanged(); AddVehicleCommand.RaiseCanExecuteChanged(); }
    }

    public string? NewVehicleType
    {
        get => _newVehicleType;
        set { _newVehicleType = value; OnPropertyChanged(); }
    }
    
    public RelayCommand SwitchTabCommand { get; }
    public RelayCommand SearchMembersCommand { get; }
    public RelayCommand SelectMemberCommand { get; }
    public RelayCommand RegisterMemberCommand { get; }
    public RelayCommand SubscribeCommand { get; }
    public RelayCommand RenewCommand { get; }
    public RelayCommand TopUpCommand { get; }
    public RelayCommand CancelSubscriptionCommand { get; }
    public RelayCommand ConfirmSupervisorApprovalCommand { get; }
    public RelayCommand CancelSupervisorApprovalCommand { get; }
    public RelayCommand AddVehicleCommand { get; }
    public RelayCommand RemoveVehicleCommand { get; }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(LoadSummaryAsync(), LoadActivePlansAsync());
    }

    private void SwitchTab(string? tab)
    {
        IsLookupTabActive = tab != "manage";
    }

    private async Task LoadSummaryAsync()
    {
        var response = await _memberService.GetSummary();
        if (response is BaseResponse<MembersSummaryDto> { Success: true, Data: not null } success)
        {
            TotalMembers = success.Data.TotalMembers;
            RegularMembers = success.Data.RegularMembers;
            VipMembers = success.Data.VipMembers;
            ExpiredMembers = success.Data.ExpiredMembers;
        }
    }

    private async Task LoadActivePlansAsync()
    {
        var response = await _memberService.GetActivePlans();
        if (response is BaseResponse<List<MemberPlanDto>> { Success: true, Data: not null } success)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ActivePlans.Clear();
                ActivePlanOptions.Clear();
                foreach (var plan in success.Data)
                {
                    ActivePlans.Add(plan);
                    ActivePlanOptions.Add(new LabelValueWrapper(plan.Name??"", plan.Id.ToString(), referenceObject: plan));
                }
            });
        }
    }

    private async Task SearchMembersAsync()
    {
        IsSearching = true;
        try
        {
            var response = await _memberService.SearchMembers(new MemberSearchRequest { Keyword = SearchKeyword});
            if (response is BaseResponse<SearchResultDto<MemberDto>> { Success: true, Data: not null } success)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SearchResults.Clear();
                    foreach (var member in success.Data.Results) SearchResults.Add(member);
                });
            }
            else if (response is BaseErrorResponse<string> error)
            {
                ToastService.ShowError(error.Message ?? "Member search failed.");
            }
        }
        finally
        {
            IsSearching = false;
        }
    }

    private async Task SelectMemberAsync(MemberDto? member)
    {
        if (member == null) return;

        SelectedMember = member;
        SelectedSubscription = null;
        TransactionHistory = [];

        var response = await _memberService.GetMemberSubscription(member.Id);
        if (response is BaseResponse<MemberSubscriptionDto> { Success: true, Data: not null } success)
        {
            SelectedSubscription = success.Data;

            SelectedPlanOption = ActivePlanOptions.Where(i => i.Value == success.Data.PlanId.ToString())
                .FirstOrDefault(LabelValueWrapper.Empty);

            var transactionsResponse = await _memberService.GetTransactionHistory(member.Id, success.Data.Id);
            if (transactionsResponse is BaseResponse<List<MemberBalanceTransactionDto>> { Success: true, Data: not null } transactionsSuccess)
            {
                TransactionHistory = new ObservableCollection<MemberBalanceTransactionDto>(transactionsSuccess.Data);
            }
        }
    }

    private async Task ExecuteSupervisorGatedAsync(Func<long?, Task> action)
    {
        if (IsCurrentOperatorSupervisor)
        {
            await action(CurrentOperatorId);
            return;
        }

        _pendingAction = action;
        IsSupervisorModalOpen = true;
    }

    private async Task ConfirmSupervisorApprovalAsync()
    {
        var owner = BaseWindow.MainWindowInstance;
        DialogService.ShowLoadingDialog(owner, "Verifying supervisor...");

        Response? response;
        try
        {
            response = await _memberService.ValidateSupervisor(new SupervisorApprovalRequest
            {
                Username = SupervisorUsername,
                Password = SupervisorPassword
            });
        }
        finally
        {
            DialogService.HideLoading(owner);
        }

        if (response is BaseResponse<SupervisorApprovalDto> { Success: true, Data: not null } success)
        {
            var pending = _pendingAction;
            CloseSupervisorModal();
            if (pending != null) await pending(success.Data.SupervisorId);
        }
        else
        {
            var reason = (response as BaseErrorResponse<string>)?.Message ?? "Invalid supervisor credentials.";
            ToastService.ShowError(reason);
        }
    }

    private void CloseSupervisorModal()
    {
        IsSupervisorModalOpen = false;
        SupervisorUsername = null;
        SupervisorPassword = null;
        _pendingAction = null;
    }

    

    private async Task SubscribeAsync(long? approvedBySupervisorId)
    {
        if (SelectedMember == null || SelectedPlanForSubscribe == null) return;

        var response = await _memberService.Subscribe(SelectedMember.Id, SelectedPlanForSubscribe.Id, approvedBySupervisorId);
        if (response is BaseResponse<MemberSubscriptionDto> { Success: true, Data: not null } success)
        {
            SelectedSubscription = success.Data;
            SelectedPlanOption = null;
            ToastService.ShowSuccess("Member subscribed successfully.");
            await LoadSummaryAsync();
        }
        else
        {
            var reason = (response as BaseErrorResponse<string>)?.Message ?? "Failed to subscribe member.";
            ToastService.ShowError(reason);
        }
    }

    private async Task RenewAsync(long? approvedBySupervisorId)
    {
        if (SelectedMember == null || SelectedSubscription == null) return;

        var response = await _memberService.Renew(SelectedMember.Id, SelectedSubscription.Id, approvedBySupervisorId);
        if (response is BaseResponse<MemberSubscriptionDto> { Success: true, Data: not null } success)
        {
            SelectedSubscription = success.Data;
            ToastService.ShowSuccess("Subscription renewed successfully.");
        }
        else
        {
            var reason = (response as BaseErrorResponse<string>)?.Message ?? "Failed to renew subscription.";
            ToastService.ShowError(reason);
        }
    }

    private async Task TopUpAsync(long? approvedBySupervisorId)
    {
        if (SelectedMember == null || SelectedSubscription == null || !TopUpAmount.HasValue) return;

        var response = await _memberService.TopUp(SelectedMember.Id, SelectedSubscription.Id,
            new TopUpBalanceRequest { Amount = TopUpAmount.Value, Remark = TopUpRemark }, approvedBySupervisorId);

        if (response is BaseResponse<MemberSubscriptionDto> { Success: true, Data: not null } success)
        {
            SelectedSubscription = success.Data;
            ToastService.ShowSuccess("Balance topped up successfully.");
            TopUpAmount = null;
            TopUpRemark = null;
        }
        else
        {
            var reason = (response as BaseErrorResponse<string>)?.Message ?? "Failed to top up balance.";
            ToastService.ShowError(reason);
        }
    }

    private async Task CancelSubscriptionAsync(long? approvedBySupervisorId)
    {
        if (SelectedMember == null || SelectedSubscription == null) return;

        var response = await _memberService.Cancel(SelectedMember.Id, SelectedSubscription.Id, approvedBySupervisorId);
        if (response is BaseResponse<MemberSubscriptionDto> { Success: true, Data: not null } success)
        {
            SelectedSubscription = success.Data;
            ToastService.ShowSuccess("Subscription cancelled successfully.");
        }
        else
        {
            var reason = (response as BaseErrorResponse<string>)?.Message ?? "Failed to cancel subscription.";
            ToastService.ShowError(reason);
        }
    }

    protected override void OnDispose()
    {
    }
}