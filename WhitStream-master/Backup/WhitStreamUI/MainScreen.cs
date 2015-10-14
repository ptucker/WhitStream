using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using WhitStream;
using WhitStream.Data;
using WhitStream.QueryEngine;
using WhitStream.QueryEngine.QueryOperators;
using WhitStream.QueryEngine.Scheduler;
using WhitStream.Utility;

namespace Whitstream_GUI
{
    class WhitStream_GUI_Main
    {
        #region Global Variables
        /* Common_Background sets a color that all controls within the application use.
           In some cases the background is set individually, but this keeps a consistent color throughout.*/
        static Color Common_Background = Color.White;   // Background color for all screens
        /* Common_Foreground sets a color that all controls within the application use.*/
        static Color Common_Foreground = Color.Black;   // Foreground color for all screens
		//Connection manager to hold all the incoming connections
        static ConnectionManager cm;
        #endregion

        #region Global Main
        static void Main(string[] args)
        {
            //Create the new connection manager to use port 4000 as the listening port
			cm = new ConnectionManager("4000");
            /*These lines of code are required to set up the form environment and run mainscreen.*/
            Application.EnableVisualStyles();   // Required method
            Application.SetCompatibleTextRenderingDefault(false);   // Required method
			MainScreen ms = new MainScreen();
            Application.Run(ms);  // Runs MainScreen
        }
        #endregion

		#region WhitStream Main Screen
        /*MainScreen is the only actual form with the other screens built in as tab windows.
         The first tabbed window is the Builder Screen where queries are constructed and/or edited.
         The second tabbed window is the Viewer Screen where queries are viewed, organized, activated, etc.
         The third tabbed window is the Credits Screen where members of the WhitStream project are listed.*/

        /// <summary>
        /// The MainScreen form is the first window that gives the user three options.
        /// 1) Query Screen
        /// 2) Query Builder
        /// 3) View credits
        /// </summary>
        public class MainScreen : Form
        {   
            #region Constructor(s)
            /*The MainScreen constructor is split into separate regions to separate variables and controls
             related to respective tab windows. The Window Settings region contains attributes of the form
             itself. MainScreen contains controls and variables connected to the outter window screen.
             BuilderScreen, ViewerScreen, and CreditsScreen are the internal tabbed windows.*/
            public MainScreen()
            {
                #region Window Settings
                Text = "WhitStream";  // Title at top of screen
                Width = MS_W;   // Width of window
                Height = MS_H;  // Height of window
                MinimizeBox = true; // Allows minimize option at top right of window
                MaximizeBox = true;    // Allows maximize operation at top right of window
                ResizeEnd += MainScreen_Resize; // When screen leaves resize state, MainScreen_Resize is called
                ShowInTaskbar = true;   // Allows MainScreen to be seen in taskbar when running
                BackColor = Color.DarkSlateGray;// Common_Background;  // Backcolor set to common
                BackgroundImageLayout = ImageLayout.Stretch;    // Background image stretches over entire surface
                ForeColor = Common_Foreground;  // Forecolor set to common
                CenterToScreen();   // MainScreen gets centered when opened
				//Beginning of making the window appear in the upper left corner
				SetDisplayRectLocation(0, 0);
                FormClosed += MainScreen_FormClosed;    // Event that occurs when the form closed
				WindowState = FormWindowState.Maximized;    // The state the window starts in
				this.AutoScroll = true; // Allows window to scroll
                #endregion

                #region Main Screen
                /* This region defines variables used in the Main Screen. 
                * Control definitions related to Main Screen are also here.
                * Methods related to Main Screen are called here.*/

                #region MainScreen Variables
                /* Variables that relate to the window as a whole*/
                #endregion

                #region MainScreen Control Definitions
                /*Controls that relate to the window as a whole*/
                // Mainscreen Menu
                /*This File menu item is located at the top of the screen and drops a menu box containing
                 other menu items with the name beginning with M_File_ and ending in their respective names*/
                M_File = ToolStripMenuItemMaker("&File", "M_File");
                /*Part of the main menu, it exits the application when clicked*/
                M_File_Exit = ToolStripMenuItemMaker("E&xit", "M_File_Exit", M_File_Exit_Click);
                /*Part of the main menu, it opens the ViewerTab*/
                M_File_OpenViewerTab = ToolStripMenuItemMaker("Open Query &Viewer", "M_File_OpenViewerTab", M_File_OpenViewerTab_Click);
                /*Part of the main menu, it opens the BuilderTab*/
                M_File_OpenBuilderTab = ToolStripMenuItemMaker("Open Query &Builder", "M_File_OpenBuilderTab", M_File_OpenBuilderTab_Click);
                /*This Format menu item is located at the top of the screen and drops a menu box containing
                 other menu items with the name beginning with M_Format_ and ending in their respective names*/
                M_Format = ToolStripMenuItemMaker("For&mat", "M_Format");
                /*Part of the main menu, it allows the user to change the color scheme of the application*/
                M_Format_ChangeColorScheme = ToolStripMenuItemMaker("Change &Color Scheme", "M_Format_ChangeColorScheme", M_Format_ChangeColorScheme_Click);
                /*This Window menu item is located at the top of the screen and drops a menu box containing
                 other menu items with the name beginning with M_Window_ and ending in their respective names*/
                M_Window = ToolStripMenuItemMaker("&Window", "M_Window");
                /*Part of the main menu, it sets the controls to their default location*/
                M_Window_ResetToDefault = ToolStripMenuItemMaker("&Reset Window Positions", "M_Window_ResetToDefault", M_Window_ResetToDefault_Click);
                /*Part of the main menu, it centers the application on the screen*/
                M_Window_CenterToScreen = ToolStripMenuItemMaker("&Center To Screen", "M_Window_CenterToScreen", M_Window_CenterToScreen_Click);
                /*This Help menu item is located at the top of the screen and drops a menu box containing
                 other menu items with the name beginning with M_Help_ and ending in their respective names*/
                M_Help = ToolStripMenuItemMaker("&Help", "M_Help");
                /*Part of the main menu, it displays information on how to contact the WhitStream team*/
                M_Help_ContactUs = ToolStripMenuItemMaker("&Contact Us", "M_Help_ContactUs", M_Help_ContactUs_Click);
                /*Part of the main menu, it opens the CreditsTab*/
                M_Help_AboutWS = ToolStripMenuItemMaker("About &WhitStream", "M_Help_AboutWS", M_Help_AboutWS_Click);
                /*Part of the main menu, it shows the user hotkeys/metakeys that are available*/
                M_Help_MetaKeys = ToolStripMenuItemMaker("Meta&Keys", "M_Help_MetaKeys", M_Help_MetaKeys_Click);
                // Tabs in MainScreen
                /*MainTabControl is the control containing the three windows of BuilderScreen, ViewerScreen, and CreditsScreen
                 The size and location of these three windows is determined by the attributes of MainTabControl*/
                MainTabControl = TabControlMaker("MainTabControl", new Point(0, 23), new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height));
                /*BuilderTab is the tab also known as BuilderScreen. It is where all of the building and editing
                 of queries takes place.*/
                BuilderTab = TabPageMaker("Builder", "BuilderTab");
                /*ViewerTab is the tab where the constructed queries are organized, activated, deactivated, viewed, etc.*/
                ViewerTab = TabPageMaker("Viewer", "ViewerTab");
                /*CreditsTab is the tab where members of the WhitStream project are recognized*/
                CreditsTab = TabPageMaker("Credits", "CreditsTab");
                #endregion

                #region Method Calls
                /*These are methods related to loading mainscreen*/
                /*This method is called to add the controls found on the main screen. This includes the main tabs
                 and the instruction box.*/
                MainScreen_LoadControls();
                #endregion
                #endregion

                #region Builder
                /* This region defines variables used in the Builder Screen. 
                * Control definitions related to Builder Screen are also here.
                * Methods related to Builder Screen are called here.*/

                #region Builder Variables
                /*Variables used by BuilderScreen*/
                /*Moving is used when Operator_MouseMove is invoked. It deals with moving the 
                 * operator pictureboxes around the screen. It is set when Operator_MouseDown is invoked.*/
                Moving = false;
                /*Connect is used in Operator_MouseClick when creating a connection between two operators.
                 It is set when Connect_Click is called*/
                Connect = false;
                /*Disconnect1 is used to disconnect Input1 from a GraphicOperator. It is set when Disconnect1_Click is
                 called and used when Operator_MouseClick is called*/
                Disconnect1 = false;
                /*Disconnect2 is used to disconnect Input2 from a GraphicOperator. It is set when Disconnect2_Click is
                 called and used when Operator_MouseClick is called*/
                Disconnect2 = false;
                /*Add is used to send a finished query to the ViewerTab to be used. It is set when Add_Click is invoked
                 and is used when an OutputStream operator is clicked through Operator_MouseClick*/
                Add = false;
                /*When EditPropertiesActive is true, then the EditPropertiesTab is being shown. It is set when an
                 operator is right clicked and selects Properties from the context menu.*/
                EditPropertiesActive = false;
                /*Setting pen caps determines what the ends of the connection lines will look like*/
                blackpen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                blackpen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                /*These are the types of GroupBys that can be selected. The list is filled to later fill a list box*/
                GroupByListBoxItems = new List<string>();
                GroupByListBoxItems.Add("GroupByCount");
                GroupByListBoxItems.Add("GroupByAvg");
                GroupByListBoxItems.Add("GroupBySum");
                GroupByListBoxItems.Add("GroupByMax");
                GroupByListBoxItems.Add("GroupByMin");
                #endregion

                #region Builder Control Definitions
                #region Menu
                #region Menu/File
                // Menu/File
                /*File is part of a menu specific to BuilderScreen. When clicked it produces a list of
                 menu items that begin with B_File_ and end with their respective names*/
                B_File = ToolStripMenuItemMaker("&File", "File");
                /*Part of the BuilderScreen menu, it sets up the screen for a new query to be built*/
                B_File_New = ToolStripMenuItemMaker("&New Query", "New", B_File_New_Click);
                /*Part of the BuilderScreen menu, it opens a previously saved query*/
                B_File_Open = ToolStripMenuItemMaker("&Open Query", "Open", B_File_Open_Click);
                /*Part of the BuilderScreen menu, it closes the current query*/
                B_File_Close = ToolStripMenuItemMaker("&Close Query", "Close", B_File_Close_Click);
                /*Part of the BuilderScreen menu, it saves the setup of the current query*/
                B_File_Save = ToolStripMenuItemMaker("&Save", "Save", B_File_Save_Click);
                /*Part of the BuilderScreen menu, it saves the setup of the current query under a new name*/
                B_File_SaveAs = ToolStripMenuItemMaker("Sa&ve As...", "SaveAs", B_File_SaveAs_Click);
                /*Part of the BuilderScreen menu, it adds the query to ViewerTab*/
                B_File_Add = ToolStripMenuItemMaker("&Add Query To List", "Add", Button_List_Add_Click);
                #endregion
                #region Menu/Edit
                // Menu/Edit
                /*Edit is part of a menu specific to BuilderScreen. When clicked it produces a list of
                 menu items that begin with B_Edit_ and end with their respective names*/
                B_Edit = ToolStripMenuItemMaker("&Edit", "Edit");
                /*Part of the BuilderScreen menu, it deletes an operator*/
                B_Edit_DeleteOp = ToolStripMenuItemMaker("Delete Operator", "DeleteOp", B_Edit_DeleteOp_Click);
                /*Part of the BuilderScreen menu, it connects two operators by invoking Connect_Click*/
                B_Edit_Connect = ToolStripMenuItemMaker("&Connect Operators", "Connect", Button_List_Connect_Click);
                /*Part of the BuilderScreen menu, it disconnects input connections to an operator*/
                B_Edit_Disconnect = ToolStripMenuItemMaker("Disconnect Operators", "Disconnect", Button_List_Disconnect1_Click);
                /*Part of the BuilderScreen menu, it duplicates a given operator on the screen*/
                B_Edit_Duplicate = ToolStripMenuItemMaker("Duplicate Operator", "Duplicate", B_Edit_Duplicate_Click);
                /*Part of the BuilderScreen menu, it replaces one operator with another*/
                B_Edit_Replace = ToolStripMenuItemMaker("Replace Operator", "Replace", B_Edit_Replace_Click);
                /*Part of the BuilderScreen menu, it displays information about an operator*/
                B_Edit_NodeInfo = ToolStripMenuItemMaker("View Node Information", "NodeInfo", B_Edit_OpInfo_Click);
                /*Part of the BuilderScreen menu, it allows the used to view/edit a note about the query*/
                B_Edit_QueryNote = ToolStripMenuItemMaker("Add/Edit Query Note", "QueryNote", B_Edit_QueryNote_Click);
                /*Part of the BuilderScreen menu, it organizes the operators on the screen by invoking Organize_Click*/
                B_Edit_Organize = ToolStripMenuItemMaker("Organize Query", "Organize", Button_List_Organize_Click);
                /*Part of the BuilderScreen menu, it restores the trash bin*/
                B_Edit_RestoreBin = ToolStripMenuItemMaker("Restore Recycle Bin", "RestoreBin", B_Edit_RestoreBin_Click);
                /*Part of the BuilderScreen menu, it deletes operators put into the trash bin*/
                B_Edit_EmptyBin = ToolStripMenuItemMaker("Empty Recycle Bin", "EmptyBin", B_Edit_EmptyBin_Click);
                #endregion
                #region Menu/View
                // Menu/View
                /*View is part of a menu specific to BuilderScreen. When clicked it produces a list of
                 menu items that begin with B_View_ and end with their respective names*/
                B_View = ToolStripMenuItemMaker("View", "View");
                /*Part of the BuilderScreen menu, it zooms in on the current query*/
                B_View_ZoomIn = ToolStripMenuItemMaker("Zoom In", "ZoomIn", B_View_ZoomIn_Click);
                /*Part of the BuilderScreen menu, it zooms out from the current query*/
                B_View_ZoomOut = ToolStripMenuItemMaker("Zoom Out", "ZoomOut", B_View_ZoomOut_Click);
                /*Part of the BuilderScreen menu, it selects the SourceCodeTab*/
                B_View_Code = ToolStripMenuItemMaker("Source Code", "Code", B_View_XMLCode_Click);
                /*Part of the BuilderScreen menu, it displays the query in full screen*/
                B_View_FullScreen = ToolStripMenuItemMaker("Full Screen", "FullScreen", B_View_FullScreen_Click);
                /*Part of the BuilderScreen menu, it lets the user see what is in the trash bin*/
                B_View_Bin = ToolStripMenuItemMaker("Recycle Bin", "ViewRecycleBin", B_View_TrashBin_Click);
                #endregion
                #region Menu/Insert
                // Menu/Insert
                /*Insert is part of a menu specific to BuilderScreen. When clicked it produces a list of
                 menu items that begin with B_Insert_ and end with their respective names*/
                B_Insert = ToolStripMenuItemMaker("Insert", "Insert");
                /*Part of the BuilderScreen menu, it replaces the current query with a sample*/
                B_Insert_Sample = ToolStripMenuItemMaker("Sample Query", "SampleQuery", B_Insert_SampleQuery_Click);
                /*Part of the BuilderScreen menu, it allows the user to insert a comment in the query*/
                B_Insert_Comment = ToolStripMenuItemMaker("Comment", "Comment", B_Insert_Comment_Click);
                /*Part of the BuilderScreen menu, it allows the user to import code into the current query*/
                B_Insert_ImportCode = ToolStripMenuItemMaker("Import Code", "ImportCode", B_Insert_ImportXML_Click);
                /*Part of the BuilderScreen menu, it places new operator on the screen*/
                B_Insert_Select = ToolStripMenuItemMaker("Select Operator", "InsertSelect", Select_Click);
                /*Part of the BuilderScreen menu, it places new operator on the screen*/
                B_Insert_Project = ToolStripMenuItemMaker("Project Operator", "InsertProject", Project_Click);
                /*Part of the BuilderScreen menu, it places new operator on the screen*/
                B_Insert_DupElim = ToolStripMenuItemMaker("DupElim Operator", "InsertDupElim", DupElim_Click);
                /*Part of the BuilderScreen menu, it places new operator on the screen*/
                B_Insert_GroupBy = ToolStripMenuItemMaker("GroupBy Operator", "InsertGroupBy", GroupBy_Click);
                /*Part of the BuilderScreen menu, it places new operator on the screen*/
                B_Insert_Sort = ToolStripMenuItemMaker("Sort Operator", "InsertSort", Sort_Click);
                /*Part of the BuilderScreen menu, it places new operator on the screen*/
                B_Insert_Join = ToolStripMenuItemMaker("Join Operator", "InsertJoin", Join_Click);
                /*Part of the BuilderScreen menu, it places new operator on the screen*/
                B_Insert_Intersect = ToolStripMenuItemMaker("Intersect Operator", "InsertIntersect", Intersect_Click);
                /*Part of the BuilderScreen menu, it places new operator on the screen*/
                B_Insert_Union = ToolStripMenuItemMaker("Union Operator", "InsertUnion", Union_Click);
                /*Part of the BuilderScreen menu, it places new operator on the screen*/
                B_Insert_Difference = ToolStripMenuItemMaker("Difference Operator", "InsertDifference", Difference_Click);
                /*Part of the BuilderScreen menu, it places new operator on the screen*/
                B_Insert_InputStream = ToolStripMenuItemMaker("Input Stream Operator", "InsertInputStream", InputStream_Click);
                /*Part of the BuilderScreen menu, it places new operator on the screen*/
                B_Insert_OutputStream = ToolStripMenuItemMaker("Output Stream Operator", "InsertOutputStream", OutputStream_Click);
                #endregion
                #region ClickMenu
                /*The menu that appears when PictureBoxes are right clicked*/
                /*ClickMenu_Properties allows the user to change attributes of an operator by displaying the
                 EditPropertiesTab and assigning the clicked operator's information to its controls*/
                ClickMenu_Properties = ToolStripMenuItemMaker("Properties", "ClickMenu_Properties", ClickMenu_Properties_Click);
                #endregion
                #endregion
                #region Tab Controls and Pages
                #region Operator Tab
                /*The tab where operators and their properties are found*/
                /*OperatorsTabControl is the main box where operators are created and edited.*/
                OperatorsTabControl = TabControlMaker("OperatorsTabControl", new Point(1, 24), new Size(MainTabControl.Width * 2 / 10, MainTabControl.Height * 7 / 10 - 50));    //new Size(280, 550));
                #region Operator Tab
                /*Displays all of the operators available as buttons that can be clicked to bring up SetPropertiesTab.*/
                OperatorTab = TabPageMaker("Operators", "OperatorTab");
                #region Icon Images
                /*Connecting the images to their file*/
                SelectIconPic   = new Bitmap("Select.jpg");
                ProjectIconPic  = new Bitmap("Project.jpg");
                DupElimIconPic  = new Bitmap("DupElim.jpg");
                GroupByIconPic  = new Bitmap("GroupBy.jpg");
                GroupByCountIconPic = new Bitmap("groupby_count.jpg");
                GroupByMaxIconPic   = new Bitmap("GroupBy.jpg");
                GroupByMinIconPic   = new Bitmap("GroupBy.jpg");/*__ Changed?*/
                GroupBySumIconPic   = new Bitmap("groupby_sum.jpg");
                GroupByAvgIconPic   = new Bitmap("groupby_avg.jpg");
                SortIconPic = new Bitmap("Sort.jpg");
                JoinIconPic = new Bitmap("Join.jpg");
                UnionIconPic    = new Bitmap("Union.jpg");
                IntersectIconPic    = new Bitmap("Intersect.jpg");
                DifferenceIconPic   = new Bitmap("Difference.jpg");
                #endregion
                #region Icon Definitions
                /*Defining the buttons for creating new operators to work with on the buildermain*/
                SelectIcon  =   ButtonMaker("SelectIcon",   new Point(15, 15),  new Size(50, 50), Select_Click, SelectIconPic,  Select_Hover);
                ProjectIcon =   ButtonMaker("ProjectIcon",  new Point(80, 15),  new Size(50, 50), Project_Click, ProjectIconPic, Project_Hover);
                DupElimIcon =   ButtonMaker("DupElimIcon",  new Point(145, 15), new Size(50, 50), DupElim_Click, DupElimIconPic, DupElim_Hover);
                GroupByIcon =   ButtonMaker("GroupByIcon",  new Point(15, 80),  new Size(50, 50), GroupBy_Click, GroupByIconPic, GroupBy_Hover);
                SortIcon    =   ButtonMaker("SortIcon",     new Point(80, 80),  new Size(50, 50), Sort_Click,   SortIconPic,    Sort_Hover);
                JoinIcon    =   ButtonMaker("JoinIcon",     new Point(15, 185), new Size(50, 50), Join_Click,   JoinIconPic,    Join_Hover);
                UnionIcon   =   ButtonMaker("UnionIcon",    new Point(80, 185), new Size(50, 50), Union_Click,  UnionIconPic,   Union_Hover);
                IntersectIcon   =   ButtonMaker("IntersectIcon",    new Point(145, 185), new Size(50, 50), Intersect_Click, IntersectIconPic, Intersect_Hover);
                DifferenceIcon  =   ButtonMaker("DifferenceIcon",   new Point(15, 255), new Size(50, 50), Difference_Click, DifferenceIconPic, Difference_Hover);
                #endregion
                #endregion
                #region Streams Tab
                /*Assigning the images to their respective files*/
                InputStreamIconPic  = new Bitmap("InputStream.jpg");
                OutputStreamIconPic = new Bitmap("OutputStream.jpg");
                /*Displays the input and output stream operators as buttons.*/
                StreamsTab = TabPageMaker("Streams", "StreamsTab");
                /*Defining the buttons for creating new stream operators to work with on the buildermain*/
                InputStreamIcon  = ButtonMaker("InputStreamIcon",  new Point(15, 15), new Size(50, 50), InputStream_Click,  InputStreamIconPic, InputStream_Hover);
                OutputStreamIcon = ButtonMaker("OutputStreamIcon", new Point(15, 80), new Size(50, 50), OutputStream_Click, OutputStreamIconPic, OutputStream_Hover);
                #endregion
                #region Set Properties Tab
                /*When a new operator is being set up, this tab is opened*/
                SetPropertiesTab = TabPageMaker("Operator Properties", "SetPropertiesTab");
                //OpName
                /*Operators require a name to be referenced individually later, this textbox allows for that*/
                SetPropertiesOpName = TextBoxMaker("Name box", "TextBoxName", new Point(5, 80), new Size(250, 15), false, false);
                SetPropertiesOpName.Font = new Font("Times New Roman", 12);
                /*A label indicating what the OpName text box is used for*/
                SetPropertiesOpNameLabel = LabelMaker("SetPropertiesOpNameLabel", "Name:", new Point(5, 60), new Size(250, 20));
                SetPropertiesOpNameLabel.TextAlign = ContentAlignment.MiddleLeft;
                //Done
                /*When the user has set the attributes, done is clicked and those attributes are assigned to the operator*/
                SetPropertiesDone = ButtonMaker("Add Operator", "SetPropertiesButtonDone", new Point(25, OperatorsTabControl.Bottom - 95), new Size(100, 40), SetPropertiesDone_Click);
                //Cancel
                /*Resets the OperatorTabControl to normal and takes no action to make an operator*/
                SetPropertiesCancel = ButtonMaker("Cancel", "SetPropertiesButtonCancel", new Point(140, OperatorsTabControl.Bottom - 90), new Size(70, 30), SetPropertiesCancel_Click);
                //Picbox
                /*A visual display of what operator is being made*/
                SetPropertiesPicBox = PicBoxMaker(new Point(5, 5), new Size(50, 50));
                /*A label displaying the type of operator being made*/
                SetPropertiesLabel = LabelMaker("SetPropertiesLabel", "Will Change", new Point(60, 20), new Size(190, 25));
                SetPropertiesLabel.Font = new Font("Times New Roman", 13);
                //Predicate
                /*A textbox that sets up the predicate of the operator being made*/
                SetPropertiesPredicate = TextBoxMaker("Insert Predicate Here", "SetPropertiesTextBoxPredicate", new Point(5, 135), new Size(250, 15), false, false);
                SetPropertiesPredicate.Font = SetPropertiesOpName.Font;
                /*A label that indicates what the control below it is doing. Originally this label was made only for the predicate, but
                 this label has a second purpose in that it changes when a groupby operator is being setup.*/
                SetPropertiesPredicateLabel = LabelMaker("SetPropertiesPredicateLabel", "Predicate:", new Point(5, 115), new Size(250, 20));
                SetPropertiesPredicateLabel.Font = SetPropertiesOpNameLabel.Font;
                //CheckedListBox
                /*Changes to display the contents of the control directly below it*/
                SetPropertiesInput1Label = LabelMaker("SetPropertiesInput1Label", "Checked List Box", new Point(5, 170), new Size(250, 20));
                /*Changes item values to reflect the operator being setup*/
                SetPropertiesCheckedListBox = CheckedListBoxMaker("Set Properties", "SetPropertiesCheckedListBox", new Point(5, 190), new Size(250, 20), SetPropertiesCheckedListBox_Click, SetPropertiesCheckedListBox_MouseHover, SetPropertiesCheckedListBox_MouseLeave);
                //ListBox
                /*Changes to display the purpose of the control directly below it*/
                SetPropertiesInput2Label = LabelMaker("SetPropertiesInput2Label", "List Box", new Point(5, 225), new Size(250, 20));
                /*Changes items values depending on the operator being setup*/
                SetPropertiesListBox = ListBoxMaker("Set Properties", "SetPropertiesListBox", new Point(5, 245), new Size(250, 20), SetPropertiesListBox_Click, SetPropertiesListBox_MouseHover, SetPropertiesListBox_MouseLeave);
                //JoinListBox
                /*Changes to display the purpose of the control directly below it*/
                SetPropertiesInput3Label = LabelMaker("SetPropertiesInput3Label", "Join List Box", new Point(5, 225), new Size(250, 20));
                /*Displays different options for a join operator, eventually will be removed when the optimizer is implemented*/
                SetPropertiesJoinListBox = CheckedListBoxMaker("Set Properties", "SetPropertiesJoinListBox", new Point(5, 245), new Size(250, 20), SetPropertiesJoinListBox_Click, SetPropertiesJoinListBox_MouseHover, SetPropertiesJoinListBox_MouseLeave);
                //Note as label
                /*A note that explains information about the operator being set up*/
                SetPropertiesNoteLabel = LabelMaker("SetPropertiesNoteLabel", "Note: Goes here", new Point(5, 335), new Size(250, 130));
                SetPropertiesNoteLabel.TextAlign = ContentAlignment.TopLeft;
                //GroupBy
                /*A list box that provides the different kinds of GroupBy available*/
                SetPropertiesGroupByListBox = ListBoxMaker("Select GroupBy", "SetPropertiesGroupByListBox", new Point(5, 135), new Size(250, 20), SetPropertiesGroupByListBox_Click, SetPropertiesGroupByListBox_MouseHover, SetPropertiesGroupByListBox_MouseLeave);
                #endregion
                #region Edit Properties Tab
                /*When operators are being edited, this tab is opened*/
                EditPropertiesTab = TabPageMaker("Operator Properties", "EditPropertiesTab");
                //OpName
                /*Allows the operator's name to be changed. Steps are taken to assure that the new name is still unique and so will not conflict with other operators*/
                EditPropertiesOpName = TextBoxMaker("Name box", "TextBoxName", new Point(5, 80), new Size(250, 15), false, false);
                EditPropertiesOpName.Font = new Font("Times New Roman", 12);
                /*A label that tells what the OpName text box is for*/
                EditPropertiesOpNameLabel = LabelMaker("EditPropertiesOpNameLabel", "Name:", new Point(5, 60), new Size(250, 20));
                EditPropertiesOpNameLabel.TextAlign = ContentAlignment.MiddleLeft;
                //Done
                /*When clicked, the changes made to the operator are committed permanently*/
                EditPropertiesDone = ButtonMaker("Commit Changes", "EditPropertiesButtonDone", new Point(25, OperatorsTabControl.Bottom - 95), new Size(100, 40), EditPropertiesDone_Click);
                //Cancel
                /*Does not make any changes to the operator and returns the OperatorsTabControl to its normal settings*/
                EditPropertiesCancel = ButtonMaker("Cancel", "EditPropertiesButtonCancel", new Point(140, OperatorsTabControl.Bottom - 90), new Size(70, 30), EditPropertiesCancel_Click);
                //Picbox
                /*Visually displays which operator is being edited*/
                EditPropertiesPicBox = PicBoxMaker(new Point(5, 5), new Size(50, 50));
                /*Displays the type of operator being edited*/
                EditPropertiesLabel = LabelMaker("EditPropertiesLabel", "Will Change", new Point(60, 20), new Size(190, 25));
                EditPropertiesLabel.Font = new Font("Times New Roman", 13);
                //Predicate
                /*Allows for the predicate to be changed*/
                EditPropertiesPredicate = TextBoxMaker("Insert Predicate Here", "EditPropertiesTextBoxPredicate", new Point(5, 135), new Size(250, 20), false, false);
                EditPropertiesPredicate.Font = EditPropertiesOpName.Font;
                /*Tells what the Predicate text box is for, also used to tell what GroupBy types are available*/
                EditPropertiesPredicateLabel = LabelMaker("EditPropertiesPredicateLabel", "Predicate:", new Point(5, 115), new Size(250, 20));
                EditPropertiesPredicateLabel.Font = EditPropertiesOpNameLabel.Font;
                //CheckedListBox
                /*Changes to display the use of the control directly below it*/
                EditPropertiesInput1Label = LabelMaker("EditPropertiesInput1Label", "Checked List Box", new Point(5, 170), new Size(250, 20));
                /*Changes to show values associated with the operator being edited*/
                EditPropertiesCheckedListBox = CheckedListBoxMaker("Set Properties", "EditPropertiesCheckedListBox", new Point(5, 190), new Size(250, 20), EditPropertiesCheckedListBox_Click, EditPropertiesCheckedListBox_MouseHover, EditPropertiesCheckedListBox_MouseLeave);
                //ListBox
                /*Changes to display the use of the control directly below it*/
                EditPropertiesInput2Label = LabelMaker("EditPropertiesInput2Label", "List Box", new Point(5, 225), new Size(250, 20));
                /*Changes to show the values associated with the operator being edited*/
                EditPropertiesListBox = ListBoxMaker("Set Properties", "EditPropertiesListBox", new Point(5, 245), new Size(250, 20), EditPropertiesListBox_Click, EditPropertiesListBox_MouseHover, EditPropertiesListBox_MouseLeave);
                //JoinListBox
                /*Changes to display the use of the control directly below it*/
                EditPropertiesInput3Label = LabelMaker("EditPropertiesInput3Label", "Join List Box", new Point(5, 225), new Size(250, 20));
                /*Shows the types of joins available, will eventually be removed when the optimizer is implemented*/
                EditPropertiesJoinListBox = CheckedListBoxMaker("Set Properties", "EditPropertiesJoinListBox", new Point(5, 245), new Size(250, 20), EditPropertiesJoinListBox_Click, EditPropertiesJoinListBox_MouseHover, EditPropertiesJoinListBox_MouseLeave);
                //Note as label
                /*A note displaying information about the current operator being edited*/
                EditPropertiesNoteLabel = LabelMaker("EditPropertiesNoteLabel", "Note: Goes here", new Point(5, 335), new Size(250, 130));
                EditPropertiesNoteLabel.TextAlign = ContentAlignment.TopLeft;
                //GroupBy
                /*Displays the different types of groupbys available*/
                EditPropertiesGroupByListBox = ListBoxMaker("Select GroupBy", "EditPropertiesGroupByListBox", new Point(5, 135), new Size(250, 20), EditPropertiesGroupByListBox_Click, EditPropertiesGroupByListBox_MouseHover, EditPropertiesGroupByListBox_MouseLeave);
                #endregion
                #endregion
                #region BuilderMain Tab
                /*The tab where the query is built/edited either visually or through code*/
                BuilderMainTabControl = TabControlMaker("BuilderMainTabControl", new Point(OperatorsTabControl.Right + 1, 24), new Size(MainTabControl.Width - OperatorsTabControl.Width - 300, MainTabControl.Height - 105)); //new Size(1013, 775));
                /*The tab where the query is built and/or edited visually*/
                BuilderMainTab = TabPageMaker("Construction Area", "BuilderMainTab");
                /*The tab where the query is built and/or edited through code*/
                SourceCodeTab = TabPageMaker("Source Code", "SourceCodeTab");
                /*Assigns the event that is called when BuilderMainTab is repainted*/
                BuilderMainTab.Paint += BuilderMainTab_Paint;
                #endregion
                #region ButtonList Tab
                /*A button list for easy use*/
                ButtonListTabControl = TabControlMaker("ButtonListTabControl", new Point(BuilderMainTabControl.Right + 1, 24), new Size(130, MainTabControl.Height - 105));
                /*A list of buttons that are commonly used*/
                DefaultButtonListTab = TabPageMaker("Main List", "DefaultButtonListTab");
                /*A list of buttons that the user will be able to define themselves, not implemented yet*/
                UserDefinedListTab = TabPageMaker("User List", "UserDefinedListTab");
                #endregion
                #endregion
                #region Button List Tab
                /*The buttons on the button list tab*/
                /*Allows the user to save the query they are working on, not implemented yet*/
                ButtonListSave = ButtonMaker("Save", "ButtonListSave", new Point(10, 15), new Size(100, 30), Button_List_Save_Click);
                /*Sets the bool Add to true. The real use of it happens when Operator_MouseClick is called*/
                ButtonListAdd = ButtonMaker("Add", "ButtonListAdd", new Point(10, 60), new Size(100, 30), Button_List_Add_Click);
                /*Organizes the operators on the screen*/
                ButtonListOrganize = ButtonMaker("Organize", "ButtonListOrganize", new Point(10, 105), new Size(100, 30), Button_List_Organize_Click);
                /*Erases the current query and replaces it with a sample*/
                ButtonListSample = ButtonMaker("Sample Query", "ButtonListSample", new Point(10, 150), new Size(100, 30), Button_List_SampleQuery_Click);
                /*Displays the query in full screen, not implemented yet*/
                ButtonListFullScreen = ButtonMaker("Full Screen", "ButtonListFullScreen", new Point(10, 195), new Size(100, 30), Button_List_FullScreen_Click);
                /*Aligns the operators on the screen to a grid*/
                ButtonListAlignToGrid = ButtonMaker("Align to Grid", "ButtonListAlignToGrid", new Point(10, 285), new Size(100, 30), Button_List_ShowGrid_Click);
                /*Sets bool Connect to true. The real use of it comes when Operator_MouseClick is called*/
                ButtonListConnect = ButtonMaker("Connect", "ButtonListConnect", new Point(10, 375), new Size(100, 30), Button_List_Connect_Click);
                /*Sets bool Disconnect1 to true. The real use of it comes when Operator_MouseClick is called*/
                ButtonListDisconnect1 = ButtonMaker("Disconnect I1", "ButtonListDisconnect1", new Point(10, 465), new Size(100, 30), Button_List_Disconnect1_Click);
                /*Sets bool Disconnect2 to true. The real use of it comes when Operator_MouseClick is called*/
                ButtonListDisconnect2 = ButtonMaker("Disconnect I2", "ButtonListDisconnect2", new Point(10, 510), new Size(100, 30), Button_List_Disconnect2_Click);
                #endregion
                
                #region Source Code Tab
                /*The tab where code will be displayed*/
                SourceCodeText = TextBoxMaker("This is where the code behind the query will be displayed", "SourceCodeTextBox", new Point(10, 12), new Size(BuilderMainTabControl.Width - 30, BuilderMainTabControl.Height - 49), false, true);
                #endregion
                #region Builder Main Tab
                /*This tab is where the query is visually built*/
                /*A label at the top of BuilderMainTab*/
                BuilderLabel = LabelMaker("BuilderLabel", "Building Screen", new Point(BuilderMainTabControl.Width / 2 - 150, 5), new Size(300, 20));
                BuilderLabel.TextAlign = ContentAlignment.TopCenter;
                /*Assigning images to their respective files*/
                B_TrashPic = new Bitmap("B_Trash.jpg");
                B_ZoomInPic = new Bitmap("B_ZoomIn.jpg");
                B_ZoomOutPic = new Bitmap("B_ZoomOut.jpg");
                /*Setting up the trash bin picturebox*/
                B_Trash = PicBoxMaker("B_Trash", new Point(949, 9), new Size(52, 52), B_TrashPic);
                /*Setting up the zoom in button*/
                ZoomIn = ButtonMaker("BuilderMainZoomIn", new Point(BuilderMainTabControl.Width - 58, BuilderMainTabControl.Height - 51), new Size(25, 25), BuilderMain_ZoomIn_Click, B_ZoomInPic, BuilderMain_ZoomIn_Hover);
                /*Setting up the zoom out button*/
                ZoomOut = ButtonMaker("BuilderMainZoomOut", new Point(BuilderMainTabControl.Width - 33, BuilderMainTabControl.Height - 51), new Size(25, 25), BuilderMain_ZoomOut_Click, B_ZoomOutPic, BuilderMain_ZoomOut_Hover);
                #endregion
                #region Onscreen Controls
                /*Controls on the mainscreen that aren't directly related to a certain tab*/
                /*A text box that updates the user on what is happening while they are working*/
                InstructionBox = TextBoxMaker("Instruction Box: Help for current actions are displayed here", "InstructionBox", new Point(1, OperatorsTabControl.Bottom + 1), new Size(OperatorsTabControl.Width, MainTabControl.Height - OperatorsTabControl.Height - 105), true, true);
                #endregion
                #endregion

                #region Method Calls
                /*Methods that relate to BuilderScreen*/
                /*This method is called to load the controls found on BuilderScreen*/
                BuilderScreen_LoadControls();
                /*Sets up the sample query to have something to work with right away*/
                Set_Sample_Query();
                /*Nullifies the controls on EditPropertiesTab*/
                Fill_EditProperties(null);
                #endregion
                #endregion

                #region Viewer
                /* This region defines variables used in the Viewer Screen. 
                * Control definitions related to Viewer Screen are also here.
                * Methods related to Viewer Screen are called here.*/

                #region Viewer Variables
                /*Variables related to Query Viewer Screen*/
				timer = new System.Windows.Forms.Timer();
				timer.Interval = 1000;
				timer.Tick += new EventHandler(UpdateInfo);
				timer.Start();
                #endregion

                #region Viewer Control Definitions
                /*Controls related to Query Viewer Screen*/
                /* View_All is a button that shows all queries in the query box*/
                View_All = ButtonMaker("View All Queries", "View_All", new Point(15, 15), new Size(BTN_W, BTN_H), View_All_Click);
                /* View_Active is a button that shows active queries in the query box*/
                View_Active = ButtonMaker("View Active Queries", "View_Active", new Point(BTN_W + 30, 15), new Size(BTN_W, BTN_H), View_Active_Click);
                /* View_Inactive is a button that shows inactive queries in the query box*/
                View_Inactive = ButtonMaker("View Inactive Queries", "View_Inactive", new Point((2 * BTN_W) + 45, 15), new Size(BTN_W, BTN_H), View_Inactive_Click);
                /* Browse is a button that allows the user to search for a query saved outside of the application*/
                Browse = ButtonMaker("Import Query", "Browse", new Point((77 * QS_W / 100), 15), new Size(BTN_W, BTN_H), Browse_Click);
                /* QS_Activate is a button that activates the selected query in the query box*/
                QS_Activate = ButtonMaker("Activate Query", "Activate", new Point(15, 60 * QS_H / 100), new Size(BTN_W, BTN_H), Activate_Click);
                /* QS_Deactivate is a button that deactivates the selected query in the query box*/
                QS_Deactivate = ButtonMaker("Deactivate Query", "Deactivate", new Point(BTN_W + 30, 60 * QS_H / 100), new Size(BTN_W, BTN_H), Deactivate_Click);
                /* Confirm_Button is a button that pops up to confirm certain actions taken by the user*/
                Confirm_Button = ButtonMaker("CONFIRM", "Confirm_Button", new Point((2 * BTN_W) + 45, 60 * QS_H / 100), new Size(BTN_W, BTN_H), Confirm_Button_Click, Color.Green, 16);
                /* Cancel_Button is a button that pops up to cancel certain actions taken by the user*/
                Cancel_Button = ButtonMaker("CANCEL", "Cancel_Button", new Point((3 * BTN_W) + 60, 60 * QS_H / 100), new Size(BTN_W, BTN_H), Cancel_Button_Click, Color.Red, 16);
                /* Open_Q is a button that opens the selected query for editing*/
                Open_Q = ButtonMaker("Open Query for Editing", "Open_Q", new Point(77 * QS_W / 100, 67 * QS_H / 100), new Size(BTN_W, BTN_H), Open_Q_Click);
                /* Remove_Q is a button that removes a query from the query box*/
                Remove_Q = ButtonMaker("Remove Query From List", "Remove_Q", new Point(77 * QS_W / 100, 75 * QS_H / 100), new Size(BTN_W, BTN_H), Remove_Q_Click);
                /* Return_Main is a button that closes ViewerTab and opens MainScreen*/
                Return_Main = ButtonMaker("Return to Main Screen", "Return_Main", new Point(77 * QS_W / 100, 83 * QS_H / 100), new Size(BTN_W, BTN_H), Return_Main_Click);
                /* Q_Description is a textbox that displays information related to the selected query*/
                Q_Description = TextBoxMaker("Query Description Text Box", "Q_Desc", new Point(QS_W / 100, (60 * QS_H / 100) + BTN_H + 15), new Size(4 * BTN_W + 45, 200), true, true);
				Q_Description.Font = new Font("Times New Roman", 12);
				Q_Description.ScrollBars = ScrollBars.Vertical;
                /* Q_Info is a textbox that displays information related to the selected query*/
                Q_Info = TextBoxMaker("Query Information Text Box", "Q_Info", new Point(77 * QS_W / 100, BTN_H + 30), new Size(BTN_W, (485 - BTN_H)), true, true);
                /* QueryListBox displays all available queries*/
                QueryListBox = ListBoxMaker("Query List", "QueryList", new Point(15, BTN_H + 30), new Size(4 * BTN_W + 45, 485 - BTN_H), QueryList_Click);
                QueryListBox.Font = new Font("Times New Roman", 15);
                /* QueryList holds the available queries*/
                QueryList = new List<Query>();
                /* AddedQueries holds the queries that need to be added and have not yet*/
                AddedQueries = new List<Query>();
                #endregion

                #region Method Calls
                /*Methods related to Query Viewer Screen*/
                /*This method loads the controls relevant to ViewerTab*/
                ViewerTab_LoadControls();
                #endregion
                #endregion

                #region Credits
                /* This region defines variables used in the Credits Screen. 
                * Control definitions related to Credits Screen are also here.
                * Methods related to Credits Screen are called here.*/

                #region Credits Variables
                /*Variables related to Credits Screen*/
                #endregion

                #region Credits Control Definitions
                /*Controls associated with the credits screen*/
                /* WhitworthImage is a Picture displayed next to the member names*/
                WhitworthImage = PicBoxMaker("WhitworthLogo", new Point(75, MainTabControl.Height / 4), new Size(500, 300), WhitworthLogo);
                /* Whitstream_Developer is a label*/
                Whitstream_Developer = LabelMaker("Whitstream_Developer", "Whitstream Project Director", new Point(MainTabControl.Width / 2, 20), new Size(300, 20));
                Whitstream_Developer.TextAlign = ContentAlignment.MiddleCenter;
                Whitstream_Developer.Font = new Font("Times New Roman", 14);
                /* Whitstream_Dev_Name is a label*/
                Whitstream_Dev_Name = LabelMaker("Whitstream_Dev_Name", "Pete Tucker", new Point(MainTabControl.Width / 2, 45), new Size(300, 20), 12);
                /* Whitstream_Members is a label used to insert member names into an arraylist*/
                Whitstream_Members = LabelMaker("Whitstream_Members", "Whitstream Project Developers", new Point(MainTabControl.Width / 2, 95), new Size(300, 20));
                Whitstream_Members.TextAlign = ContentAlignment.MiddleCenter;
                Whitstream_Members.Font = new Font("Times New Roman", 14);
                #region Whitworth Researchers
                /*Members of the WhitStream team*/
                Members[0] = "Dallas Crockett";
                Members[1] = "Ryan Knuth";
                Members[2] = "Paul Stevens";
                Members[3] = "Steven Ash";
                Members[4] = "Andrew Zellman";
                Members[5] = "Daniel Sanders";
                Members[6] = "Laura Blum";
                Members[7] = "Will Sehorn";
                Members[8] = "Kalen Spees";
                Members[9] = "Chris Peterson";
                Members[10] = "Jennifer Gimera";
                int i = 125;
                int j = 0;
                string tempname;
                for (int k = 0; k < Members.Length-1; k++)
                {
                    if (Members[k].Length < Members[k + 1].Length)
                    {
                        tempname = Members[k];
                        Members[k] = Members[k+1];
                        Members[k+1] = tempname;
                        k = 0;
                    }
                }
                foreach (string m in Members)
                {
                    Member_Labels[j] = LabelMaker(String.Format("Member_{0}", j), m, new Point(MainTabControl.Width / 2, i), new Size(300, 20), 12);
                    i += 25;
                    j++;
                }
                #endregion
                #endregion

                #region Method Calls
                /*Methods called related to CreditsScreen*/
                /*This method is called to load controls relevant to CreditsScreen*/
                CreditsScreen_LoadControls();   // Adds controls to MainScreen
                #endregion
                #endregion

				#region Scheduler
				//List of all the result handlers for the queries
				result_handler = new List<ResultsHandler>(4);

				#endregion
			}

            #endregion

            #region Main Screen
            /* This region contains variables used in the Main Screen. 
             * Method definitions and Events related to Main Screen are also here.*/
            #region MainScreen Variables
            /*Variables related to MainScreen*/
            int MS_W = 740; // Sets window width of MainScreen
            int MS_H = 500; // Sets window height of MainScreen
            /*M_Menu is what the rest of the main menu it built from*/
            MenuStrip M_Menu = new MenuStrip();
            /*Declaring the menu File and its items*/
            ToolStripMenuItem M_File, M_File_Exit, M_File_OpenViewerTab, M_File_OpenBuilderTab;
            /*Declaring the menu Format and its items*/
            ToolStripMenuItem M_Format, M_Format_ChangeColorScheme;
            /*Declaring the menu Window and its items*/
            ToolStripMenuItem M_Window, M_Window_ResetToDefault, M_Window_CenterToScreen;
            /*Declaring the menu Help and its items*/
            ToolStripMenuItem M_Help, M_Help_ContactUs, M_Help_AboutWS, M_Help_MetaKeys;
            /*A line inserted to separate menu items*/
            ToolStripSeparator M_Line = new ToolStripSeparator();
            /*Declaring the main tab control*/
            TabControl MainTabControl;
            /*Declaring the tabs to be inserted in MainTabControl*/
            TabPage BuilderTab, ViewerTab, CreditsTab;
			/*Scheduler to run the queries*/
			IScheduler sch = null;
            //One result handler per query that we are running.  We store them here!
			List<ResultsHandler> result_handler;
            #endregion

            #region MainScreen Method Definitions
            /// <summary>
            /// Adds Controls to MainScreen
            /// </summary>
            private void MainScreen_LoadControls()
            {
                Controls.Add(MainTabControl);
                #region Menu
                Controls.Add(M_Menu);
                M_Menu.Items.Add(M_File);
                M_Menu.Items.Add(M_Format);
                M_Menu.Items.Add(M_Window);
                M_Menu.Items.Add(M_Help);
                M_File.DropDownItems.Add(M_File_OpenBuilderTab);
                M_File.DropDownItems.Add(M_File_OpenViewerTab);
                M_File.DropDownItems.Add(M_File_Exit);
                M_Format.DropDownItems.Add(M_Format_ChangeColorScheme);
                M_Window.DropDownItems.Add(M_Window_ResetToDefault);
                M_Window.DropDownItems.Add(M_Window_CenterToScreen);
                M_Help.DropDownItems.Add(M_Help_AboutWS);
                M_Help.DropDownItems.Add(M_Help_ContactUs);
                M_Help.DropDownItems.Add(M_Help_MetaKeys);
                #endregion
                #region BuilderTab
                MainTabControl.Controls.Add(BuilderTab);
                #endregion
                #region ViewerTab
				MainTabControl.Controls.Add(ViewerTab);
                #endregion
                #region CreditsTab
				MainTabControl.Controls.Add(CreditsTab);
                #endregion
            }
            #endregion

            #region MainScreen Event Definitions
            /// <summary>
            /// When MainScreen ends a resize state, this event is called
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void MainScreen_Resize(object sender, EventArgs e)
            {

            }
            /// <summary>
            /// Occurs when the form closes
            ///</summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void MainScreen_FormClosed(object sender, EventArgs e)
            {
				cm.Kill();
				Application.Exit();
            }
            /// <summary>
            /// Exits the application
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void M_File_Exit_Click(object sender, EventArgs e)
            {
                Application.Exit();
            }
            /// <summary>
            /// Selects ViewerTab
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void M_File_OpenViewerTab_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action assigned to that control yet";
            }
            /// <summary>
            /// Selects BuilderTab
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void M_File_OpenBuilderTab_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action assigned to that control yet";
            }
            /// <summary>
            /// Change the color scheme
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void M_Format_ChangeColorScheme_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action assigned to that control yet";
            }
            /// <summary>
            /// Reset controls to default location
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void M_Window_ResetToDefault_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action assigned to that control yet";
            }
            /// <summary>
            /// Center window on screen
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void M_Window_CenterToScreen_Click(object sender, EventArgs e)
            {
                CenterToScreen();
            }
            /// <summary>
            /// View contact information for WhitStream project
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void M_Help_ContactUs_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action assigned to that control yet";
            }
            /// <summary>
            /// View information about WhitStream
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void M_Help_AboutWS_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action assigned to that control yet";
            }
            /// <summary>
            /// View information about available metakeys
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void M_Help_MetaKeys_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action assigned to that control yet";
            }
            #endregion
            #endregion

            #region Builder
            /* This region contains variables used in the Builder Screen. 
             * Method definitions and Events related to Builder Screen are also here.*/
            #region Builder Variables
            #region Global
            /*Sets BuilderS*/
            //int BS_W = 1448;    // 1448 BuilderScreen window width maximized
            //int BS_H = 878; // 878  BuilderScreen window height maximized
            /**/
            int oldX, oldY; // Used for repositioning Pictureboxes
            /**/
            int QueuePos;
            /**/
            bool Moving, Connect, Disconnect1, Disconnect2, EditPropertiesActive, Add;
            /**/
            string PropertiesType, C, GroupByListBoxSelected;
            /**/
            Pen blackpen = new Pen(Color.Black, 5);
            Pen coolpen = Pens.Firebrick;
            /**/
            PictureBox Origin = new PictureBox();
            /**/
            PictureBox LastClicked = new PictureBox();
            /**/
            Image BuilderScreenBackground = new Bitmap("Background1.jpg");
            /**/
            List<string> GroupByListBoxItems;
            #endregion
            #region Menu Controls
            /**/
            MenuStrip B_Menu = new MenuStrip();
            /**/
            ToolStripMenuItem B_File, B_File_New, B_File_Open, B_File_Close, B_File_Save, B_File_SaveAs, B_File_Add;
            /**/
            ToolStripMenuItem B_Edit, B_Edit_DeleteOp, B_Edit_Connect, B_Edit_Disconnect, B_Edit_Duplicate, B_Edit_Replace, B_Edit_NodeInfo, B_Edit_QueryNote, B_Edit_Organize, B_Edit_RestoreBin, B_Edit_EmptyBin;
            /**/
            ToolStripMenuItem B_View, B_View_ZoomIn, B_View_ZoomOut, B_View_Code, B_View_FullScreen, B_View_Bin;
            /**/
            ToolStripMenuItem B_Insert, B_Insert_Sample, B_Insert_Comment, B_Insert_ImportCode, B_Insert_Select, B_Insert_Project, B_Insert_DupElim, B_Insert_GroupBy, B_Insert_Sort, B_Insert_Join, B_Insert_Intersect, B_Insert_Union, B_Insert_Difference, B_Insert_InputStream, B_Insert_OutputStream;
            /**/
            ToolStripSeparator B_File_Line = new ToolStripSeparator();
            ToolStripSeparator B_Edit_Line1 = new ToolStripSeparator();
            ToolStripSeparator B_Edit_Line2 = new ToolStripSeparator();
            ToolStripSeparator B_Insert_Line = new ToolStripSeparator();
            /**/
            ContextMenuStrip RightClick_Menu = new ContextMenuStrip();
            /**/
            ToolStripMenuItem ClickMenu_Properties;
            #endregion
            #region Tab Controls
            /**/
            TabControl OperatorsTabControl, BuilderMainTabControl, ButtonListTabControl;
            /**/
            TabPage OperatorTab, StreamsTab, BuilderMainTab, SourceCodeTab, DefaultButtonListTab, UserDefinedListTab, SetPropertiesTab, EditPropertiesTab;
            /**/
            Button ButtonListSave, ButtonListAdd, ButtonListOrganize, ButtonListSample, ButtonListFullScreen, ButtonListAlignToGrid, ButtonListConnect, ButtonListDisconnect1, ButtonListDisconnect2;
            /**/
            Button ZoomIn, ZoomOut, SelectIcon, ProjectIcon, DupElimIcon, GroupByIcon, SortIcon;
            /**/
            Button JoinIcon, UnionIcon, IntersectIcon, DifferenceIcon, InputStreamIcon, OutputStreamIcon, SetPropertiesDone, EditPropertiesDone, SetPropertiesCancel, EditPropertiesCancel;
            /**/
            PictureBox B_Trash, NewPicBox, SetPropertiesPicBox, EditPropertiesPicBox;
            /**/
            TextBox SourceCodeText, SetPropertiesOpName, SetPropertiesPredicate, EditPropertiesOpName, EditPropertiesPredicate;
            /**/
            Label SetPropertiesLabel, SetPropertiesOpNameLabel, SetPropertiesPredicateLabel, SetPropertiesInput1Label, SetPropertiesInput2Label, SetPropertiesInput3Label, SetPropertiesNoteLabel;
            /**/
            Label EditPropertiesLabel, EditPropertiesOpNameLabel, EditPropertiesPredicateLabel, EditPropertiesInput1Label, EditPropertiesInput2Label, EditPropertiesInput3Label, EditPropertiesNoteLabel;
            /**/
            CheckedListBox SetPropertiesCheckedListBox, EditPropertiesCheckedListBox;
            /**/
            ListBox SetPropertiesJoinListBox, EditPropertiesJoinListBox, SetPropertiesListBox, EditPropertiesListBox, SetPropertiesGroupByListBox, EditPropertiesGroupByListBox;
            #endregion
            #region OnScreen Controls
            /**/
            TextBox InstructionBox;
            /**/
            List<GraphicOperator> OperatorListGraphic = new List<GraphicOperator>();
            /**/
            List<GraphicOperator> TrashList = new List<GraphicOperator>();
            #endregion
            #region Images and Labels
            Image SelectIconPic, ProjectIconPic, DupElimIconPic, GroupByIconPic, GroupByCountIconPic, GroupByMaxIconPic, GroupByMinIconPic, GroupBySumIconPic, GroupByAvgIconPic, SortIconPic;
            Image JoinIconPic, UnionIconPic, IntersectIconPic, DifferenceIconPic;
            Image InputStreamIconPic, OutputStreamIconPic;
            Image B_TrashPic, B_ZoomInPic, B_ZoomOutPic;
            /**/
            Label BuilderLabel;
            #endregion
            #endregion

            #region Builder Method Definitions
            /**/
            /// <summary>
            /// Adds Controls to BuilderScreen
            /// </summary>
            private void BuilderScreen_LoadControls()
            {
                BuilderTab.Controls.Add(B_Menu);
                BuilderTab.Controls.Add(InstructionBox);
                B_Menu_Add_All();
                RightClick_Menu_Add_All();
                Add_Pages_To_Tabs();
                Add_Controls_To_Tabs();
                BuilderTab.Controls.Add(OperatorsTabControl);
                BuilderTab.Controls.Add(BuilderMainTabControl);
                BuilderTab.Controls.Add(ButtonListTabControl);
                BuilderTab.BackColor = Color.Blue;
            }
            /**/
            /// <summary>
            /// Adds all menu items to main menu
            /// </summary>
            private void B_Menu_Add_All()
            {
                // Menu
                B_Menu.Items.Add(B_File);
                B_Menu.Items.Add(B_Edit);
                B_Menu.Items.Add(B_View);
                B_Menu.Items.Add(B_Insert);
                // File
                B_File.DropDownItems.Add(B_File_New);
                B_File.DropDownItems.Add(B_File_Open);
                B_File.DropDownItems.Add(B_File_Close);
                B_File.DropDownItems.Add(B_File_Save);
                B_File.DropDownItems.Add(B_File_SaveAs);
                B_File.DropDownItems.Add(B_File_Add);
                // Edit
                B_Edit.DropDownItems.Add(B_Edit_DeleteOp);
                B_Edit.DropDownItems.Add(B_Edit_Connect);
                B_Edit.DropDownItems.Add(B_Edit_Disconnect);
                B_Edit.DropDownItems.Add(B_Edit_Duplicate);
                B_Edit.DropDownItems.Add(B_Edit_Replace);
                B_Edit.DropDownItems.Add(B_Edit_Line1);
                B_Edit.DropDownItems.Add(B_Edit_NodeInfo);
                B_Edit.DropDownItems.Add(B_Edit_QueryNote);
                B_Edit.DropDownItems.Add(B_Edit_Organize);
                B_Edit.DropDownItems.Add(B_Edit_Line2);
                B_Edit.DropDownItems.Add(B_Edit_RestoreBin);
                B_Edit.DropDownItems.Add(B_Edit_EmptyBin);

                // View
                B_View.DropDownItems.Add(B_View_ZoomIn);
                B_View.DropDownItems.Add(B_View_ZoomOut);
                B_View.DropDownItems.Add(B_View_FullScreen);
                B_View.DropDownItems.Add(B_View_Code);
                B_View.DropDownItems.Add(B_View_Bin);
                // Insert
                B_Insert.DropDownItems.Add(B_Insert_Sample);
                B_Insert.DropDownItems.Add(B_Insert_Comment);
                B_Insert.DropDownItems.Add(B_Insert_ImportCode);
                B_Insert.DropDownItems.Add(B_Insert_Line);
                B_Insert.DropDownItems.Add(B_Insert_Select);
                B_Insert.DropDownItems.Add(B_Insert_Project);
                B_Insert.DropDownItems.Add(B_Insert_DupElim);
                B_Insert.DropDownItems.Add(B_Insert_GroupBy);
                B_Insert.DropDownItems.Add(B_Insert_Sort);
                B_Insert.DropDownItems.Add(B_Insert_Join);
                B_Insert.DropDownItems.Add(B_Insert_Union);
                B_Insert.DropDownItems.Add(B_Insert_Intersect);
                B_Insert.DropDownItems.Add(B_Insert_Difference);
                B_Insert.DropDownItems.Add(B_Insert_InputStream);
                B_Insert.DropDownItems.Add(B_Insert_OutputStream);
            }
            /**/
            /// <summary>
            /// Adds all menu items to right clicked menu
            /// </summary>
            private void RightClick_Menu_Add_All()
            {
                // Menu
                RightClick_Menu.Items.Add(ClickMenu_Properties);
            }
            /**/
            /// <summary>
            /// Add controls to tabs in Builder Screens
            /// </summary>
            /// <returns></returns>
            private void Add_Controls_To_Tabs()
            {
                #region Operators Tab
                OperatorTab.Controls.Add(SelectIcon);
                OperatorTab.Controls.Add(ProjectIcon);
                OperatorTab.Controls.Add(DupElimIcon);
                OperatorTab.Controls.Add(GroupByIcon);
                OperatorTab.Controls.Add(SortIcon);
                OperatorTab.Controls.Add(JoinIcon);
                OperatorTab.Controls.Add(UnionIcon);
                OperatorTab.Controls.Add(IntersectIcon);
                OperatorTab.Controls.Add(DifferenceIcon);
                #endregion
                #region Streams Tab
                StreamsTab.Controls.Add(InputStreamIcon);
                StreamsTab.Controls.Add(OutputStreamIcon);
                #endregion
                #region Properties Tabs
                SetPropertiesTab.Controls.Add(SetPropertiesDone);
                SetPropertiesTab.Controls.Add(SetPropertiesCancel);
                SetPropertiesTab.Controls.Add(SetPropertiesOpName);
                SetPropertiesTab.Controls.Add(SetPropertiesLabel);
                SetPropertiesTab.Controls.Add(SetPropertiesPicBox);
                SetPropertiesTab.Controls.Add(SetPropertiesOpNameLabel);
                SetPropertiesTab.Controls.Add(SetPropertiesNoteLabel);


                EditPropertiesTab.Controls.Add(EditPropertiesDone);
                EditPropertiesTab.Controls.Add(EditPropertiesCancel);
                EditPropertiesTab.Controls.Add(EditPropertiesOpName);
                EditPropertiesTab.Controls.Add(EditPropertiesLabel);
                EditPropertiesTab.Controls.Add(EditPropertiesPicBox);
                EditPropertiesTab.Controls.Add(EditPropertiesOpNameLabel);
                EditPropertiesTab.Controls.Add(EditPropertiesNoteLabel);
                #endregion
                #region Builder Main Tab
                BuilderMainTab.Controls.Add(BuilderLabel);
                BuilderMainTab.Controls.Add(B_Trash);
                BuilderMainTab.Controls.Add(ZoomIn);
                BuilderMainTab.Controls.Add(ZoomOut);
                #endregion
                #region Source Code Tab
                SourceCodeTab.Controls.Add(SourceCodeText);
                #endregion
                #region Default Button List Tab
                DefaultButtonListTab.Controls.Add(ButtonListSave);
                DefaultButtonListTab.Controls.Add(ButtonListAdd);
                DefaultButtonListTab.Controls.Add(ButtonListOrganize);
                DefaultButtonListTab.Controls.Add(ButtonListSample);
                DefaultButtonListTab.Controls.Add(ButtonListAlignToGrid);
                DefaultButtonListTab.Controls.Add(ButtonListConnect);
                DefaultButtonListTab.Controls.Add(ButtonListDisconnect1);
                DefaultButtonListTab.Controls.Add(ButtonListDisconnect2);
                #endregion
                #region User Button List Tab
                //UserDefinedListTab.Controls.Add();
                #endregion
            }
            /**/
            /// <summary>
            /// This function is called to pop up a window to confirm an action that could permanently change the user's work
            /// </summary>
            private void Confirm_Action_Fuction()
            {
                MessageBox.Show("Have you saved your work? Press OK to continue or CANCEL to return", "Exiting Application", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            }
            /**/
            /// <summary>
            /// Adds Pages to Tab Controls
            /// </summary>
            private void Add_Pages_To_Tabs()
            {
                OperatorsTabControl.Controls.Add(OperatorTab);
                OperatorsTabControl.Controls.Add(StreamsTab);
                BuilderMainTabControl.Controls.Add(BuilderMainTab);
                BuilderMainTabControl.Controls.Add(SourceCodeTab);
                ButtonListTabControl.Controls.Add(DefaultButtonListTab);
                ButtonListTabControl.Controls.Add(UserDefinedListTab);
            }
            /**/
            /// <summary>
            /// Searches for open space on screen to place point
            /// </summary>
            private void Search_For_Open_Area_BuilderMain(PictureBox p)
            {
                bool again = false;
                foreach (GraphicOperator go in OperatorListGraphic)
                {
                    if (p.Bounds.IntersectsWith(go.PicBox.Bounds))
                    {
                        if (p.Location.X < 850)
                            p.Location = new Point(p.Location.X + 51, p.Location.Y);
                        else
                            p.Location = new Point(25, p.Location.Y + 51);
                        again = true;
                        break;
                    }
                }
                if (again)
                    Search_For_Open_Area_BuilderMain(p);
            }
            /**/
            /// <summary>
            /// Makes an operator and adds it to the list
            /// </summary>
            /// <param name="type"></param>
            /// <param name="pb"></param>
            /// <param name="name"></param>
            public GraphicOperator MakeOperator(string type, PictureBox pb, string name)
            {
                GraphicOperator GO = new GraphicOperator(type, pb, name, OperatorListGraphic.Count);
                pb.Tag = String.Format("{0} Operator:: {1}", type, name);
                GO.PicBoxName = GO.Name;
                OperatorListGraphic.Add(GO);
                BuilderMainTab.Controls.Add(GO.PicBox);
                return GO;
            }
            /**/
            /// <summary>
            /// Finds and returns a unique name among the operators
            /// </summary>
            /// <param name="type"></param>
            /// <param name="num"></param>
            /// <returns></returns>
            public string Search_For_Unique_Name(string type, int num)
            {
                string name = String.Format("{0} {1}", type, num);
                bool again = false;
                foreach (GraphicOperator go in OperatorListGraphic)
                {
                    if (go.Name == name)
                    {
                        name = String.Format("{0} {1}", type, num);
                        num++;
                        again = true;
                        break;
                    }
                }

                if (again)
                    name = Search_For_Unique_Name(type, num);
                return name;
            }
            /**/
            /// <summary>
            /// Checks for a unique name among the operators. Returns true if the name is unique.
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public bool Check_For_Unique_Name(string name)
            {
                foreach (GraphicOperator go in OperatorListGraphic)
                {
                    if (go.Name == name)
                        return false;
                }
                return true;
            }
            /**/
            /// <summary>
			/// Sets a sample query on the builderscreen
			/// </summary>
			public void Set_Sample_Query()
			{
				PictureBox tempInput1 = PicBoxMaker(new Point(100, 250), new Size(50, 50), InputStreamIconPic, "Sample Input Stream1");
				PictureBox tempInput2 = PicBoxMaker(new Point(100, 450), new Size(50, 50), InputStreamIconPic, "Sample Input Stream2");
				PictureBox tempJoin = PicBoxMaker(new Point(450, 350), new Size(50, 50), JoinIconPic, "Sample Join Operator");
				PictureBox tempSelect1 = PicBoxMaker(new Point(300, 250), new Size(50, 50), SelectIconPic, "Sample Select1 Operator");
				PictureBox tempSelect2 = PicBoxMaker(new Point(300, 450), new Size(50, 50), SelectIconPic, "Sample Select2 Operator");
				PictureBox tempProject = PicBoxMaker(new Point(600, 350), new Size(50, 50), ProjectIconPic, "Sample Project Operator");
				PictureBox tempOutput = PicBoxMaker(new Point(750, 350), new Size(50, 50), OutputStreamIconPic, "Sample Output Stream");
				GraphicOperator InStream1 = new GraphicOperator("InputStream", tempInput1, "Input Stream 1");
				GraphicOperator InStream2 = new GraphicOperator("InputStream", tempInput2, "Input Stream 2");
				GraphicOperator Join = new GraphicOperator("Join", tempJoin, "Join");
				GraphicOperator Select1 = new GraphicOperator("Select", tempSelect1, "Select 1");
				GraphicOperator Select2 = new GraphicOperator("Select", tempSelect2, "Select 2");
				GraphicOperator Project = new GraphicOperator("Project", tempProject, "Project");
				GraphicOperator OutStream = new GraphicOperator("OutputStream", tempOutput, "Output Stream");
				tempInput1.Name = InStream1.Name;
				tempInput2.Name = InStream2.Name;
				tempJoin.Name = Join.Name;
				tempSelect1.Name = Select1.Name;
				tempSelect2.Name = Select2.Name;
				tempProject.Name = Project.Name;
				tempOutput.Name = OutStream.Name;
				Join.Predicate = "$1.1 = $2.1";
				Select1.Predicate = "$1.2 > i50";
				Select2.Predicate = "$1.2 > i50";
				Select1.Input_1 = InStream1;
				Select2.Input_1 = InStream2;
				Join.Input_1 = Select1;
				Join.Input_2 = Select2;
				Project.Input_1 = Join;
				OutStream.Input_1 = Project;
				BuilderMainTab.Controls.Add(tempInput1);
				BuilderMainTab.Controls.Add(tempInput2);
				BuilderMainTab.Controls.Add(tempJoin);
				BuilderMainTab.Controls.Add(tempSelect1);
				BuilderMainTab.Controls.Add(tempSelect2);
				BuilderMainTab.Controls.Add(tempProject);
				BuilderMainTab.Controls.Add(tempOutput);
				OperatorListGraphic.Add(InStream1);
				OperatorListGraphic.Add(InStream2);
				OperatorListGraphic.Add(Join);
				OperatorListGraphic.Add(Select1);
				OperatorListGraphic.Add(Select2);
				OperatorListGraphic.Add(Project);
				OperatorListGraphic.Add(OutStream);
			}
            /**/
            /// <summary>
			/// Sets a sample query on the builderscreen
			/// </summary>
			public void Set_Sample_Query2()
			{
				PictureBox tempInput1 = PicBoxMaker(new Point(100, 250), new Size(50, 50), InputStreamIconPic, "Sample Input Stream1");
				PictureBox tempSelect1 = PicBoxMaker(new Point(300, 250), new Size(50, 50), SelectIconPic, "Sample Select1 Operator");
				PictureBox tempOutput = PicBoxMaker(new Point(BuilderMainTabControl.Width -50, 350), new Size(50, 50), OutputStreamIconPic, "Sample Output Stream");
				GraphicOperator InStream1 = new GraphicOperator("InputStream", tempInput1, "Input Stream 1");
				GraphicOperator Select1 = new GraphicOperator("Select", tempSelect1, "Select 1");
				GraphicOperator OutStream = new GraphicOperator("OutputStream", tempOutput, "Output Stream");
				tempInput1.Name = InStream1.Name;
				tempSelect1.Name = Select1.Name;
				tempOutput.Name = OutStream.Name;
				Select1.Predicate = "$1.2 > i50";
				Select1.Input_1 = InStream1;
				OutStream.Input_1 = Select1;
				BuilderMainTab.Controls.Add(tempInput1);
				BuilderMainTab.Controls.Add(tempSelect1);
				BuilderMainTab.Controls.Add(tempOutput);
				OperatorListGraphic.Add(InStream1);
				OperatorListGraphic.Add(Select1);
				OperatorListGraphic.Add(OutStream);
			}
            /**/
            /// <summary>
            /// Connects a picturebox with its connected graphicoperator
            /// </summary>
            /// <param name="pb"></param>
            /// <returns></returns>
            public int Find_OpGraphicList_Index(PictureBox pb)
            {
                for (int i = 0; i < OperatorListGraphic.Count; i++)
                {
                    if (pb.Name == OperatorListGraphic[i].Name)
                        return i;
                }
                return -1;
            }
            /**/
            ///// <summary>
            ///// Sets the values of EditPropertiesTab controls
            ///// </summary>
            ///// <param name="go"></param>
            //public void Set_EditProperties(GraphicOperator go)
            //{
            //    if (go != null)
            //    {
            //        EditPropertiesLabel.Text = String.Format("{0} Operator", go.Type);
            //        EditPropertiesOpName.Text = go.Name;
            //        EditPropertiesPicBox.Image = go.PicBox.Image;
            //        EditPropertiesPredicate.Text = go.Predicate;
            //    }
            //    else
            //    {
            //        EditPropertiesLabel.Text = String.Format("Click an Operator");
            //        EditPropertiesOpName.Text = "Switch operators by clicking";
            //        EditPropertiesPicBox.Image = null;
            //        EditPropertiesPredicate.Text = "Click Edit to make permanent";
            //        QueuePos = -1;
            //    }
            //}
            /// <summary>
            /// Keeps PicBox names up to date
            /// </summary>
            /// <param name="oldname"></param>
            /// <param name="newname"></param>
            public void Change_PicBox_Name(string oldname, string newname)
            {
                foreach (Control c in BuilderMainTab.Controls)
                {
                    if (c is PictureBox)
                    {
                        PictureBox pb = c as PictureBox;
                        if (pb.Name == oldname)
                        {
                            pb.Name = newname;
                            break;
                        }
                    }
                }
            }
            /**/
            /// <summary>
            /// Resets booleans to false
            /// </summary>
            public void Set_Button_Bools_False()
            {
                Connect = false;
                Disconnect1 = false;
                Disconnect2 = false;
				Add = false;
            }
            /**/
            /// <summary>
            /// Sets certain booleans to false to avoid confliction of their actions
            /// </summary>
            /// <param name="type"></param>
            public void Fill_SetProperties(string type)
            {
                SetPropertiesTab.Controls.Clear();
                SetPropertiesTab.Controls.Add(SetPropertiesDone);
                SetPropertiesTab.Controls.Add(SetPropertiesCancel);
                SetPropertiesTab.Controls.Add(SetPropertiesOpName);
                SetPropertiesTab.Controls.Add(SetPropertiesLabel);
                SetPropertiesTab.Controls.Add(SetPropertiesPicBox);
                SetPropertiesTab.Controls.Add(SetPropertiesOpNameLabel);
                SetPropertiesTab.Controls.Add(SetPropertiesNoteLabel);
                SetPropertiesInput1Label.Text = "Predicate"; //Keeps it constant unless changed below
                if (type == "Select")   // Name,Predicate
                {
                    SetPropertiesTab.Controls.Add(SetPropertiesPredicateLabel);
                    SetPropertiesTab.Controls.Add(SetPropertiesPredicate);
                    SetPropertiesNoteLabel.Text = "Select Operator Note";
                    SetPropertiesPredicate.Text = "Predicate";
                }
                else if (type == "Project" || type == "DupElim")     // Name, CheckList
                {
                    SetPropertiesTab.Controls.Add(SetPropertiesInput1Label);
                    SetPropertiesTab.Controls.Add(SetPropertiesCheckedListBox);
                    SetPropertiesCheckedListBox.Items.Clear();
                    if (type == "Project")
                    {
                        SetPropertiesInput1Label.Text = "Project Checked List Box";
                        SetPropertiesNoteLabel.Text = "Project Operator Note";
                        for (int i = 0; i < 5; i++)
                            SetPropertiesCheckedListBox.Items.Add(String.Format("Filler {0}", i));
                    }
                    else
                    {
                        SetPropertiesInput1Label.Text = "DupElim Checked List Box";
                        SetPropertiesNoteLabel.Text = "DupElim Operator Note";
                        for (int i = 0; i < 5; i++)
                            SetPropertiesCheckedListBox.Items.Add(String.Format("Filler {0}", i));
                    }
                }
                else if (type == "Sort" || type == "InputStream")    // Name, List
                {
                    SetPropertiesTab.Controls.Add(SetPropertiesInput2Label);
                    SetPropertiesTab.Controls.Add(SetPropertiesListBox);
                    SetPropertiesListBox.Items.Clear();
                    if (type == "InputStream")
                    {
                        SetPropertiesNoteLabel.Text = "Set Input Stream values";
                        SetPropertiesInput2Label.Text = String.Format("Ports");
                        SetPropertiesNoteLabel.Text = "Input Stream Note";
                        SetPropertiesPicBox.Image = InputStreamIconPic;
                        SetPropertiesListBox.BeginUpdate();
                        for (int i = 0; i < cm.Connections.Count; i++)
                        {
                            SetPropertiesListBox.Items.Add(cm.Connections[i]);
                        }
                        SetPropertiesListBox.EndUpdate();
                    }
                    else // Sort
                    {
                        SetPropertiesInput2Label.Text = "Sort List Box";
                        SetPropertiesNoteLabel.Text = "Sort Operator Note";
                        SetPropertiesListBox.BeginUpdate();
                        for (int i = 0; i < 5; i++)
                        {
                            SetPropertiesListBox.Items.Add(String.Format("Filler {0}", i));
                        }
                        SetPropertiesListBox.EndUpdate();
                    }
                }
                else if (type == "Join")    // Name, Predicate, JoinList
                {
                    SetPropertiesTab.Controls.Add(SetPropertiesPredicateLabel);
                    SetPropertiesTab.Controls.Add(SetPropertiesPredicate);
                    SetPropertiesTab.Controls.Add(SetPropertiesInput3Label);
                    SetPropertiesTab.Controls.Add(SetPropertiesJoinListBox);
                    SetPropertiesPredicate.Text = "Predicate";
                    SetPropertiesInput3Label.Text = "Join List Box";
                    SetPropertiesNoteLabel.Text = "Operator Note";
                    SetPropertiesJoinListBox.Items.Clear();
                    for (int i = 0; i < 5; i++)
                    {
                        SetPropertiesJoinListBox.Items.Add(String.Format("Filler {0}", i));
                    }

                }
                else if (type == "Intersect" || type == "Union" || type == "Difference")    // Name, Note
                {
                    if (type == "Project")
                    {
                        SetPropertiesInput1Label.Text = "Project";
                        SetPropertiesNoteLabel.Text = "Project Operator Note";
                    }
                    else if (type == "Union")
                    {
                        SetPropertiesInput1Label.Text = "DupElim";
                        SetPropertiesNoteLabel.Text = "DupElim Operator Note";
                    }
                    else
                    {
                        SetPropertiesInput1Label.Text = "DupElim";
                        SetPropertiesNoteLabel.Text = "DupElim Operator Note";
                    }
                }
                else if (type == "GroupBy")    // Name, Checklist, List
                {
                    SetPropertiesTab.Controls.Add(SetPropertiesPredicateLabel);
                    SetPropertiesTab.Controls.Add(SetPropertiesGroupByListBox);
                    SetPropertiesTab.Controls.Add(SetPropertiesInput1Label);
                    SetPropertiesTab.Controls.Add(SetPropertiesCheckedListBox);
                    SetPropertiesTab.Controls.Add(SetPropertiesInput2Label);
                    SetPropertiesTab.Controls.Add(SetPropertiesListBox);
                    SetPropertiesPredicateLabel.Text = "GroupBy Types";
                    SetPropertiesInput1Label.Text = "Attributes to Group On: Checked List Box";
                    SetPropertiesInput2Label.Text = "Attribute to type: List Box";
                    SetPropertiesNoteLabel.Text = "Operator Note";
                    SetPropertiesGroupByListBox.Items.Clear();
                    SetPropertiesListBox.Items.Clear();
                    SetPropertiesCheckedListBox.Items.Clear();
                    for(int i=0;i < GroupByListBoxItems.Count;i++)
                    {
                        SetPropertiesGroupByListBox.Items.Add(GroupByListBoxItems[i]);
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        SetPropertiesCheckedListBox.Items.Add(String.Format("Filler {0}", i));
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        SetPropertiesListBox.Items.Add(String.Format("Filler {0}", i));
                    }
                }
                else // Outputstream
                {
                    SetPropertiesNoteLabel.Text = "Set Output Stream values";
                    SetPropertiesPicBox.Image = OutputStreamIconPic;
                }
            }
            /**/
            /// <summary>
            /// Fills Edit Properties Tab Controls with GraphicOperator values
            /// </summary>
            /// <param name="go"></param>
            public void Fill_EditProperties(GraphicOperator go)
            {
                if (go != null)
                {
                    EditPropertiesTab.Controls.Clear();
                    EditPropertiesTab.Controls.Add(EditPropertiesDone);
                    EditPropertiesTab.Controls.Add(EditPropertiesCancel);
                    EditPropertiesTab.Controls.Add(EditPropertiesOpName);
                    EditPropertiesTab.Controls.Add(EditPropertiesLabel);
                    EditPropertiesTab.Controls.Add(EditPropertiesPicBox);
                    EditPropertiesTab.Controls.Add(EditPropertiesOpNameLabel);
                    EditPropertiesTab.Controls.Add(EditPropertiesNoteLabel);
                    EditPropertiesLabel.Text = String.Format("{0} Operator", go.Type);
                    EditPropertiesOpName.Text = go.Name;
                    EditPropertiesPredicateLabel.Text = "Predicate"; //Keeps it constant unless changed below
                    EditPropertiesPicBox.Image = go.PicBox.Image;
                    if (go.Type == "Select")   // Name,Predicate
                    {
                        EditPropertiesTab.Controls.Add(EditPropertiesPredicateLabel);
                        EditPropertiesTab.Controls.Add(EditPropertiesPredicate);
                        EditPropertiesNoteLabel.Text = "Select Operator Note";
                        EditPropertiesPredicate.Text = go.Predicate;
                    }
                    else if (go.Type == "Project" || go.Type == "DupElim")     // Name, CheckList
                    {
                        EditPropertiesTab.Controls.Add(EditPropertiesInput1Label);
                        EditPropertiesTab.Controls.Add(EditPropertiesCheckedListBox);
                        if (go.Type == "Project")
                        {
                            EditPropertiesInput1Label.Text = "Project";
                            EditPropertiesNoteLabel.Text = "Project Operator Note";
                            // Set go attributes to Checked list
                            for (int i = 0; i < 5; i++)
                            {
                                EditPropertiesCheckedListBox.Items.Add(String.Format("Filler {0}", i));
                            }
                        }
                        else
                        {
                            EditPropertiesInput1Label.Text = "DupElim";
                            EditPropertiesNoteLabel.Text = "DupElim Operator Note";
                            // Set go attributes to Checked list
                            for (int i = 0; i < 5; i++)
                            {
                                EditPropertiesCheckedListBox.Items.Add(String.Format("Filler {0}", i));
                            }
                        }
                    }
                    else if (go.Type == "Sort" || go.Type == "InputStream")    // Name, List
                    {
						EditPropertiesTab.Controls.Add(EditPropertiesInput2Label);
						EditPropertiesTab.Controls.Add(EditPropertiesListBox);
						if (go.Type == "InputStream")
                        {
                            EditPropertiesNoteLabel.Text = "Set Input Stream values";
							EditPropertiesInput2Label.Text = String.Format("Current: {0}", go.Value);
							EditPropertiesNoteLabel.Text = "Input Stream Note";
                            EditPropertiesListBox.Items.Clear();
							EditPropertiesListBox.BeginUpdate();
							for(int i=0; i < cm.Connections.Count; i++)
							{
								EditPropertiesListBox.Items.Add(cm.Connections[i]);
							}
							EditPropertiesListBox.EndUpdate();
                        }
						else // Sort
						{
							EditPropertiesInput2Label.Text = "Sort";
							EditPropertiesNoteLabel.Text = "Operator Note";
							// Set go attributes to list
                            for (int i = 0; i < 5; i++)
                            {
                                EditPropertiesListBox.Items.Add(String.Format("Filler {0}", i));
                            }
						}
                    }
                    else if (go.Type == "Join")    // Name, Predicate, JoinList
                    {
                        EditPropertiesTab.Controls.Add(EditPropertiesPredicateLabel);
                        EditPropertiesTab.Controls.Add(EditPropertiesPredicate);
                        EditPropertiesTab.Controls.Add(EditPropertiesInput3Label);
                        EditPropertiesTab.Controls.Add(EditPropertiesJoinListBox);
                        EditPropertiesPredicate.Text = go.Predicate;
                        // Set go attributes to join list
                        EditPropertiesInput3Label.Text = "Join";
                        EditPropertiesNoteLabel.Text = "Operator Note";
                        for (int i = 0; i < 5; i++)
                        {
                            EditPropertiesJoinListBox.Items.Add(String.Format("Filler {0}", i));
                        }
                    }
                    else if (go.Type == "Intersect" || go.Type == "Union" || go.Type == "Difference")    // Name, Note
                    {
                        if (go.Type == "Intersect")
                        {
                            EditPropertiesInput1Label.Text = "Intersect";
                            EditPropertiesNoteLabel.Text = "Intersect Operator Note";
                        }
                        else if (go.Type == "Union")
                        {
                            EditPropertiesInput1Label.Text = "Union";
                            EditPropertiesNoteLabel.Text = "Union Operator Note";
                        }
                        else
                        {
                            EditPropertiesInput1Label.Text = "Difference";
                            EditPropertiesNoteLabel.Text = "Difference Operator Note";
                        }
                    }
					else if (go.Type == "GroupBy")    // Name, Checklist, List
					{
						EditPropertiesTab.Controls.Add(EditPropertiesPredicateLabel);
						EditPropertiesTab.Controls.Add(EditPropertiesGroupByListBox);
						EditPropertiesTab.Controls.Add(EditPropertiesInput1Label);
						EditPropertiesTab.Controls.Add(EditPropertiesCheckedListBox);
						EditPropertiesTab.Controls.Add(EditPropertiesInput2Label);
						EditPropertiesTab.Controls.Add(EditPropertiesListBox);
						// Set go attributes to Checklist and List
						// Group By Types
                        EditPropertiesPredicateLabel.Text = String.Format("GroupBy Type: Current {0}", go.SecondaryType);
                        EditPropertiesGroupByListBox.Items.Clear();
                        for(int i = 0; i < GroupByListBoxItems.Count; i++)
                            EditPropertiesGroupByListBox.Items.Add(GroupByListBoxItems[i]);
                        // Attributes to group on
                        for (int i = 0; i < 5; i++)
                        {
                            EditPropertiesCheckedListBox.Items.Add(String.Format("Filler {0}", i));
                        }
                        for (int i = 0; i < 5; i++)
                        {
                            EditPropertiesListBox.Items.Add(String.Format("Filler {0}", i));
                        }
                        EditPropertiesInput1Label.Text = "Attributes to Group On";
						EditPropertiesInput2Label.Text = "Attribute to type";
						EditPropertiesNoteLabel.Text = "Operator Note";
					}
					else // Outputstream
					{
						EditPropertiesNoteLabel.Text = "Set Output Stream values";
					}
                }
                else
                {
                    EditPropertiesLabel.Text = String.Format("Click an Operator");
                    EditPropertiesOpName.Text = "Switch operators by clicking";
                    EditPropertiesPicBox.Image = null;
                    EditPropertiesPredicate.Text = "Click Edit to make permanent";
                }
            }
            /**/
            public Query InitChild(GraphicOperator input)
			{
				switch (input.Type)
				{
					case "Select":
						return new OpSelect(input.Predicate, InitChild(input.Input_1));
					case "Project":
						return new OpProject(null, input.Attrs, InitChild(input.Input_1));
					case "DupElim":
						return new OpDupElim(InitChild(input.Input_1));
					case "GroupByCount":
						return new OpGroupByCount(input.Attrs, InitChild(input.Input_1));
					case "GroupByAvg":
						return new OpGroupByAvg(input.Attrs, input.Value, InitChild(input.Input_1));
					case "GroupBySum":
						return new OpGroupBySum(input.Attrs, input.Value, InitChild(input.Input_1));
					case "GroupByMax":
						return new OpGroupByMax(input.Attrs, input.Value, InitChild(input.Input_1));
					case "GroupByMin":
						return new OpGroupByMin(input.Attrs, input.Value, InitChild(input.Input_1));
					//case "Sort":
					//    return new OpSort(
					case "Join":
						return new OpJoin(input.Predicate, InitChild(input.Input_1), InitChild(input.Input_2));
					case "Union":
						return new OpUnion(InitChild(input.Input_1), InitChild(input.Input_2));
					case "Intersect":
						return new OpIntersect(InitChild(input.Input_1), InitChild(input.Input_2));
					case "Difference":
						return new OpDifference(InitChild(input.Input_1), InitChild(input.Input_2));
					case "InputStream":
						WhitStream.QueryEngine.QueryOperators.Connection c = cm.LocateCon(input.Value);
						if(c != null)
							c.StartThreads();
						return c;
					case "OutputStream":
						return InitChild(input.Input_1);
					default:
						return null;
				}
			}
            #endregion

            #region Builder Event Definitions
            #region Menu/File
            /**/
            /**/
            /// <summary>
            /// Creates a new query project
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_File_New_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action is assigned to this control yet.";
            }
            /**/
            /// <summary>
            /// Opens and existing query project
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_File_Open_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action is assigned to this control yet.";
            }
            /**/
            /// <summary>
            /// Closes the current query
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_File_Close_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "Closed Query.";
                for (int i = 0; i < OperatorListGraphic.Count; i++)
                    BuilderMainTab.Controls.Remove(OperatorListGraphic[i].PicBox);
                OperatorListGraphic.Clear();
                TrashList.Clear();
            }
            /**/
            /// <summary>
            /// Saves the current query
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_File_Save_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action is assigned to this control yet.";
            }
            /**/
            /// <summary>
            /// Saves the current query under a new name
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_File_SaveAs_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action is assigned to this control yet.";
            }
            #endregion
            #region Menu/Edit
            /**/
            /**/
            /// <summary>
            /// Delete an operator
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_Edit_DeleteOp_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action is assigned to this control yet.";
            }
            /**/
            /// <summary>
            /// Make a duplicate of an operator
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_Edit_Duplicate_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action is assigned to this control yet.";
            }
            /**/
            /// <summary>
            /// Replace one operator with another
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_Edit_Replace_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action is assigned to this control yet.";
            }
            /**/
            /// <summary>
            /// View information about the operator
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_Edit_OpInfo_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action is assigned to this control yet.";
            }
            /// <summary>
            /// View/Edit a note about the query
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_Edit_QueryNote_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action is assigned to this control yet.";
            }
            /// <summary>
            /// Restore operators sent to the trash bin
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_Edit_RestoreBin_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action is assigned to this control yet.";
            }
            /// <summary>
            /// Permanently remove operators sent to the trash bin
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_Edit_EmptyBin_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action is assigned to this control yet.";
            }
            #endregion
            #region Menu/View
            /// <summary>
            /// Zoom in on the building area
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_View_ZoomIn_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action is assigned to this control yet.";
            }
            /// <summary>
            /// Zoom out of the building area
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_View_ZoomOut_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action is assigned to this control yet.";
            }
            /// <summary>
            /// View the XML query plan
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_View_XMLCode_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action is assigned to this control yet.";
            }
            /// <summary>
            /// View the query in full screen
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_View_FullScreen_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action is assigned to this control yet.";
            }
            /// <summary>
            /// View operators sent to the trash bin
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_View_TrashBin_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action is assigned to this control yet.";
            }
            #endregion
            #region Menu/Insert
            /// <summary>
            /// Sets up a sample query
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_Insert_SampleQuery_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action is assigned to this control yet.";
            }
            /// <summary>
            /// Create/Edit a comment on the query
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_Insert_Comment_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action is assigned to this control yet.";
            }
            /// <summary>
            /// Import an XML query
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_Insert_ImportXML_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action is assigned to this control yet.";
            }
            #endregion
            #region Menu/Format
            /// <summary>
            /// Change the color scheme of the application
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void B_Format_ChangeColorScheme_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "No action is assigned to this control yet.";
            }
            #endregion
            #region ClickMenu
            /// <summary>
            /// Edit properties of an operator
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void ClickMenu_Properties_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "Properties Menu Opened";
                OperatorsTabControl.Controls.Remove(SetPropertiesTab);
                OperatorsTabControl.Controls.Remove(EditPropertiesTab);
                OperatorsTabControl.Controls.Add(EditPropertiesTab);
                OperatorsTabControl.SelectedTab = EditPropertiesTab;
                EditPropertiesActive = true;
                QueuePos = Find_OpGraphicList_Index(LastClicked);
                if(QueuePos != -1)
                    Fill_EditProperties(OperatorListGraphic[QueuePos]);
            }
            #endregion
            #region Window Events
            /// <summary>
            /// Occurs when BuilderScreen is activated
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void BuilderScreen_Activated(object sender, EventArgs e)
            {

            }
            /// <summary>
            /// Occurs when BuilderScreen closes
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void BuilderScreen_FormClosing(object sender, EventArgs e)
            {
                //MessageBox.Show("Will ask to save here.");
            }
            /// <summary>
            /// Occurs when BuilderScreen changes size
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void BuilderScreen_Resize(object sender, EventArgs e)
            {
                int BS_H = this.Height;
                int BS_W = this.Width;
                #region Location Reset
                //InstructionBox.Location = new Point(0, 7 * BS_H / 10);
                //OpBox.Location = new Point(0, 0);
                //InstructionButtons.Location = new Point(0, 6 * BS_H / 10);
                //OptionBox.Location = new Point(4 * BS_W / 5, 0);
                //BuilderMain.Location = new Point(BS_W / 5, 0);
                #endregion
                #region Size Reset
                //InstructionBox.Size = new Size(BS_W / 5, 3 * BS_H / 10);
                //OpBox.Size = new Size(BS_W, 6 * BS_H / 10);
                //InstructionButtons.Size = new Size(BS_W / 5, BS_H / 10);
                //OptionBox.Size = new Size(BS_W / 5, BS_H);
                //BuilderMain.Size = new Size(3 * BS_W / 5, BS_H);
                #endregion
            }
            #endregion
            #region Tab Events
            /// <summary>
            /// Saves the current query
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Button_List_Save_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "Sample Query 2 added to screen";
                foreach (GraphicOperator go in OperatorListGraphic)
                { BuilderMainTab.Controls.Remove(go.PicBox); }
                OperatorListGraphic.Clear();
                Set_Sample_Query2();
                MainTabControl.Invalidate(true);
                MainTabControl.Update();
            }
            /// <summary>
            /// Adds the query to the query viewing list
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Button_List_Add_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "Add Button Clicked :: Choose an output stream to construct";
				Set_Button_Bools_False();
				Add = true;
            }
            /// <summary>
            /// Organizes the query
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Button_List_Organize_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "";
                int iy = 0;
                int oy = 0;
                int ix = 0;
                int ox = 0;
                foreach (GraphicOperator go in OperatorListGraphic)
                {
                    if (go.Type == "OutputStream")
                    {
                        go.PicBox.Location = new Point(800-ox, 50 + oy);
                        if (oy < 800)
                            oy += 100;
                        else
                        {
                            oy = 50;
                            ox += 50;
                        }
                    }
                    else if (go.Type == "InputStream")
                    {
                        go.PicBox.Location = new Point(50+ix, 50 + iy);
                        if (iy < 800)
                            iy += 100;
                        else
                        {
                            iy = 50;
                            ix += 50;
                        }
                    }
                }
                for (int i = 0; i < OperatorListGraphic.Count; i++)
                {
                    if (OperatorListGraphic[i].PicBox.Location.X % 50 != 0)
                    {
                        OperatorListGraphic[i].PicBox.Location = new Point(OperatorListGraphic[i].PicBox.Location.X - 1, OperatorListGraphic[i].PicBox.Location.Y);
                        i = -1;
                    }
                    else if (OperatorListGraphic[i].PicBox.Location.Y % 50 != 0)
                    {
                        OperatorListGraphic[i].PicBox.Location = new Point(OperatorListGraphic[i].PicBox.Location.X, OperatorListGraphic[i].PicBox.Location.Y - 1);
                        i = -1;
                    }
                }
                for (int i = 0; i < OperatorListGraphic.Count; i++)
                {
                    for (int j = i + 1; j < OperatorListGraphic.Count; j++)
                    {
                        if (OperatorListGraphic[i].PicBox.Location == OperatorListGraphic[j].PicBox.Location && OperatorListGraphic[j].Type != "InputStream" && OperatorListGraphic[j].Type != "OutputStream")
                        {
                            if (OperatorListGraphic[j].PicBox.Location.X < 800)
                                OperatorListGraphic[j].PicBox.Location = new Point(OperatorListGraphic[j].PicBox.Location.X + 50, OperatorListGraphic[j].PicBox.Location.Y);
                            else
                                OperatorListGraphic[j].PicBox.Location = new Point(0, OperatorListGraphic[j].PicBox.Location.Y + 50);
                            if (OperatorListGraphic[j].PicBox.Location.Y < 800)
                                OperatorListGraphic[j].PicBox.Location = new Point(OperatorListGraphic[j].PicBox.Location.X, 50);
                            i = -1;
                            break;
                        }
                    }
                }
                MainTabControl.Invalidate(true);
                MainTabControl.Update();
            }
            /// <summary>
            /// Sets up a sample query
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Button_List_SampleQuery_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "Sample Query added to screen";
                foreach (GraphicOperator go in OperatorListGraphic)
                { BuilderMainTab.Controls.Remove(go.PicBox); }
                OperatorListGraphic.Clear();
                Set_Sample_Query();
                MainTabControl.Invalidate(true);
                MainTabControl.Update();
            }
            /// <summary>
            /// View the query in full screen
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Button_List_FullScreen_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "Full Screen Button Clicked";
            }
            /// <summary>
            /// Makes a grid appear/disappear
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Button_List_ShowGrid_Click(object sender, EventArgs e)
            {
                // Any out of bounds are brought in
                for (int i = 0; i < OperatorListGraphic.Count; i++)
                {
                    if (OperatorListGraphic[i].PicBox.Location.X > BuilderMainTabControl.Width - 75)
                        OperatorListGraphic[i].PicBox.Location = new Point(BuilderMainTabControl.Width - 75, OperatorListGraphic[i].PicBox.Location.Y);
                    if (OperatorListGraphic[i].PicBox.Location.Y > BuilderMainTabControl.Height - 75)
                        OperatorListGraphic[i].PicBox.Location = new Point(OperatorListGraphic[i].PicBox.Location.X, BuilderMainTabControl.Height -75);
                }
                // Aligned to %50 space
                for (int i = 0; i < OperatorListGraphic.Count; i++)
                {
                    if (OperatorListGraphic[i].PicBox.Location.X % 50 != 0)
                    {
                        OperatorListGraphic[i].PicBox.Location = new Point(OperatorListGraphic[i].PicBox.Location.X - 1, OperatorListGraphic[i].PicBox.Location.Y);
                        i = -1;
                    }
                    else if (OperatorListGraphic[i].PicBox.Location.Y % 50 != 0)
                    {
                        OperatorListGraphic[i].PicBox.Location = new Point(OperatorListGraphic[i].PicBox.Location.X, OperatorListGraphic[i].PicBox.Location.Y - 1);
                        i = -1;
                    }
                }
                // Finds an empty space
                for (int i = 0; i < OperatorListGraphic.Count; i++)
                {
                    for (int j = i + 1; j < OperatorListGraphic.Count; j++)
                    {
                        if (OperatorListGraphic[i].PicBox.Location == OperatorListGraphic[j].PicBox.Location)
                        {
                            if (OperatorListGraphic[j].PicBox.Location.X < BuilderMainTabControl.Width - 100)
                                OperatorListGraphic[j].PicBox.Location = new Point(OperatorListGraphic[j].PicBox.Location.X + 50, OperatorListGraphic[j].PicBox.Location.Y);
                            else
                                OperatorListGraphic[j].PicBox.Location = new Point(0, OperatorListGraphic[j].PicBox.Location.Y + 50);
                            i = -1;
                            break;
                        }
                    }
                }
                MainTabControl.Invalidate(true);
                MainTabControl.Update();
            }
            /// <summary>
            /// Connects two operators
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Button_List_Connect_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "Select the first operator to connect";
                Set_Button_Bools_False();
                Connect = true;
            }
            /// <summary>
            /// Disconnects input 1
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Button_List_Disconnect1_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "Select the first operator to disconnect";
                Set_Button_Bools_False();
                Disconnect1 = true;
            }
            /// <summary>
            /// Disconnects input 2
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Button_List_Disconnect2_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "Select the first operator to disconnect";
                Set_Button_Bools_False();
                Disconnect2 = true;
            }
            /// <summary>
            /// Zoom in on builder screen
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void BuilderMain_ZoomIn_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "Zoom In Button Clicked";
                
            }
            /// <summary>
            /// Zoom out of builder screen
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void BuilderMain_ZoomOut_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "Zoom Out Button Clicked";
                
            }
            /// <summary>
            /// Occurs when mouse is hovering over the zoom in button
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void BuilderMain_ZoomIn_Hover(object sender, EventArgs e)
            {
                InstructionBox.Text = "Click to zoom in";
            }
            /// <summary>
            /// Occurs when the mouse is hovering over the zoom out button
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void BuilderMain_ZoomOut_Hover(object sender, EventArgs e)
            {
                InstructionBox.Text = "Click to zoom out";
            }
            /// <summary>
            /// Occurs when the Done button in the properties tab is clicked
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void SetPropertiesDone_Click(object sender, EventArgs e)
            {
                if (Check_For_Unique_Name(SetPropertiesOpName.Text))
                {
                    InstructionBox.Text = String.Format("Operator Added: {0}", SetPropertiesOpName.Text); ;
                    GraphicOperator GO = MakeOperator(PropertiesType, NewPicBox, SetPropertiesOpName.Text);
                    QueuePos = Find_OpGraphicList_Index(GO.PicBox);
                    OperatorListGraphic[QueuePos].Predicate = SetPropertiesPredicate.Text;
                    // Predicate changes
                    if (OperatorListGraphic[QueuePos].has_Predicate())
                    {
                        OperatorListGraphic[QueuePos].Predicate = SetPropertiesPredicate.Text;
                    }
                    // Instream Connection
                    if (OperatorListGraphic[QueuePos].Type == "InputStream")
                    {
                        foreach (Connection c in cm.Connections)
                        {
                            if (c.ToString() == C)
                                OperatorListGraphic[QueuePos].Value = c.ID;
                        }
                    }
                    // GroupBy
                    if (OperatorListGraphic[QueuePos].Type == "GroupBy")
                    {
                        OperatorListGraphic[QueuePos].SecondaryType = GroupByListBoxSelected;
                        switch (OperatorListGraphic[QueuePos].SecondaryType)
                        {
                            case "GroupByCount":
                                {
                                    OperatorListGraphic[QueuePos].PicBox.Image = GroupByCountIconPic;
                                    break;
                                }
                            case "GroupByMax":
                                {
                                    OperatorListGraphic[QueuePos].PicBox.Image = GroupByMaxIconPic;
                                    break;
                                }
                            case "GroupByMin":
                                {
                                    OperatorListGraphic[QueuePos].PicBox.Image = GroupByMinIconPic;
                                    break;
                                }
                            case "GroupBySum":
                                {
                                    OperatorListGraphic[QueuePos].PicBox.Image = GroupBySumIconPic;
                                    break;
                                }
                            case "GroupByAvg":
                                {
                                    OperatorListGraphic[QueuePos].PicBox.Image = GroupByAvgIconPic;
                                    break;
                                }
                            default:
                                {
                                    OperatorListGraphic[QueuePos].PicBox.Image = GroupByCountIconPic;
                                    break;
                                }
                        }
                    }
                    // Remove Properties Tab
                    OperatorsTabControl.Controls.Clear();
                    OperatorsTabControl.Controls.Add(OperatorTab);
                    OperatorsTabControl.Controls.Add(StreamsTab);
                }
                else
                {
                    InstructionBox.Text = "That name has already been used, please try another";
                }
            }
            /// <summary>
            /// Occurs when the ChangeOp button is clicked
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void EditPropertiesDone_Click(object sender, EventArgs e)
            {
                //Change operator attributes here
                if (QueuePos != -1)
                {   
                    InstructionBox.Text = String.Format("{0}", OperatorListGraphic[QueuePos].Name);
                    string OldName = OperatorListGraphic[QueuePos].Name;
                    // Unique, new
                    if ((Check_For_Unique_Name(EditPropertiesOpName.Text)) && (OldName != EditPropertiesOpName.Text))
                    {
                        OperatorListGraphic[QueuePos].Name = EditPropertiesOpName.Text;
                        InstructionBox.Text = String.Format("{0}: Name Edited to {1}", InstructionBox.Text, EditPropertiesOpName.Text); ;
                    }
                    // Not Unique, new
                    else if (!Check_For_Unique_Name(EditPropertiesOpName.Text) && OldName != EditPropertiesOpName.Text)
                    {
                        InstructionBox.Text = String.Format("{0}: Warning!! Name already in use, unchanged", InstructionBox.Text);
                    } //else name is same
                    Change_PicBox_Name(OldName, OperatorListGraphic[QueuePos].Name);
                    // Predicate changes
                    if (OperatorListGraphic[QueuePos].has_Predicate() && OperatorListGraphic[QueuePos].Predicate != EditPropertiesPredicate.Text)
                    {
                        OperatorListGraphic[QueuePos].Predicate = EditPropertiesPredicate.Text;
                        InstructionBox.Text = String.Format("{0}: Predicate changed", InstructionBox.Text);
                    }
                    // Instream Connection
                    if (OperatorListGraphic[QueuePos].Type == "InputStream")
                    {
                        foreach (Connection c in cm.Connections)
                        {
                            if(c.ToString() == C)
                                OperatorListGraphic[QueuePos].Value = c.ID;
                        }
                    }
                    // GroupBy
                    if (OperatorListGraphic[QueuePos].Type == "GroupBy")
                    {
                        OperatorListGraphic[QueuePos].SecondaryType = GroupByListBoxSelected;
                        switch (OperatorListGraphic[QueuePos].SecondaryType)
                        {
                            case "GroupByCount":
                                {
                                    OperatorListGraphic[QueuePos].PicBox.Image = GroupByCountIconPic;
                                    break;
                                }
                            case "GroupByMax":
                                {
                                    OperatorListGraphic[QueuePos].PicBox.Image = GroupByMaxIconPic;
                                    break;
                                }
                            case "GroupByMin":
                                {
                                    OperatorListGraphic[QueuePos].PicBox.Image = GroupByMinIconPic;
                                    break;
                                }
                            case "GroupBySum":
                                {
                                    OperatorListGraphic[QueuePos].PicBox.Image = GroupBySumIconPic;
                                    break;
                                }
                            case "GroupByAvg":
                                {
                                    OperatorListGraphic[QueuePos].PicBox.Image = GroupByAvgIconPic;
                                    break;
                                }
                            default:
                                {
                                    OperatorListGraphic[QueuePos].PicBox.Image = GroupByCountIconPic;
                                    break;
                                }
                        }
                    }
                }
                Fill_EditProperties(OperatorListGraphic[QueuePos]);
            }
            /// <summary>
            /// Occurs when the Cancel button in the properties tab is clicked
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void SetPropertiesCancel_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "Cancelled";
                OperatorsTabControl.Controls.Clear();
                OperatorsTabControl.Controls.Add(OperatorTab);
                OperatorsTabControl.Controls.Add(StreamsTab);
                Fill_SetProperties(null);
            }
            /// <summary>
            /// Occurs when the Cancel button in the properties tab is clicked
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void EditPropertiesCancel_Click(object sender, EventArgs e)
            {
                InstructionBox.Text = "Cancelled";
                OperatorsTabControl.Controls.Clear();
                OperatorsTabControl.Controls.Add(OperatorTab);
                OperatorsTabControl.Controls.Add(StreamsTab);
                Fill_EditProperties(null);
                EditPropertiesActive = false;
            }
            /// <summary>
            /// Occurs when the Checked List Box in the Set Properties Tab is clicked
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void SetPropertiesCheckedListBox_Click(object sender, EventArgs e)
            {

            }
            /// <summary>
            /// Occurs when the Checked List Box in the Edit Properties Tab is clicked
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void EditPropertiesCheckedListBox_Click(object sender, EventArgs e)
            {

            }
            /// <summary>
            /// Occurs when the Join List Box in the Set Properties Tab is clicked
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void SetPropertiesJoinListBox_Click(object sender, EventArgs e)
            {

            }
            /// <summary>
            /// Occurs when the Join List Box in the Edit Properties Tab is clicked
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void EditPropertiesJoinListBox_Click(object sender, EventArgs e)
            {

            }
            /// <summary>
            /// Occurs when the List Box in the Set Properties Tab is clicked
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void SetPropertiesListBox_Click(object sender, EventArgs e)
            {
                if (SetPropertiesListBox.SelectedItems.Count > 0)
                    InstructionBox.Text = String.Format("SelectedItem: {0}      ", SetPropertiesListBox.SelectedItem.ToString());
                // Connection C is assigned to equivalent connection
                // SelectedItem.ToString is how it will match
                if (SetPropertiesListBox.SelectedItems.Count > 0)
                    C = SetPropertiesListBox.SelectedItem.ToString();
            }
            /// <summary>
            /// Occurs when the List Box in the Edit Properties Tab is clicked
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void EditPropertiesListBox_Click(object sender, EventArgs e)
            {
                if(EditPropertiesListBox.SelectedItems.Count > 0)
                    InstructionBox.Text = String.Format("SelectedItem: {0}      ", EditPropertiesListBox.SelectedItem.ToString());
                // Connection C is assigned to equivalent connection
                // SelectedItem.ToString is how it will match
                if(EditPropertiesListBox.SelectedItems.Count > 0)
                    C = EditPropertiesListBox.SelectedItem.ToString();
            }
            /// <summary>
            /// Occurs when the GroupBy List Box in the Set Properties Tab is clicked
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void SetPropertiesGroupByListBox_Click(object sender, EventArgs e)
            {
                GroupByListBoxSelected = SetPropertiesGroupByListBox.SelectedItem.ToString();
                InstructionBox.Text = String.Format("Selected GroupBy Type: {0}", GroupByListBoxSelected);
                for (int i = 0; i < SetPropertiesGroupByListBox.Items.Count; i++)
                {
                    if (SetPropertiesGroupByListBox.Items[i].ToString() == GroupByListBoxSelected)
                    {
                        SetPropertiesGroupByListBox.Items.RemoveAt(i);
                        SetPropertiesGroupByListBox.Items.Insert(0, GroupByListBoxSelected);
                        break;
                    }
                }
            }
            /// <summary>
            /// Occurs when the GroupBy List Box in the Edit Properties Tab is clicked
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void EditPropertiesGroupByListBox_Click(object sender, EventArgs e)
            {
                GroupByListBoxSelected = EditPropertiesGroupByListBox.SelectedItem.ToString();
                InstructionBox.Text = String.Format("Selected GroupBy Type: {0}", GroupByListBoxSelected);
                for (int i = 0; i < EditPropertiesGroupByListBox.Items.Count; i++)
                {
                    if (EditPropertiesGroupByListBox.Items[i].ToString() == GroupByListBoxSelected)
                    {
                        EditPropertiesGroupByListBox.Items.RemoveAt(i);
                        EditPropertiesGroupByListBox.Items.Insert(0, GroupByListBoxSelected);
                        break;
                    }
                }
            }
            /// <summary>
            /// Occurs when the Checked List Box in the Set Properties Tab is hovered over
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void SetPropertiesCheckedListBox_MouseHover(object sender, EventArgs e)
            {
                SetPropertiesCheckedListBox.BringToFront();
                if(SetPropertiesCheckedListBox.Items.Count > 24)
                    SetPropertiesCheckedListBox.Size = new Size(250, 400);
                else
                    SetPropertiesCheckedListBox.Size = new Size(250, SetPropertiesCheckedListBox.Items.Count * 16);
            }
            /// <summary>
            /// Occurs when the mouse leaves the Checked List Box in the Set Properties Tab 
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void SetPropertiesCheckedListBox_MouseLeave(object sender, EventArgs e)
            {
                SetPropertiesCheckedListBox.Size = new Size(250, 20);
            }
            /// <summary>
            /// Occurs when the Checked List Box in the Edit Properties Tab is hovered over
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void EditPropertiesCheckedListBox_MouseHover(object sender, EventArgs e)
            {
                EditPropertiesCheckedListBox.BringToFront();
                if (EditPropertiesCheckedListBox.Items.Count > 24)
                    EditPropertiesCheckedListBox.Size = new Size(250, 400);
                else
                    EditPropertiesCheckedListBox.Size = new Size(250, EditPropertiesCheckedListBox.Items.Count * 16);
            }
            /// <summary>
            /// Occurs when the mouse leaves the Checked List Box in the Edit Properties Tab
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void EditPropertiesCheckedListBox_MouseLeave(object sender, EventArgs e)
            {
                EditPropertiesCheckedListBox.Size = new Size(250, 20);
            }
            /// <summary>
            /// Occurs when the List Box in the Set Properties Tab is hovered over
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void SetPropertiesListBox_MouseHover(object sender, EventArgs e)
            {
                SetPropertiesListBox.BringToFront();
                if (SetPropertiesListBox.Items.Count > 24)
                    SetPropertiesListBox.Size = new Size(250, 400);
                else
                    SetPropertiesListBox.Size = new Size(250, SetPropertiesListBox.Items.Count * 16);
            }
            /// <summary>
            /// Occurs when the mouse leaves the List Box in the Set Properties Tab
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void SetPropertiesListBox_MouseLeave(object sender, EventArgs e)
            {
                SetPropertiesListBox.Size = new Size(250, 20);
            }
            /// <summary>
            /// Occurs when the List Box in the Edit Properties Tab is hovered over
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void EditPropertiesListBox_MouseHover(object sender, EventArgs e)
            {
                EditPropertiesListBox.BringToFront();
                if (EditPropertiesListBox.Items.Count > 24)
                    EditPropertiesListBox.Size = new Size(250, 400);
                else
                    EditPropertiesListBox.Size = new Size(250, EditPropertiesListBox.Items.Count * 16);
            }
            /// <summary>
            /// Occurs when the mouse leaves the List Box in the Edit Properties Tab
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void EditPropertiesListBox_MouseLeave(object sender, EventArgs e)
            {
                EditPropertiesListBox.Size = new Size(250, 20);
            }
            /// <summary>
            /// Occurs when the GroupBy List Box in the Set Properties Tab is hovered over
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void SetPropertiesGroupByListBox_MouseHover(object sender, EventArgs e)
            {
                SetPropertiesGroupByListBox.BringToFront();
                if (SetPropertiesGroupByListBox.Items.Count > 24)
                    SetPropertiesGroupByListBox.Size = new Size(250, 400);
                else
                    SetPropertiesGroupByListBox.Size = new Size(250, SetPropertiesGroupByListBox.Items.Count * 16);
            }
            /// <summary>
            /// Occurs when the mouse leaves the GroupBy List Box in the Set Properties Tab
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void SetPropertiesGroupByListBox_MouseLeave(object sender, EventArgs e)
            {
                SetPropertiesGroupByListBox.Size = new Size(250, 20);
            }
            /// <summary>
            /// Occurs when the GroupBy List Box in the Edit Properties Tab is hovered over
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void EditPropertiesGroupByListBox_MouseHover(object sender, EventArgs e)
            {
                EditPropertiesGroupByListBox.BringToFront();
                if (EditPropertiesGroupByListBox.Items.Count > 24)
                    EditPropertiesGroupByListBox.Size = new Size(250, 400);
                else
                    EditPropertiesGroupByListBox.Size = new Size(250, EditPropertiesGroupByListBox.Items.Count * 16);
            }
            /// <summary>
            /// Occurs when the mouse leaves the GroupBy List Box in the Edit Properties Tab
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void EditPropertiesGroupByListBox_MouseLeave(object sender, EventArgs e)
            {
                EditPropertiesGroupByListBox.Size = new Size(250, 20);
            }
            /// <summary>
            /// Occurs when the Join List Box in the Edit Properties Tab is hovered over
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void EditPropertiesJoinListBox_MouseHover(object sender, EventArgs e)
            {
                EditPropertiesJoinListBox.BringToFront();
                if (EditPropertiesJoinListBox.Items.Count > 24)
                    EditPropertiesJoinListBox.Size = new Size(250, 400);
                else
                    EditPropertiesJoinListBox.Size = new Size(250, EditPropertiesJoinListBox.Items.Count * 16);
            }
            /// <summary>
            /// Occurs when the mouse leaves the Join List Box in the Edit Properties Tab
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void EditPropertiesJoinListBox_MouseLeave(object sender, EventArgs e)
            {
                EditPropertiesJoinListBox.Size = new Size(250, 20);
            }
            /// <summary>
            /// Occurs when the Join List Box in the Set Properties Tab is hovered over
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void SetPropertiesJoinListBox_MouseHover(object sender, EventArgs e)
            {
                SetPropertiesJoinListBox.BringToFront();
                if (SetPropertiesJoinListBox.Items.Count > 24)
                    SetPropertiesJoinListBox.Size = new Size(250, 400);
                else
                    SetPropertiesJoinListBox.Size = new Size(250, SetPropertiesJoinListBox.Items.Count * 16);
            }
            /// <summary>
            /// Occurs when the mouse leaves the Join List Box in the Set Properties Tab
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void SetPropertiesJoinListBox_MouseLeave(object sender, EventArgs e)
            {
                SetPropertiesJoinListBox.Size = new Size(250, 20);
            }
            #endregion
            #region Operator Events
            #region Click Events
            /// <summary>
            /// Creates a new Select operator
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Select_Click(object sender, EventArgs e)
            {
                Set_Button_Bools_False();
                InstructionBox.Text = "Fill in information for the Select operator";
                NewPicBox = PicBoxMaker(new Point(25, 25), SelectIcon.Size, SelectIconPic, "Tag Comment goes here");
                Search_For_Open_Area_BuilderMain(NewPicBox);
                PropertiesType = "Select";
                SetPropertiesOpName.Text = Search_For_Unique_Name("Select", 1);
                SetPropertiesLabel.Text = "Select Operator";
                SetPropertiesPicBox.Image = SelectIconPic;
                OperatorsTabControl.Controls.Remove(EditPropertiesTab);
                OperatorsTabControl.Controls.Remove(SetPropertiesTab);
                OperatorsTabControl.Controls.Add(SetPropertiesTab);
                Fill_SetProperties(PropertiesType);
                OperatorsTabControl.SelectedTab = SetPropertiesTab;
            }
            /// <summary>
            /// Creates a new Project operator
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Project_Click(object sender, EventArgs e)
            {
                Set_Button_Bools_False();
                InstructionBox.Text = "Fill in information for the Project operator";
                NewPicBox = PicBoxMaker(new Point(25, 25), ProjectIcon.Size, ProjectIconPic, "Tag Comment goes here");
                Search_For_Open_Area_BuilderMain(NewPicBox);
                PropertiesType = "Project";
                SetPropertiesOpName.Text = Search_For_Unique_Name("Project", 1);
                SetPropertiesLabel.Text = "Project Operator";
                SetPropertiesPicBox.Image = ProjectIconPic;
                OperatorsTabControl.Controls.Remove(EditPropertiesTab);
                OperatorsTabControl.Controls.Remove(SetPropertiesTab);
                OperatorsTabControl.Controls.Add(SetPropertiesTab);
                Fill_SetProperties(PropertiesType);
                OperatorsTabControl.SelectedTab = SetPropertiesTab;
            }
            /// <summary>
            /// Creates a new Duplicate Elimination operator
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void DupElim_Click(object sender, EventArgs e)
            {
                Set_Button_Bools_False();
                InstructionBox.Text = "Fill in information for the Duplicate Elimination operatorn";
                NewPicBox = PicBoxMaker(new Point(25, 25), DupElimIcon.Size, DupElimIconPic, "Tag Comment goes here");
                Search_For_Open_Area_BuilderMain(NewPicBox);
                PropertiesType = "DupElim";
                SetPropertiesOpName.Text = Search_For_Unique_Name("DupElim", 1);
                SetPropertiesLabel.Text = "Duplicate Elimination Operator";
                SetPropertiesPicBox.Image = DupElimIconPic;
                OperatorsTabControl.Controls.Remove(EditPropertiesTab);
                OperatorsTabControl.Controls.Remove(SetPropertiesTab);
                OperatorsTabControl.Controls.Add(SetPropertiesTab);
                Fill_SetProperties(PropertiesType);
                OperatorsTabControl.SelectedTab = SetPropertiesTab;
            }
            /// <summary>
            /// Creates a new GroupBy operator
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void GroupBy_Click(object sender, EventArgs e)
            {
                Set_Button_Bools_False();
                InstructionBox.Text = "Fill in information for the GroupBy operator";
                NewPicBox = PicBoxMaker(new Point(25, 25), GroupByIcon.Size, GroupByIconPic, "Tag Comment goes here");
                Search_For_Open_Area_BuilderMain(NewPicBox);
                PropertiesType = "GroupBy";
                SetPropertiesOpName.Text = Search_For_Unique_Name("GroupBy", 1);
                SetPropertiesLabel.Text = "Group By Operator";
                SetPropertiesPicBox.Image = GroupByIconPic;
                OperatorsTabControl.Controls.Remove(EditPropertiesTab);
                OperatorsTabControl.Controls.Remove(SetPropertiesTab);
                OperatorsTabControl.Controls.Add(SetPropertiesTab);
                Fill_SetProperties(PropertiesType);
                OperatorsTabControl.SelectedTab = SetPropertiesTab;
            }
            /// <summary>
            /// Creates a new Sort operator
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Sort_Click(object sender, EventArgs e)
            {
                Set_Button_Bools_False();
                InstructionBox.Text = "Fill in information for the Sort operator";
                NewPicBox = PicBoxMaker(new Point(25, 25), SortIcon.Size, SortIconPic, "Tag Comment goes here");
                Search_For_Open_Area_BuilderMain(NewPicBox);
                PropertiesType = "Sort";
                SetPropertiesOpName.Text = Search_For_Unique_Name("Sort", 1);
                SetPropertiesLabel.Text = "Sort Operator";
                SetPropertiesPicBox.Image = SortIconPic;
                OperatorsTabControl.Controls.Remove(EditPropertiesTab);
                OperatorsTabControl.Controls.Remove(SetPropertiesTab);
                OperatorsTabControl.Controls.Add(SetPropertiesTab);
                Fill_SetProperties(PropertiesType);
                OperatorsTabControl.SelectedTab = SetPropertiesTab;
            }
            /// <summary>
            /// Creates a new Join operator
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Join_Click(object sender, EventArgs e)
            {
                Set_Button_Bools_False();
                InstructionBox.Text = "Fill in information for the Join operator";
                NewPicBox = PicBoxMaker(new Point(25, 25), JoinIcon.Size, JoinIconPic, "Tag Comment goes here");
                Search_For_Open_Area_BuilderMain(NewPicBox);
                PropertiesType = "Join";
                SetPropertiesOpName.Text = Search_For_Unique_Name("Join", 1);
                SetPropertiesLabel.Text = "Join Operator";
                SetPropertiesPicBox.Image = JoinIconPic;
                OperatorsTabControl.Controls.Remove(EditPropertiesTab);
                OperatorsTabControl.Controls.Remove(SetPropertiesTab);
                OperatorsTabControl.Controls.Add(SetPropertiesTab);
                Fill_SetProperties(PropertiesType);
                OperatorsTabControl.SelectedTab = SetPropertiesTab;
            }
            /// <summary>
            /// Creates a new Union operator
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Union_Click(object sender, EventArgs e)
            {
                Set_Button_Bools_False();
                InstructionBox.Text = "Fill in information for the Union operator";
                NewPicBox = PicBoxMaker(new Point(25, 25), UnionIcon.Size, UnionIconPic, "Tag Comment goes here");
                Search_For_Open_Area_BuilderMain(NewPicBox);
                PropertiesType = "Union";
                SetPropertiesOpName.Text = Search_For_Unique_Name("Union", 1);
                SetPropertiesLabel.Text = "Union Operator";
                SetPropertiesPicBox.Image = UnionIconPic;
                OperatorsTabControl.Controls.Remove(EditPropertiesTab);
                OperatorsTabControl.Controls.Remove(SetPropertiesTab);
                OperatorsTabControl.Controls.Add(SetPropertiesTab);
                Fill_SetProperties(PropertiesType);
                OperatorsTabControl.SelectedTab = SetPropertiesTab;
            }
            /// <summary>
            /// Creates a new Intersect operator
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Intersect_Click(object sender, EventArgs e)
            {
                Set_Button_Bools_False();
                InstructionBox.Text = "Fill in information for the Intersect operator";
                NewPicBox = PicBoxMaker(new Point(25, 25), IntersectIcon.Size, IntersectIconPic, "Tag Comment goes here");
                Search_For_Open_Area_BuilderMain(NewPicBox);
                PropertiesType = "Intersect";
                SetPropertiesOpName.Text = Search_For_Unique_Name("Intersect", 1);
                SetPropertiesLabel.Text = "Intersect Operator";
                SetPropertiesPicBox.Image = IntersectIconPic;
                OperatorsTabControl.Controls.Remove(EditPropertiesTab);
                OperatorsTabControl.Controls.Remove(SetPropertiesTab);
                OperatorsTabControl.Controls.Add(SetPropertiesTab);
                Fill_SetProperties(PropertiesType);
                OperatorsTabControl.SelectedTab = SetPropertiesTab;
            }
            /// <summary>
            /// Creates a new Difference operator
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Difference_Click(object sender, EventArgs e)
            {
                Set_Button_Bools_False();
                InstructionBox.Text = "Fill in information for the Difference operator";
                NewPicBox = PicBoxMaker(new Point(25, 25), DifferenceIcon.Size, DifferenceIconPic, "Tag Comment goes here");
                Search_For_Open_Area_BuilderMain(NewPicBox);
                PropertiesType = "Difference";
                SetPropertiesOpName.Text = Search_For_Unique_Name("Difference", 1);
                SetPropertiesLabel.Text = "Difference Operator";
                SetPropertiesPicBox.Image = DifferenceIconPic;
                OperatorsTabControl.Controls.Remove(EditPropertiesTab);
                OperatorsTabControl.Controls.Remove(SetPropertiesTab);
                OperatorsTabControl.Controls.Add(SetPropertiesTab);
                Fill_SetProperties(PropertiesType);
                OperatorsTabControl.SelectedTab = SetPropertiesTab;
            }
            /// <summary>
            /// Creates a new Input Stream
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void InputStream_Click(object sender, EventArgs e)
            {
                Set_Button_Bools_False();
                InstructionBox.Text = "Fill in information for the Input Stream operator";
                NewPicBox = PicBoxMaker(new Point(25, 25), InputStreamIcon.Size, InputStreamIconPic, "Tag Comment goes here");
                Search_For_Open_Area_BuilderMain(NewPicBox);
                PropertiesType = "InputStream";
                SetPropertiesOpName.Text = Search_For_Unique_Name("InputStream", 1);
                SetPropertiesLabel.Text = "Input Stream Operator";
                SetPropertiesPicBox.Image = InputStreamIconPic;
                OperatorsTabControl.Controls.Remove(EditPropertiesTab);
                OperatorsTabControl.Controls.Remove(SetPropertiesTab);
                OperatorsTabControl.Controls.Add(SetPropertiesTab);
                Fill_SetProperties(PropertiesType);
                OperatorsTabControl.SelectedTab = SetPropertiesTab;
            }
            /// <summary>
            /// Creates a new Output Stream
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void OutputStream_Click(object sender, EventArgs e)
            {
                Set_Button_Bools_False();
                InstructionBox.Text = "Fill in information for the Output Stream operator";
                NewPicBox = PicBoxMaker(new Point(25, 25), OutputStreamIcon.Size, OutputStreamIconPic, "Tag Comment goes here");
                Search_For_Open_Area_BuilderMain(NewPicBox);
                PropertiesType = "OutputStream";
                SetPropertiesOpName.Text = Search_For_Unique_Name("OutputStream", 1);
                SetPropertiesLabel.Text = "Output Stream Operator";
                SetPropertiesPicBox.Image = OutputStreamIconPic;
                OperatorsTabControl.Controls.Remove(EditPropertiesTab);
                OperatorsTabControl.Controls.Remove(SetPropertiesTab);
                OperatorsTabControl.Controls.Add(SetPropertiesTab);
                Fill_SetProperties(PropertiesType);
                OperatorsTabControl.SelectedTab = SetPropertiesTab;
            }
            #endregion
            #region Hover Events
            /*Events that relate to the mouse hovering over a control*/
            /// <summary>
            /// Occurs when the mouse hovers over the Select button
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Select_Hover(object sender, EventArgs e)
            {
                InstructionBox.Text = "Select Operator";
            }
            /// <summary>
            /// Occurs when the mouse hovers over the Select button
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Project_Hover(object sender, EventArgs e)
            {
                InstructionBox.Text = "Project Operator";
            }
            /// <summary>
            /// Occurs when the mouse hovers over the Project button
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void DupElim_Hover(object sender, EventArgs e)
            {
                InstructionBox.Text = "Duplicate Elimination Operator";
            }
             
            /// <summary>
            /// Occurs when the mouse hovers over the DupElim button
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void GroupBy_Hover(object sender, EventArgs e)
            {
                InstructionBox.Text = "Group By Operator";
            }
             
            /// <summary>
            /// Occurs when the mouse hovers over the GroupBy button
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Sort_Hover(object sender, EventArgs e)
            {
                InstructionBox.Text = "Sort Operator";
            }
             
            /// <summary>
            /// Occurs when the mouse hovers over the Sort button
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Join_Hover(object sender, EventArgs e)
            {
                InstructionBox.Text = "Join Operator";
            }
             
            /// <summary>
            /// Occurs when the mouse hovers over the Join button
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Union_Hover(object sender, EventArgs e)
            {
                InstructionBox.Text = "Union Operator";
            }
             
            /// <summary>
            /// Occurs when the mouse hovers over the Union button
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Intersect_Hover(object sender, EventArgs e)
            {
                InstructionBox.Text = "Intersect Operator";
            }
             
            /// <summary>
            /// Occurs when the mouse hovers over the Intersect button
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Difference_Hover(object sender, EventArgs e)
            {
                InstructionBox.Text = "Difference Operator";
            }
             
            /// <summary>
            /// Occurs when the mouse hovers over the InputStream button
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void InputStream_Hover(object sender, EventArgs e)
            {
                InstructionBox.Text = "Input Stream";
            }
             
            /// <summary>
            /// Occurs when the mouse hovers over the OutputStream button
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void OutputStream_Hover(object sender, EventArgs e)
            {
                InstructionBox.Text = "Output Stream";
            }
            #endregion
            #region Mouse Events
            /*Events that deal with the mouse*/
             
            /// <summary>
            /// Occurs when the mouse clicks an operator in the build area
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Operator_MouseClick(object sender, MouseEventArgs e)
            {
                if (sender is PictureBox)
                {
                    PictureBox pb = sender as PictureBox;
                    QueuePos = Find_OpGraphicList_Index(pb);
                    if (e.Button == MouseButtons.Left)
                    {
                        #region Connect Input 1
                        if (Connect)
                        {
                            InstructionBox.Text = String.Format("{0}", pb.Tag);
                            if (Origin == null)
                                Origin = pb;
                            else
                            {
                                for (int i = 0; i < OperatorListGraphic.Count; i++)
                                    if (OperatorListGraphic[i].PicBoxName == pb.Name)
                                    {
                                        if (OperatorListGraphic[i].Input_1 == null)
                                        {
                                            for (int j = 0; j < OperatorListGraphic.Count; j++)
                                                if (OperatorListGraphic[j].PicBoxName == Origin.Name)
                                                {
                                                    if (i == j)
                                                        break;
                                                    OperatorListGraphic[i].Input_1 = OperatorListGraphic[j];
                                                    break;
                                                }
                                            break;
                                        }
                                        else if (OperatorListGraphic[i].isBinaryOp())
                                        {
                                            for (int j = 0; j < OperatorListGraphic.Count; j++)
                                                if (OperatorListGraphic[j].PicBoxName == Origin.Name)
                                                {
                                                    if (i == j)
                                                        break;
                                                    OperatorListGraphic[i].Input_2 = OperatorListGraphic[j];
                                                    break;
                                                }
                                            break;
                                        }
                                        else
                                        {
                                            for (int j = 0; j < OperatorListGraphic.Count; j++)
                                                if (OperatorListGraphic[j].PicBoxName == Origin.Name)
                                                {
                                                    if (i == j)
                                                        break;
                                                    OperatorListGraphic[i].Input_1 = OperatorListGraphic[j];
                                                    break;
                                                }
                                            break;
                                        }
                                    }
                                Origin = null;
                                Connect = false;
                            }
                        }
                        #endregion
                        #region Disconnect Input 1
                        else if (Disconnect1)
                        {
                            InstructionBox.Text = "Input 1 Disconnected";
                            int i = Find_OpGraphicList_Index(pb);
                            if (i != -1)
                                OperatorListGraphic[i].Input_1 = null;
                            Disconnect1 = false;
                        }
                        #endregion
                        #region Disconnect Input 2
                        else if (Disconnect2)
                        {
                            InstructionBox.Text = "Input 2 Disconnected";
                            int i = Find_OpGraphicList_Index(pb);
                            if (i != -1)
                                OperatorListGraphic[i].Input_2 = null;
                            Disconnect2 = false;
                        }
                        #endregion
						#region Add
						else if (Add && OperatorListGraphic[QueuePos].Type == "OutputStream")
						{
							Query addedQuery = InitChild(OperatorListGraphic[QueuePos]);
							QueryList.Add(addedQuery);
							AddedQueries.Add(addedQuery);
							Add = false;
						}
						#endregion
						#region Edit Properties Active
						if (EditPropertiesActive)
                        {
                            QueuePos = Find_OpGraphicList_Index(pb);
                            if (QueuePos != -1)
                                Fill_EditProperties(OperatorListGraphic[QueuePos]);
                        }
                        #endregion
                    }
                    if (e.Button == MouseButtons.Right)
                    {
                        MessageBox.Show("Hit Right mouse");
                    }
                    pb.Parent.Invalidate(true);
                    pb.Parent.Update();
                }
            }
             
            /// <summary>
            /// Occurs when the mouse hovers over an operator in the build area
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Operator_MouseHover(object sender, EventArgs e)
            {
                //Go to hashtable to find picturebox
                if (sender is PictureBox)
                {
                    PictureBox pb = sender as PictureBox;
                    InstructionBox.Text = String.Format("{0}", pb.Tag);
                    LastClicked.Name = pb.Name;
                }
            }
             
            /// <summary>
            /// Occurs when a mouse button is held down on an operator in the build area
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Operator_MouseDown(object sender, MouseEventArgs e)
            {
                //Go to hashtable to find picturebox
                if (sender is PictureBox)
                {
                    PictureBox pb = sender as PictureBox;
                    oldX = e.X;
                    oldY = e.Y;
                    Moving = true;
                }
            }
             
            /// <summary>
            /// Occurs when the mouse moves an operator
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Operator_MouseMove(object sender, MouseEventArgs e)
            {
                if (Moving)
                {
                    if (sender is PictureBox)
                    {
                        PictureBox pb = sender as PictureBox;
                        pb.Top = pb.Top + (e.Y - oldY);
                        pb.Left = pb.Left + (e.X - oldX);
                        if (pb.Top > BuilderMainTabControl.Bottom-100)
                            pb.Top = BuilderMainTabControl.Bottom - 100;
                        else if (pb.Top < 10)
                            pb.Top = 10;
                        if (pb.Left > BuilderMainTabControl.Width-60)
                            pb.Left = BuilderMainTabControl.Width - 60;
                        else if (pb.Left < 10)
                            pb.Left = 10;
                    }
                }
            }
             
            /// <summary>
            /// Occurs when a mouse button releases an operator
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Operator_MouseUp(object sender, MouseEventArgs e)
            {
                if (sender is PictureBox)
                {
                    PictureBox pb = sender as PictureBox;
                    Moving = false;
                    //Delete trashed icons here
                }
            }
            #endregion
            #region Paint Events
            /*Events that repaint the screen*/
             
            /// <summary>
            /// Repaints the BuilderMain tab
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="p"></param>
            public void BuilderMainTab_Paint(object sender, PaintEventArgs p)
            {
                Point OffsetOrigin = new Point(), OffsetConnectedTo = new Point();
                Graphics g = p.Graphics;
                if (Moving)
                {
                    for (int i = 0; i < OperatorListGraphic.Count; i++)
                    {
                        if (OperatorListGraphic[i].has_Input1() && OperatorListGraphic[i].has_Input2())
                        {
                            OffsetOrigin = OperatorListGraphic[i].Input_1.PicBox.Location;
                            OffsetOrigin.Offset(50, 25);
                            OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                            OffsetConnectedTo.Offset(0, 15);
                            g.DrawLine(blackpen, OffsetOrigin, OffsetConnectedTo);

                            OffsetOrigin = OperatorListGraphic[i].Input_2.PicBox.Location;
                            OffsetOrigin.Offset(50, 25);
                            OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                            OffsetConnectedTo.Offset(0, 35);
                            g.DrawLine(blackpen, OffsetOrigin, OffsetConnectedTo);
                        }
                        else if (OperatorListGraphic[i].has_Input1() && !OperatorListGraphic[i].has_Input2())
                        {
                            OffsetOrigin = OperatorListGraphic[i].Input_1.PicBox.Location;
                            OffsetOrigin.Offset(50, 25);
                            OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                            OffsetConnectedTo.Offset(0, 25);
                            g.DrawLine(blackpen, OffsetOrigin, OffsetConnectedTo);
                        }
                        else if (OperatorListGraphic[i].has_Input2() && !OperatorListGraphic[i].has_Input1())
                        {
                            OffsetOrigin = OperatorListGraphic[i].Input_2.PicBox.Location;
                            OffsetOrigin.Offset(50, 25);
                            OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                            OffsetConnectedTo.Offset(0, 35);
                            g.DrawLine(blackpen, OffsetOrigin, OffsetConnectedTo);
                        }
                    }
                    BuilderMainTab.Invalidate();
                }
                else
                {
                    for (int i = 0; i < OperatorListGraphic.Count; i++)
                    {
                        if (OperatorListGraphic[i].has_Input1() && OperatorListGraphic[i].has_Input2())
                        {
                            OffsetOrigin = OperatorListGraphic[i].Input_1.PicBox.Location;
                            OffsetOrigin.Offset(50, 25);
                            OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                            OffsetConnectedTo.Offset(0, 15);
                            g.DrawLine(blackpen, OffsetOrigin, OffsetConnectedTo);

                            OffsetOrigin = OperatorListGraphic[i].Input_2.PicBox.Location;
                            OffsetOrigin.Offset(50, 25);
                            OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                            OffsetConnectedTo.Offset(0, 35);
                            g.DrawLine(blackpen, OffsetOrigin, OffsetConnectedTo);
                        }
                        else if (OperatorListGraphic[i].has_Input1() && !OperatorListGraphic[i].has_Input2())
                        {
                            OffsetOrigin = OperatorListGraphic[i].Input_1.PicBox.Location;
                            OffsetOrigin.Offset(50, 25);
                            OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                            OffsetConnectedTo.Offset(0, 25);
                            g.DrawLine(blackpen, OffsetOrigin, OffsetConnectedTo);
                        }
                        else if (OperatorListGraphic[i].has_Input2() && !OperatorListGraphic[i].has_Input1())
                        {
                            OffsetOrigin = OperatorListGraphic[i].Input_2.PicBox.Location;
                            OffsetOrigin.Offset(50, 25);
                            OffsetConnectedTo = OperatorListGraphic[i].PicBox.Location;
                            OffsetConnectedTo.Offset(0, 35);
                            g.DrawLine(blackpen, OffsetOrigin, OffsetConnectedTo);
                        }
                    }
                }
            }
            #endregion
            #endregion
            #endregion
            #endregion

            #region Viewer
            /* This region contains variables used in the Viewer Screen. 
             * Method definitions and Events related to Viewer Screen are also here.*/
            #region Viewer Variables
            /*Variables related to Viewer Screen*/
            /**/
            int QS_W = 1448;    // ViewerTab window width
            int QS_H = 878;     // ViewerTab window height
            /**/
            int BTN_W = 240;    // QS_W * 0.2
            int BTN_H = 30;     // QS_H * 0.0375
            /**/
            Button View_All, View_Active, View_Inactive, Browse;
            /**/
            Button QS_Activate, QS_Deactivate, Confirm_Button, Cancel_Button;
            /**/
            Button Open_Q, Remove_Q, Return_Main;
            /**/
            TextBox Q_Description, Q_Info;
            /**/
            Image ViewerTabBackground = new Bitmap("Background5.jpg");
            /**/
            ListBox QueryListBox;
            /**/
            List<Query> QueryList;
            /**/
            List<Query> AddedQueries;
            /**/
            System.Windows.Forms.Timer timer;
			string Q_info_string = "";
            #endregion

            #region Viewer Method Definitions
            /*Methods related to Viewer Screen*/
             
            /// <summary>
            /// Adds Controls to ViewerTab
            /// </summary>
            private void ViewerTab_LoadControls()
            {
                ViewerTab.Controls.Add(View_All);
                ViewerTab.Controls.Add(View_Active);
                ViewerTab.Controls.Add(View_Inactive);
                ViewerTab.Controls.Add(Browse);
                ViewerTab.Controls.Add(QS_Activate);
                ViewerTab.Controls.Add(QS_Deactivate);
                ViewerTab.Controls.Add(Open_Q);
                ViewerTab.Controls.Add(Remove_Q);
                ViewerTab.Controls.Add(Return_Main);
                ViewerTab.Controls.Add(Q_Description);
                ViewerTab.Controls.Add(Q_Info);
                ViewerTab.Controls.Add(QueryListBox);
                ViewerTab.BackgroundImage = ViewerTabBackground;
                ViewerTab.BackgroundImageLayout = ImageLayout.Stretch;
            }
             
            /// <summary>
            /// Puts queries in the Query list
            /// </summary>
            private void Fill_QueryList()
            {
				foreach (Query q in QueryList)
					QueryListBox.Items.Add(q);
            }
             
            /// <summary>
            /// This function is called to pop up a window to confirm an action that could permanently change the user's work
            /// </summary>
            private bool Viewer_Confirm_Action_Fuction()
            {
                //MessageBox.Show("Have you saved your work? Press OK to continue or CANCEL to return", "Exiting Application", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                return true;
            }
            /**/
            private void UpdateInfo(object sender, EventArgs args)
			{
				Q_Description.Text = Q_info_string;
				Q_Description.SelectionStart = Q_info_string.Length;
				Q_Description.ScrollToCaret();
				Q_Description.Invalidate();
				Q_Description.Update();
			}
             #endregion

            #region Viewer Event Definitions
            /*Events related to Viewer Screen*/
             
            /// <summary>
            /// Sets what happens when View_All_Click is clicked.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void View_All_Click(object sender, EventArgs e)
            {
                Q_Info.Text = "View All Clicked";
				QueryListBox.Items.Clear();
				Fill_QueryList();
            }
             
            /// <summary>
            /// Sets what happens when View_Active_Click is clicked.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void View_Active_Click(object sender, EventArgs e)
            {
                Q_Info.Text = "View Active Clicked";
            }
             
            /// <summary>
            /// Sets what happens when View_Inactive_Click is clicked.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void View_Inactive_Click(object sender, EventArgs e)
            {
                Q_Info.Text = "View Inactive Clicked";
            }
             
            /// <summary>
            /// Sets what happens when Browse_Click is clicked.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Browse_Click(object sender, EventArgs e)
            {
                Q_Info.Text = "Browse Clicked";
            }
             
            /// <summary>
            /// Sets what happens when Activate_Click is clicked.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Activate_Click(object sender, EventArgs e)
            {
				Query ToActivate = null;
				foreach (Query q in QueryList)
				{
					if (q.ToString() == QueryListBox.SelectedItem.ToString())
					{
						ToActivate = q;
					}
				}
				if (ToActivate != null)
				{
					if (sch == null)
					{
						sch = new RoundRobinScheduler();
						IResults[] ires =  sch.Init(false, ToActivate);
						foreach (IResults ir in ires)
						{
							result_handler.Add(new ResultsHandler(ToActivate.ToString(), ir));
							result_handler[result_handler.Count - 1].Results.DataArrived += new DataEvent(ProcessResults);
						}
						sch.Execute();
					}
					else
					{
						IResults[] ires = sch.AddQuery(false, ToActivate);
						foreach (IResults ir in ires)
						{
							result_handler.Add(new ResultsHandler(ToActivate.ToString(), ir));
							result_handler[result_handler.Count - 1].Results.DataArrived += new DataEvent(ProcessResults);
						}
					}
				}
            }
             
            /// <summary>
            /// Sets what happens when Deactivate_Click is clicked.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Deactivate_Click(object sender, EventArgs e)
            {
                Q_Info.Text = "Deactivate Clicked";
                if (Viewer_Confirm_Action_Fuction())
                {
                    Q_Info.Text = "Deactivate Clicked: Action Confirmed";
                }
                else
                {
                    Q_Info.Text = "Deactivate Clicked: Action Cancelled";
                }
            }
             
            /// <summary>
            /// Sets what happens when Confirm_Button is clicked.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Confirm_Button_Click(object sender, EventArgs e)
            {
                Q_Info.Text = "Confirm Button Clicked";
                Controls.Remove(Confirm_Button);
                Controls.Remove(Cancel_Button);
            }
             
            /// <summary>
            /// Sets what happens when Cancel_Button is clicked.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Cancel_Button_Click(object sender, EventArgs e)
            {
                Q_Info.Text = "Cancel Button Clicked";
                Controls.Remove(Confirm_Button);
                Controls.Remove(Cancel_Button);
            }
             
            /// <summary>
            /// Sets what happens when Open_Q_Click is clicked.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Open_Q_Click(object sender, EventArgs e)
            {
                Q_Info.Text = "Open Query Clicked";
            }
             
            /// <summary>
            /// Sets what happens when Remove_Q_Click is clicked.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Remove_Q_Click(object sender, EventArgs e)
            {
                Q_Info.Text = "Remove Query Clicked";
                if (Viewer_Confirm_Action_Fuction())
                {
                    Q_Info.Text = "Remove Query: Action Confirmed";
                }
                else
                {
                    Q_Info.Text = "Remove Query: Action Cancelled";
                }
            }
             
            /// <summary>
            /// Sets what happens when Return_Main_Click is clicked.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Return_Main_Click(object sender, EventArgs e)
            {

            }
             
            /// <summary>
            /// Sets what happens when QueryList is clicked.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void QueryList_Click(object sender, EventArgs e)
            {
                if (sender is ListBox)
                {
                    Q_Info.Text = String.Format("Name: {0}        Index: {1}     Active Status: unknown", QueryListBox.SelectedItem, QueryListBox.SelectedIndex);
                }
            }

             
            /// <summary>
            /// Sets what happens when ViewerTab is resized.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void ViewerTab_Resize(object sender, EventArgs e)
            {
                /*
                //QS_H = ViewerTab.ActiveForm.Height;
                QS_H = this.Height;
                //QS_W = ViewerTab.ActiveForm.Width;
                QS_W = this.Width;
                BTN_H = 375 * QS_H / 10000; //.0375
                BTN_W = 2 * QS_W / 10;  //.2
                #region Control Location Changes
                View_All.Location = new Point(QS_W / 100, QS_H / 100);
                View_Active.Location = new Point((QS_W / 100) + BTN_W + 15, QS_H / 100);
                View_Inactive.Location = new Point((QS_W / 100) + (2 * BTN_W) + 30, QS_H / 100);
                Browse.Location = new Point((QS_W / 100) + (3 * BTN_W) + 45, QS_H / 100);
                QS_Activate.Location = new Point(QS_W / 100, 60 * QS_H / 100);
                QS_Deactivate.Location = new Point((QS_W / 100) + BTN_W + 15, 60 * QS_H / 100);
                Open_Q.Location = new Point(74 * QS_W / 100, 70 * QS_H / 100);
                Remove_Q.Location = new Point(74 * QS_W / 100, 78 * QS_H / 100);
                Return_Main.Location = new Point(74 * QS_W / 100, 86 * QS_H / 100);
                Q_Description.Location = new Point(QS_W / 100, (60 * QS_H / 100) + BTN_H + 15);
                Q_Info.Location = new Point(70 * QS_W / 100, (QS_H / 100) + BTN_H + 15);
                #endregion
                #region Control Size Changes
                Q_Description.Size = new Size(65 * QS_W / 100, 28 * QS_H / 100);
                Q_Info.Size = new Size(28 * QS_W / 100, (3 * QS_H / 5) - 15);
                View_All.Size = new Size(BTN_W, BTN_H);
                View_Active.Size = new Size(BTN_W, BTN_H);
                View_Inactive.Size = new Size(BTN_W, BTN_H);
                Browse.Size = new Size(BTN_W, BTN_H);
                QS_Activate.Size = new Size(BTN_W, BTN_H);
                QS_Deactivate.Size = new Size(BTN_W, BTN_H);
                Open_Q.Size = new Size(BTN_W, BTN_H);
                Remove_Q.Size = new Size(BTN_W, BTN_H);
                Return_Main.Size = new Size(BTN_W, BTN_H);
                #endregion
                 * */
            }

             
            /// <summary>
            /// Sets what happens when Confirm_Button is added
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void Confirm_Button_ControlAdded(object sender, EventArgs e)
            {

            }

             
            /// <summary>
            /// Called when ViewerTab is activated.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void ViewerTab_Activated(object sender, EventArgs e)
            {
            }
             
            /// <summary>
            /// Called when ViewerTab is closed.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public void ViewerTab_FormClosed(object sender, EventArgs e)
            {

            }
            #endregion
            #endregion

            #region Credits
            /* This region contains variables used in the Credits Screen. 
             * Method definitions and Events related to Credits Screen are also here.*/
            #region Credits Variables
            /*Variables related to Credits Screen*/
            /**/
            string[] Members = new string[11];
            Label[] Member_Labels = new Label[16];  // Member names are stored in a list and displayed
            /**/
            PictureBox WhitworthImage;  // Picture of Whitworth Logo
            /**/
            Label Whitstream_Members;   // Title
            /**/
            Label Whitstream_Developer; // Title
            /**/
            Label Whitstream_Dev_Name;  // Title
            /**/
            Image WhitworthLogo = new Bitmap("WhitworthLogo.JPG");  // Bitmap image of Whitworth Logo
            #endregion

            #region Credits Method Definitions
            /*Methods related to Credits Screen*/
             
            /// <summary>
            /// Adds Controls to CreditsScreen
            /// </summary>
            private void CreditsScreen_LoadControls()
            {
                CreditsTab.Controls.Add(WhitworthImage);
                CreditsTab.Controls.Add(Whitstream_Developer);
                CreditsTab.Controls.Add(Whitstream_Dev_Name);
                CreditsTab.Controls.Add(Whitstream_Members);
                foreach (Label l in Member_Labels)
                {
                    CreditsTab.Controls.Add(l);
                }
            }
            #endregion

            #region Credits Event Definitions
            /*Events related to Credits Screen*/
            #endregion
            #endregion

            #region Makers
            /*These are Makers that make it easy to add window form controls such as:
             * 1) Button 2) PictureBox 3) TextBox 4) TrackBar 5) ListBox 6) CheckBox 7) CheckedListBox
             * 8) ComboBox 9) RadioButton 10) Label 11) ToolTip 12) UpDown 13) ErrorProvider 14) TabControl
             * 15) TabPage*/
            #region Tab Makers
            /**/
            private TabControl TabControlMaker(string name_of_tab, Point location, Size size)
            {
                TabControl t = new TabControl();  // Declares new control
                t.Name = name_of_tab;
                t.Location = location;
                t.Size = size;
                t.MaximumSize = size;
                t.BackColor = Common_Background;
                t.ForeColor = Common_Foreground;
                return t;
            }
            /**/
            private TabPage TabPageMaker(string text_on_tp, string name_of_tp)
            {
                TabPage tp = new TabPage();  // Declares new control
                tp.Name = name_of_tp;
                tp.Text = text_on_tp;
                tp.BackColor = Common_Background;
                tp.ForeColor = Common_Foreground;
                return tp;
            }
            #endregion

            #region Button Makers
            /// Button Makers
            /**/
            private Button ButtonMaker()
            {
                Button btn = new Button();  // Declares new control
                btn.Text = "Default";   // Sets text displayed on control
                btn.Name = "Default";   // Sets name of control for specific reference
                btn.Font = new Font("Times New Roman", 12);
                btn.Location = new Point(1, 1);  // Sets the location of control in the window
                btn.Size = new Size(50, 50); // Sets the height and width of control
                btn.BackColor = Common_Background;
                btn.ForeColor = Common_Foreground;
                return btn;
            }
            private Button ButtonMaker(string text_on_btn, string name_of_btn, Point point, Size size, EventHandler e)
            {
                Button btn = new Button();  // Declares new control
                btn.Text = text_on_btn; // Sets text displayed on control
                btn.Name = name_of_btn; // Sets name of control for specific reference
                btn.Font = new Font("Times New Roman", 10);
                btn.Location = point;   // Sets the location of control in the window
                btn.Size = size;    // Sets the height and width of control
                btn.BackColor = Common_Background;
                btn.ForeColor = Common_Foreground;
                btn.Click += e; // Assigns what happens when control is clicked
                return btn;
            }
            private Button ButtonMaker(string text_on_btn, string name_of_btn, Point point, Size size, EventHandler e, int fontsize)
            {
                Button btn = new Button();  // Declares new control
                btn.Text = text_on_btn; // Sets text displayed on control
                btn.Name = name_of_btn; // Sets name of control for specific reference
                btn.Font = new Font("Times New Roman", fontsize);
                btn.Location = point;   // Sets the location of control in the window
                btn.Size = size;    // Sets the height and width of control
                btn.BackColor = Common_Background;
                btn.ForeColor = Common_Foreground;
                btn.Click += e; // Assigns what happens when control is clicked
                return btn;
            }
            private Button ButtonMaker(string name_of_btn, Point point, Size size, EventHandler click, Image image, EventHandler hover)
            {
                Button btn = new Button();  // Declares new control
                btn.Name = name_of_btn; // Sets name of control for specific reference
                btn.Location = point;   // Sets the location of control in the window
                btn.Size = size;    // Sets the height and width of control
                btn.BackgroundImage = image;
                btn.Click += click; // Assigns what happens when control is clicked
                btn.MouseHover += hover;
                btn.BackgroundImageLayout = ImageLayout.Stretch;
                return btn;
            }
            private Button ButtonMaker(Point point, Size size, Image bg_image)
            {
                Button pb = new Button();   // Declares new control
                pb.Location = point;    // Sets the location of control in the window
                pb.Size = size; // Sets the height and width of control
                pb.Image = bg_image;  // Sets location of image
                pb.Enabled = true;  // Sets whether the user can interact with control
                pb.MouseDown += Operator_MouseDown;
                pb.MouseMove += Operator_MouseMove;
                pb.MouseUp += Operator_MouseUp;
                //pb.DragDrop += Operator_DragDrop;
                pb.MouseClick += Operator_MouseClick;
                return pb;
            }
            private Button ButtonMaker(string text_on_btn, string name_of_btn, Point point, Size size, EventHandler e, Color font_color, int font_size)
            {
                Button btn = new Button();  // Declares new control
                btn.Text = text_on_btn; // Sets text displayed on control
                btn.Name = name_of_btn; // Sets name of control for specific reference
                btn.Font = new Font("Times New Roman", font_size);
                btn.Location = point;   // Sets the location of control in the window
                btn.Size = size;    // Sets the height and width of control
                btn.BackColor = Common_Background;
                btn.ForeColor = font_color;
                btn.Click += e; // Assigns what happens when control is clicked
                return btn;
            }
            private Button ButtonMaker(string text_on_btn, string name_of_btn, Point point, Size size, EventHandler e1, EventHandler e2)
            {
                Button btn = new Button();  // Declares new control
                btn.Text = text_on_btn; // Sets text displayed on control
                btn.Name = name_of_btn; // Sets name of control for specific reference
                btn.Font = new Font("Times New Roman", 14);
                btn.Location = point;   // Sets the location of control in the window
                btn.Size = size;    // Sets the height and width of control
                btn.BackColor = Common_Background;
                btn.ForeColor = Common_Foreground;
                btn.Click += e1; // Assigns what happens when control is clicked
                btn.Resize += e2;   // Assigns what happens when control is resized
                return btn;
            }
            #endregion

            #region PictureBox Makers
            /// PictureBox Makers
            /**/
            private PictureBox PicBoxMaker(string name_of_picbox, Point point, EventHandler e)
            {
                PictureBox pb = new PictureBox();   // Declares new control
                pb.Name = name_of_picbox;   // Sets name of control for specific reference
                pb.Location = point;    // Sets the location of control in the window
                pb.Size = new Size(88, 88); // Sets the height and width of control
                pb.Click += e;  // Assigns what happens when control is clicked
                pb.Enabled = true;  // Sets whether the user can interact with control
                return pb;
            }
            private PictureBox PicBoxMaker(string name_of_picbox, Point point, Size size, EventHandler e)
            {
                PictureBox pb = new PictureBox();   // Declares new control
                pb.Name = name_of_picbox;   // Sets name of control for specific reference
                pb.Location = point;    // Sets the location of control in the window
                pb.Size = size; // Sets the height and width of control
                pb.Click += e;  // Assigns what happens when control is clicked
                pb.Enabled = true;  // Sets whether the user can interact with control
                return pb;
            }
            private PictureBox PicBoxMaker(string name_of_picbox, Point point, EventHandler e, Image bg_image)
            {
                PictureBox pb = new PictureBox();   // Declares new control
                pb.Name = name_of_picbox;   // Sets name of control for specific reference
                pb.Location = point;    // Sets the location of control in the window
                pb.Size = new Size(88, 88); // Sets the height and width of control
                pb.Image = bg_image;  // Sets location of image
                pb.Click += e;  // Assigns what happens when control is clicked
                pb.Enabled = true;  // Sets whether the user can interact with control
                return pb;
            }
            private PictureBox PicBoxMaker(string name_of_picbox, Point point, Size size, EventHandler e, Image bg_image)
            {
                PictureBox pb = new PictureBox();   // Declares new control
                pb.Name = name_of_picbox;   // Sets name of control for specific reference
                pb.Location = point;    // Sets the location of control in the window
                pb.Size = size; // Sets the height and width of control
                pb.Image = bg_image;  // Sets location of image
                pb.Click += e;  // Assigns what happens when control is clicked
                pb.Enabled = true;  // Sets whether the user can interact with control
                return pb;
            }
            private PictureBox PicBoxMaker(string name_of_picbox, Point point)
            {
                PictureBox pb = new PictureBox();   // Declares new control
                pb.Name = name_of_picbox;   // Sets name of control for specific reference
                pb.Location = point;    // Sets the location of control in the window
                pb.Size = new Size(88, 88); // Sets the height and width of control
                pb.Enabled = true;  // Sets whether the user can interact with control

                return pb;
            }
            private PictureBox PicBoxMaker(string name_of_picbox, Point point, Size size)
            {
                PictureBox pb = new PictureBox();   // Declares new control
                pb.Name = name_of_picbox;   // Sets name of control for specific reference
                pb.Location = point;    // Sets the location of control in the window
                pb.Size = size; // Sets the height and width of control
                pb.Enabled = true;  // Sets whether the user can interact with control
                return pb;
            }
            private PictureBox PicBoxMaker(string name_of_picbox, Point point, Image bg_image)
            {
                PictureBox pb = new PictureBox();   // Declares new control
                pb.Name = name_of_picbox;   // Sets name of control for specific reference
                pb.Location = point;    // Sets the location of control in the window
                pb.Size = new Size(88, 88); // Sets the height and width of control
                pb.Image = bg_image;  // Sets location of image
                pb.Enabled = true;  // Sets whether the user can interact with control
                return pb;
            }
            private PictureBox PicBoxMaker(string name_of_picbox, Point point, Size size, Image bg_image)
            {
                PictureBox pb = new PictureBox();   // Declares new control
                pb.Name = name_of_picbox;   // Sets name of control for specific reference
                pb.Location = point;    // Sets the location of control in the window
                pb.Size = size; // Sets the height and width of control
                pb.Image = bg_image;  // Sets location of image
                pb.Enabled = true;  // Sets whether the user can interact with control
                pb.SizeMode = PictureBoxSizeMode.StretchImage;
                return pb;
            }
            private PictureBox PicBoxMaker(string name_of_picbox, Point point, Size size, Image bg_image, EventHandler click, EventHandler doubleclick, DragEventHandler dragdrop, GiveFeedbackEventHandler dragdropfeedback, MouseEventHandler mousedown, MouseEventHandler mousemove, MouseEventHandler mouseup)
            {
                PictureBox pb = new PictureBox();   // Declares new control
                pb.Name = name_of_picbox;   // Sets name of control for specific reference
                pb.Location = point;    // Sets the location of control in the window
                pb.Size = size; // Sets the height and width of control
                pb.Image = bg_image;  // Sets location of image
                pb.Enabled = true;  // Sets whether the user can interact with control
                pb.SizeMode = PictureBoxSizeMode.StretchImage;
                pb.Click += click;
                pb.DoubleClick += doubleclick;
                pb.DragDrop += dragdrop;
                pb.GiveFeedback += dragdropfeedback;
                pb.MouseDown += new MouseEventHandler(mousedown);
                pb.MouseMove += new MouseEventHandler(mousemove);
                pb.MouseUp += new MouseEventHandler(mouseup);
                pb.Cursor = Cursors.Hand;
                return pb;
            }
            private PictureBox PicBoxMaker(Point point, Size size, Image bg_image)
            {
                PictureBox pb = new PictureBox();   // Declares new control
                pb.Location = point;    // Sets the location of control in the window
                pb.Size = size; // Sets the height and width of control
                pb.Image = bg_image;  // Sets location of image
                pb.Enabled = true;  // Sets whether the user can interact with control
                pb.SizeMode = PictureBoxSizeMode.StretchImage;
                return pb;
            }
            private PictureBox PicBoxMaker(Point point, Size size, Image bg_image, MouseEventHandler mousedown, MouseEventHandler mousemove, MouseEventHandler mouseup)
            {
                PictureBox pb = new PictureBox();   // Declares new control
                pb.Location = point;    // Sets the location of control in the window
                pb.Size = size; // Sets the height and width of control
                pb.Image = bg_image;  // Sets location of image
                pb.Enabled = true;  // Sets whether the user can interact with control
                pb.SizeMode = PictureBoxSizeMode.StretchImage;
                pb.BorderStyle = BorderStyle.FixedSingle;
                pb.MouseDown += mousedown;
                pb.MouseMove += mousemove;
                pb.MouseUp += mouseup;
                return pb;
            }
            private PictureBox PicBoxMaker(Point point, Size size, Image bg_image, string tag)
            {
                PictureBox pb = new PictureBox();   // Declares new control
                pb.Location = point;    // Sets the location of control in the window
                pb.Size = size; // Sets the height and width of control
                pb.Image = bg_image;  // Sets location of image
                pb.Enabled = true;  // Sets whether the user can interact with control
                pb.SizeMode = PictureBoxSizeMode.StretchImage;
                pb.BorderStyle = BorderStyle.FixedSingle;
                pb.MouseDown += Operator_MouseDown;
                pb.MouseMove += Operator_MouseMove;
                pb.MouseUp += Operator_MouseUp;
                pb.MouseClick += Operator_MouseClick;
                pb.MouseHover += Operator_MouseHover;
                pb.Tag = tag;
                pb.ContextMenuStrip = RightClick_Menu;
                return pb;
            }
            private PictureBox PicBoxMaker(Point point, Size size)
            {
                PictureBox pb = new PictureBox();   // Declares new control
                pb.Location = point;    // Sets the location of control in the window
                pb.Size = size; // Sets the height and width of control
                pb.SizeMode = PictureBoxSizeMode.StretchImage;
                pb.BorderStyle = BorderStyle.FixedSingle;
                return pb;
            }
            #endregion

            #region TextBox Makers
            /// TextBox Makers
            /**/
            private TextBox TextBoxMaker(string text_in_box, string name_of_box, Point point, Size size, bool read_only, bool multiline)
            {
                TextBox tb = new TextBox(); // Declares new control
                tb.Text = text_in_box;  // Sets text displayed on control
                tb.Multiline = multiline;   // Sets whether box allows multiple lines of text
                tb.Name = name_of_box;  // Sets name of control for specific reference
                tb.Location = point;    // Sets the location of control in the window
                tb.Size = size; // Sets the height and width of control
                tb.BackColor = Common_Background;
                tb.ForeColor = Common_Foreground;
                tb.ReadOnly = read_only;    // Sets if the user can interact with control
                tb.TextAlign = HorizontalAlignment.Left;    // Positions text
                tb.AcceptsReturn = true;    // Sets if return starts a new line
                tb.AcceptsTab = true;   // Sets if a tab within box is textual or moves to next control
                tb.ScrollBars = ScrollBars.None;    // Sets if scrollbars are used
                tb.CharacterCasing = CharacterCasing.Normal;    // Sets casing of text
                tb.Font = new Font("Times New Roman", 16);
                if (tb.Multiline)
                    tb.WordWrap = true; // Sets word wrap if multiple lines are allowed
                return tb;
            }
            /// MaskedTextBox Makers
            /**/
            private MaskedTextBox MaskedTextBoxMaker(string text_in_box, string name_of_box, Point point, Size size, bool read_only, bool multiline)
            {
                #region Masked Text Box Tokens
                // Masked Text Boxes have specified sequences of characters (SSN, phone#, etc)
                // 0 Digit 0-9
                // 9 Optional Digit or space
                // L Required Letter(upper or lower)
                // ? Optional Letter(upper or lower)
                // , Represents a thousands separator placeholder
                // : Represents a time placeholder
                // / Represents a date placeholder
                // $ Represents a currency symbol
                #endregion
                MaskedTextBox mtb = new MaskedTextBox();    // Declares new control
                mtb.Text = text_in_box; // Sets text displayed on control
                mtb.Multiline = multiline;  // Sets whether box allows multiple lines of text
                mtb.Name = name_of_box; // Sets name of control for specific reference
                mtb.Location = point;   // Sets the location of control in the window
                mtb.Size = size;    // Sets the height and width of control
                mtb.BackColor = Common_Background;
                mtb.ForeColor = Common_Foreground;
                mtb.ReadOnly = read_only;   // Sets if the user can interact with control
                mtb.TextAlign = HorizontalAlignment.Left;   // Positions text
                mtb.AcceptsTab = true;  // Sets if a tab within box is textual or moves to next control
                if (mtb.Multiline)
                    mtb.WordWrap = true;    // Sets word wrap if multiple lines are allowed
                return mtb;
            }
            #endregion

            #region TrackBar Maker
            /// TrackBar Makers
            /**/
            private TrackBar TrackBarMaker(string name_of_bar, Point point, Size size, int min, int max, int tick_freq, EventHandler e)
            {
                TrackBar trb = new TrackBar();  // Declares new control
                trb.Name = name_of_bar; // Sets name of control for specific reference
                trb.Location = point;   // Sets the location of control in the window
                trb.Size = size;    // Sets the height and width of control
                trb.TickFrequency = tick_freq;  // Sets difference in value between tick marks
                trb.TickStyle = TickStyle.Both; // Determines look of tick marks
                trb.Scroll += e;    // Updates reason for scrolling
                trb.Maximum = max;  // Sets maximum value of trackbar
                trb.Minimum = min;  // Sets minimum value of trackbar
                trb.BackColor = Common_Background;
                trb.Orientation = Orientation.Horizontal;   // Sets horizontal or vertical trackbar
                trb.Value = ((max + min) / 2);  // Sets current value of trackbar
                trb.LargeChange = (trb.Value - min) / 5;  // Sets change in value for clicks directly on trackbar
                trb.Enabled = true; // Sets if the user can interact with control
                return trb;
            }
            private TrackBar TrackBarMaker(string name_of_bar, Point point, Size size, int min, int max, int tick_freq, EventHandler e, bool horizontal)
            {
                TrackBar trb = new TrackBar();  // Declares new control
                trb.Name = name_of_bar; // Sets name of control for specific reference
                trb.Location = point;   // Sets the location of control in the window
                trb.Size = size;    // Sets the height and width of control
                trb.TickFrequency = tick_freq;  // Difference in value between ticks
                trb.TickStyle = TickStyle.Both; // Sets look of tick marks
                trb.Scroll += e;    // Updates reason for scrolling
                trb.Maximum = max;  // Sets maximum value of trackbar
                trb.Minimum = min;  // Sets minimum value of trackbar
                trb.BackColor = Common_Background;
                if (horizontal)
                    trb.Orientation = Orientation.Horizontal;   // Sets horizontal trackbar
                else
                    trb.Orientation = Orientation.Vertical; // Sets vertical trackbar
                trb.Value = ((max + min) / 2);  // Sets current value of trackbar
                trb.LargeChange = (trb.Value - min) / 5;    // Sets change for clicks on trackbar
                trb.Enabled = true; // Sets if the user can interact with control
                return trb;
            }
            #endregion

            #region ListBox Makers
            /// ListBox Makers
            /**/
            private ListBox ListBoxMaker(string text_in_lb, string name_of_lb, Point point, Size size, EventHandler e)
            {
                ListBox lb = new ListBox(); // Declares new control
                lb.BackColor = Common_Background;
                lb.ForeColor = Common_Foreground;
                lb.HorizontalScrollbar = false; // Allows horizontal scrollbar
                lb.Text = text_in_lb;   // Sets text displayed on control
                lb.Name = name_of_lb;   // Sets name of control for specific reference
                lb.Location = point;    // Sets the location of control in the window
                lb.Size = size; // Sets the height and width of control
                lb.Click += e;  // Assigns what happens when control is clicked
                return lb;
            }
            private ListBox ListBoxMaker(string text_in_lb, string name_of_lb, Point point, Size size, EventHandler e, EventHandler mousehover, EventHandler mouseleave)
            {
                ListBox lb = new ListBox(); // Declares new control
                lb.BackColor = Common_Background;
                lb.ForeColor = Common_Foreground;
                lb.HorizontalScrollbar = false; // Allows horizontal scrollbar
                lb.Text = text_in_lb;   // Sets text displayed on control
                lb.Name = name_of_lb;   // Sets name of control for specific reference
                lb.Location = point;    // Sets the location of control in the window
                lb.Size = size; // Sets the height and width of control
                lb.Click += e;  // Assigns what happens when control is clicked
                lb.MouseHover += mousehover;
                lb.MouseLeave += mouseleave;
                return lb;
            }
            private ListBox ListBoxMaker(string text_in_lb, string name_of_lb, Point point, Size size, EventHandler e, bool horizontal_scrollbar)
            {
                ListBox lb = new ListBox(); // Declares new control
                lb.BackColor = Common_Background;
                lb.ForeColor = Common_Foreground;
                lb.HorizontalScrollbar = horizontal_scrollbar;  // Allows horizontal scrollbar
                lb.Text = text_in_lb;   // Sets text displayed on control
                lb.Name = name_of_lb;   // Sets name of control for specific reference
                lb.Location = point;    // Sets the location of control in the window
                lb.Size = size; // Sets the height and width of control
                lb.Click += e;  // Assigns what happens when control is clicked
                return lb;
            }
            #endregion

            #region CheckBox Makers
            /// CheckBox Makers
            /**/
            private CheckBox CheckBoxMaker(string text_by_box, string name_of_box, Point point, Size size, EventHandler e)
            {
                CheckBox cb = new CheckBox();   // Declares new control
                cb.Location = point;    // Sets the location of control in the window
                cb.Size = size; // Sets the height and width of control
                cb.BackColor = Common_Background;
                cb.ForeColor = Common_Foreground;
                cb.CheckState = CheckState.Unchecked;   // Sets if starts out checked
                cb.Text = text_by_box;  // Sets text displayed on control
                cb.Name = name_of_box;  // Sets name of control for specific reference
                cb.Click += e;  // Assigns what happens when control is clicked
                cb.Appearance = Appearance.Normal;  // Can look like button or normal box
                cb.AutoCheck = true;    // If clicked, automatically checks
                cb.CheckAlign = ContentAlignment.MiddleLeft;    // Positions where check is in relation to size
                cb.ThreeState = false;  // Allows for three check states
                return cb;
            }
            private CheckBox CheckBoxMaker(string text_by_box, string name_of_box, Point point, Size size, EventHandler e, bool check)
            {
                CheckBox cb = new CheckBox();   // Declares new control
                cb.Location = point;    // Sets the location of control in the window
                cb.Size = size; // Sets the height and width of control
                cb.BackColor = Common_Background;
                cb.ForeColor = Common_Foreground;
                cb.CheckState = CheckState.Unchecked;   // Sets check state
                cb.Text = text_by_box;  // Sets text displayed on control
                cb.Name = name_of_box;  // Sets name of control for specific reference
                cb.Click += e;  // Assigns what happens when control is clicked
                cb.Checked = check; // Sets if checked or not
                cb.Appearance = Appearance.Normal;  // Can look like button or normal box
                cb.AutoCheck = true;    // If clicked automatically checked
                cb.CheckAlign = ContentAlignment.MiddleLeft;    // Positions where check is in relation to size
                cb.ThreeState = false;  // Allows for three check states
                return cb;
            }
            #endregion

            #region CheckedListBox Makers
            /// CheckedListBox Makers
            /**/
            private CheckedListBox CheckedListBoxMaker(string text_by_box, string name_of_box, Point point, Size size, EventHandler e)
            {
                CheckedListBox clb = new CheckedListBox();  // Declares new control
                clb.Location = point;   // Sets the location of control in the window
                clb.Size = size;    // Sets the height and width of control
                clb.BackColor = Common_Background;
                clb.ForeColor = Common_Foreground;
                clb.CheckOnClick = true;    // Allows click to check box
                clb.HorizontalScrollbar = false;    // Allows horizontal scroll bar
                clb.Text = text_by_box; // Sets text displayed on control
                clb.Name = name_of_box; // Sets name of control for specific reference
                clb.Click += e; // Assigns what happens when control is clicked
                return clb;
            }
            private CheckedListBox CheckedListBoxMaker(string text_by_box, string name_of_box, Point point, Size size, EventHandler e, EventHandler mousehover, EventHandler mouseleave)
            {
                CheckedListBox clb = new CheckedListBox();  // Declares new control
                clb.Location = point;   // Sets the location of control in the window
                clb.Size = size;    // Sets the height and width of control
                clb.BackColor = Common_Background;
                clb.ForeColor = Common_Foreground;
                clb.CheckOnClick = true;    // Allows click to check box
                clb.HorizontalScrollbar = false;    // Allows horizontal scroll bar
                clb.Text = text_by_box; // Sets text displayed on control
                clb.Name = name_of_box; // Sets name of control for specific reference
                clb.Click += e; // Assigns what happens when control is clicked
                clb.MouseHover += mousehover;
                clb.MouseLeave += mouseleave;
                return clb;
            }
            
            #endregion

            #region ComboBox Makers
            /// ComboBox Makers
            // ComboBoxes allow for a list of strings to be displayed as well as allow for more to be added by the user
            /**/
            private ComboBox ComboBoxMaker(string text_by_box, string name_of_box, Point point, Size size, EventHandler e)
            {
                ComboBox cb = new ComboBox();   // Declares new control
                int height = 25;    // Sets control height
                int width = 100;    // Sets control width
                cb.Location = point;    // Sets the location of control in the window
                cb.Size = size; // Sets the height and width of control
                cb.BackColor = Common_Background;
                cb.ForeColor = Common_Foreground;
                cb.Text = text_by_box;  // Sets text displayed on control
                cb.Name = name_of_box;  // Sets name of control for specific reference
                cb.Click += e;  // Assigns what happens when control is clicked
                cb.Height = height; // Sets height
                cb.Width = width;   // Sets width
                cb.DropDownHeight = height * 5; // Sets height of drop down options
                cb.DropDownWidth = width * 2;   // Sets width of drop down options
                cb.Enabled = true;  // Sets if user can interact with control
                cb.Font = new Font("Times New Roman", 12);
                cb.Sorted = true;   // Sets if choices are organized
                return cb;
            }
            private ComboBox ComboBoxMaker(string text_by_box, string name_of_box, Point point, Size size, EventHandler e, int height, int width)
            {
                ComboBox cb = new ComboBox();   // Declares new control
                cb.Location = point;    // Sets the location of control in the window
                cb.Size = size; // Sets the height and width of control
                cb.BackColor = Common_Background;
                cb.ForeColor = Common_Foreground;
                cb.Text = text_by_box;  // Sets text displayed on control
                cb.Name = name_of_box;  // Sets name of control for specific reference
                cb.Click += e;  // Assigns what happens when control is clicked
                cb.Height = height; // Control height
                cb.Width = width;   // COntrol width
                cb.DropDownHeight = height * 5;   // Sets height when drop down option are open
                cb.DropDownWidth = width * 2; // Sets width when drop down options are open
                cb.Enabled = true;  // Sets if user can interact with control
                cb.Font = new Font("Times New Roman", 12);
                cb.Sorted = true;   // Sets if choices are organized
                return cb;
            }
            private ComboBox ComboBoxMaker(string text_by_box, string name_of_box, Point point, Size size, EventHandler e, int height, int width, int drop_down_height_multiplier)
            {
                ComboBox cb = new ComboBox();   // Declares new control
                cb.Location = point;    // Sets the location of control in the window
                cb.Size = size; // Sets the height and width of control
                cb.BackColor = Common_Background;
                cb.ForeColor = Common_Foreground;
                cb.Text = text_by_box;  // Sets text displayed on control
                cb.Name = name_of_box;  // Sets name of control for specific reference
                cb.Click += e;  // Assigns what happens when control is clicked
                cb.Height = height; // Control height
                cb.Width = width;   // Control width
                cb.DropDownHeight = height * drop_down_height_multiplier;   // Sets height of drop down options
                cb.DropDownWidth = width;   // Sets width of drop down options
                cb.Enabled = true;  // Sets if user can interact with control
                cb.Font = new Font("Times New Roman", 12);
                cb.Sorted = true;   // Sets if choices are organized
                return cb;
            }
            private ComboBox ComboBoxMaker(string text_by_box, string name_of_box, Point point, Size size, EventHandler e, int height, int width, int drop_down_height, int drop_down_width)
            {
                ComboBox cb = new ComboBox();   // Declares new control
                cb.Location = point;    // Sets the location of control in the window
                cb.Size = size; // Sets the height and width of control
                cb.BackColor = Common_Background;
                cb.ForeColor = Common_Foreground;
                cb.Text = text_by_box;  // Sets text displayed on control
                cb.Name = name_of_box;  // Sets name of control for specific reference
                cb.Click += e;  // Assigns what happens when control is clicked
                cb.Height = height; // Control height
                cb.Width = width;   // Control width
                cb.DropDownHeight = drop_down_height;   // Height when drop down options show
                cb.DropDownWidth = drop_down_width;     // Width when drop down options show
                cb.Enabled = true;  // Sets if user can interact with control
                cb.Font = new Font("Times New Roman", 12);
                cb.Sorted = true;   // Sets if choices are organized
                return cb;
            }
            #endregion

            #region RadioButton Makers
            /// RadioButton Makers
            /**/
            private RadioButton RadioButtonMaker(string text_by_rb, string name_of_box, Point point, Size size, EventHandler e, bool check)
            {
                RadioButton rb = new RadioButton(); // Declares new control
                rb.Location = point;    // Sets the location of control in the window
                rb.Size = size; // Sets the height and width of control
                rb.BackColor = Common_Background;
                rb.ForeColor = Common_Foreground;
                rb.Text = text_by_rb;   // Sets text displayed on control
                rb.Name = name_of_box;  // Sets name of control for specific reference
                rb.Click += e;  // Assigns what happens when control is clicked
                rb.Checked = check; // Sets check state
                rb.Appearance = Appearance.Normal;  // Can appear as button or normal
                rb.AutoCheck = true;    // Automatically checked when clicked
                rb.CheckAlign = ContentAlignment.MiddleLeft;    // Sets position in relation to size
                return rb;
            }
            private RadioButton RadioButtonMaker(string text_by_rb, string name_of_box, Point point, Size size, EventHandler e)
            {
                RadioButton rb = new RadioButton(); // Declares new control
                rb.Location = point;    // Sets the location of control in the window
                rb.Size = size; // Sets the height and width of control
                rb.BackColor = Common_Background;
                rb.ForeColor = Common_Foreground;
                rb.Text = text_by_rb;   // Sets text displayed on control
                rb.Name = name_of_box;  // Sets name of control for specific reference
                rb.Click += e;  // Assigns what happens when control is clicked
                rb.Checked = false; // Sets check state
                rb.Appearance = Appearance.Normal;  // Can appear as button or normal
                rb.AutoCheck = true;    // Automatically checked when clicked
                rb.CheckAlign = ContentAlignment.MiddleLeft; // Sets position in relation to size
                return rb;
            }
            #endregion

            #region Label Makers
            /// Label Makers
            /**/
            private Label LabelMaker(string name_of_label, string text_on_label, Point point, Size size, EventHandler e)
            {
                Label l = new Label();  // Declares new control
                l.Name = name_of_label; // Sets name of control for specific reference
                l.Text = text_on_label; // Sets text displayed on control
                l.Location = point; // Sets the location of control in the window
                l.Size = size;  // Sets the height and width of control
                l.Click += e;   // Assigns what happens when control is clicked
                l.BackColor = Common_Background;
                l.ForeColor = Common_Foreground;
                return l;
            }
            private Label LabelMaker(string name_of_label, string text_on_label, Point point, Size size)
            {
                Label l = new Label();  // Declares new control
                l.Name = name_of_label; // Sets name of control for specific reference
                l.Text = text_on_label; // Sets text displayed on control
                l.Location = point; // Sets the location of control in the window
                l.Size = size;  // Sets the height and width of control
                l.BackColor = Common_Background;
                l.ForeColor = Common_Foreground;
                l.TextAlign = ContentAlignment.MiddleLeft;
                return l;
            }
            private Label LabelMaker(string name_of_label, string text_on_label, Point point, Size size, int font_size)
            {
                Label l = new Label();  // Declares new control
                l.Name = name_of_label; // Sets name of control for specific reference
                l.Text = text_on_label; // Sets text displayed on control
                l.Location = point; // Sets the location of control in the window
                l.Size = size;  // Sets the height and width of control
                l.BackColor = Common_Background;
                l.ForeColor = Common_Foreground;
                l.Font = new Font("Times New Roman", font_size);
                l.TextAlign = ContentAlignment.MiddleCenter;
                return l;
            }
            #endregion

            #region ToolTip Makers
            ///ToolTip Makers
            // Icon can be Error, Warning, Info, or None
            /**/
            private ToolTip ToolTipMaker(string title, object tagged)
            {
                ToolTip tt = new ToolTip(); // Declares new control
                tt.BackColor = Common_Background;
                tt.ForeColor = Common_Foreground;
                tt.Tag = tagged;    // Selects object to set tip to
                tt.Active = true;   // Sets if tip is on
                tt.AutomaticDelay = 100;    // Sets delay before appearing
                tt.InitialDelay = 100;  // Sets delay after startup before able to appear
                tt.IsBalloon = true; // Appears like a balloon
                tt.ShowAlways = false;  // Sets if always appears
                tt.ToolTipTitle = title;    // Sets title of tip
                tt.UseAnimation = false;    // Appears with animation
                tt.UseFading = true;    // Fades when finished
                tt.ToolTipIcon = ToolTipIcon.Info;  // Sets appearance of icon
                return tt;
            }
            #endregion

            #region UpDown Makers
            /// UpDown Makers
            // Domain is text based
            /**/
            private DomainUpDown TextualUpDownMaker(string name_of_updown, string text_in_updown, Point point, Size size)
            {
                int height = 25;    // Sets window height
                int width = 100;    // Sets window width
                DomainUpDown ud = new DomainUpDown();   // Declares new control
                ud.BackColor = Common_Background;
                ud.ForeColor = Common_Foreground;
                ud.Enabled = true;  // Sets if user can interact with control 
                ud.Height = height; // Window height
                ud.Width = width;   // WIndow width
                ud.InterceptArrowKeys = false; // Arrow keys change value
                ud.Location = point;    // Sets the location of control in the window
                ud.Size = size; // Sets the height and width of control
                ud.Name = name_of_updown;   // Sets name of control for specific reference
                ud.ReadOnly = false;    // Sets if user can interact with control without the updown buttons
                ud.Text = text_in_updown;   // Sets text displayed on control
                ud.TextAlign = HorizontalAlignment.Left;    // Positioning of text
                ud.UpDownAlign = LeftRightAlignment.Left;   // Positioning of buttons
                ud.Sorted = true;   // Determines if list is displayed alphabetically
                ud.Wrap = true; // Determines if text wraps to a new line
                return ud;
            }
            private DomainUpDown TextualUpDownMaker(string name_of_updown, string text_in_updown, Point point, Size size, int height, int width)
            {
                DomainUpDown ud = new DomainUpDown();   // Declares new control
                ud.BackColor = Common_Background;
                ud.ForeColor = Common_Foreground;
                ud.Enabled = true;  // Sets if user can interact with control
                ud.Height = height; // Window height
                ud.Width = width;   // Window width
                ud.InterceptArrowKeys = false; // Arrow keys change value
                ud.Location = point;    // Sets the location of control in the window
                ud.Size = size; // Sets the height and width of control
                ud.Name = name_of_updown;   // Sets name of control for specific reference
                ud.ReadOnly = false;    // Sets if user can interact with control without the updown buttons
                ud.Text = text_in_updown;   // Sets text displayed on control
                ud.TextAlign = HorizontalAlignment.Left; //Positioning of text
                ud.UpDownAlign = LeftRightAlignment.Left; //Positioning of up/down controls
                ud.Sorted = true;   // Determines if list is displayed alphabetically
                ud.Wrap = true; // Determines if text wraps to a new line
                return ud;
            }
            private DomainUpDown TextualUpDownMaker(string name_of_updown, string text_in_updown, Point point, Size size, int height, int width, bool sorted, bool wrap)
            {
                DomainUpDown ud = new DomainUpDown();   // Declares new control
                ud.BackColor = Common_Background;
                ud.ForeColor = Common_Foreground;
                ud.Enabled = true;  // Sets if user can interact with control
                ud.Height = height; // Window height
                ud.Width = width;   // Window width
                ud.InterceptArrowKeys = false; // Arrow keys change value
                ud.Location = point;    // Sets the location of control in the window
                ud.Size = size; // Sets the height and width of control
                ud.Name = name_of_updown;   // Sets name of control for specific reference
                ud.ReadOnly = false;    // Sets if user can interact with control without the updown buttons
                ud.Text = text_in_updown;   // Sets text displayed on control
                ud.TextAlign = HorizontalAlignment.Left; //Positioning of text
                ud.UpDownAlign = LeftRightAlignment.Left; //Positioning of up/down controls
                ud.Sorted = sorted; // Determines if list is displayed alphabetically
                ud.Wrap = wrap; // Determines if text wraps to a new line
                return ud;
            }
            // Numeric is text based
            /**/
            private NumericUpDown NumericUpDownMaker(string name_of_updown, string text_in_updown, Point point, Size size, int height, int width, int min, int max, int decimal_places, int increment)
            {
                NumericUpDown ud = new NumericUpDown(); // Declares new control
                ud.BackColor = Common_Background;
                ud.ForeColor = Common_Foreground;
                ud.Enabled = true;  // Sets if user can interact with control
                ud.Height = height; // Window height
                ud.Width = width;   // Window width
                ud.InterceptArrowKeys = false; // Arrow keys change value
                ud.Location = point;    // Sets the location of control in the window
                ud.Size = size; // Sets the height and width of control
                ud.Name = name_of_updown;   // Sets name of control for specific reference
                ud.ReadOnly = false;    // Sets if user can interact with control without the updown buttons
                ud.Text = text_in_updown;   // Sets text displayed on control
                ud.TextAlign = HorizontalAlignment.Left; //Positioning of text
                ud.UpDownAlign = LeftRightAlignment.Left; //Positioning of up/down controls
                ud.Minimum = min;   // Sets the minimum value the trackbar can reach
                ud.Maximum = max;   // Sets the maximum value the trackbar can reach
                ud.Value = (max + min) / 2; // Sets the value the trackbar currently has
                ud.Increment = increment;   // Sets the increment between ticks on bar
                ud.DecimalPlaces = decimal_places;  // Sets how many decimal places are displayed
                ud.ThousandsSeparator = false;  // Determines if numbers show comma or not in the thousands place
                ud.Hexadecimal = false; // Allows number to be displayed in hexadecimal
                return ud;
            }
            private NumericUpDown NumericUpDownMaker(string name_of_updown, string text_in_updown, Point point, Size size, int height, int width, int min, int max, int decimal_places, int increment, bool read_only)
            {
                NumericUpDown ud = new NumericUpDown(); // Declares new control
                ud.BackColor = Common_Background;
                ud.ForeColor = Common_Foreground;
                ud.Enabled = true;  // Sets if user can interact with control
                ud.Height = height; // Window height
                ud.Width = width;   // Window width
                ud.InterceptArrowKeys = false; // Arrow keys change value
                ud.Location = point;    // Sets the location of control in the window
                ud.Size = size; // Sets the height and width of control
                ud.Name = name_of_updown;   // Sets name of control for specific reference
                ud.ReadOnly = read_only;    // Sets if user can interact with control without the updown buttons
                ud.Text = text_in_updown;   // Sets text displayed on control
                ud.TextAlign = HorizontalAlignment.Left; //Positioning of text
                ud.UpDownAlign = LeftRightAlignment.Left; //Positioning of up/down controls
                ud.Minimum = min;   // Sets the minimum value the trackbar can reach
                ud.Maximum = max;   // Sets the maximum value the trackbar can reach
                ud.Value = (max + min) / 2; // Sets the value the trackbar currently has
                ud.Increment = increment;   // Sets the increment between ticks on bar
                ud.DecimalPlaces = decimal_places;  // Sets how many decimal places are displayed
                ud.ThousandsSeparator = false;  // Determines if numbers show comma or not in the thousands place
                ud.Hexadecimal = false; // Allows number to be displayed in hexadecimal
                return ud;
            }
            private NumericUpDown NumericUpDownMaker(string name_of_updown, string text_in_updown, Point point, Size size, int current_value, int min, int max, int decimal_places, int increment, bool read_only, bool thousands_separator, bool arrowkeys)
            {
                NumericUpDown ud = new NumericUpDown(); // Declares new control
                ud.BackColor = Common_Background;
                ud.ForeColor = Common_Foreground;
                ud.Enabled = true;  // Sets if user can interact with control
                ud.InterceptArrowKeys = arrowkeys; // Arrow keys change value
                ud.Location = point;    // Sets the location of control in the window
                ud.Size = size; // Sets the height and width of control
                ud.Name = name_of_updown;   // Sets name of control for specific reference
                ud.ReadOnly = read_only;    // Sets if user can interact with control without the updown buttons
                ud.Text = text_in_updown;   // Sets text displayed on control
                ud.TextAlign = HorizontalAlignment.Left; //Positioning of text
                ud.UpDownAlign = LeftRightAlignment.Left; //Positioning of up/down controls
                ud.Minimum = min;   // Sets the minimum value the trackbar can reach
                ud.Maximum = max;   // Sets the maximum value the trackbar can reach
                ud.Value = current_value;   // Sets the value the trackbar currently has
                ud.Increment = increment;   // Sets the increment between ticks on bar
                ud.DecimalPlaces = decimal_places;  // Sets how many decimal places are displayed
                ud.ThousandsSeparator = thousands_separator;    // Determines if numbers show comma or not in the thousands place
                ud.Hexadecimal = false; // Allows number to be displayed in hexadecimal
                return ud;
            }
            private NumericUpDown NumericUpDownMaker(string name_of_updown, string text_in_updown, Point point, Size size, int height, int width, int current_value, int min, int max, int decimal_places, int increment, bool read_only, bool thousands_separator, bool arrowkeys)
            {
                NumericUpDown ud = new NumericUpDown(); // Declares new control
                ud.BackColor = Common_Background;
                ud.ForeColor = Common_Foreground;
                ud.Enabled = true;  // Allows user to interact with control
                ud.Height = height; // Window hieght
                ud.Width = width;   // Window width
                ud.InterceptArrowKeys = arrowkeys; // Arrow keys change value
                ud.Location = point;    // Sets the location of control in the window
                ud.Size = size; // Sets the height and width of control
                ud.Name = name_of_updown;   // Sets name of control for specific reference
                ud.ReadOnly = read_only;    // Sets if user can interact with control without the updown buttons
                ud.Text = text_in_updown;   // Sets text displayed on control
                ud.TextAlign = HorizontalAlignment.Left; //Positioning of text
                ud.UpDownAlign = LeftRightAlignment.Left; //Positioning of up/down controls
                ud.Minimum = min;   // Sets the minimum value the trackbar can reach
                ud.Maximum = max;   // Sets the maximum value the trackbar can reach
                ud.Value = current_value;   // Sets the value the trackbar currently has
                ud.Increment = increment;   // Sets the increment between ticks on bar
                ud.DecimalPlaces = decimal_places;  // Sets how many decimal places are displayed
                ud.ThousandsSeparator = thousands_separator;    // Determines if numbers show comma or not in the thousands place
                ud.Hexadecimal = false; // Allows number to be displayed in hexadecimal
                return ud;
            }
            #endregion

            #region ErrorProvider Maker
            /// ErrorProvider Makers
            // Error can blink Always, Never, or BlinkIfDifferentError
            /**/
            private ErrorProvider ErrorProviderMaker(object monitored_object, string error_message)
            {
                ErrorProvider ep = new ErrorProvider(); // Declares new control
                ep.BlinkRate = 30;  // Sets rate the icon will blink
                ep.BlinkStyle = ErrorBlinkStyle.BlinkIfDifferentError;  // Sets reason for blinking
                ep.DataSource = monitored_object;   // Sets control to object
                ep.DataMember = error_message;  // The message to be displayed
                return ep;
            }
            private ErrorProvider ErrorProviderMaker(object monitored_object, string error_message, int blink_rate)
            {
                ErrorProvider ep = new ErrorProvider(); // Declares new control
                ep.BlinkRate = blink_rate;  // Sets rate the icon will blink
                ep.BlinkStyle = ErrorBlinkStyle.BlinkIfDifferentError;  // Sets reason for blinking
                ep.DataSource = monitored_object;   // Sets control to object
                ep.DataMember = error_message;  // The message to be displayed
                return ep;
            }
            #endregion

            #region ToolStrip Makers
            /**/
            private ToolStripMenuItem ToolStripMenuItemMaker(string text_on_item, string name_of_item)
            {
                ToolStripMenuItem t = new ToolStripMenuItem();
                t.Alignment = ToolStripItemAlignment.Left;
                t.Anchor = AnchorStyles.Top;
                t.BackColor = Common_Background;
                t.ForeColor = Common_Foreground;
                t.Dock = DockStyle.Top;
                t.DoubleClickEnabled = false;
                t.Enabled = true;
                t.Name = name_of_item;
                t.Text = text_on_item;
                t.TextAlign = ContentAlignment.MiddleCenter;
                t.TextDirection = ToolStripTextDirection.Horizontal;
                t.ShowShortcutKeys = true;
                t.Visible = true;
                return t;
            }
            private ToolStripMenuItem ToolStripMenuItemMaker(string text_on_item, string name_of_item, EventHandler click)
            {
                ToolStripMenuItem t = new ToolStripMenuItem();
                t.Alignment = ToolStripItemAlignment.Left;
                t.Anchor = AnchorStyles.Top;
                t.BackColor = Common_Background;
                t.ForeColor = Common_Foreground;
                t.Click += new EventHandler(click);
                t.Dock = DockStyle.Top;
                t.DoubleClickEnabled = false;
                t.Enabled = true;
                t.Name = name_of_item;
                t.Text = text_on_item;
                t.TextAlign = ContentAlignment.MiddleCenter;
                t.TextDirection = ToolStripTextDirection.Horizontal;
                t.ShowShortcutKeys = true;
                t.Visible = true;
                return t;
            }
            private ToolStripMenuItem ToolStripMenuItemMaker(string text_on_item, string name_of_item, EventHandler click, EventHandler doubleclick)
            {
                ToolStripMenuItem t = new ToolStripMenuItem(); // Declares new control
                t.Alignment = ToolStripItemAlignment.Left;
                t.Anchor = AnchorStyles.Top;
                t.BackColor = Common_Background;
                t.ForeColor = Common_Foreground;
                t.Click += new EventHandler(click);
                t.DoubleClick += new EventHandler(doubleclick);
                t.Dock = DockStyle.Top;
                t.DoubleClickEnabled = true;
                t.Enabled = true;
                t.Name = name_of_item;
                t.Text = text_on_item;
                t.TextAlign = ContentAlignment.MiddleCenter;
                t.TextDirection = ToolStripTextDirection.Horizontal;
                t.ShowShortcutKeys = true;
                t.Visible = true;
                return t;
            }
            /**/
            private ToolStripDropDownMenu ToolStripDropDownMenuMaker(string text_on_item, string name_of_item, EventHandler click)
            {
                ToolStripDropDownMenu t = new ToolStripDropDownMenu();
                t.Anchor = AnchorStyles.Top;
                t.BackColor = Common_Background;
                t.ForeColor = Common_Foreground;
                t.Click += new EventHandler(click);
                t.Dock = DockStyle.Top;
                t.Enabled = true;
                t.Name = name_of_item;
                t.Text = text_on_item;
                t.TextDirection = ToolStripTextDirection.Horizontal;
                t.Visible = true;
                return t;
            }
            #endregion

            #region Grouping
            /* GroupBox collects buttons together and allows for certain changes to happen within its area.
            * Panels are essentially GroupBoxes with the ability to scroll.*/
            #endregion

            #region Rectangle Maker
            /**/
            private Rectangle RectangleMaker(Point point, Size size)
            {
                Rectangle r = new Rectangle();
                r.Location = point;
                r.Size = size;
                return r;
            }
            #endregion
            #endregion

			#region Result Handler

			/// <summary>
			/// Class to handle that come from an executing query
			/// </summary>
			private class ResultsHandler
			{
				IResults res;
				string name;

				/// <summary>
				/// Constructs a new Result Handler
				/// </summary>
				/// <param name="n">Name of the Handler</param>
				/// <param name="r">IResult from the added query</param>
				public ResultsHandler(string n, IResults r)
				{
					res = r;
					name = n;
				}

				/// <summary>
				/// Gets whether the query has ended or not
				/// </summary>
				public bool EOF { get { return res.EndQuery; } }

				/// <summary>
				/// Gets the name of the Result Handler
				/// </summary>
				public string Name { get { return name; } }

				/// <summary>
				/// Gets the IResult for the Query
				/// </summary>
				public IResults Results
				{
					get { return res; }
				}
			}

			/// <summary>
			/// Called when the data event DataArrived happens for an IResult
			/// inside a Result Handler
			/// </summary>
			/// <param name="results">The IResult that fired the event</param>
			private void ProcessResults(IResults results)
			{
				List<DataItem> ldi = results.Results;
				//Put the items into a string that the Description box displays
				foreach (DataItem di in ldi)
				{
					Q_info_string = String.Format(Q_info_string + di.ToString());
				}
			}
            #endregion
		}
        #endregion
        
        #region Graphic Operator
        /// <summary>
        /// The visual representation of operators displayed on the screen
        /// </summary>
        public class GraphicOperator
        {
            #region Variables
            /**/
            PictureBox OperatorPicBox;
            /**/
            string OpType;
            /**/
            string Op2ndType;
            /**/
            string OpName;
            /**/
            string OpPredicate;
            /**/
            int oldX, oldY;
            /**/
            int Qposition;
            /**/
            GraphicOperator Input1, Input2;
            /**/
            int[] attrs = new int[1];
            /**/
			int value;
            #endregion

            #region Contructor(s)
            /**/
            public GraphicOperator()
            {
                Input1 = null;
                Input2 = null;
            }
            /**/
            public GraphicOperator(GraphicOperator go)
            {
                OpType = go.OpType;
                OperatorPicBox = go.OperatorPicBox;
                OpName = go.OpName;
                Input1 = null;
                Input2 = null;
            }
            /**/
            /// <summary>
            /// This is the main constructor for setting up an operator visually
            /// </summary>
            /// <param name="optype"></param>
            /// <param name="pb"></param>
            /// <param name="name"></param>
            /// <param name="PosInQ"></param>
            public GraphicOperator(string optype, PictureBox pb, string name, int PosInQ)
            {
                OpType = optype;
                OperatorPicBox = pb;
                OpName = name;
                Input1 = null;
                Input2 = null;
                Qposition = PosInQ;
                Op2ndType = null;
            }
            /**/
            public GraphicOperator(string optype, PictureBox pb, string name)
            {
                OpType = optype;
                OperatorPicBox = pb;
                OpName = name;
                Input1 = null;
                Input2 = null;
            }
            #endregion

            #region Methods
            /**/
            public int OldX { get { return oldX; } set { oldX = value; } }
            /**/
            public int OldY { get { return oldY; } set { oldY = value; } }
            /**/
            public PictureBox PicBox { get { return OperatorPicBox; } set { OperatorPicBox = value; } }
            /**/
            public GraphicOperator Input_1
            {
                get { return Input1; }
                set
                {
                    if (OpType == "InputStream")
                        Input1 = null;
                    else
                        Input1 = value;
                }
            }
            /**/
            public GraphicOperator Input_2
            {
                get { return Input2; }
                set
                {
                    if (OpType == "InputStream" || OpType == "OutputStream" || OpType == "Select" || OpType == "Project" || OpType == "DupElim" || OpType == "GroupBy" || OpType == "Sort")
                        Input2 = null;
                    else
                        Input2 = value;
                }
            }
            /**/
            public string Name { get { return OpName; } set { OpName = value; } }
            /**/
            public string Type { get { return OpType; } set { OpType = value; } }
            /**/
            public string SecondaryType { get { return Op2ndType; } set { Op2ndType = value; } }
            /**/
            public string Predicate { get { return OpPredicate; } set { OpPredicate = value; } }
            /**/
            public string PicBoxName { get { return OperatorPicBox.Name; } set { OperatorPicBox.Name = value; } }
            /**/
            public int PositionInQueue { get { return Qposition; } set { Qposition = value; } }
            /**/
            public bool has_Input1()
            {
                if (Input1 != null)
                    return true;
                else
                    return false;
            }
            /**/
            public bool has_Input2()
            {
                if (Input2 != null)
                    return true;
                else
                    return false;
            }
            /**/
            public void set_XY(int X, int Y)
            {
                if (OperatorPicBox != null)
                {
                    OperatorPicBox.Top = OperatorPicBox.Top + (Y - oldY);
                    OperatorPicBox.Left = OperatorPicBox.Left + (X - oldX);
                    if (OperatorPicBox.Top > 650)
                        OperatorPicBox.Top = 650;
                    else if (OperatorPicBox.Top < 10)
                        OperatorPicBox.Top = 10;
                    if (OperatorPicBox.Left > 800)
                        OperatorPicBox.Left = 800;
                    else if (OperatorPicBox.Left < 10)
                        OperatorPicBox.Left = 10;
                }
            }
            /**/
            public bool has_Predicate()
            {
                if (Type == "Select" || Type == "Join")
                    return true;
                else
                    return false;
            }
            /**/
            public int[] Attrs { get { return attrs; } set { attrs = value; } }
            /**/
            public int Value { get { return this.value; } set { this.value = value; } }
            /**/
            public bool isUnaryOp()
            {
                if (Type == "Join" || Type == "Intersect" || Type == "Union" || Type == "Difference")
                    return false;
                else
                    return true;
            }
            /**/
            public bool isBinaryOp()
            {
                if (Type == "Join" || Type == "Intersect" || Type == "Union" || Type == "Difference")
                    return true;
                else
                    return false;
            }
            #endregion
        }
        #endregion
    }
}