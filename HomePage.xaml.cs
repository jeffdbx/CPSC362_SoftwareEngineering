// CPSC362 - Software Engineering
// Property Management application
// Author: Jeff Bohlin
// Date: 03-01-2013
//
// HomePage.xaml.cs  - The application's home page (property listing).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Data;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Animation;


namespace PropertyManagement
{
    // This class implements the logic that interacts with PropertyManagementHome.xaml
    public partial class HomePage : Page
    {
        string propertyID;
        string propertyName;
        bool   userIsEditingTheImage = false;
        string onlyImageFileName = "";
        string sourcePath = "";
        string targetPath = "";

        public HomePage()
        {
            InitializeComponent();

            // This is an adapter that allows for communication between the WPF UI and the database. Basically, 
            // this is created by creating a DataSet (the .xsd file in Solution Explorer).  The dataset provides
            // generated DB connection code for us.  We simply create an adapter instance, then do our insert,
            // delete, and update queries through it.
            RentAwareDBDataSetTableAdapters.rw_propertyTableAdapter myAdapter = 
                new RentAwareDBDataSetTableAdapters.rw_propertyTableAdapter();
            
            // The DataContext is what our GridView, ListBoxes, etc. use to populate their fields.
            propertyDataGrid.DataContext = myAdapter.GetProperties();

            //Disable the edit form until the user explicitly clicks the "Edit" button.
            editPropertyGrid.IsEnabled = false;
            addButton.IsEnabled        = false;

            try
            {
                // Try to fill our property ImageBox with the "No Image Available" picture.
                string baseDir = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                PictureBoxImage1.Source = new BitmapImage(new Uri(baseDir + @"\none.jpg", UriKind.RelativeOrAbsolute));
            }
            catch (System.Exception ex)
            {
                // If the "No Image Available" picture can't be found, leave the ImageBox blank.
                PictureBoxImage1.Source = null;

                // No need to print out an error message.
                //System.Windows.MessageBox.Show(ex.Message);
            }
        }

        //----------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------
        // These 2 functions are used to automatically disable/enable certain buttons depending
        // on if there are errors on the form.  For example, this code disables the Update button
        // under "Edit Property" if there are textboxes with errors in them.
        private void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsValid(sender as DependencyObject);
        }

        private bool IsValid(DependencyObject obj)
        {
            // The dependency object is valid if it has no errors, 
            // and all of its children (that are dependency objects) are error-free.
            return !Validation.GetHasError(obj) && LogicalTreeHelper.GetChildren(obj)
                .OfType<DependencyObject>()
                .All(child => IsValid(child));
        }
        //----------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------

        // Delete a Property
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (propertyDataGrid.SelectedIndex == -1)
                System.Windows.MessageBox.Show("Please select a property to delete.");
            else
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure you want to delete this property?", 
                    "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Create an instance of each adapter to allow us to delete properties from the database.
                        RentAwareDBDataSetTableAdapters.rw_propertyTableAdapter propertyAdapter = 
                            new RentAwareDBDataSetTableAdapters.rw_propertyTableAdapter();

                        RentAwareDBDataSetTableAdapters.rw_unitTableAdapter unitAdapter = 
                            new RentAwareDBDataSetTableAdapters.rw_unitTableAdapter();

                        RentAwareDBDataSetTableAdapters.rw_tenantTableAdapter tenantAdapter = 
                            new RentAwareDBDataSetTableAdapters.rw_tenantTableAdapter();
                        
                        // This creates an instance of one entire row of the currently selected property in the DataGrid.  
                        // Meaning, if "Bell Gardens Apartments" is selected, then all of it's row data is available here.
                        DataRowView row = (DataRowView)propertyDataGrid.SelectedItems[0];

                        // Delete the currently selected property.  "DeleteProperty" is a custom query setup that searches 
                        // for the property ID and then deletes that corresponding property from the database.
                        propertyAdapter.DeleteProperty((int)row["PROP_ID"]);

                        // Delete all of the units that were associated with the property we just deleted.
                        unitAdapter.DeleteUnitByPropertyID((int)row["PROP_ID"]);

                        // Delete all of the tenants that were associated with the property we just deleted.
                        tenantAdapter.DeleteTenantByPropertyID((int)row["PROP_ID"]);

                        // Update our property DataGrid box on the main page, showing that the deleted property is indeed gone.
                        // NOTE: This next line is throwing a vague exception and I don't know why.  However everything 
                        //       continues working fine, so I just disabled the error message that pops up. Not the best 
                        //       solution, but it works for now.
                        propertyDataGrid.DataContext = propertyAdapter.GetProperties();

                        // Animate the success check mark
                        DoubleAnimation imageAnimation = new DoubleAnimation();
                        imageAnimation.From            = 0;
                        imageAnimation.To              = 1;
                        imageAnimation.Duration        = new Duration(TimeSpan.FromSeconds(1));
                        imageAnimation.AutoReverse     = true;
                        propertyCheckMark.BeginAnimation(OpacityProperty, imageAnimation);
                    }
                    catch (System.Exception ex)
                    {
                        System.Windows.MessageBox.Show("Delete Property failed.\n\n" + ex.Message);
                    }
                }
            }
        }

        // Add a property.
        private void AddPropertyButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure you want to add this property?", 
                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Create an instance of an adapter to allow us to add properties to the database.
                    RentAwareDBDataSetTableAdapters.rw_propertyTableAdapter myAdapter = 
                        new RentAwareDBDataSetTableAdapters.rw_propertyTableAdapter();
                        
                    // The Access database we are using generates an error if we try to submit an empty value 
                    // for PROP_UNITS_QTY. So this little bit of code simply sets the units quantity to zero 
                    // if the totalUnitsBox (in the UI) is empty.
                    short units = 0;
                    if (totalUnitsBox.Text != "")
                        units = Convert.ToInt16(totalUnitsBox.Text);

                    // Add the property using all of the data from our UI textboxes.  I had to update the Access database to 
                    // allow for empty values. Before, if there was an empty value in any of these boxes, the database would 
                    // complain. Now the only required value is that the user provides a property name. 
                    myAdapter.InsertProperty(nameBox.Text, addressBox.Text, cityBox.Text, zipBox.Text, stateBox.Text, units, 
                                             mgrBox.Text, mgrUnitBox.Text, mgrPhoneBox.Text, commentsBox.Text, onlyImageFileName);

                    if (userIsEditingTheImage)
                    {
                        try
                        {
                            if (targetPath != sourcePath)
                                System.IO.File.Copy(sourcePath, targetPath, true);
                            sourcePath = "";
                            targetPath = "";
                        }
                        catch (System.Exception ex)
                        {
                            // This will throw an error if I try to pick the same image that is already set as the current 
                            // properties image.
                            System.Windows.MessageBox.Show(ex.Message);
                        }
                        userIsEditingTheImage = false;
                    }

                    // Update our property DataGrid box on the main page, showing the newly added property.
                    propertyDataGrid.DataContext = myAdapter.GetProperties();

                    // When the user has finished adding a new property, reset the form like it was at the start of the program.
                    addButton.IsEnabled        = false;
                    editPropertyGrid.IsEnabled = false;

                    // Animate the success check mark
                    DoubleAnimation imageAnimation = new DoubleAnimation();
                    imageAnimation.From            = 0;
                    imageAnimation.To              = 1;
                    imageAnimation.Duration        = new Duration(TimeSpan.FromSeconds(1));
                    imageAnimation.AutoReverse     = true;
                    propertyCheckMark.BeginAnimation(OpacityProperty, imageAnimation);

                }
                catch (System.Exception ex)
                {
                    System.Windows.MessageBox.Show("Insert failed.\n\n" + ex.Message);
                }
            }
       
        }

        // This function keeps track of the currently selected item in the Property data grid.
        private void propertyDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            // Each time the user selects a different property, disable the edit portion of the program until
            // the user explicitly presses the "Edit" button.
            editPropertyGrid.IsEnabled = false;
            addButton.IsEnabled        = false;

            // Bounds checking! Leave this if statement here or bad things will happen!
            if (propertyDataGrid.SelectedIndex > (-1) && propertyDataGrid.SelectedIndex < propertyDataGrid.Items.Count)
            {
                // Just like in our Add Property and Delete Property functions, we want to grab all of the data in the 
                // currently selected property's row.
                DataRowView row = (DataRowView)propertyDataGrid.SelectedItems[0];

                propertyID = row["PROP_ID"].ToString();
                propertyName = row["PROP_NAME"].ToString();

                // This code is used for the property ImageBox but is little confusing. What it is doing is making sure 
                // that the path to the image files is relative to where the PropertyManagement .exe is being run.
                // Basically, "GetExecutingAssembly().Location" returns the path of where the .exe is running. However,
                // we need to strip "\PropertyManagement.exe" from that string because we only want the working directory
                // of where that .exe resides to use for our images path.
                const string removeString = "\\PropertyManagement.exe";
                string sourceString = System.Reflection.Assembly.GetExecutingAssembly().Location;
                int index = sourceString.IndexOf(removeString);
                string myString = sourceString.Remove(sourceString.IndexOf(removeString), removeString.Length);

                try
                {
                    // This opens up one of the images within the same directory that our .exe is running in and displays 
                    // it in our property ImageBox on the home page.
                    string path = myString + @"\" + row["PROP_PICTURE"];
                    PictureBoxImage1.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
                }
                catch (System.Exception ex)
                {
                    try
                    {
                        // If the property image does not exist (for example, "1.jpg"), then use the "No Image Available" 
                        // jpeg as default.
                        // string path = myString + @"\none.jpg";
                        // PictureBoxImage1.Source = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
                        string baseDir = 
                            System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                        PictureBoxImage1.Source = new BitmapImage(new Uri(baseDir + @"\none.jpg", UriKind.RelativeOrAbsolute));
                    }
                    catch (System.Exception ex1)
                    {
                        // If for some reason the "No Image Available" .jpg isn't found, then just leave the ImageBox blank.
                        PictureBoxImage1.Source = null;
                    }
                }
            }
            addButton.IsEnabled = false;

            // Each time a new property is selected, update all of the textboxes with the corresponding property data.
            PopulateFormData();
        }

        // This function populates all of the homepage textboxes with the corresponding data from the database.
        private void PopulateFormData()
        {
            // Bounds checking.  Leave this code here, or bad things will happen!
            if (propertyDataGrid.SelectedIndex > (-1) && propertyDataGrid.SelectedIndex < propertyDataGrid.Items.Count)
            {
                try
                {
                    DataRowView row = (DataRowView)propertyDataGrid.SelectedItems[0];

                    // Make sure that the database value exists for each item before trying to populate the form.
                    if (row["PROP_NAME"] != DBNull.Value)
                        nameBox.Text = (string)row["PROP_NAME"];
                    else
                        nameBox.Text = "";

                    if (row["PROP_STRT_ADDR"] != DBNull.Value)
                        addressBox.Text = (string)row["PROP_STRT_ADDR"];
                    else
                        addressBox.Text = "";

                    if (row["PROP_CITY"] != DBNull.Value)
                        cityBox.Text = (string)row["PROP_CITY"];
                    else
                        cityBox.Text = "";

                    if (row["PROP_ZIP"] != DBNull.Value)
                        zipBox.Text = (string)row["PROP_ZIP"];
                    else
                        zipBox.Text = "";

                    if (row["PROP_ST"] != DBNull.Value)
                        stateBox.Text = (string)row["PROP_ST"];
                    else
                        stateBox.Text = "";

                    if (row["PROP_UNITS_QTY"] != DBNull.Value)
                        totalUnitsBox.Text = row["PROP_UNITS_QTY"].ToString();
                    else
                        totalUnitsBox.Text = "";

                    if (row["PROP_MGR_NAME"] != DBNull.Value)
                        mgrBox.Text = (string)row["PROP_MGR_NAME"];
                    else
                        mgrBox.Text = "";

                    if (row["PROP_MGR_PH"] != DBNull.Value)
                        mgrPhoneBox.Text = (string)row["PROP_MGR_PH"];
                    else
                        mgrPhoneBox.Text = "";

                    if (row["PROP_MGR_UNIT"] != DBNull.Value)
                        mgrUnitBox.Text = (string)row["PROP_MGR_UNIT"];
                    else
                        mgrUnitBox.Text = "";

                    if (row["PROP_COMMENTS"] != DBNull.Value)
                        commentsBox.Text = (string)row["PROP_COMMENTS"];
                    else
                        commentsBox.Text = "";
                }
                catch (System.Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
        }

        // Edit a property.
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            PopulateFormData();

            // Make sure that a property is selected.
            if (propertyDataGrid.SelectedIndex == -1)
                System.Windows.MessageBox.Show("Please select a property to edit.");
            else
            {
                // Make the editPropertyGrid that's been grayed out this whole time now accessible to the user.
                editPropertyGrid.IsEnabled = true;
                updateButton.IsEnabled     = true;
                addButton.IsEnabled        = false;
            }
        }

        private void ClearFormButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForms();
        }

        private void ClearForms()
        {
            nameBox.Text       = "";
            addressBox.Text    = "";
            cityBox.Text       = "";
            zipBox.Text        = "";
            stateBox.Text      = "";
            totalUnitsBox.Text = "";
            mgrBox.Text        = "";
            mgrPhoneBox.Text   = "";
            mgrUnitBox.Text    = "";
            commentsBox.Text   = "";
        }

        // Manage a property.
        private void ManageButton_Click(object sender, RoutedEventArgs e)
        {
            if (propertyDataGrid.SelectedIndex == -1)
                System.Windows.MessageBox.Show("Please select a property to manage.");
            else
            {
                editPropertyGrid.IsEnabled = false;
                ManagePage bob = new ManagePage(propertyID, propertyName);
                this.NavigationService.Navigate(bob);
            }
        }

        // Update a property.
        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            // This code is all very similar to the Add Property / Delete Property code.
            if (propertyDataGrid.SelectedIndex == -1)
                System.Windows.MessageBox.Show("Make sure you have a property selected from the list before attempting to edit.");
            else
            {
                // Make sure that the user has provided at least a property name before attempting to update.
                if (!string.IsNullOrWhiteSpace(nameBox.Text))
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure you want to update this property?", 
                        "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            DataRowView row = (DataRowView)propertyDataGrid.SelectedItems[0];
                            RentAwareDBDataSetTableAdapters.rw_propertyTableAdapter myAdapter = 
                                new RentAwareDBDataSetTableAdapters.rw_propertyTableAdapter();
                            
                            short units = 0;
                            if (totalUnitsBox.Text != "")
                                units = Convert.ToInt16(totalUnitsBox.Text);

                            myAdapter.UpdateProperty(nameBox.Text, addressBox.Text, cityBox.Text, zipBox.Text, stateBox.Text, 
                                                     units, mgrBox.Text, mgrUnitBox.Text, mgrPhoneBox.Text, commentsBox.Text, 
                                                     (int)(row["PROP_ID"]));

                            if (userIsEditingTheImage)
                            {
                                try
                                {
                                    if(targetPath != sourcePath)
                                        System.IO.File.Copy(sourcePath, targetPath, true);
                                    myAdapter.UpdatePropertyPicture(onlyImageFileName, (int)(row["PROP_ID"]));
                                    sourcePath = "";
                                    targetPath = "";
                                }
                                catch (System.Exception ex)
                                {
                                    // This will throw an error if I try to pick the same image that is already set  
                                    // as the current properties image.
                                     System.Windows.MessageBox.Show("Attempting to Update image failed.\n\n" + ex.Message);
                                }
                                userIsEditingTheImage = false;
                            }

                            // Update our property DataGrid box on the main page, showing the newly updated property changes.
                            propertyDataGrid.DataContext = myAdapter.GetProperties();

                            // Animate the success check mark
                            DoubleAnimation imageAnimation = new DoubleAnimation();
                            imageAnimation.From            = 0;
                            imageAnimation.To              = 1;
                            imageAnimation.Duration        = new Duration(TimeSpan.FromSeconds(1));
                            imageAnimation.AutoReverse     = true;
                            propertyCheckMark.BeginAnimation(OpacityProperty, imageAnimation);
                        }
                        catch (System.Exception ex)
                        {
                            System.Windows.MessageBox.Show("Update property failed.\n\n" + ex.Message);
                        }
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Please provide a property name.");
                }
            }
        }

        // Add a new property.
        private void AddNewProperty_Button_Click(object sender, RoutedEventArgs e)
        {
            // Clear all of the textboxes and enable the editPropertyGrid that has been grayed out this whole time.
            ClearForms();
            editPropertyGrid.IsEnabled = true;

            addButton.IsEnabled    = true;
            updateButton.IsEnabled = false;
            userIsEditingTheImage  = false;
            onlyImageFileName      = "";

            // Try to load our picture box with the "No Image Availabe" .jpg. If that image isn't found, 
            // then just leave the ImageBox blank.
            try
            {
                string baseDir = 
                    System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

                PictureBoxImage1.Source = new BitmapImage(new Uri(baseDir + @"\none.jpg", UriKind.RelativeOrAbsolute));
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                PictureBoxImage1.Source = null;
            }
        }

        // Edit the property image.
        private void editImageButton_Click(object sender, RoutedEventArgs e)
        {
            userIsEditingTheImage = true;

            // This allows the user to go out and add whatever picture they want.  This code forces the user to only 
            // pick a .jpg as their choice.  This code is not fully functional yet. It does not make any changes to 
            // the database.
            Microsoft.Win32.OpenFileDialog openfile = new Microsoft.Win32.OpenFileDialog();
            openfile.DefaultExt = "*.jpg";
            openfile.Filter = "Image Files|*.jpg";
            Nullable<bool> result = openfile.ShowDialog();
            if (result == true)
            {
                sourcePath = openfile.FileName;
                onlyImageFileName = System.IO.Path.GetFileName(openfile.FileName);
                
                const string removeString = "\\PropertyManagement.exe";
                string sourceString = System.Reflection.Assembly.GetExecutingAssembly().Location;
                int index = sourceString.IndexOf(removeString);
                targetPath = sourceString.Remove(sourceString.IndexOf(removeString), removeString.Length);
                targetPath += "\\" + onlyImageFileName;
               
                PictureBoxImage1.Source = new BitmapImage(new Uri(openfile.FileName));
            }
        }
    }
}
