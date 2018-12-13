# Support helper

Support helper is a rich issue submission form for the Kentico administration interface, meant to replace the existing **Submit a question or bug** link in the help toolbar. It gathers user details, uploaded files, and useful information (metrics) using the Kentico API. It also provides a way to register custom metrics. The data is sent using POST to a destination server.

## Package contents

Each extension package contains four files:

- _SupportHelper.1.XX.0.nupkg_ – NuGet package containing the Support helper module and files.
- _SupportHelper\_SampleMetric.cs_ – Sample custom metric code.
- _SupportHelper\_SampleDestination.cs_ – Sample destination server code.
- _SupportHelper\_MetricCodeNames.txt_ – A list of available metric code names, in the same order as in the Support helper form.

## Installation

The following steps describe how to install Support helper on the source server, where you want to install the form. Step 5 occurs on the destination server:

1. Open your Kentico solution in Visual Studio (using the **WebSite.sln** or **WebApp.sln** file).
2. Use the NuGet Package Manager to install the appropriate NuGet package:
  * **Kentico 10** – _SupportHelper.1.10.0.nupkg_
  * **Kentico 11** – _SupportHelper.1.11.0.nupkg_
3. Add the following key into the _AppSettings_ section of your project's _web.config_ file:
  ```xml
  <add key="SHSubmitEndpoint" value="~/supportsubmission/submit"/>
  ```
  * Replace _~/supportsubmission/submit_ with the full URL to the API controller on the destination server as defined by the destination code. For example, if the destination server has domain _domain.com_ and you use the sample destination code without any changes, the URL would be _domain.com/supportsubmission/submit_.
4. (Optional) Configure additional settings as described in the **Configuring optional settings** section.
5. If your project is installed in the web application format, rebuild the solution.
6. Configure the destination server as described in the **Configuring the destination server** section.

## Configuring optional settings

Support helper includes several optional settings that can change how it is used and what metrics it gathers.

- **Help toolbar button** – By default, Support helper installs an admin application called **Submit a question or bug**. The following steps describe how to insert a button into the blue help toolbar that opens Support helper as a modal window instead:
  1. Make sure that Support helper is installed according to the **Installation** section.
  2. In your Kentico solution, open the _~\CMSAdminControls\UI\ContextHelp.ascx_ file.
  3. Add to the bottom of the file the following line:
    ```asp 
    <%=SupportHelper.SupportHelper.ContextHelpScript%>
    ```
  4. The help toolbar should now have a button that looks like this:
 ![](data:image/*;base64,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)
- **Disable application** – If you want to enable the help toolbar button then having the application visible is no longer necessary. To disable the application, add the following key into the _AppSettings_ section of your project's _web.config_ file:
  ```xml
  <add key="SHEnableApp" value="false"/>
  ```
- **Resource strings** – Support helper uses custom resource strings for the text in its files. The following steps describe how to change the values of the resource strings:
  1. Open the _~\CMSResources\SupportHelper\SupportHelper.resx_ file.
  2. Find the key of a text string. For example, the submit button with the text _Submit request_ has a key of **support.submit**.
  3. In the Kentico admin, follow these steps from the documentation: [Adding your own strings](https://docs.kentico.com/k11/multilingual-websites/setting-up-a-multilingual-user-interface/working-with-resource-strings#Workingwithresourcestrings-Addingyourownstrings).

- **Custom metrics** – Support helper includes a way to add custom metrics to the submission form. The following steps describe how to add a custom metric:
  1. Open your Kentico solution in Visual Studio.
  2. Create a new _Class Library_ project in the Kentico solution.
  3. Add references to the required Kentico and Support helper libraries (DLLs) for the module project:
    - Right-click the project and select  **Add > Reference**.
    - Select the  **Browse**  tab of the  **Reference manager**  dialog, click  **Browse**  and navigate to the  **Bin** _ _folder of your Kentico web project.
    - Add references to the following libraries (and any others that you use in the module's code):
      - **CMS.Core.dll**
      - **SupportHelper.dll**
  4. Reference the custom module project from the Kentico web project _(CMSApp_ or _CMS)_.
  5. Edit the custom metric project's  **AssemblyInfo.cs**  file (in the _Properties_ folder).
  6. Add the  **AssemblyDiscoverable**  assembly attribute:
    ```csharp
    using CMS;  
    
    [assembly:AssemblyDiscoverable]
    ```
  7. Place the _SupportHelper\_SampleMetric.cs_ file in the custom metric project.
  8. Use the Kentico API to gather the metric data that you want. By default, the file returns a single field with the string _sample data_. The file also contains templates for returning a list of fields or a list of key-value pairs. For example, a list of fields is used by the **Discovered assemblies** metric and a list of key-value pairs is used by the **Azure settings** metric.
  9. **Build**  the custom module project.
  10. In the Kentico admin, open **Settings > Integration > Support helper**.
  11. Click **Add new custom metric** and fill out the form. The metric that you created should be in the **Data class** selector. The **Category** field selects the metric category that you want to include the metric under, as shown in the submission form or listed in the submission data.
  12. The metric should now appear in the submission form after clicking **Advanced (metrics)**.
- **Disabled metrics** – By default, all the metrics in Support helper are selected. To disable one or more metrics, add the following key into the _AppSettings_ section of your project's _web.config_ file:
    ```xml
    <add key="SHDisabledMetrics" value="metric1;metric2"/>
    ```
* Replace _metric1;metric2_ with the metric code names separated by semicolons (;) that you want to disable. You can find the default metric code names in the _SupportHelper\_MetricCodeNames.txt_ file.
- **Log submission** – Support helper allows the submission data (except for attachments) to be logged to the **Event log** as well as sent to the destination server. To log the submission data, add the following key into the _AppSettings_ section of your project's _web.config_ file:
    ```xml
    <add key="SHLogSubmission" value="true"/>
    ```
## Configuring the destination server

The destination server needs to have a Web API controller that receives the POST request from the form on the source server. The following steps describe how to configure the destination server to process Support helper submissions:

1. Open your Kentico solution in Visual Studio.
2. Create a new _Class Library_ project in the Kentico solution.
3. Add references to the required Kentico and Support helper libraries (DLLs) for the module project:
  - Right-click the project and select  **Add > Reference**.
  - Select the  **Browse**  tab of the  **Reference manager**  dialog, click  **Browse**  and navigate to the  **Bin** _ _folder of your Kentico web project.
  - Add references to the following libraries (and any others that you use in the module's code):
    - **CMS.Core.dll**
    - **CMS.Base.dll**
    - **CMS.DataEngine.dll**
    - **CMS.EmailEngine.dll**
    - **SupportHelper.dll**
    - **Newtonsoft.Json.dll** (in Kentico 10, this assembly is in _~\CMSDependencies\Newtonsoft.Json.6.0.0.0_)
    - **System.Net.Http.Formatting.dll** (in Kentico 10, this assembly is in _~\CMSDependencies\System.Net.Http.Formatting.5.2.2.0_)
    - **System.Web.Http.dll** (in Kentico 10, this assembly is in _~\CMSDependencies\System.Web.Http.5.2.2.0_)
    - **System.Web.Http.WebHost.dll** (in Kentico 10, this assembly is in _~\CMSDependencies\System.Web.Http.WebHost.5.2.2.0_)
  - Alternatively, you can install the **Newtonsoft.Json**  and **Microsoft.AspNet.WebApi**  packages into the project. If you do, make sure theirs versions match the ones used by the main Kentico application.
4. Reference the custom module project from the Kentico web project _(CMSApp_ or _CMS)_.
5. Edit the custom metric project's  **AssemblyInfo.cs**  file (in the _Properties_ folder).
6. Add the  **AssemblyDiscoverable**  assembly attribute:
    ```csharp
    using CMS;  
    
    [assembly:AssemblyDiscoverable]
    ```
7. Place the _SupportHelper\_SampleDestination.cs_ file in the custom metric project.
8. Change the **SUPPORT\_EMAIL** constant to the email that you want to use.
9. (Optional) Make any other changes to the code, such as the HTTP route in the **OnInit** method of the **SupportHelperDestinationModule** class, the template of the email, any metrics or submission data that are used in the body of the email (by default, the version, category, and email are used), or add any processing to the attachments.
10. **Build**  the custom module project.
11. The server should now process Support helper submissions.
