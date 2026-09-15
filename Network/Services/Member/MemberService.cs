using icpms_client.Network.DTO;
using icpms_client.Network.DTO.Member;
using icpms_client.Network.Request.Member;
using icpms_client.Network.Response;
using icpms_client.Network.Session;
using log4net;

namespace icpms_client.Network.Services.Member;

public class MemberService: ApiService
{
    private readonly ILog _log = LogManager.GetLogger(typeof(MemberService));

    public async Task<Response.Response?> SearchMembers(MemberSearchRequest request)
    {
        try
        {
            var accessToken = UserSession.CurrentUser.CurrentAuth?.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                _log.Error("Access token is null or empty");
                return new BaseErrorResponse<string>
                {
                    Success = false,
                    Message = "No active session/access token found.",
                    Data = "No active session/access token found."
                };
            }

            var url = $"members/search?page={request.PageNo}";
            if (!string.IsNullOrWhiteSpace(request.Name)) url += $"&name={Uri.EscapeDataString(request.Name)}";
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber)) url += $"&phoneNumber={Uri.EscapeDataString(request.PhoneNumber)}";
            if (!string.IsNullOrWhiteSpace(request.Keyword)) url += $"&keyword={Uri.EscapeDataString(request.Keyword)}";
            if (request.Status.HasValue) url += $"&status={request.Status.Value}";

            return await ApiClient.GetAsync<SearchResultDto<MemberDto>>(url, true, accessToken);
        }
        catch (Exception ex)
        {
            _log.Error($"SearchMembers Error: {ex.Message}");
            return new BaseErrorResponse<string>
            {
                Success = false,
                Message = ex.Message,
                Data = ex.Message
            };
        }
    }

    public async Task<Response.Response?> GetMemberById(long id)
    {
        try
        {
            var accessToken = UserSession.CurrentUser.CurrentAuth?.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                _log.Error("Access token is null or empty");
                return new BaseErrorResponse<string>
                {
                    Success = false,
                    Message = "No active session/access token found.",
                    Data = "No active session/access token found."
                };
            }

            return await ApiClient.GetAsync<MemberDto>($"members/{id}", true, accessToken);
        }
        catch (Exception ex)
        {
            _log.Error($"GetMemberById Error: {ex.Message}");
            return new BaseErrorResponse<string>
            {
                Success = false,
                Message = ex.Message,
                Data = ex.Message
            };
        }
    }

    public async Task<Response.Response?> GetMemberSubscription(long id)
    {
        try
        {
            var accessToken = UserSession.CurrentUser.CurrentAuth?.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                _log.Error("Access token is null or empty");
                return new BaseErrorResponse<string>
                {
                    Success = false,
                    Message = "No active session/access token found.",
                    Data = "No active session/access token found."
                };
            }

            return await ApiClient.GetAsync<MemberSubscriptionDto>($"members/{id}/subscription", true, accessToken);
        }
        catch (Exception ex)
        {
            _log.Error($"GetMemberSubscription Error: {ex.Message}");
            return new BaseErrorResponse<string>
            {
                Success = false,
                Message = ex.Message,
                Data = ex.Message
            };
        }
    }

    public async Task<Response.Response?> GetActivePlans()
    {
        try
        {
            var accessToken = UserSession.CurrentUser.CurrentAuth?.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                _log.Error("Access token is null or empty");
                return new BaseErrorResponse<string>
                {
                    Success = false,
                    Message = "No active session/access token found.",
                    Data = "No active session/access token found."
                };
            }

            return await ApiClient.GetAsync<List<MemberPlanDto>>("members/plans/active", true, accessToken);
        }
        catch (Exception ex)
        {
            _log.Error($"GetActivePlans Error: {ex.Message}");
            return new BaseErrorResponse<string>
            {
                Success = false,
                Message = ex.Message,
                Data = ex.Message
            };
        }
    }

    public async Task<Response.Response?> GetSummary()
    {
        try
        {
            var accessToken = UserSession.CurrentUser.CurrentAuth?.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                _log.Error("Access token is null or empty");
                return new BaseErrorResponse<string>
                {
                    Success = false,
                    Message = "No active session/access token found.",
                    Data = "No active session/access token found."
                };
            }

            return await ApiClient.GetAsync<MembersSummaryDto>("members/summary", true, accessToken);
        }
        catch (Exception ex)
        {
            _log.Error($"GetSummary Error: {ex.Message}");
            return new BaseErrorResponse<string>
            {
                Success = false,
                Message = ex.Message,
                Data = ex.Message
            };
        }
    }

    public async Task<Response.Response?> RegisterMember(RegisterMemberRequest request, long? approvedBySupervisorId)
    {
        try
        {
            var accessToken = UserSession.CurrentUser.CurrentAuth?.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                _log.Error("Access token is null or empty");
                return new BaseErrorResponse<string>
                {
                    Success = false,
                    Message = "No active session/access token found.",
                    Data = "No active session/access token found."
                };
            }

            var url = "members";
            if (approvedBySupervisorId.HasValue) url += $"?approvedBySupervisorId={approvedBySupervisorId.Value}";

            return await ApiClient.PostAsync<MemberDto>(url, request, true, accessToken);
        }
        catch (Exception ex)
        {
            _log.Error($"RegisterMember Error: {ex.Message}");
            return new BaseErrorResponse<string>
            {
                Success = false,
                Message = ex.Message,
                Data = ex.Message
            };
        }
    }

    public async Task<Response.Response?> Subscribe(long memberId, long planId, long? approvedBySupervisorId)
    {
        try
        {
            var accessToken = UserSession.CurrentUser.CurrentAuth?.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                _log.Error("Access token is null or empty");
                return new BaseErrorResponse<string>
                {
                    Success = false,
                    Message = "No active session/access token found.",
                    Data = "No active session/access token found."
                };
            }

            var url = $"members/{memberId}/subscribe?planId={planId}";
            if (approvedBySupervisorId.HasValue) url += $"&approvedBySupervisorId={approvedBySupervisorId.Value}";

            return await ApiClient.PostAsync<MemberSubscriptionDto>(url, new { }, true, accessToken);
        }
        catch (Exception ex)
        {
            _log.Error($"Subscribe Error: {ex.Message}");
            return new BaseErrorResponse<string>
            {
                Success = false,
                Message = ex.Message,
                Data = ex.Message
            };
        }
    }

    public async Task<Response.Response?> Renew(long memberId, long subscriptionId, long? approvedBySupervisorId)
    {
        try
        {
            var accessToken = UserSession.CurrentUser.CurrentAuth?.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                _log.Error("Access token is null or empty");
                return new BaseErrorResponse<string>
                {
                    Success = false,
                    Message = "No active session/access token found.",
                    Data = "No active session/access token found."
                };
            }

            var url = $"members/{memberId}/subscriptions/{subscriptionId}/renew";
            if (approvedBySupervisorId.HasValue) url += $"?approvedBySupervisorId={approvedBySupervisorId.Value}";

            return await ApiClient.PostAsync<MemberSubscriptionDto>(url, new { }, true, accessToken);
        }
        catch (Exception ex)
        {
            _log.Error($"Renew Error: {ex.Message}");
            return new BaseErrorResponse<string>
            {
                Success = false,
                Message = ex.Message,
                Data = ex.Message
            };
        }
    }

    public async Task<Response.Response?> TopUp(long memberId, long subscriptionId, TopUpBalanceRequest request, long? approvedBySupervisorId)
    {
        try
        {
            var accessToken = UserSession.CurrentUser.CurrentAuth?.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                _log.Error("Access token is null or empty");
                return new BaseErrorResponse<string>
                {
                    Success = false,
                    Message = "No active session/access token found.",
                    Data = "No active session/access token found."
                };
            }

            var url = $"members/{memberId}/subscriptions/{subscriptionId}/topup";
            if (approvedBySupervisorId.HasValue) url += $"?approvedBySupervisorId={approvedBySupervisorId.Value}";

            return await ApiClient.PostAsync<MemberSubscriptionDto>(url, request, true, accessToken);
        }
        catch (Exception ex)
        {
            _log.Error($"TopUp Error: {ex.Message}");
            return new BaseErrorResponse<string>
            {
                Success = false,
                Message = ex.Message,
                Data = ex.Message
            };
        }
    }

    public async Task<Response.Response?> Cancel(long memberId, long subscriptionId, long? approvedBySupervisorId)
    {
        try
        {
            var accessToken = UserSession.CurrentUser.CurrentAuth?.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                _log.Error("Access token is null or empty");
                return new BaseErrorResponse<string>
                {
                    Success = false,
                    Message = "No active session/access token found.",
                    Data = "No active session/access token found."
                };
            }

            var url = $"members/{memberId}/subscriptions/{subscriptionId}/cancel";
            if (approvedBySupervisorId.HasValue) url += $"?approvedBySupervisorId={approvedBySupervisorId.Value}";

            return await ApiClient.PostAsync<MemberSubscriptionDto>(url, new { }, true, accessToken);
        }
        catch (Exception ex)
        {
            _log.Error($"Cancel Error: {ex.Message}");
            return new BaseErrorResponse<string>
            {
                Success = false,
                Message = ex.Message,
                Data = ex.Message
            };
        }
    }

    public async Task<Response.Response?> ValidateSupervisor(SupervisorApprovalRequest request)
    {
        try
        {
            var accessToken = UserSession.CurrentUser.CurrentAuth?.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                _log.Error("Access token is null or empty");
                return new BaseErrorResponse<string>
                {
                    Success = false,
                    Message = "No active session/access token found.",
                    Data = "No active session/access token found."
                };
            }

            return await ApiClient.PostAsync<SupervisorApprovalDto>("auth/validate-supervisor", request, true, accessToken);
        }
        catch (Exception ex)
        {
            _log.Error($"ValidateSupervisor Error: {ex.Message}");
            return new BaseErrorResponse<string>
            {
                Success = false,
                Message = ex.Message,
                Data = ex.Message
            };
        }
    }
    
    public async Task<Response.Response?> GetTransactionHistory(long memberId, long subscriptionId)
    {
        var accessToken = UserSession.CurrentUser?.CurrentAuth?.AccessToken;
        if (string.IsNullOrEmpty(accessToken))
        {
            return new BaseErrorResponse<string> { Message = "No active session/access token found." };
        }

        try
        {
            return await ApiClient.GetAsync<List<MemberBalanceTransactionDto>>(
                $"members/{memberId}/subscriptions/{subscriptionId}/transactions", true, accessToken);
        }
        catch (Exception ex)
        {
            _log.Error($"GetTransactionHistory Error: {ex.Message}");
            return new BaseErrorResponse<string> { Message = ex.Message };
        }
    }
}