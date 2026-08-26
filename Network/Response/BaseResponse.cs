namespace icpms_client.Network.Response;

public class BaseResponse<T>: Response
{
    public T? Data { get; set; }
}