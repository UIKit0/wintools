using System;
using System.Diagnostics;
using System.Windows.Forms;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;

namespace Mfconsulting.Vsprj2make
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public class Connect : IDTExtensibility2, IDTCommandTarget
	{
        private DTE2 _applicationObject;
        private AddIn _addInInstance;
        private OutputWindowPane outputWindowPane;
        private CommandBarComboBox versionComboBox = null;
        private CommandBarButton testInMonoCommandBarButton = null;

        public string ProgID
        {
            get { return _addInInstance.ProgID; }
        }

        public _DTE DTE
        {
            get { return _applicationObject; }
        }
        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
		public Connect()
		{
		}

		/// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
            Mfconsulting.Vsprj2make.RegistryHelper regHlpr = new RegistryHelper();
            string[] monoVersions = regHlpr.GetMonoVersions();
            
            _applicationObject = (DTE2)application;
			_addInInstance = (AddIn)addInInst;
            int selectedIndexForComboBox = 1;

            OutputWindow outputWindow = (OutputWindow)_applicationObject.Windows.Item(Constants.vsWindowKindOutput).Object;
            outputWindowPane = outputWindow.OutputWindowPanes.Add("Monoaddin Messages");

			if(connectMode == ext_ConnectMode.ext_cm_AfterStartup)
			{
				object []contextGUIDS = new object[] { };
				Commands2 commands = (Commands2)_applicationObject.Commands;
				string toolsMenuName;

                _CommandBars commandBars = (Microsoft.VisualStudio.CommandBars._CommandBars)_applicationObject.CommandBars;
                CommandBar cmdBarMonoBarra;
                CommandBarPopup popMenu;	// Prj2Make popupmenu
                CommandBarPopup popMenu2;	// Explorer Current Project
                
                try
				{
					//If you would like to move the command to a different menu, change the word "Tools" to the 
					//  English version of the menu. This code will take the culture, append on the name of the menu
					//  then add the command to that menu. You can find a list of all the top-level menus in the file
					//  CommandBar.resx.
					ResourceManager resourceManager = new ResourceManager("monoaddin.CommandBar", Assembly.GetExecutingAssembly());
					CultureInfo cultureInfo = new System.Globalization.CultureInfo(_applicationObject.LocaleID);
					string resourceName = String.Concat(cultureInfo.TwoLetterISOLanguageName, "Tools");
					toolsMenuName = resourceManager.GetString(resourceName);
				}
				catch
				{
					//We tried to find a localized version of the word Tools, but one was not found.
					//  Default to the en-US word, which may work for the current culture.
					toolsMenuName = "Tools";
				}

				//Place the command on the tools menu.
				//Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
				Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar = (
                    (Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars)["MenuBar"];

				//Find the Tools command bar on the MenuBar command bar:
                CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
                CommandBarControls commandBarControls;
                commandBarControls = ((CommandBarPopup)toolsControl).Controls;

                                
                // Create Makefile
                Command command1 = null;
                // Generate MonoDevelop files
                Command command2 = null;
                // Generate a distribution unit
                Command command3 = null;
                // Import MonoDevelop Solutions
                Command command4 = null;
                // Run on Mono
                Command command5 = null;
                // vsprj2make Options
                Command command6 = null;
                // Explore current solution
                Command command7 = null;
                // Explore current Project
                Command command8 = null;


                // ------------- Add Pop-up menu ----------------
                popMenu2 = (CommandBarPopup)commandBarControls.Add(
                    MsoControlType.msoControlPopup,
                    System.Reflection.Missing.Value, // Object ID
                    System.Reflection.Missing.Value, // Object parameters
                    1, // Object before
                    true
                    );

                popMenu2.Caption = "&Windows Explore";

                // ------------- Add Pop-up menu ----------------
                popMenu = (CommandBarPopup)commandBarControls.Add(
                    MsoControlType.msoControlPopup,
                    System.Reflection.Missing.Value, // Object ID
                    System.Reflection.Missing.Value, // Object parameters
                    1, // Object before
                    true
                    );

                popMenu.Caption = "Prj&2Make";

                //This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
                //  just make sure you also update the QueryStatus/Exec method to include the new command names.
                try
                {
                    // Add the create makefile command -- command1
                    //command1 = commands.AddNamedCommand2(
                    //    _addInInstance, "CreateMake", "Create &Makefile",
                    //    "Generate Makefile", true, 59, ref contextGUIDS,
                    //    (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled,
                    //    (int)vsCommandStyle.vsCommandStylePictAndText,
                    //    vsCommandControlType.vsCommandControlTypeButton
                    //);

                    // Add the create makefile command -- command1
                    command1 = CreateNamedCommand(
                        _addInInstance,
                        commands,
                        "CreateMake",
                        "Create &Makefile",
                        "Generate Makefile",
                        ref contextGUIDS
                        );
                    
                    //Add a control for the command to the tools menu:
                    if ((command1 == null) && (popMenu != null))
                    {
                        command1 = GetExistingNamedCommand(commands, "Mfconsulting.Vsprj2make.Connect.CreateMake");
                        command1.AddControl(popMenu.CommandBar, 1);
                    }

                }
                catch (System.ArgumentException exc)
                {
                    //If we are here, then the exception is probably because a command with that name
                    //  already exists. If so there is no need to recreate the command and we can 
                    //  safely ignore the exception.
                    Trace.WriteLine(String.Format("Error during OnConnect of Add-in:\n {0}", exc.Message));
                }

                try
                {
                    // Add the GenMDFiles command -- command2
                    command2 = CreateNamedCommand(
                        _addInInstance,
                        commands,
                        "GenMDFiles",
                        "Create Mono&Develop Solution",
                        "Generate MonoDevelop Solution",
                        ref contextGUIDS
                        );

                    //Add a control for the command to the tools menu:
                    if ((command2 == null) && (popMenu != null))
                    {
                        command2 = GetExistingNamedCommand(commands, "Mfconsulting.Vsprj2make.Connect.GenMDFiles");
                        command2.AddControl(popMenu.CommandBar, 2);
                    }
                }
                catch (System.ArgumentException exc)
                {
                    Trace.WriteLine(String.Format("Error during OnConnect of Add-in:\n {0}", exc.Message));
                }

                try
                {
                    // Add the generate a dist unit command -- command3
                    command3 = CreateNamedCommand(
                        _addInInstance,
                        commands,
                        "GenDistUnit",
                        "Generate Distribution &Unit",
                        "Generates a distribution unit (zip file)",
                        ref contextGUIDS
                        );

                    //Add a control for the command to the tools menu:
                    if ((command3 == null) && (popMenu != null))
                    {
                        command3 = GetExistingNamedCommand(commands, "Mfconsulting.Vsprj2make.Connect.GenDistUnit");
                        command3.AddControl(popMenu.CommandBar, 3);
                    }
                }
                catch (System.ArgumentException exc)
                {
                    Trace.WriteLine(String.Format("Error during OnConnect of Add-in:\n {0}", exc.Message));
                }
                
                try
                {
                    // Add the ImportMD Solution command -- command4
                    command4 = CreateNamedCommand(
                        _addInInstance,
                        commands,
                        "PrjxToCsproj",
                        "&Import MonoDevelop Solution...",
                        "Imports a MonoDevelop Solution",
                        ref contextGUIDS
                        );

                    //Add a control for the command to the tools menu:
                    if ((command4 == null) && (popMenu != null))
                    {
                        command4 = GetExistingNamedCommand(commands, "Mfconsulting.Vsprj2make.Connect.PrjxToCsproj");
                        command4.AddControl(popMenu.CommandBar, 4);
                    }
                }
                catch (System.ArgumentException exc)
                {
                    Trace.WriteLine(String.Format("Error during OnConnect of Add-in:\n {0}", exc.Message));
                }

                try
                {
                    // Add the Run on Mono command -- command5
                    command5 = CreateNamedCommand(
                        _addInInstance,
                        commands,
                        "RunOnMono",
                        "&Run on Mono",
                        "Run solution on mono",
                        ref contextGUIDS
                        );

                    //Add a control for the command to the tools menu:
                    if ((command5 == null) && (popMenu != null))
                    {
                        command5 = GetExistingNamedCommand(commands, "Mfconsulting.Vsprj2make.Connect.RunOnMono");
                        command5.AddControl(popMenu.CommandBar, 5);
                    }
                }
                catch (System.ArgumentException exc)
                {
                    Trace.WriteLine(String.Format("Error during OnConnect of Add-in:\n {0}", exc.Message));
                }

                try
                {
                    // Add the Options command -- command6
                    command6 = CreateNamedCommand(
                        _addInInstance,
                        commands,
                        "Options",
                        "&Options...",
                        "Options for prj2make Add-in",
                        ref contextGUIDS
                        );

                    //Add a control for the command to the tools menu:
                    if ((command6 == null) && (popMenu != null))
                    {
                        command6 = GetExistingNamedCommand(commands, "Mfconsulting.Vsprj2make.Connect.Options");
                        command6.AddControl(popMenu.CommandBar, 6);
                    }
                }
                catch (System.ArgumentException exc)
                {
                    Trace.WriteLine(String.Format("Error during OnConnect of Add-in:\n {0}", exc.Message));
                }

                try
                {
                    // Add the ExploreCurrSln command -- command7
                    command7 = CreateNamedCommand(
                        _addInInstance,
                        commands,
                        "ExploreCurrSln",
                        "Current &Solution",
                        "Explore the current solution",
                        ref contextGUIDS
                        );

                    //Add a control for the command to the tools menu:
                    if ((command7 == null) && (popMenu != null))
                    {
                        command7 = GetExistingNamedCommand(commands, "Mfconsulting.Vsprj2make.Connect.ExploreCurrSln");
                        command7.AddControl(popMenu2.CommandBar, 1);
                    }
                }
                catch (System.ArgumentException exc)
                {
                    Trace.WriteLine(String.Format("Error during OnConnect of Add-in:\n {0}", exc.Message));
                }

                try
                {
                    // Add the ExploreCurrDoc command -- command8
                    command8 = CreateNamedCommand(
                        _addInInstance,
                        commands,
                        "ExploreCurrDoc",
                        "Current &Document",
                        "Explore the current Document",
                        ref contextGUIDS
                        );

                    //Add a control for the command to the tools menu:
                    if ((command8 == null) && (popMenu != null))
                    {
                        command8 = GetExistingNamedCommand(commands, "Mfconsulting.Vsprj2make.Connect.ExploreCurrDoc");
                        command8.AddControl(popMenu2.CommandBar, 2);
                    }
                }
                catch (System.ArgumentException exc)
                {
                    Trace.WriteLine(String.Format("Error during OnConnect of Add-in:\n {0}", exc.Message));
                }

                // Mono Toolbar
                CommandBar cmdBarBuild = (CommandBar)commandBars["Build"];

                try
                {
                    cmdBarMonoBarra = (CommandBar)commandBars["MonoBarra"];
                }
                catch (Exception)
                {
                    commands.AddCommandBar("MonoBarra",
                        vsCommandBarType.vsCommandBarTypeToolbar,
                        cmdBarBuild,
                        1
                        );

                    cmdBarMonoBarra = (CommandBar)commandBars["MonoBarra"];
                    cmdBarMonoBarra.Visible = true;
                }

                if (testInMonoCommandBarButton == null)
                {

                    // Create the Run on Mono Button
                    testInMonoCommandBarButton = (CommandBarButton)cmdBarMonoBarra.Controls.Add(
                        Microsoft.VisualStudio.CommandBars.MsoControlType.msoControlButton,
                        System.Reflection.Missing.Value,
                        System.Reflection.Missing.Value,
                        1,
                        false
                        );

                    testInMonoCommandBarButton.Caption = "Run on &Mono";
                    testInMonoCommandBarButton.DescriptionText = "Run solution with the mono runtime";
                    testInMonoCommandBarButton.TooltipText = "Run on mono";
                    testInMonoCommandBarButton.ShortcutText = "Run on &Mono";
                    testInMonoCommandBarButton.Style = MsoButtonStyle.msoButtonCaption;
                    testInMonoCommandBarButton.Click += new _CommandBarButtonEvents_ClickEventHandler(testInMonoCommandBarButton_Click);
                }

                if (versionComboBox == null)
                {

                    // Create the combobox
                    versionComboBox = (CommandBarComboBox)cmdBarMonoBarra.Controls.Add(
                        Microsoft.VisualStudio.CommandBars.MsoControlType.msoControlDropdown,
                        System.Reflection.Missing.Value,
                        System.Reflection.Missing.Value,
                        2,
                        false
                        );

                    for (int i = 0; i < monoVersions.Length; i++)
                    {
                        versionComboBox.AddItem(monoVersions[i], i + 1);
                        if (monoVersions[i].CompareTo(regHlpr.GetDefaultClr()) == 0)
                        {
                            selectedIndexForComboBox = i + 1;
                        }
                    }

                    versionComboBox.Change += new _CommandBarComboBoxEvents_ChangeEventHandler(versionComboBox_Change);

                    // Select the active index based on
                    // the current mono version
                    versionComboBox.ListIndex = selectedIndexForComboBox;
                }
            }
        }

		/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
		}

		/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref Array custom)
		{
		}

		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
		}
		
		/// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
		/// <param term='commandName'>The name of the command to determine state for.</param>
		/// <param term='neededText'>Text that is needed for the command.</param>
		/// <param term='status'>The state of the command in the user interface.</param>
		/// <param term='commandText'>Text requested by the neededText parameter.</param>
		/// <seealso class='Exec' />
		public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
		{
            if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {                
                if (commandName == "Mfconsulting.Vsprj2make.Connect.CreateMake")
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                }

                if (commandName == "Mfconsulting.Vsprj2make.Connect.GenMDFiles")
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                }

                if (commandName == "Mfconsulting.Vsprj2make.Connect.GenDistUnit")
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                }

                if (commandName == "Mfconsulting.Vsprj2make.Connect.PrjxToCsproj")
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                }

                if (commandName == "Mfconsulting.Vsprj2make.Connect.RunOnMono")
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                }

                if (commandName == "Mfconsulting.Vsprj2make.Connect.Options")
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                }

                if (commandName == "Mfconsulting.Vsprj2make.Connect.ExploreCurrSln")
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                }

                if (commandName == "Mfconsulting.Vsprj2make.Connect.ExploreCurrDoc")
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                }
            }
		}

		/// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
		/// <param term='commandName'>The name of the command to execute.</param>
		/// <param term='executeOption'>Describes how the command should be run.</param>
		/// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
		/// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
		/// <param term='handled'>Informs the caller if the command was handled or not.</param>
		/// <seealso class='Exec' />
		public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
		{
            handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                if (commandName == "Mfconsulting.Vsprj2make.Connect.CreateMake")
                {
                    // handled = true;
                    handled = CreateMakefile();
                    return;
                }

                if (commandName == "Mfconsulting.Vsprj2make.Connect.GenDistUnit")
                {
                    // handled = true;
                    handled = GenerateDistUnit();
                    return;
                }

                if (commandName == "Mfconsulting.Vsprj2make.Connect.GenMDFiles")
                {
                    // handled = true;
                    handled = GenerateMDcmbx();
                    return;
                }

                if (commandName == "Mfconsulting.Vsprj2make.Connect.PrjxToCsproj")
                {
                    // handled = true;
                    handled = ImportMDcmbx();
                    return;
                }

                if (commandName == "Mfconsulting.Vsprj2make.Connect.RunOnMono")
                {
                    // handled = true;
                    handled = TestInMono();
                    return;
                }

                if (commandName == "Mfconsulting.Vsprj2make.Connect.Options")
                {
                    // handled = true;
                    handled = OptionsAndSettings();
                    return;
                }

                if (commandName == "Mfconsulting.Vsprj2make.Connect.ExploreCurrSln")
                {
                    // handled = true;
                    handled = ExploreCurrSln();
                    return;
                }

                if (commandName == "Mfconsulting.Vsprj2make.Connect.ExploreCurrDoc")
                {
                    // handled = true;
                    handled = ExploreCurrDoc();
                    return;
                }
            }
        }

        // public void CreateMakefile(Object commandBarControl, ref bool handled, ref bool cancelDefault)
        public bool CreateMakefile()
        {
            MessageBox.Show("Makefile creation is not implemented yet.", "prj2make-sharp debug");

            //// Create Makefiles
            //Prj2MakeHelper p2mhObj = new Prj2MakeHelper();
            //string strSLNFile = _applicationObject.Solution.FileName;

            //_applicationObject.StatusBar.Clear();
            //outputWindowPane.OutputString("--------------------------------------\nCreating Makefile for ");
            //outputWindowPane.OutputString(String.Format("Solution: {0}\n", strSLNFile));

            //_applicationObject.StatusBar.Text = "Creating Makefile.Win32 file...";
            //outputWindowPane.OutputString("\tCreating Makefile.Win32 file...\n");

            //// csc and nmake
            //outputWindowPane.OutputString(p2mhObj.CreateMakeFile(true, true, strSLNFile));
            //_applicationObject.StatusBar.Text = "Makefile.csc.nmake file created!";
            //outputWindowPane.OutputString("\tMakefile.csc.nmake file created!\n");
            //// mcs and nmake
            //outputWindowPane.OutputString(p2mhObj.CreateMakeFile(false, true, strSLNFile));
            //_applicationObject.StatusBar.Text = "Makefile.mcs.nmake file created!";
            //outputWindowPane.OutputString("\tMakefile.mcs.nmake file created!\n");
            //// mcs and gmake
            //outputWindowPane.OutputString(p2mhObj.CreateMakeFile(false, false, strSLNFile));
            //_applicationObject.StatusBar.Text = "Makefile.mcs.gmake file created!";
            //outputWindowPane.OutputString("\tMakefile.mcs.gmake file created!\n");

            return true;
        }

        // Generate a MonoDevelop combine and/or prjx
        // public void GenerateMDcmbx(Object commandBarControl, ref bool handled, ref bool cancelDefault)
        public bool GenerateMDcmbx()
        {
            MessageBox.Show("Generate MonoDevelop Solution is not implemented yet.", "prj2make-sharp debug");

            //// Create MonoDevelop files
            //Prj2MakeHelper p2mhObj = new Prj2MakeHelper();
            //string strSLNFile = _applicationObject.Solution.FileName;

            //_applicationObject.StatusBar.Clear();
            //outputWindowPane.OutputString("--------------------------------------\nGenerating MonoDevelop files for ");
            //outputWindowPane.OutputString(String.Format("Solution: {0}\n", strSLNFile));

            //_applicationObject.StatusBar.Text = "Creating MonoDevelop files...";
            //outputWindowPane.OutputString("\tCreating MonoDevelop files...\n");

            //outputWindowPane.OutputString(p2mhObj.CreateMdFiles(strSLNFile));

            //_applicationObject.StatusBar.Text = "MonoDevelop files created!";
            //outputWindowPane.OutputString("\tMonoDevelop files created!\n");

            return true;
        }

        // Generate a distribution unit
        public bool GenerateDistUnit()
        {
            // Create Distribution Unit
            Mfconsulting.Vsprj2make.RegistryHelper regH = new RegistryHelper();
            CreateZipHelper zipObj = new CreateZipHelper();
            string strSLNFile = _applicationObject.Solution.FileName;
            string strIgDirs;
            string strIgEx;
            int nLevel;

            // Get Regvalues
            strIgDirs = regH.IgnoredDirectories;
            strIgEx = regH.IgnoredExtensions;
            nLevel = regH.CompressionLevel;

            _applicationObject.StatusBar.Clear();
            outputWindowPane.OutputString("--------------------------------------\nGenerating a distribution unit for ");
            outputWindowPane.OutputString(String.Format("Solution: {0}\n", strSLNFile));

            _applicationObject.StatusBar.Text = "Creating Zip file...";
            outputWindowPane.OutputString("\tCreating Zip file...\n");

            outputWindowPane.OutputString(zipObj.CreateZipFile(
                strSLNFile,
                strIgDirs,
                strIgEx,
                nLevel
                )
                );

            _applicationObject.StatusBar.Text = "Zip file created!";
            outputWindowPane.OutputString("\tZip file created!\n");

            return true;
        }

        // Import MonoDevelop combine and/or prjx
        public bool ImportMDcmbx()
        {
            MessageBox.Show("Import MonoDevelop Solution is not implemented yet.", "prj2make-sharp debug");
            /*
            string strSLNFile = _applicationObject.Solution.FileName;

            _applicationObject.StatusBar.Clear();
            outputWindowPane.OutputString("--------------------------------------\nCompile and Run in Mono: ");
            outputWindowPane.OutputString(String.Format("Solution: {0}\n", strSLNFile));

            _applicationObject.StatusBar.Text = "Attemp to build in Mono...";
            outputWindowPane.OutputString("\tAttemp to build in Mono...\n");

            // Build for Mono

            _applicationObject.StatusBar.Text = "Launch in Mono...";
            outputWindowPane.OutputString("\tLaunch in Mono...\n");

            // Lanunch in Mono
            */

            return true;
        }

        // Run on Mono
        public bool TestInMono()
        {
            // Registry Helper Obj
            Mfconsulting.Vsprj2make.RegistryHelper regH = new RegistryHelper();

            // MonoLaunchHelper
            Mfconsulting.Vsprj2make.MonoLaunchHelper launchHlpr = new MonoLaunchHelper();

            // Web
            bool isWebProject = false;
            string aciveFileSharePath = @"C:\inetpub\wwwroot";
            string startPage = "index.aspx";
            string portForXsp = "8189";

            // Other
            string startUpProject = "";
            string projectOutputFileName = "";
            string projectOutputPathFromBase = "";
            int projectOutputType = 0;
            string projectOutputWorkingDirectory = "";

            EnvDTE.Solution thisSln = _applicationObject.Solution;

            // Run in Mono Process
            System.Diagnostics.ProcessStartInfo procInfo = new ProcessStartInfo();
            System.Diagnostics.Process monoLauncC = new System.Diagnostics.Process();
            monoLauncC.StartInfo = procInfo;

            // Get the Solution's startup project
            for (int propIdx = 1; propIdx <= thisSln.Properties.Count; propIdx++)
            {
                if (thisSln.Properties.Item(propIdx).Name.CompareTo("StartupProject") == 0)
                {
                    startUpProject = thisSln.Properties.Item(propIdx).Value.ToString();
                }
            }

            // Run in Mono
            EnvDTE.Projects projs = thisSln.Projects;
            foreach (EnvDTE.Project proj in projs)
            {
                if (startUpProject.CompareTo(proj.Name) == 0)
                {
                    foreach (EnvDTE.Property prop in proj.Properties)
                    {
                        try
                        {
                            if (prop.Name.CompareTo("ProjectType") == 0)
                            {
                                if (Convert.ToInt32(prop.Value) == 1)
                                {
                                    isWebProject = true;
                                    portForXsp = regH.Port.ToString();
                                }
                            }

                            // Web Root for XSP
                            if (prop.Name.CompareTo("ActiveFileSharePath") == 0)
                            {
                                aciveFileSharePath = prop.Value.ToString();
                            }

                            if (prop.Name.CompareTo("OutputType") == 0)
                            {
                                projectOutputType = Convert.ToInt32(prop.Value);
                            }

                            if (prop.Name.CompareTo("OutputFileName") == 0)
                            {
                                projectOutputFileName = prop.Value.ToString();
                            }

                            if (prop.Name.CompareTo("LocalPath") == 0)
                            {
                                projectOutputWorkingDirectory = prop.Value.ToString();
                            }
                        }
                        catch (System.Runtime.InteropServices.COMException exc)
                        {
                            Trace.WriteLine(String.Format("Error during RunOnMono function call:\n {0}", exc.Message));
                        }
                    }

                    // Active Configuration Properties
                    foreach (EnvDTE.Property prop in proj.ConfigurationManager.ActiveConfiguration.Properties)
                    {
                        // XSP startup page
                        if (prop.Name.CompareTo("StartPage") == 0)
                        {
                            startPage = prop.Value.ToString();
                        }

                        // Output path for non web projects
                        if (prop.Name.CompareTo("OutputPath") == 0)
                        {
                            projectOutputPathFromBase = prop.Value.ToString();
                        }
                    }

                    // If is an Executable and not a DLL or web project
                    // then launch it with monoLaunchC
                    if (isWebProject == false && (projectOutputType == 0 || projectOutputType == 1))
                    {
                        monoLauncC.StartInfo.FileName = launchHlpr.MonoLaunchCPath;
                        monoLauncC.StartInfo.WorkingDirectory = System.IO.Path.Combine(
                            projectOutputWorkingDirectory,
                            projectOutputPathFromBase);
                        monoLauncC.StartInfo.Arguments = projectOutputFileName;

                        monoLauncC.Start();
                        return true;
                    }

                    // Web Project execution and launching of XSP
                    if (isWebProject == true)
                    {
                        string startURL = String.Format(
                            "http://localhost:{0}/{1}",
                            portForXsp,
                            startPage);

                        System.Diagnostics.ProcessStartInfo procInfo1 = new ProcessStartInfo();
                        System.Diagnostics.Process launchStartPage = new System.Diagnostics.Process();
                        launchStartPage.StartInfo = procInfo1;

                        monoLauncC.StartInfo.FileName = launchHlpr.MonoLaunchCPath;
                        monoLauncC.StartInfo.WorkingDirectory = aciveFileSharePath;
                        monoLauncC.StartInfo.Arguments = String.Format(
                            "{0} --root . --port {1} --applications /:.",
                            launchHlpr.GetXspExePath(regH.XspExeSelection),
                            portForXsp
                            );


                        // Actually start XSP
                        monoLauncC.Start();

                        launchStartPage.StartInfo.UseShellExecute = true;
                        launchStartPage.StartInfo.Verb = "open";
                        launchStartPage.StartInfo.FileName = startURL;

                        // Do a little delay so XSP can launch
                        System.Threading.Thread.Sleep(1500);
                        launchStartPage.Start();
                    }
                }
            }

            /*
            string strSLNFile = _applicationObject.Solution.FileName;

            _applicationObject.StatusBar.Clear();
            outputWindowPane.OutputString("--------------------------------------\nCompile and Run in Mono: ");
            outputWindowPane.OutputString(String.Format("Solution: {0}\n", strSLNFile));

            _applicationObject.StatusBar.Text = "Attemp to build in Mono...";
            outputWindowPane.OutputString("\tAttemp to build in Mono...\n");

            // Build for Mono

            _applicationObject.StatusBar.Text = "Launch in Mono...";
            outputWindowPane.OutputString("\tLaunch in Mono...\n");

            // Lanunch in Mono
            */

            return true;
        }

        // vsprj2make settings
        public bool OptionsAndSettings()
        {
            OptionsDlg optDlg = new OptionsDlg();

            if (optDlg.ShowDialog() == DialogResult.OK)
            {
            }
            return true;
        }

        public bool ExploreCurrSln()
        {
            string strSlnFile = _applicationObject.Solution.FileName;
            System.Diagnostics.ProcessStartInfo procInfo = new ProcessStartInfo();
            System.Diagnostics.Process procLaunchExplore = new System.Diagnostics.Process();
            procLaunchExplore.StartInfo = procInfo;

            if (strSlnFile != null)
            {
                procLaunchExplore.StartInfo.UseShellExecute = true;
                procLaunchExplore.StartInfo.Verb = "Open";
                procLaunchExplore.StartInfo.FileName = System.IO.Path.GetDirectoryName(strSlnFile);
                procLaunchExplore.Start();
            }

            return true;
        }

        public bool ExploreCurrDoc()
        {
            string strDocFile = null;
            System.Diagnostics.ProcessStartInfo procInfo = new ProcessStartInfo();
            System.Diagnostics.Process procLaunchExplore = new System.Diagnostics.Process();
            procLaunchExplore.StartInfo = procInfo;

            try
            {
                strDocFile = _applicationObject.ActiveDocument.Path;
            }
            catch (Exception exc)
            {
                Trace.WriteLine(exc.Message);
            }

            if (strDocFile != null)
            {
                procLaunchExplore.StartInfo.UseShellExecute = true;
                procLaunchExplore.StartInfo.Verb = "Open";
                procLaunchExplore.StartInfo.FileName = System.IO.Path.GetDirectoryName(strDocFile);
                procLaunchExplore.Start();
            }

            return true;
        }

        #region Utility functions

        /// <summary>
        /// Searches the existing commands for a match based on the input command name
        /// </summary>
        /// <param name="cmdCollectionContainer">Collection of Command objects</param>
        /// <param name="strCommandName">A string representing the command name</param>
        /// <returns>Returns an Command object instance. If it fails returns null</returns>
        protected Command GetExistingNamedCommand(Commands cmdCollectionContainer, string strCommandName)
        {
            Command cmdRetVal = null;

            // Itereate through all of the commands
            foreach (Command cmd in cmdCollectionContainer)
            {
                if (cmd != null && cmd.Name != null)
                {
                    if (cmd.Name.CompareTo(strCommandName) == 0)
                    {
                        cmdRetVal = cmd;
                        break;
                    }
                }
            }

            return cmdRetVal;
        }

        /// <summary>
        /// Creates a Named command.
        /// </summary>
        /// <param name="addInInstance">Addin Instance</param>
        /// <param name="cmdCollectionContainer">Collection of Command objects</param>
        /// <param name="strCtrlName">The control name</param>
        /// <param name="strCtrlCaption">The control caption</param>
        /// <param name="strCtrlToolTip">Text for tool tip</param>
        /// <param name="contextGUIDS"></param>
        /// <returns>Returns a newly create Command object or null if it fails</returns>
        protected Command CreateNamedCommand(AddIn addInInstance, Commands cmdCollectionContainer, string strCtrlName, string strCtrlCaption, string strCtrlToolTip, ref object[] contextGUIDS)
        {
            Command cmdRetVal = null;
            try
            {
                cmdRetVal = cmdCollectionContainer.AddNamedCommand(
                    addInInstance,
                    strCtrlName,
                    strCtrlCaption,
                    strCtrlToolTip,
                    true,
                    0,
                    ref contextGUIDS,
                    (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled
                    );
            }
            catch (System.Exception exc)
            {
                Trace.WriteLine(String.Format("Exception caught in Adding Command1:\n {0}", exc.Message));
                Trace.WriteLine(String.Format("Number of commands:\n {0}", cmdCollectionContainer.Count));
            }

            return cmdRetVal;
        }

        #endregion

        #region Event handlers for the MonoBarra toolbar

        private void versionComboBox_Change(CommandBarComboBox Ctrl)
        {
            Mfconsulting.Vsprj2make.RegistryHelper regHlpr = new RegistryHelper();
            regHlpr.SetDefaultClr(versionComboBox.Text);

        }

        private void testInMonoCommandBarButton_Click(CommandBarButton Ctrl, ref bool CancelDefault)
        {
            TestInMono();
        }

        #endregion

	}
}