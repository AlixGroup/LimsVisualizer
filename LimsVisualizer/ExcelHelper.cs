﻿using System;
using System.Reflection;
using Excel = Microsoft.Office.Interop.Excel;

namespace LimsVisualizer
{
    class ExcelHelper
    {
        private Excel.Application mApplication;
        private Excel._Workbook mWorkbook;
        private Excel._Worksheet mWorksheet;
        private Excel.Range mRange;

        public void StartExcel()
        {
            try
            {
                MainForm.LogWriter.WriteDebugMessage("Starting Excel.");
                mApplication = new Excel.Application {Visible = true};
                mApplication.UserControl = false;
                MainForm.LogWriter.WriteDebugMessage("Started Excel.");
            }
            catch (Exception exception)
            {
                MainForm.LogWriter.WriteFailureMessage("Starting Excel failed!");
                MainForm.LogWriter.WriteException(exception);
                MainForm.ShowErrorMessage(exception.Message);
            }
        }

        public void AddHeaders(Document document)
        {
            try
            {
                MainForm.LogWriter.WriteDebugMessage(string.Format("Adding Header. Device: '{0}' Line: '{1}' Product: '{2}'",
                                                                        document.Summary.Device.Id,
                                                                        document.Summary.Line.Name,
                                                                        document.Summary.ActiveProduct.Product.Name));

                var channelNames = new string[document.MeasuringData.Channels.Count + 1];
                channelNames[0] = "Timestamp";

                for (int i = 1; i < channelNames.Length; i++)
                {
                    channelNames[i] = document.MeasuringData.Channels[i - 1].Name + " [" + document.MeasuringData.Channels[i - 1].Unit.Name + "]";
                }

                mWorksheet.Cells[1, 1] = "Device Name:";
                mWorksheet.Cells[1, 2] = document.Summary.Device.Id;
                mWorksheet.Cells[2, 1] = "Line Name:";
                mWorksheet.Cells[2, 2] = document.Summary.Line.Name;
                mWorksheet.Cells[3, 1] = "Product Type:";
                mWorksheet.Cells[3, 2] = document.Summary.ActiveProduct.Product.ProductType.Name;
                mWorksheet.Cells[4, 1] = "Product Name:";
                mWorksheet.Cells[4, 2] = document.Summary.ActiveProduct.Product.Name;

                mWorksheet.Range["A1", "B4"].Borders.Weight = Excel.XlBorderWeight.xlMedium;

                mRange = mWorksheet.Range["A6", "A6"].Resize[Missing.Value, channelNames.Length];
                mRange.Value2 = channelNames;
                mRange.Font.Bold = true;
                mRange.EntireColumn.AutoFit();
                mRange.Borders.Weight = Excel.XlBorderWeight.xlMedium;
                mWorksheet.Application.ActiveWindow.SplitRow = 6;
                mWorksheet.Application.ActiveWindow.FreezePanes = true;
                MainForm.LogWriter.WriteDebugMessage("Added Header successfully.");
            }
            catch (Exception exception)
            {
                MainForm.LogWriter.WriteFailureMessage("Adding Header failed!");
                MainForm.LogWriter.WriteException(exception);
                MainForm.ShowErrorMessage(exception.Message);
            }
        }

        public void AddMeasuringValues(Document document)
        {
            try
            {
                MainForm.LogWriter.WriteDebugMessage(string.Format("Adding Measuring Data. Device: '{0}' Line: '{1}' Product: '{2}'",
                                                                        document.Summary.Device.Id,
                                                                        document.Summary.Line.Name,
                                                                        document.Summary.ActiveProduct.Product.Name));

                mApplication.ScreenUpdating = false;
                var measuringValues = new double[document.MeasuringData.Channels.Count];

                for (var i = 0; i < measuringValues.Length; i++)
                {
                    if (document.MeasuringData.Channels[i].MeasuringValue.Value != null)
                    {
                        measuringValues[i] = Convert.ToDouble(document.MeasuringData.Channels[i].MeasuringValue.Value);
                    }
                }

                //Add timestamp
                var cell = "A" + (mWorksheet.UsedRange.Rows.Count + 1);
                mWorkbook.Activate();
                mWorksheet.Activate();
                mRange = mWorksheet.Range[cell];
                mRange.Value2 = document.MeasuringData.Timestamp.Local.ToShortDateString() + " " +
                                document.MeasuringData.Timestamp.Local.ToLongTimeString() + "." + document.MeasuringData.Timestamp.Local.Millisecond;

                //Add measuring values
                cell = "B" + (mWorksheet.UsedRange.Rows.Count);
                mRange = mWorksheet.Range[cell, cell].Resize[Missing.Value, measuringValues.Length];
                mRange.Value2 = measuringValues;

                //Format row
                cell = "A" + (mWorksheet.UsedRange.Rows.Count);
                mRange = mWorksheet.Range[cell, cell].Resize[Missing.Value, measuringValues.Length + 1];
                mRange.EntireColumn.AutoFit();
                mRange.Borders.Weight = Excel.XlBorderWeight.xlThin;

                //Display and focus new row
                mApplication.ScreenUpdating = true;
                mWorkbook.Activate();
                mWorksheet.Activate();
                mRange.Activate();

                MainForm.LogWriter.WriteDebugMessage("Added Measuring Data successfully.");
            }
            catch (Exception exception)
            {
                MainForm.LogWriter.WriteFailureMessage("Adding Measuring Data failed!");
                MainForm.LogWriter.WriteException(exception);
                MainForm.ShowErrorMessage(exception.Message);
            }
        }

        public void Dispose()
        {
            try
            {
                MainForm.LogWriter.WriteDebugMessage("Disposing Excel.");

                //Give the user control of Microsoft Excel's lifetime.
                mApplication.UserControl = true;

                if (mRange != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(mRange);

                if (mWorksheet != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(mWorksheet);

                if (mWorkbook != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(mWorkbook);

                if (mApplication != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(mApplication);

                MainForm.LogWriter.WriteDebugMessage("Disposed Excel successfully.");
            }
            catch (Exception exception)
            {
                MainForm.LogWriter.WriteFailureMessage("Disposing failed!");
                MainForm.LogWriter.WriteException(exception);
                MainForm.ShowErrorMessage(exception.Message);
            }
        }

        public void CreateWorkbook()
        {
            MainForm.LogWriter.WriteDebugMessage("Creating new workbook.");
            mWorkbook = mApplication.Workbooks.Add(Missing.Value);
            mWorksheet = (Excel._Worksheet)mWorkbook.ActiveSheet;
            MainForm.HeadersWritten = false;
            MainForm.LogWriter.WriteDebugMessage("Created new workbook.");
        }
    }
}