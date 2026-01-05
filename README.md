![XAML Studio Header Image](docs/images/header.png)

<h1 align="center">
    XAML Studio v2
</h1>

> [!IMPORTANT]  
> The `main` branch of XAML Studio is of the v1.1 Microsoft Store release. The most recent developments are currently in the `dev` branch for v2. Outside of PRs to docs, any contributions should pertain to the `dev` branch.

Created for WinUI XAML developers, XAML Studio helps you rapidly prototype ideas before integrating them into your app within Visual Studio. It provides:

- **Live Edit and Interaction** ✏️
- **Binding Debugger** 🐞
- **Data Context Editor** 🗒️
- Auto-save and restore Documents 💾
- IntelliSense ✨
- Documentation Toolbox 📚
- Alignment Guides 📏
- Namespace Helpers 🌎 

<p align="center">
<img src="docs/images/hero-image-1.png" alt="XAML Studio" width="600"/>
</p>
<p align="center">
<a href="http://aka.ms/GetXAMLStudio">
	<img alt="Store badge" src="docs/images/store-badge.png" width="200"/>
</a><br/>
(Store Version still v1.1)
</p>

## New Features in V2

- **New Fluent UI Design**
- **Folder Support**: Image Loading, Design Data Loading, and More!
- **Live Property Panel**: Edit, Inspect, and Experiment
- More Quick Access Preview Options like Refresh, Alignment Grid, Clipping, and Theme Toggle
- Breadcrumb XAML Navigation Bar
- Right-Click Menu to Duplicate the Open Tab
- Updated Libraries & Bug Fixes

Learn more about these features and provide feedback on them in the [Feature Feedback](https://github.com/dotnet/XAMLStudio/discussions/categories/feature-feedback) discussion category.

## 🚀 Getting started

Download [XAML Studio from the Microsoft Store](http://aka.ms/GetXAMLStudio) or follow these steps to install it manually:

### 1. Set up the environment

> [!IMPORTANT]
> XAML Studio requires [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or later for building and Windows 10 or newer to run.
If you're new to building apps with WinUI and the Windows App SDK, follow the [installation instructions](https://learn.microsoft.com/windows/apps/get-started/start-here). (Note: XAML Studio is still a UWP app, so you need the Universal Windows Platform tools as well.)

**Required [Visual Studio components](https://learn.microsoft.com/windows/apps/get-started/start-here#22-required-workloads-and-components):**

- Windows application development workload
- Universal Windows Platform tools
- Windows 17763 SDK

### 2. Clone the repository

```shell
git clone https://github.com/dotnet/XAMLStudio.git
```

### 3. Checkout the `dev` branch

```shell
git checkout dev
```

Current work is being done on the `dev` branch, not `main`.

### 3. Open XAMLStudio.slnx with Visual Studio!

Ensure that the `XAMLStudio` project is set as the startup project in Visual Studio.

Press <kbd>F5</kbd> to run XAML Studio!

> [!NOTE]
> On ARM64-based PCs, make sure to build and run the solution as `ARM64` (and not as `x64`). However, there's currently an issue: see #14.

> [!NOTE]
> Having issues installing the app on your machine? Let us know by <a href="https://github.com/dotnet/XAMLStudio/discussions/categories/q-a">opening a discussion</a> and we'll do our best to help!

## Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

See [CONTRIBUTING.md](CONTRIBUTING.md) for more details.

## 3rd Party OSS Usage

See the [NOTICE.json](NOTICE.json) for third-party attributions (as displayed in app).

## Telemetry

You can turn off telemetry in the Settings page under **Feedback**. Turn the **Diagnostic Data** setting to "No".

No telemetry key is used/sent for builds from source. _App Center_ has been deprecated, so no telemetry is used currently, see Issue #9. Telemetry included basic information about which features were utilized, performance of the application based on document sizes, as well as crash information to help further product development. The following notice is left in case a new solution for diagnostics is used for v2.

The software may collect information about you and your use of the software and send it to Microsoft. Microsoft may use this information to provide services and improve our products and services. You may turn off the telemetry as described in the repository. There are also some features in the software that may enable you and Microsoft to collect data from users of your applications. If you use these features, you must comply with applicable law, including providing appropriate notices to users of your applications together with a copy of Microsoft’s privacy statement. Our privacy statement is located at https://aka.ms/privacy. You can learn more about data collection and use in the help documentation and our privacy statement. Your use of the software operates as your consent to these practices.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft’s Trademark & Brand Guidelines](https://www.microsoft.com/legal/intellectualproperty/trademarks/usage/general). Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship. Any use of third-party trademarks or logos are subject to those third-party’s policies.

## LICENSE

MIT
