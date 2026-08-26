namespace icpms_client.Network.Response;

public class BaseErrorResponse<T>: Response
{
    public T? Data { get; set; }
}