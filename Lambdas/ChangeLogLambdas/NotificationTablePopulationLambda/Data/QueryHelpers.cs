using NotificationTablePopulationLambda.Entities;

namespace NotificationTablePopulationLambda.Data;

public static class QueryHelpers
{
    public static UserRole[] UserRolesToNotify => new UserRole[]
    {
        new(Guid: Guid.Parse("125938C0-5C75-4D5E-8B9F-1B4B2F8EB8F2"), Name: "SiteManager"),
        new(Guid: Guid.Parse("24C8218C-3698-47A9-8A7A-94348D59F8A1"), Name: "SiteOperator"),
        new(Guid: Guid.Parse("B54AA493-D3B6-42B1-A8E5-34FF944BBB06"), Name: "Reader"),
        new(Guid: Guid.Parse("A5E9CD4B-C139-492B-BA8E-AC03648E8DCD"), Name: "SiteAdministrator"),
        new(Guid: Guid.Parse("0CA4358E-801A-4CA5-AF8C-89DEC2F8671C"), Name: "Writer"),
        new(Guid: Guid.Parse("D3B2BA56-9C12-4C1A-A20D-946EC2E9D281"), Name: "SuperAdmin"),
        new(Guid: Guid.Parse("CAEE4291-20D1-4C5D-A594-A95ABB4F665B"), Name: "CompanyAdmin"),
        new(Guid: Guid.Parse("588CCBEE-E1C1-4A2C-99F3-0FEBF2C5F320"), Name: "Auditor"),
    };
}
