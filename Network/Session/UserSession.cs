using icpms_client.Network.DTO.Shift;

namespace icpms_client.Network.Session;

public class UserSession
{
    public static UserDataObject CurrentUser
    {
        get;
        set;
    } = new UserDataObject();

    public static ShiftDto CurrentShift
    {
        get;
        set;
    } = new ShiftDto();
}