﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QuoteSwift
{
    public partial class FrmAddPump : Form
    {
        AppContext passed;

        public ref AppContext Passed { get => ref passed; }

        public FrmAddPump()
        {
            InitializeComponent();
        }

        private void CloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MainProgramCode.RequestConfirmation("Are you sure you want to close the application?", "REQUEST - Application Termination"))
                QuoteSwiftMainCode.CloseApplication(true);
        }

        private void BtnAddPump_Click(object sender, EventArgs e)
        {
            //When done Editing / Adding a pump, all mandatory parts need to be added first to the part list
            //This is for the for loop when the form gets activated to work correctly.

            if (ValidInput())
            {
                BindingList<Product_Part> NewPumpParts = RetreivePumpPartList();

                if (NewPumpParts == null)
                {
                    MainProgramCode.ShowError("There wasn't any parts chosen from any of the lists below\nPlease ensure that parts are selected and/or that there is parts available to select from.", "ERROR - No Pump Part Selection");
                    return;
                }

                if (passed.ChangeSpecificObject) // Update Part List if true
                {
                    RecordNewInformation();
                    MainProgramCode.ShowInformation(passed.PumpToChange.ProductName + " has been updated in the list of pumps", "INFORMATION - Pump Update Successfully");

                    //Set ChangeSpecificObject to false and convert to View

                    passed.ChangeSpecificObject = false;
                    ConvertToViewForm();

                    //Enable menu strip item that converts form to Update a pump 
                    updatePumpToolStripMenuItem.Enabled = true;
                }
                else //Create New Pump And Add To Pump List
                {
                    Product newPump = new Product(mtxtPumpName.Text, mtxtPumpDescription.Text, (float)Convert.ToDouble(mtxtNewPumpPrice.Text), ref NewPumpParts); // Cast used since Convert.To does not support float
                    if (passed.ProductMap == null) passed.ProductMap = new Dictionary<string, Product>(){ {newPump.ProductName, newPump} }; else passed.ProductMap.Add(newPump.ProductName, newPump);
                    MainProgramCode.ShowInformation(newPump.ProductName + " has been added to the list of pumps", "INFORMATION - Pump Added Successfully");
                }
            }

        }

        private void MtxtPumpName_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            if (!passed.ChangeSpecificObject)
                ChangeViewToEdit();
        }

        private void MtxtPumpDescription_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            if (!passed.ChangeSpecificObject)
                ChangeViewToEdit();
        }

        private void MtxtNewPumpPrice_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            if (!passed.ChangeSpecificObject)
                ChangeViewToEdit();
        }

        private void DgvMandatoryPartView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!passed.ChangeSpecificObject)
                ChangeViewToEdit();
        }

        private void DgvNonMandatoryPartView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!passed.ChangeSpecificObject)
                ChangeViewToEdit();
        }

        private void FrmAddPump_Load(object sender, EventArgs e)
        {
            LoadMandatoryParts();
            LoadNonMandatoryParts();

            if (passed != null && passed.PumpToChange != null && passed.ChangeSpecificObject == true) //Determine if Edit
            {
                ConvertToEditForm();
                Read_OnlyMainComponents();
                PopulateFormWithPassedPump();
            }
            else if (passed != null && passed.PumpToChange != null && passed.ChangeSpecificObject == false) //Determine if View
            {
                ConvertToViewForm();
                Read_OnlyMainComponents();
                PopulateFormWithPassedPump();
            }
            else if (passed != null && passed.PumpToChange == null && passed.ChangeSpecificObject == false) // Determine if Add New
            {
                mtxtPumpName.Focus();
            }
            else //This should never happen. Error message displayed and application will not allow input
            {
                MainProgramCode.ShowError("An error occurred that was not suppose to ever happen.\nAll input will now be disabled for this current screen", "ERROR - Undefined Action Called");

                Read_OnlyMainComponents();
            }

            dgvMandatoryPartView.RowsDefaultCellStyle.BackColor = Color.Bisque;
            dgvMandatoryPartView.AlternatingRowsDefaultCellStyle.BackColor = Color.Beige;

            dgvNonMandatoryPartView.RowsDefaultCellStyle.BackColor = Color.Bisque;
            dgvNonMandatoryPartView.AlternatingRowsDefaultCellStyle.BackColor = Color.Beige;
        }

        /** Form Specific Functions And Procedures: 
        *
        * Note: Not all Functions or Procedures below are being used more than once
        *       Some of them are only here to keep the above events easily understandable 
        *       and clutter free.                                                          
        */

        //Disable Main Components On This Form:

        void Read_OnlyMainComponents()
        {
            dgvMandatoryPartView.ReadOnly = true;
            dgvNonMandatoryPartView.ReadOnly = true;
            mtxtNewPumpPrice.ReadOnly = true;
            mtxtPumpDescription.ReadOnly = true;
            mtxtPumpName.ReadOnly = true;
            btnAddPump.Enabled = false;
        }

        //Enable Main Components On This Form:

        void ReadWriteMainComponents()
        {
            dgvMandatoryPartView.ReadOnly = false;
            dgvNonMandatoryPartView.ReadOnly = false;
            mtxtNewPumpPrice.ReadOnly = false;
            mtxtPumpDescription.ReadOnly = false;
            mtxtPumpName.ReadOnly = false;
            btnAddPump.Enabled = true;
        }

        //Convert Form To Edit:

        void ConvertToEditForm()
        {
            ReadWriteMainComponents();
            Text = "Updating " + passed.PumpToChange.ProductName + " Pump";
            btnAddPump.Text = "Update Pump";
            btnAddPump.Visible = true;
            updatePumpToolStripMenuItem.Enabled = true;
        }

        //Convert Form To View:

        void ConvertToViewForm()
        {
            Text = "Viewing " + passed.PumpToChange.ProductName + " Pump";
            btnAddPump.Visible = false;
            Read_OnlyMainComponents();
            updatePumpToolStripMenuItem.Enabled = false;
        }

        //Populates the form with the passed Pump object and checks the check boxes in the data grid view where parts are being used.

        void PopulateFormWithPassedPump()
        {
            mtxtNewPumpPrice.Text = passed.PumpToChange.NewProductPrice.ToString();
            mtxtPumpDescription.Text = passed.PumpToChange.PumpDescription;
            mtxtPumpName.Text = passed.PumpToChange.ProductName;

            for (int i = 0; i < passed.MandatoryPartMap.Count; i++)
                for (int k = 0; k < passed.PumpToChange.PartList.Count; k++)
                {
                    if (passed.MandatoryPartMap.Values.ToArray()[i].OriginalItemPartNumber == passed.PumpToChange.PartList[k].ProductPart.OriginalItemPartNumber)
                    {
                        DataGridViewCheckBoxCell cbx = (DataGridViewCheckBoxCell)dgvMandatoryPartView.Rows[i].Cells["clmAddToPumpSelection"];
                        cbx.Value = true;
                        dgvMandatoryPartView.Rows[i].Cells["clmMPartQuantity"].Value = passed.PumpToChange.PartList[k].PumpPartQuantity.ToString();
                    }
                }

            for (int s = 0; s < passed.NonMandatoryPartMap.Count; s++)
                for (int d = 0; d < passed.PumpToChange.PartList.Count; d++)
                {
                    if (passed.NonMandatoryPartMap.Values.ToArray()[s].OriginalItemPartNumber == passed.PumpToChange.PartList[d].ProductPart.OriginalItemPartNumber)
                    {
                        DataGridViewCheckBoxCell cbx = (DataGridViewCheckBoxCell)dgvNonMandatoryPartView.Rows[s].Cells["ClmNonMandatoryPartSelection"];
                        cbx.Value = true;
                        dgvNonMandatoryPartView.Rows[s].Cells["clmNMPartQuantity"].Value = passed.PumpToChange.PartList[d].PumpPartQuantity.ToString();
                    }
                }
        }

        //Links the binding-lists with the corresponding datagridview components

        bool ValidInput()
        {
            if (mtxtPumpName.TextLength < 3)
            {
                MainProgramCode.ShowInformation("Please ensure the input for the Pump Name is correct and longer than 3 characters.", "INFORMATION -Pump Name Input Incorrect");
                mtxtPumpName.Focus();
                return false;
            }

            if (mtxtPumpDescription.TextLength < 3)
            {
                MainProgramCode.ShowInformation("Please ensure the input for the description of the pump is correct and longer than 3 characters.", "INFORMATION - Pump Description Input Incorrect");
                mtxtPumpDescription.Focus();
                return false;
            }

            if (NewPumpValueInput() == 0)
            {
                MainProgramCode.ShowInformation("Please ensure the input for the price of the pump is correct and longer than 2 characters.", "INFORMATION - Pump Price Input Incorrect");
                mtxtNewPumpPrice.Focus();
                return false;
            }
            return true;
        }

        BindingList<Product_Part> RetreivePumpPartList()
        {
            //When done Editing / Adding a pump, all mandatory parts need to be added first to the part list
            //This is for the for loop when the form gets activated to work correctly.

            BindingList<Product_Part> ReturnList = null;
            Product_Part newPart;
            //Mandatory added first
            for (int i = 0; i < dgvMandatoryPartView.Rows.Count; i++)
                try
                {
                    if ((bool)(dgvMandatoryPartView.Rows[i].Cells["clmAddToPumpSelection"].Value) == true)
                    {
                        try
                        {
                            newPart = new Product_Part(passed.MandatoryPartMap.Values.ToArray()[i], QuoteSwiftMainCode.ParseInt(dgvMandatoryPartView.Rows[i].Cells["clmMPartQuantity"].Value.ToString())); // Cast used rather than convert; not much but to a degree faster
                        }
                        catch
                        {
                            newPart = null;
                        }

                        if (newPart != null)
                            if (ReturnList == null)
                                ReturnList = new BindingList<Product_Part> { newPart };
                            else ReturnList.Add(newPart);
                    }
                }
                catch
                {
                    //Do Nothing
                }


            //Non-Mandatory added second
            for (int k = 0; k < dgvNonMandatoryPartView.Rows.Count; k++)
                try
                {
                    if ((bool)(dgvNonMandatoryPartView.Rows[k].Cells["ClmNonMandatoryPartSelection"].Value) == true)
                    {
                        try
                        {
                            newPart = new Product_Part(passed.NonMandatoryPartMap.Values.ToArray()[k], QuoteSwiftMainCode.ParseInt(dgvNonMandatoryPartView.Rows[k].Cells["clmNMPartQuantity"].Value.ToString())); // Cast used rather than convert; not much but to a degree faster
                        }
                        catch
                        {
                            newPart = null;
                        }

                        if (newPart != null)
                            if (ReturnList == null)
                                ReturnList = new BindingList<Product_Part> { newPart };
                            else ReturnList.Add(newPart);
                    }
                }
                catch
                {
                    //Do Nothing
                }

            return ReturnList;
        }

        void ChangeViewToEdit()
        {
            if (passed != null && passed.PumpToChange != null && passed.ChangeSpecificObject == false)
                if (MainProgramCode.RequestConfirmation("You are currently viewing " + passed.PumpToChange.ProductName + " pump, would you like to edit it instead?", "REQUEST - View To Edit REQUEST"))
                {
                    ConvertToEditForm();
                    passed.ChangeSpecificObject = true;
                }
        }

        void RecordNewInformation()
        {
            if (mtxtPumpName.Text != passed.PumpToChange.ProductName) passed.PumpToChange.ProductName = mtxtPumpName.Text;

            if (mtxtPumpDescription.Text != passed.PumpToChange.PumpDescription) passed.PumpToChange.PumpDescription = mtxtPumpDescription.Text;

            if (NewPumpValueInput() != passed.PumpToChange.NewProductPrice) passed.PumpToChange.NewProductPrice = NewPumpValueInput();

            passed.PumpToChange.PartList = RetreivePumpPartList();
        }

        float NewPumpValueInput()
        {
            float.TryParse(mtxtNewPumpPrice.Text, out float TempNewPumpPrice);
            return TempNewPumpPrice;
        }



        void LoadMandatoryParts()
        {
            if (passed.MandatoryPartMap != null)
            {
                dgvMandatoryPartView.Rows.Clear();

                for (int i = 0; i < passed.MandatoryPartMap.Count; i++)
                {
                    //Manually setting the data grid's rows' values:
                    dgvMandatoryPartView.Rows.Add(passed.MandatoryPartMap.Values.ToArray()[i].PartName, passed.MandatoryPartMap.Values.ToArray()[i].PartDescription, passed.MandatoryPartMap.Values.ToArray()[i].OriginalItemPartNumber, passed.MandatoryPartMap.Values.ToArray()[i].NewPartNumber, passed.MandatoryPartMap.Values.ToArray()[i].PartPrice, false, 0);
                }
            }
        }

        void LoadNonMandatoryParts()
        {
            if (passed.NonMandatoryPartMap != null)
            {
                dgvNonMandatoryPartView.Rows.Clear();

                for (int k = 0; k < passed.NonMandatoryPartMap.Count; k++)
                {
                    //Manually setting the data grid's rows' values:
                    dgvNonMandatoryPartView.Rows.Add(passed.NonMandatoryPartMap.Values.ToArray()[k].PartName, passed.NonMandatoryPartMap.Values.ToArray()[k].PartDescription, passed.NonMandatoryPartMap.Values.ToArray()[k].OriginalItemPartNumber, passed.NonMandatoryPartMap.Values.ToArray()[k].NewPartNumber, passed.NonMandatoryPartMap.Values.ToArray()[k].PartPrice, false, 0);
                }
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (MainProgramCode.RequestConfirmation("By canceling the current event, any parts not added will not be available in the part's list.", "REQUEAST - Action Cancellation")) Close();
        }

        private void UpdatePumpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!passed.ChangeSpecificObject)
                ChangeViewToEdit();
            updatePumpToolStripMenuItem.Enabled = false;
        }

        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Still Needs Implementation.
        }

        private void FrmAddPump_FormClosing(object sender, FormClosingEventArgs e)
        {
            QuoteSwiftMainCode.CloseApplication(true);
        }

        /*********************************************************************************/


    }
}