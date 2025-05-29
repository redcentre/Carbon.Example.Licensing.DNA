# DNA Overview

DNA is a Windows&trade; desktop program that performs powerful data management of Carbon compatible licensing databases. DNA internally uses the following two licensing provider packages to connect to the databases.

1. **Example Licensing Provider**  
   A publicly available GitHub repository named [Carbon.Example.Licensing.Provider][git1] contains a Carbon compatible licensing provider that uses SQL Server as the backing storage. The readme in the repo describes the conventions of the Carbon licensing system and providers.
2. **Red Centre Licensing Provider**  
   A proprietary licensing provider that is only used by applications hosted in the Red Centre Software Azure subscription.

Both of the providers implement the `ILicensingProvider` interface, so DNA can load and use them interchangeably. DNA uses a dialog to prompt for options specific to each provider and select the active one.

The following tables describe the options for each provider.

**Example Licensing Provider**

| Name | Description |
| ---- | ---- |
| ADO.NET Connect | A connection string in the format expected by the .NET database driver. |
| Product Key | A key that must be provided by Red Centre Software to verify that the licensing provider is known to the distributor of Carbon licences. |


[git1]: https://github.com/redcentre/Carbon.Example.Licensing.Provider

**Red Centre Licensing Provider**

| Name | Description |
| ---- | ---- |
| API Key | A key that must be provided by Red Centre Software to allow Access to their proprietary licensing web service. |
| Service Base Address | The base Uri of the licensing web service. |
| Timeout | The timeout seconds of the web service. Default is 30. |

*To be continued…*

Last updated 29-May-2025