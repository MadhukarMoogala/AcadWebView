# Integrating WebView2 with AutoCAD: A Rich UI for Data Visualization

AutoCAD's native UI is powerful, but sometimes we need **modern, interactive, and web-based interfaces** to enhance productivity.

This sample demonstrates how to integrate **WebView2** inside AutoCAD using .NET, enabling developers to display rich HTML dashboards and visualize data extracted from AutoCAD drawings.

## **Data Extraction Overview**

The extracted data is linked to an AutoCAD drawing. In this example, we use the **AutoCAD 2025 Sample Drawing**:  
📂 `\AutoCAD 2025\Sample\Mechanical Sample\Data Extraction and Multileaders Sample.dwg`  
📄 This sample contains the `Grill Schedule.dxe` file, which stores extracted data in a **binary format**.

Since `.dxe` files are binary, we use a new tool shipped with AutoCAD 2025 called **`BFMigrator.exe`** to convert them into a **human-readable JSON file (`.dxex`)**.

The converted `.dxex` file is then parsed and sent to an embedded WebView2 instance, where a **dynamic HTML dashboard** is generated.



### DXE to JSON Conversion

Below is the C# code snippet used to convert the binary `.dxe` file into a JSON `.dxex` file using `BFMigrator.exe`:

```csharp
ProcessStartInfo psi = new ProcessStartInfo
{
    FileName = BFMigrator,
    Arguments = $"\"{dxeFile}\" \"{dxexFile}\"",
    UseShellExecute = false,
    CreateNoWindow = true
};

Process process = new Process { StartInfo = psi };
process.Start();
process.WaitForExit(); // Ensure migration is complete
```

## Build Instructions

```bash
git clone https://github.com/MadhukarMoogala/AcadWebView.git
cd AcadWebView
set ARXSDK_PATH=D:\YourPathToSDK\ARX2025
dotnet restore
dotnet build

```

## Run Instructions

- **Launch AutoCAD 2025**
- **Open the sample drawing**
  - 📂 `\AutoCAD 2025\Sample\Mechanical Sample\Data Extraction and Multileaders Sample.dwg`
- **Load the compiled .NET assembly**
  - Run the `NETLOAD` command and select the compiled DLL.
- **Run the WebView command**
  - Execute `WEBVIEWPAL` in AutoCAD to launch the WebView2 dashboard.

## DEMO



Once the `WEBVIEWPAL` command runs successfully:  

- The **WebView2 UI** will launch inside an **AutoCAD Palette**.  

- The extracted **DXE data** (now JSON) will be parsed and displayed.  

-  A **dynamic HTML dashboard** will visualize the extracted information.

### Written By

APS Developer Advocate

Madhukar Moogala,  [LICENSE](https://github.com/MadhukarMoogala/AcadWebView/blob/main/LICENSE)
