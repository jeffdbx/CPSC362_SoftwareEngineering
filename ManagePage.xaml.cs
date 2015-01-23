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
    // This class implements the logic that interacts with ManagePage.xaml
    public partial class ManagePage : Page
    {
        string propertyID;
        string unitNum;
        string propertyName;

        public ManagePage()
        {
            InitializeComponent();
        }

        // Custom constructor to pass Property management data
        public ManagePage(string id, string name)
            : this()
        {
            propertyID = id;
            propertyName = name;
            mainHeader.Content = "Manage: " + propertyName;

            RentAwareDBDataSetTableAdapters.rw_unitTableAdapter myAdapter = 
                new RentAwareDBDataSetTableAdapters.rw_unitTableAdapter();

            unitsDataGrid.DataContext = myAdapter.GetUnitsByID(Convert.ToInt32(propertyID));

            editUnitsGrid.IsEnabled  = false;
            editTenantGrid.IsEnabled = false;

            petsComboBox.Items.Add(0);
            petsComboBox.Items.Add(1);
            petsComboBox.Items.Add(2);
            petsComboBox.Items.Add(3);
            petsComboBox.Items.Add(4);
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

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void enableAllButtons()
        {
            editUnitsButton.IsEnabled    = true;
            deleteUnitsButton.IsEnabled  = true;
            newUnitButton.IsEnabled      = true;
            editTenantButton.IsEnabled   = true;
            updateUnitButton.IsEnabled   = true;
            addUnitButton.IsEnabled      = true;
            updateTenantButton.IsEnabled = true;
            deleteTenantButton.IsEnabled = true;
            addTenantButton.IsEnabled    = true;
        }

        private void unitsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            editUnitsGrid.IsEnabled  = false;
            editTenantGrid.IsEnabled = false;
            PopulateFormData();
            enableAllButtons();
            ClearTenantForms();

            try
            {
                DataRowView row = (DataRowView)unitsDataGrid.SelectedItems[0];
                unitNum = row["UNIT_ID"].ToString();

                RentAwareDBDataSetTableAdapters.rw_tenantTableAdapter tenantAdapter = 
                    new RentAwareDBDataSetTableAdapters.rw_tenantTableAdapter();

                tenantDataGrid.DataContext = tenantAdapter.GetTenantByUnitNum(Convert.ToInt32(propertyID), unitNum);
            }
            catch (System.Exception ex)
            {
                //System.Windows.MessageBox.Show(ex.Message);
            }
        }

        // This function populates all of the homepage textboxes with the corresponding data from the database.
        private void PopulateFormData()
        {
            // Bounds checking.  Leave this code here, or bad things will happen!
            if (unitsDataGrid.SelectedIndex > (-1) && unitsDataGrid.SelectedIndex < unitsDataGrid.Items.Count)
            {
                try
                {
                    DataRowView row = (DataRowView)unitsDataGrid.SelectedItems[0];

                    // Make sure that the database value exists for each item before trying to populate the form.
                    if (row["UNIT_ID"] != DBNull.Value)
                        unitNumBox.Text = (string)row["UNIT_ID"];
                    else
                        unitNumBox.Text = "";

                    if (row["BATH_QTY"] != DBNull.Value)
                        bathQtyBox.Text = row["BATH_QTY"].ToString();
                    else
                        bathQtyBox.Text = "";

                    if (row["BEDR_QTY"] != DBNull.Value)
                        bedrQtyBox.Text = row["BEDR_QTY"].ToString();
                    else
                        bedrQtyBox.Text = "";

                    if (row["FLOOR_NO"] != DBNull.Value)
                        floorNumBox.Text = row["FLOOR_NO"].ToString();
                    else
                        floorNumBox.Text = "";

                    if (row["PETS_NO"] != DBNull.Value)
                    {
                        if (Convert.ToInt32(row["PETS_NO"]) > -1 && Convert.ToInt32(row["PETS_NO"]) < 5)
                            petsComboBox.SelectedIndex = Convert.ToInt32(row["PETS_NO"]);
                        else
                            petsComboBox.SelectedIndex = 0;
                    }
                    else
                        petsComboBox.SelectedIndex = 0;

                    if (row["FLOOR_AREA_SF"] != DBNull.Value)
                        sqFtBox.Text = row["FLOOR_AREA_SF"].ToString();
                    else
                        sqFtBox.Text = "";

                    if (row["MTHLY_RENT_AM"] != DBNull.Value)
                        mthlyRentBox.Text = string.Format("{0:N2}", row["MTHLY_RENT_AM"]);
                    else
                        mthlyRentBox.Text = "0.00";

                    if (row["INIT_DEPOSIT_AM"] != DBNull.Value)
                        depositBox.Text = string.Format("{0:N2}", row["INIT_DEPOSIT_AM"]);
                    else
                        depositBox.Text = "0.00";

                    if (row["UNIT_NOTES_TX"] != DBNull.Value)
                        notesBox.Text = (string)row["UNIT_NOTES_TX"];
                    else
                        notesBox.Text = "";
                }
                catch (System.Exception ex)
                {
                    System.Windows.MessageBox.Show("Populate form data\n\n" + ex.Message);
                }
            }
        }

        private void PopulateTenantFormData()
        {
            // Bounds checking.  Leave this code here, or bad things will happen!
            if (tenantDataGrid.SelectedIndex > (-1) && tenantDataGrid.SelectedIndex < tenantDataGrid.Items.Count)
            {
                try
                {
                    DataRowView row = (DataRowView)tenantDataGrid.SelectedItems[0];

                    // Make sure that the database value exists for each item before trying to populate the form.
                    if (row["TENANT_F_NM"] != DBNull.Value)
                        firstNmBox.Text = (string)row["TENANT_F_NM"];
                    else
                        firstNmBox.Text = "";

                    if (row["TENANT_L_NM"] != DBNull.Value)
                        lastNmBox.Text = (string)row["TENANT_L_NM"];
                    else
                        lastNmBox.Text = "";

                    if (row["Tenant_PH"] != DBNull.Value)
                        tenantPhBox.Text = (string)row["Tenant_PH"];
                    else
                        tenantPhBox.Text = "";

                    if (row["TENANT_DOB"] != DBNull.Value)
                        dobPicker.SelectedDate = (DateTime)row["TENANT_DOB"];
                    else
                        dobPicker.SelectedDate = null;

                    if (row["Employer_NM"] != DBNull.Value)
                        empNmBox.Text = (string)row["Employer_NM"];
                    else
                        empNmBox.Text = "";

                    if (row["Employer_PHONE"] != DBNull.Value)
                        empPhBox.Text = (string)row["Employer_PHONE"];
                    else
                        empPhBox.Text = "";

                    if (row["RENTAL_START_DT"] != DBNull.Value)
                        rentalPicker.SelectedDate = (DateTime)row["RENTAL_START_DT"];
                    else
                        rentalPicker.SelectedDate = null;

                    if (row["DUE_AM"] != DBNull.Value)
                        amountDue.Text = string.Format("{0:N2}", row["DUE_AM"]);
                    else
                        amountDue.Text = "0.00";

                    if (row["DOWN_PMT_FLG"] != DBNull.Value)
                    {
                        if ((bool)row["DOWN_PMT_FLG"] == true)
                            depositComboBox.SelectedIndex = 0;
                        else
                            depositComboBox.SelectedIndex = 1;
                    }
                }
                catch (System.Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
        }

        private void ClearForms()
        {
            unitNumBox.Text   = "";
            bathQtyBox.Text   = "";
            bedrQtyBox.Text   = "";
            floorNumBox.Text  = "";
            petsComboBox.Text = "";
            sqFtBox.Text      = "";
            mthlyRentBox.Text = "0.00";
            depositBox.Text   = "0.00";
            notesBox.Text     = "";
        }

        private void ClearTenantForms()
        {
            firstNmBox.Text           = "";
            tenantPhBox.Text          = "";
            amountDue.Text            = "0.00";
            depositComboBox.Text      = "";
            lastNmBox.Text            = "";
            empNmBox.Text             = "";
            empPhBox.Text             = "";
            rentalPicker.SelectedDate = null;
            dobPicker.SelectedDate    = null;
        }

        private void clearUnitForm_Click(object sender, RoutedEventArgs e)
        {
            ClearForms();
        }

        private void editUnitButton_Click(object sender, RoutedEventArgs e)
        {
            if (unitsDataGrid.SelectedIndex == -1)
                System.Windows.MessageBox.Show("Please select a unit to edit.");
            else
            {
                editUnitsGrid.IsEnabled    = true;
                addUnitButton.IsEnabled    = false;
                updateUnitButton.IsEnabled = true;
                addUnitButton.IsEnabled    = false;
            }
        }

        private void tenantDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PopulateTenantFormData();
            updateTenantButton.IsEnabled = true;
            deleteTenantButton.IsEnabled = true;
        }

        private void clearTenantFormButton_Click(object sender, RoutedEventArgs e)
        {
            ClearTenantForms();
            RentAwareDBDataSetTableAdapters.rw_tenantTableAdapter myAdapter = 
                new RentAwareDBDataSetTableAdapters.rw_tenantTableAdapter();

            tenantDataGrid.DataContext = myAdapter.GetTenantByUnitNum(Convert.ToInt32(propertyID), unitNum);
        }

        private void addNewUnit_Button_Click(object sender, RoutedEventArgs e)
        {
            // Clear all of the textboxes and disable the edit Tenant grid until the user explicitly clicks "Edit Tenant".
            ClearForms();
            ClearTenantForms();
            editTenantGrid.IsEnabled = false;

            // Enable or disable the apropriate UI items.
            editUnitsGrid.IsEnabled     = true;
            addUnitButton.IsEnabled     = true;
            updateUnitButton.IsEnabled  = false;
            editUnitsButton.IsEnabled   = false;
            deleteUnitsButton.IsEnabled = false;

            // Clear the tenantDataGrid because we are adding a brand new unit.
            tenantDataGrid.DataContext = null;
            tenantDataGrid.Items.Refresh();
        }

        private void addUnitButton_Click(object sender, RoutedEventArgs e)
        {
            // Force the user to at least enter a unit number.
            if (!string.IsNullOrWhiteSpace(unitNumBox.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure you want to add this unit?", 
                    "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Create an instance of an adapter to allow us to add new units to the database.
                        RentAwareDBDataSetTableAdapters.rw_unitTableAdapter myAdapter = 
                            new RentAwareDBDataSetTableAdapters.rw_unitTableAdapter();

                        // Add the new unit using all of the data from our UI textboxes.
                        // The Access database we are using generates an error if we try to submit an empty values 
                        // for the following fields. For some reason a simple unary operater doesn't work here:   
                        // (petsComboBox.SelectedIndex == 1) ? true : false
                        int pets = 0;
                        if (petsComboBox.Text != "")
                            pets = petsComboBox.SelectedIndex;

                        short bathQty = 0;
                        if (bathQtyBox.Text != "")
                            bathQty = Convert.ToInt16(bathQtyBox.Text);

                        short bedrQty = 0;
                        if (bedrQtyBox.Text != "")
                            bedrQty = Convert.ToInt16(bedrQtyBox.Text);

                        short floorNumber = 0;
                        if (floorNumBox.Text != "")
                            floorNumber = Convert.ToInt16(floorNumBox.Text);

                        short sqFt = 0;
                        if (sqFtBox.Text != "")
                            sqFt = Convert.ToInt16(sqFtBox.Text);

                        decimal mthlyRent = 0;
                        if (mthlyRentBox.Text != "")
                            mthlyRent = Convert.ToDecimal(mthlyRentBox.Text);

                        decimal deposit = 0;
                        if (depositBox.Text != "")
                            deposit = Convert.ToDecimal(depositBox.Text);

                        // Insert the new unit.
                        myAdapter.InsertUnit(Convert.ToInt32(propertyID), unitNumBox.Text, bathQty, bedrQty, floorNumber, 
                                             pets, sqFt, mthlyRent, deposit, notesBox.Text);

                        // Update our unit DataGrid box on the main page, showing the newly added unit.
                        unitsDataGrid.DataContext = myAdapter.GetUnitsByID(Convert.ToInt32(propertyID));

                        // When the user has finished adding a new unit, reset the form like it was at the start.
                        addUnitButton.IsEnabled     = false;
                        editUnitsButton.IsEnabled   = true;
                        deleteUnitsButton.IsEnabled = true;
                        editUnitsGrid.IsEnabled     = false;
                        editTenantGrid.IsEnabled    = false;

                        // This animates the success check mark
                        DoubleAnimation imageAnimation = new DoubleAnimation();
                        imageAnimation.From            = 0;
                        imageAnimation.To              = 1;
                        imageAnimation.Duration        = new Duration(TimeSpan.FromSeconds(1));
                        imageAnimation.AutoReverse     = true;
                        checkMark.BeginAnimation(OpacityProperty, imageAnimation);
                    }
                    catch (System.Exception ex)
                    {
                        System.Windows.MessageBox.Show("Insert unit failed.\n\n" + ex.Message);
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please provide a unit number.");
            }
        }

        private void deleteUnitsButton_Click(object sender, RoutedEventArgs e)
        {
            if (unitsDataGrid.SelectedIndex == -1)
                System.Windows.MessageBox.Show("Please select a unit to delete.");
            else
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure you want to delete this unit?", 
                    "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Create an instance of an adapter to allow us to add new units to the database.
                        RentAwareDBDataSetTableAdapters.rw_unitTableAdapter myAdapter = 
                            new RentAwareDBDataSetTableAdapters.rw_unitTableAdapter();

                        RentAwareDBDataSetTableAdapters.rw_tenantTableAdapter tenantAdapter = 
                            new RentAwareDBDataSetTableAdapters.rw_tenantTableAdapter();

                        // This creates an instance of one entire row of the currently selected unit in the DataGrid.
                        DataRowView row = (DataRowView)unitsDataGrid.SelectedItems[0];

                        // Delete the currently selected unit.  "DeleteUnit" is a custom query.
                        myAdapter.DeleteUnit((int)row["UNIT_NO"]);

                        // Also, delete any tenant that belongs to the unit that we just deleted.
                        tenantAdapter.DeleteTenantByUnitID(Convert.ToInt32(propertyID), row["UNIT_ID"].ToString());

                        // Refresh the tenant data grid now that a unit and its tenants were deleted.
                        tenantDataGrid.DataContext = tenantAdapter.GetTenantByUnitNum(Convert.ToInt32(propertyID), unitNum);

                        // Update our unit DataGrid box on the main page, showing that the deleted unit is indeed gone.
                        unitsDataGrid.DataContext = myAdapter.GetUnitsByID(Convert.ToInt32(propertyID));

                        // This animates the success check mark
                        DoubleAnimation imageAnimation = new DoubleAnimation();
                        imageAnimation.From            = 0;
                        imageAnimation.To              = 1;
                        imageAnimation.Duration        = new Duration(TimeSpan.FromSeconds(1));
                        imageAnimation.AutoReverse     = true;
                        checkMark.BeginAnimation(OpacityProperty, imageAnimation);

                    }
                    catch (System.Exception ex)
                    {
                        System.Windows.MessageBox.Show("Delete Unit failed.\n\n" + ex.Message);
                    }
                }
            }
        }

        private void editTenantButton_Click(object sender, RoutedEventArgs e)
        {
            if (unitsDataGrid.SelectedIndex == -1)
                System.Windows.MessageBox.Show("Please select a unit to edit.");
            else
            {
                editTenantGrid.IsEnabled = true;
            }
        }

        private void updateUnitButton_Click(object sender, RoutedEventArgs e)
        {
            // This code is all very similar to the Add Property / Delete unit code.
            if (unitsDataGrid.SelectedIndex == -1)
                System.Windows.MessageBox.Show("Make sure you have a unit selected from the list before attempting to edit.");
            else
            {
                // Make sure that the user has provided at least a unit number before attempting to update.
                if (!string.IsNullOrWhiteSpace(unitNumBox.Text))
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure you want to update this unit?", 
                        "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            int pets = 0;
                            if (petsComboBox.Text != "")
                                pets = petsComboBox.SelectedIndex;

                            short bathQty = 0;
                            if (bathQtyBox.Text != "")
                                bathQty = Convert.ToInt16(bathQtyBox.Text);

                            short bedrQty = 0;
                            if (bedrQtyBox.Text != "")
                                bedrQty = Convert.ToInt16(bedrQtyBox.Text);

                            short floorNumber = 0;
                            if (floorNumBox.Text != "")
                                floorNumber = Convert.ToInt16(floorNumBox.Text);

                            short sqFt = 0;
                            if (sqFtBox.Text != "")
                                sqFt = Convert.ToInt16(sqFtBox.Text);

                            decimal mthlyRent = 0;
                            if (mthlyRentBox.Text != "")
                                mthlyRent = Convert.ToDecimal(mthlyRentBox.Text);

                            decimal deposit = 0;
                            if (depositBox.Text != "")
                                deposit = Convert.ToDecimal(depositBox.Text);

                            DataRowView row = (DataRowView)unitsDataGrid.SelectedItems[0];
                            RentAwareDBDataSetTableAdapters.rw_unitTableAdapter myAdapter = 
                                new RentAwareDBDataSetTableAdapters.rw_unitTableAdapter();

                            myAdapter.UpdateUnit(unitNumBox.Text, bathQty, bedrQty, floorNumber, pets, sqFt, mthlyRent,
                                                 deposit, notesBox.Text, (int)row["UNIT_NO"]);

                            // Update our units DataGrid box on the main page, showing the newly updated unit changes.
                            unitsDataGrid.DataContext = myAdapter.GetUnitsByID(Convert.ToInt32(propertyID));

                            // This animates the success check mark
                            DoubleAnimation imageAnimation = new DoubleAnimation();
                            imageAnimation.From            = 0;
                            imageAnimation.To              = 1;
                            imageAnimation.Duration        = new Duration(TimeSpan.FromSeconds(1));
                            imageAnimation.AutoReverse     = true;
                            checkMark.BeginAnimation(OpacityProperty, imageAnimation);

                        }
                        catch (System.Exception ex)
                        {
                            System.Windows.MessageBox.Show("Update unit failed.\n\n" + ex.Message);
                        }
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Please provide a unit number.");
                }
            }
        }

        private void addTenantButton_Click(object sender, RoutedEventArgs e)
        {
            // Force the user to at least enter a unit number.
            if (!string.IsNullOrWhiteSpace(lastNmBox.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure you want to add this tenant?", 
                    "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // The Access database we are using generates an error if we try to submit an empty values 
                        // for the following fields.
                        decimal dueAmount = 0;
                        if (amountDue.Text != "")
                            dueAmount = Convert.ToDecimal(amountDue.Text);

                        bool deposit;
                        if (depositComboBox.SelectedIndex == 0)
                            deposit = true;
                        else
                            deposit = false;

                        if (dobPicker.SelectedDate == null)
                            dobPicker.SelectedDate = DateTime.Now;

                        if (rentalPicker.SelectedDate == null)
                            rentalPicker.SelectedDate = DateTime.Now;

                        // Create an instance of an adapter to allow us to add new units to the database.
                        RentAwareDBDataSetTableAdapters.rw_tenantTableAdapter myAdapter = 
                            new RentAwareDBDataSetTableAdapters.rw_tenantTableAdapter();

                        // Add the new tenant using all of the data from our UI textboxes.
                        myAdapter.InsertTenant(Convert.ToInt32(propertyID), unitNum, firstNmBox.Text, lastNmBox.Text,
                                               tenantPhBox.Text, (DateTime)dobPicker.SelectedDate,
                                               empNmBox.Text, empPhBox.Text, (DateTime)rentalPicker.SelectedDate, 
                                               dueAmount, deposit);

                        // Update our tenant DataGrid box on the main page.
                        tenantDataGrid.DataContext = myAdapter.GetTenantByUnitNum(Convert.ToInt32(propertyID), unitNum);
                        
                        editTenantGrid.IsEnabled = false;

                        // Animate the success check mark
                        DoubleAnimation imageAnimation = new DoubleAnimation();
                        imageAnimation.From            = 0;
                        imageAnimation.To              = 1;
                        imageAnimation.Duration        = new Duration(TimeSpan.FromSeconds(1));
                        imageAnimation.AutoReverse     = true;
                        tenantCheckMark.BeginAnimation(OpacityProperty, imageAnimation);
                    }
                    catch (System.Exception ex)
                    {
                        System.Windows.MessageBox.Show("Insert failed.\n\n" + ex.Message);
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please provide at least the tenant's last name.");
            }
        }

        private void deleteTenantButton_Click(object sender, RoutedEventArgs e)
        {
            if (tenantDataGrid.SelectedIndex == -1)
                System.Windows.MessageBox.Show("Please select a tenant to delete.");
            else
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure you want to delete this tenant?", 
                    "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Create an instance of an adapter to allow us to delete tenants from the database.
                        RentAwareDBDataSetTableAdapters.rw_tenantTableAdapter myAdapter = 
                            new RentAwareDBDataSetTableAdapters.rw_tenantTableAdapter();

                        // This creates an instance of one entire row of the currently selected tenant in the DataGrid.
                        DataRowView row = (DataRowView)tenantDataGrid.SelectedItems[0];

                        // Delete the currently selected tenant.
                        myAdapter.DeleteTenant((int)row["TENANT_NO"]);

                        // Update our tenant DataGrid box on the main page.
                        tenantDataGrid.DataContext = myAdapter.GetTenantByUnitNum(Convert.ToInt32(propertyID), unitNum);

                        ClearTenantForms();
                        editTenantGrid.IsEnabled     = false;
                        updateTenantButton.IsEnabled = false;
                        deleteTenantButton.IsEnabled = false;

                        // Animate the success check mark
                        DoubleAnimation imageAnimation = new DoubleAnimation();
                        imageAnimation.From            = 0;
                        imageAnimation.To              = 1;
                        imageAnimation.Duration        = new Duration(TimeSpan.FromSeconds(1));
                        imageAnimation.AutoReverse     = true;
                        tenantCheckMark.BeginAnimation(OpacityProperty, imageAnimation);
                    }
                    catch (System.Exception ex)
                    {
                        System.Windows.MessageBox.Show("Delete failed.\n\n" + ex.Message);
                    }
                }
            }
        }

        private void updateTenantButton_Click(object sender, RoutedEventArgs e)
        {
            if (tenantDataGrid.SelectedIndex > (-1) && tenantDataGrid.SelectedIndex < tenantDataGrid.Items.Count)
            {
                // Make sure that the user has provided at least a unit number before attempting to update.
                if (!string.IsNullOrWhiteSpace(lastNmBox.Text))
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure you want to update this tenant?", 
                        "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            // The Access database we are using generates an error if we try to submit an empty 
                            // values for the following fields.
                            decimal dueAmount = 0;
                            if (amountDue.Text != "")
                                dueAmount = Convert.ToDecimal(amountDue.Text);

                            bool deposit;
                            if (depositComboBox.SelectedIndex == 0)
                                deposit = true;
                            else
                                deposit = false;

                            if (dobPicker.SelectedDate == null)
                                dobPicker.SelectedDate = DateTime.Now;

                            if (rentalPicker.SelectedDate == null)
                                rentalPicker.SelectedDate = DateTime.Now;

                            // Create an instance of an adapter to allow us to update a tenant in the database.
                            RentAwareDBDataSetTableAdapters.rw_tenantTableAdapter myAdapter = 
                                new RentAwareDBDataSetTableAdapters.rw_tenantTableAdapter();

                            DataRowView row = (DataRowView)tenantDataGrid.SelectedItems[0];

                            myAdapter.UpdateTenant(firstNmBox.Text, lastNmBox.Text,
                                                  tenantPhBox.Text, (DateTime)dobPicker.SelectedDate,
                                                  empNmBox.Text, empPhBox.Text,  (DateTime)rentalPicker.SelectedDate, 
                                                  dueAmount, deposit, (int)row["TENANT_NO"]);

                            // Update our tenant DataGrid box on the main page.
                            tenantDataGrid.DataContext = myAdapter.GetTenantByUnitNum(Convert.ToInt32(propertyID), unitNum);

                            editTenantGrid.IsEnabled     = false;
                            updateTenantButton.IsEnabled = false;
                            deleteTenantButton.IsEnabled = false;

                            // Animate the success check mark
                            DoubleAnimation imageAnimation = new DoubleAnimation();
                            imageAnimation.From            = 0;
                            imageAnimation.To              = 1;
                            imageAnimation.Duration        = new Duration(TimeSpan.FromSeconds(1));
                            imageAnimation.AutoReverse     = true;
                            tenantCheckMark.BeginAnimation(OpacityProperty, imageAnimation);
                        }
                        catch (System.Exception ex)
                        {
                            System.Windows.MessageBox.Show("Update failed.\n\n" + ex.Message);
                        }
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Please provide at least a last name.");
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a tenant to update.");
            }
        }
    }
}


        