# Support helper

Support helper is a rich issue submission form for the Kentico administration interface, meant to replace the existing **Submit a question or bug** link in the help toolbar. It gathers user details, uploaded files, and useful information (metrics) using the Kentico API. It also provides a way to register custom metrics. The data is sent using POST to a destination server.

## Package contents

Each extension package contains four files:

- _SupportHelper.1.XX.0.nupkg_ – NuGet package containing the Support helper module and files.
- _SupportHelper\_SampleMetric.cs_ – Sample custom metric code.
- _SupportHelper\_SampleDestination.cs_ – Sample destination server code.
- _SupportHelper\_MetricCodeNames.txt_ – A list of available metric code names, in the same order as in the Support helper form.

## Installation

The following steps describe how to install Support helper on the source server, where you want to install the form. Step 6 occurs on the destination server:

1. Open your Kentico solution in Visual Studio (using the **WebSite.sln** or **WebApp.sln** file).
2. Use the NuGet Package Manager to install the appropriate NuGet package:
   * **Kentico 10** – _SupportHelper.1.10.0.nupkg_
   * **Kentico 11** – _SupportHelper.1.11.0.nupkg_
3. Add the following key into the _AppSettings_ section of your project's _web.config_ file:
   ```xml
   <add key="SHSubmitEndpoint" value="~/supportsubmission/submit"/>
   ```
   * Replace _~/supportsubmission/submit_ with the full URL to the API controller on the destination server as defined by the destination code. For example, if the destination server has domain _domain.com_ and you use the sample destination code without any changes, the URL would be _domain.com/supportsubmission/submit_.
4. (Optional) Configure additional settings as described in the [Configuring optional settings](#configuring-optional-settings) section.
5. If your project is installed in the web application format, rebuild the solution.
6. Configure the destination server as described in the [Configuring the destination server](#configuring-the-destination-server) section.

## Configuring optional settings

Support helper includes several optional settings that can change how it is used and what metrics it gathers.

- **Help toolbar button** – By default, Support helper installs an admin application called **Submit a question or bug**. The following steps describe how to insert a button into the blue help toolbar that opens Support helper as a modal window instead:
  1. Make sure that Support helper is installed according to the [Installation](#installation) section.
  2. In your Kentico solution, open the _~\CMSAdminControls\UI\ContextHelp.ascx_ file.
  3. Add to the bottom of the file the following line:
     ```asp 
     <%=SupportHelper.SupportHelper.ContextHelpScript%>
     ```
  4. The help toolbar should now have a button that looks like this:
 ![Help toolbar](https://user-images.githubusercontent.com/34716163/49970162-75699b80-fef8-11e8-8952-b0f3150ed9d0.png)
- **Disable application** – If you want to enable the help toolbar button then having the application visible is no longer necessary. To disable the application, add the following key into the _AppSettings_ section of your project's _web.config_ file:
  ```xml
  <add key="SHEnableApp" value="false"/>
  ```
- **Resource strings** – Support helper uses custom resource strings for the text in its files. The following steps describe how to change the values of the resource strings:
  1. Open the _~\CMSResources\SupportHelper\SupportHelper.resx_ file.
  2. Find the key of a text string. For example, the submit button with the text _Submit request_ has a key of **support.submit**.
  3. In the Kentico admin, follow these steps from the documentation: [Adding your own strings](https://docs.kentico.com/k12/multilingual-websites/setting-up-a-multilingual-user-interface/working-with-resource-strings#Workingwithresourcestrings-Addingyourownstrings).

- **Custom metrics** – Support helper includes a way to add custom metrics to the submission form. The following steps describe how to add a custom metric:
  1. Open your Kentico solution in Visual Studio.
  2. Create a new _Class Library_ project in the Kentico solution.
  3. Add references to the required Kentico and Support helper libraries (DLLs) for the module project:
     - Right-click the project and select  **Add > Reference**.
     - Select the  **Browse**  tab of the  **Reference manager**  dialog, click  **Browse**  and navigate to the _Bin_ folder of your Kentico web project.
     - Add references to the following libraries (and any others that you use in the module's code):
       - **CMS.Core.dll**
       - **SupportHelper.dll**
  4. Reference the custom module project from the Kentico web project _(CMSApp_ or _CMS)_.
  5. Edit the custom metric project's **AssemblyInfo.cs** file (in the _Properties_ folder).
     - Add the **AssemblyDiscoverable** assembly attribute:
       ```csharp
       using CMS;
       
       [assembly:AssemblyDiscoverable]
       ```
  6. Place the _SupportHelper\_SampleMetric.cs_ file in the custom metric project.
  7. Use the Kentico API to gather the metric data that you want. By default, the file returns a single field with the string _sample data_. The file also contains templates for returning a list of fields or a list of key-value pairs. For example, a list of fields is used by the **Discovered assemblies** metric and a list of key-value pairs is used by the **Azure settings** metric.
  8. **Build** the custom module project.
  9. In the Kentico admin, open **Settings > Integration > Support helper**.
  10. Click **Add new custom metric** and fill out the form. The metric that you created should be in the **Data class** selector. The **Category** field selects the metric category that you want to include the metric under, as shown in the submission form or listed in the submission data.
  11. The metric should now appear in the submission form after clicking **Advanced (metrics)**.
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

1. Open your Kentico solution in Visual Studio (using the **WebSite.sln** or **WebApp.sln** file).
2. Use the NuGet Package Manager to install the appropriate NuGet package:
   * **Kentico 10** – _SupportHelper.1.10.0.nupkg_
   * **Kentico 11** – _SupportHelper.1.11.0.nupkg_
3. Create a new _Class Library_ project in the Kentico solution.
4. Add references to the required Kentico and Support helper libraries (DLLs) for the module project:
   - Right-click the project and select **Add > Reference**.
   - Select the **Browse** tab of the **Reference manager** dialog, click **Browse** and navigate to the _Bin_ folder of your Kentico web project.
   - Add references to the following libraries (and any others that you use in the module's code):
     - **CMS.Core.dll**
     - **CMS.Base.dll**
     - **CMS.DataEngine.dll**
     - **CMS.EmailEngine.dll**
     - **SupportHelper.dll**
     - **Newtonsoft.Json.dll**
       - **Kentico 10** – this assembly is in _~\CMSDependencies\Newtonsoft.Json.6.0.0.0_
     - **System.Net.Http.Formatting.dll**
       - **Kentico 10** - this assembly is in _~\CMSDependencies\System.Net.Http.Formatting.5.2.2.0_
     - **System.Web.Http.dll** 
       - **Kentico 10** - this assembly is in _~\CMSDependencies\System.Web.Http.5.2.2.0_
     - **System.Web.Http.WebHost.dll**
       - **Kentico 10** - this assembly is in _~\CMSDependencies\System.Web.Http.WebHost.5.2.2.0_
   - Alternatively, you can install the **Newtonsoft.Json** and **Microsoft.AspNet.WebApi** packages into the project. If you do, make sure their versions match the ones used by the main Kentico application.
5. Reference the custom module project from the Kentico web project (_CMSApp_ or _CMS_).
6. Edit the module project's **AssemblyInfo.cs** file (in the _Properties_ folder).
   - Add the **AssemblyDiscoverable** assembly attribute:
     ```csharp
     using CMS;
     
     [assembly:AssemblyDiscoverable]
     ```
7. Place the _SupportHelper\_SampleDestination.cs_ file in the custom metric project.
8. Change the **SUPPORT\_EMAIL** constant to the email that you want to use.
9. (Optional) Make any other changes to the code, such as the HTTP route in the method **OnInit** of the class **SupportHelperDestinationModule**, the template of the email, any metrics or submission data that are used in the body of the email (by default, the version, category, and email are used), or add any processing to the attachments.
10. **Build** the custom module project.
11. The server should now process Support helper submissions.
